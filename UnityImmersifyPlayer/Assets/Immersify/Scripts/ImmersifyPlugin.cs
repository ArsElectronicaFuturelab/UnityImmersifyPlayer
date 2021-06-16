using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImmersifyPlugin : MonoBehaviour
{
	public enum PathType
	{
		StreamingAssets = 0, // File is in StreamingAssets (e.g. "MyVideo.hvec")
		AbsolutePath = 1 // File is absolute path to a video (e.g. "C:/Videos/MyVideo.hvec")
	}

	public enum ChromaSubsampling
	{
		_420 = 0,
		_422 = 1,
		_444 = 2
	};

	public enum StereoVideoMode
	{
		MONO,
		LEFT_RIGHT,
		TOP_BOTTOM // Expecting Top = Left Eye and Bottom = Right Eye.
	}

	public enum PlayState
	{
		UNINITIALIZED,
		INITIALIZED,
		PLAYING,
		PAUSED,
		STOPPED
	}

	private const string PluginName = "ImmersifyUnityPlugin";

	public delegate void TexturesChangedDelegate(ImmersifyPlugin plugin);
	public delegate void VideoFinishedDelegate(ImmersifyPlugin plugin);

	public static event TexturesChangedDelegate OnTexturesChanged;
	public static event VideoFinishedDelegate OnVideoFinished;

	[SerializeField]
	private PathType _videoPathType = PathType.AbsolutePath;
	[SerializeField]
	private string _filename = "";
	[SerializeField, Min(1f), Tooltip("The framerate, the video needs to play at normal speed.")]
	private float _framerate = 30f;
	[SerializeField]
	private bool _loopVideo = false;
	[SerializeField, Tooltip("Shall the video be loaded and display the first frame ")]
	private bool _pauseAtStart = false;

	[SerializeField, Space]
	private StereoVideoMode _stereoMode = StereoVideoMode.MONO;
	[SerializeField, Tooltip("If e.g. the left side of the video contains the right video part or the top half of the video contains the right video part, set this true to apply the video correctly.")]
	private bool _invertLeftRight = false;
	[SerializeField, Tooltip("If the video is upside down, set this true to invert the scaling (and therfore correct the video).")]
	private bool _videoIsUpsideDown = false;

	[SerializeField, Space]
	private AudioSource _audioSrcVideo = null;
	[SerializeField]
	private AudioSource _audioSrcBgLoop = null;

	[SerializeField, Space, Tooltip("Define the amount of threads that shall be used by the decoder, -1 is the default value (the decoder takes what it needs).")]
	private int _numberOfThreads = -1;
	[SerializeField, Tooltip("Define the size of the Buffer (Frames that shall be preloaded loaded), -1 is the default value and sets the Buffer size to 120.")]
	private int _videoBufferSize = -1;
	[SerializeField, Tooltip("Defines the maximum amount of buffered frames that are decoded beforehead and put into a buffer.")]
	private int _maxQueue = 16;

	[SerializeField, Space]
	private bool _clampTextures = false;

	[SerializeField, Space]
	private bool _shouldLog = false;

	private IntPtr _playerInstance = IntPtr.Zero; // A C++ pointer to the Sequencer, that manages playing a movie.

	// These Textures could be marked as ReadOnly for debug purposes.
	private Texture2D _textureY = null;
	private Texture2D _textureU = null;
	private Texture2D _textureV = null;
	[SerializeField, ReadOnly]
	private ChromaSubsampling _chromaSubsampling = ChromaSubsampling._420;

	private TextureFormat _format = TextureFormat.BC4;
	private CommandBuffer _command;
	private bool _isFading = false;

	// This is set false when running as floor from WallFloorStarter, because the floor video is processed externaly via UDP packages comming from the wall.
	public bool processVideoTime = true;
	private float _lastVideoPlayTime = 0f;

	[SerializeField, Min(1f), Tooltip("Defines the maximum amount of frames that can be catched up in case of a short lag.")]
	private float _maxCatchUpFrames = 1f;

	private PlayState _playState = PlayState.UNINITIALIZED;

	private float _targetFramerate = 30f; // The target framerate to play the video with. This divided by the _framerate gives a factor, which needs to be mul with delta time.
	private float _targetTime = 0f; // The time the video shall be currently.

	private float _targetPlayTime = 0f;
	private bool _shallPause = false;
	private float _shallPauseAt = 0f; // Pause at this frame

	public Texture2D TextureY
	{
		get { return _textureY; }
		private set { _textureY = value; }
	}

	public Texture2D TextureU
	{
		get { return _textureU; }
		private set { _textureU = value; }
	}

	public Texture2D TextureV
	{
		get { return _textureV; }
		private set { _textureV = value; }
	}

	public float Framerate
	{
		get { return _framerate; }
	}

	public StereoVideoMode StereoMode
	{
		get { return _stereoMode; }
		private set { _stereoMode = value; }
	}

	public bool InvertLeftRight
	{
		get { return _invertLeftRight; }
		private set { _invertLeftRight = value; }
	}

	public bool VideoIsUpsideDown
	{
		get { return _videoIsUpsideDown; }
		private set { _videoIsUpsideDown = value; }
	}

	#region DLL Imports
	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr CreateSequencer(string path, int numberOfThreads = -1, int videoBufferSize = -1, int maxQueue = 16, bool shouldLog = false);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void InitPlayer(IntPtr playerInstance, IntPtr texturePtrY, IntPtr texturePtrU, IntPtr texturePtrV, int format);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void Play(IntPtr playerInstance, float framerate, bool shouldLoop);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool IsReady(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool SeekToMSec(IntPtr playerInstance, int targetTimeMS);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool GetSeekingIsSupported(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool IsFinished(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void SetPause(IntPtr playerInstance, bool pause);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool IsPaused(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void PauseAfterFirstFrame(IntPtr playerInstance, bool pauseAfFrame);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void OverrideTargetPlayingTime(IntPtr playerInstance, float targetTime);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern float GetCurrentPlayingTime(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void OverrideTargetFPS(IntPtr playerInstance, float fps);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern float GetTargetFPS(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern int GetVideoWidth(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern int GetVideoHeight(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern int GetVideoChromaSubsampling(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern long GetVideoDurationInMS(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern int GetMaxQueueSize(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr GetRenderEventFunc();

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void UnityPluginUnload();

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr GetTextureUpdateCallback(IntPtr playerInstance);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void RegisterDebugCallback(debugCallback cb);

	[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	private static extern int GetCurrentErrorCode(IntPtr playerInstance);

	//[DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
	//private static extern void DestroyPlayer(IntPtr playerInstance);

	private delegate void debugCallback(IntPtr request, int size);

	[MonoPInvokeCallback(typeof(debugCallback))]
	private static void OnDebugCallback(IntPtr request, int size)
	{
		string debug_string = Marshal.PtrToStringAnsi(request, size);
		Debug.Log(debug_string);
	}
	#endregion

	public bool SetPause(bool pause)
	{
		if (_playerInstance != IntPtr.Zero)
		{
			if (pause == true)
			{
				//OverrideTargetFPS(_framerate);
				SetTargetFramerate(_framerate);
			}

			SetPause(_playerInstance, pause);

			_playState = (pause ? PlayState.PAUSED : PlayState.PLAYING);

			return true;
		}

		return false;
	}

	// Call this method, if a pause command comes over network and the video shall pause when it is in sync with the master.
	public void PauseToSync()
	{
		_shallPause = true;
		_shallPauseAt = _targetPlayTime;
	}

	public bool IsPaused()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return IsPaused(_playerInstance);
		}

		return false; // TODO: Maybe with out parameter for isPaused, and return means success of function call.
	}

	public bool IsReady()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return IsReady(_playerInstance);
		}

		return false; // TODO: Maybe with out parameter for isReady, and return means success of function call.
	}

	public void SeekToMS(int targetTimeMS)
	{
		if (_playerInstance != IntPtr.Zero)
		{
			if(GetSeekingIsSupported() == true)
			{
				// Set Video to the requested position:
				SeekToMSec(_playerInstance, targetTimeMS);

				// Set Audio to the same position:
				if(_audioSrcVideo != null)
				{
					float targetPercent = targetTimeMS / GetVideoDurationInMS();
					float audioTime = _audioSrcVideo.clip.length * targetPercent;
					_audioSrcVideo.time = audioTime;
				}
			}
		}
	}

	public bool GetSeekingIsSupported()
	{
		bool result = false;

		if (_playerInstance != IntPtr.Zero)
		{
			result = GetSeekingIsSupported(_playerInstance);
		}

		return result;
	}

	public bool IsFinished()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return IsFinished(_playerInstance);
		}

		return false; // TODO: Maybe with out parameter for isFinished, and return means success of function call.
	}

	public int GetVideoWidth()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetVideoWidth(_playerInstance);
		}

		return -1;
	}

	public int GetVideoHeight()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetVideoHeight(_playerInstance);
		}

		return -1;
	}

	public int GetVideoChromaSubsampling()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetVideoChromaSubsampling(_playerInstance);
		}

		return -1;
	}

	public long GetVideoDurationInMS()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetVideoDurationInMS(_playerInstance);
		}

		return -1L;
	}

	public int GetMaxQueueSize()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetMaxQueueSize(_playerInstance);
		}

		return -1;
	}

	private int GetCurrentErrorCode()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetCurrentErrorCode(_playerInstance);
		}

		return -1;
	}

	private void Awake()
	{
		// RegisterDebugCallback(OnDebugCallback);

		_command = new CommandBuffer();
		_command.name = PluginName + " CommandBuffer";
	}

	private void Start()
	{
		// If start was not yet called and a video file name is set.
		if (_playState == PlayState.UNINITIALIZED && string.IsNullOrEmpty(_filename) == false)
		{
			AudioClip audioClip = null;
			if (_audioSrcVideo != null)
			{
				audioClip = _audioSrcVideo.clip;
			}
            //TODO Check if file exists, otherwise throw exception (or alternate error handling)

            if (System.IO.File.Exists(_filename))
            {
                InitializePlayer(_filename, _videoPathType, _framerate, audioClip, _maxQueue, _stereoMode, _invertLeftRight, _videoIsUpsideDown, _pauseAtStart, _loopVideo);

                Play(); // initPlayer: true
            }
            else
            {
                Debug.Log("The file \"" + _filename + "\" does not exist");
            }
		}

		if (_audioSrcBgLoop != null)
		{
			_audioSrcBgLoop.volume = 0f;
			_audioSrcBgLoop.Play();
		}

#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
	}

	private void Update()
	{
		//Debug.Log("Delta: " + Time.deltaTime + ", Time: " + Time.time + ", Plugin Time: " + GetCurrentPlayingTime());

		if (_playerInstance != IntPtr.Zero)
		{
			if (processVideoTime == true)
			{
				if (_playState == PlayState.PLAYING) // && IsPaused() == false)
				{
					// If video has reached its end and loop is true, reset target time:
					if (_loopVideo == true)
					{
						float curVideoPlayTime = GetCurrentPlayTime();
						if (curVideoPlayTime < _lastVideoPlayTime)
						{
							_targetTime = 0f;
						}
						_lastVideoPlayTime = curVideoPlayTime;
					}

					UpdateVideoTargetTime(Time.deltaTime * 1000f);

					// Set target time in ms:
					_targetTime = Mathf.Min(GetCurrentPlayTime() + 1000f * _maxCatchUpFrames / _framerate, _targetTime);
					SetTargetPlayTime(_targetTime);
				}
			}

			if (_playState == PlayState.PLAYING) // && IsPaused() == false)
			{
				if (IsFinished() == true)
				{
					_playState = PlayState.STOPPED;

					Debug.Log("Video finished");

					if (OnVideoFinished != null)
					{
						foreach (VideoFinishedDelegate VideoFinished in OnVideoFinished.GetInvocationList())
						{
							try
							{
								VideoFinished(this);
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
					}
				}
				if (_shallPause == true)
				{
					if (GetCurrentPlayTime() >= _shallPauseAt)
					{
						_shallPause = false;
						Pause();
					}
				}
			}
			else if (_playState == PlayState.PAUSED)
			{
				if (GetCurrentPlayTime() < _shallPauseAt)
				{
					_shallPause = true;
					Resume();
				}
			}

			_command.IssuePluginEventAndData(GetRenderEventFunc(), 0, _playerInstance);
			Graphics.ExecuteCommandBuffer(_command);
			_command.Clear();
		}
	}

#if UNITY_EDITOR
	private void PlayModeChanged(PlayModeStateChange state)
	{
		// This crashes the Editor: (not anymore, whyever...)
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			Debug.Log("Playmode is exiting, clearing the player instance.");

			if (_playerInstance != IntPtr.Zero)
			{
				_command.IssuePluginEventAndData(GetRenderEventFunc(), -1, _playerInstance); // -1 means destroy this player instance in the C++ plugin.
				Graphics.ExecuteCommandBuffer(_command);
				_command.Clear();

				_playerInstance = IntPtr.Zero;
			}
		}
	}
#endif

	private void OnDestroy()
	{
		DestroyTextures();

		_command.Dispose();
	}

	private void DestroyTextures()
	{
		if (_textureY != null)
		{
			Destroy(_textureY);
			_textureY = null;
		}
		if (_textureU != null)
		{
			Destroy(_textureU);
			_textureU = null;
		}
		if (_textureV != null)
		{
			Destroy(_textureV);
			_textureV = null;
		}
	}

	// play: if true fading goes from paused to play (speed 0 -> 1); else fading goes from play to pause (speed 1 -> 0).
	private IEnumerator ToggleFadedPlayPauseRoutine(bool play, float duration)
	{
		if (_playerInstance == IntPtr.Zero)
		{
			yield break;
		}

		while (_isFading)
		{
			yield return null;
		}

		float elapsedTime = 0;
		//float startingFPS = play ? 0f : _framerate; // slow down the original speed and pause
		float endingFPS = play ? _framerate : 0f; // fade to the original speed
		SetPause(false); // Unpause in case it is paused

		if (_audioSrcVideo)
		{
			if (_audioSrcVideo.isPlaying == false)
			{
				// Set the audio to the correct time, so when the video is running at normal speed, the audio is in sync:
				_audioSrcVideo.time = Mathf.Max(0f, _audioSrcVideo.time - FadeFunctions.RisingQuarticParabolaIntegral * duration);

				_audioSrcVideo.volume = 0f;
				_audioSrcVideo.UnPause();
			}
		}
		_isFading = true;

		while (elapsedTime < duration)
		{
			//float percent = (elapsedTime / duration);
			float percent = (play ? FadeFunctions.RisingQuarticParabolaFunction(elapsedTime, duration) : FadeFunctions.FallingQuarticParabolaFunction(elapsedTime, duration));

			float currentFPS = Mathf.Lerp(0f, _framerate, percent);
			//OverrideTargetFPS(_playerInstance, currentFPS);
			SetTargetFramerate(currentFPS);

			if (_audioSrcVideo)
			{
				// TODO: Do not pich but fade volume... / maybe fade is better linear than using this curve.
				//_audioSrc.pitch = (play ? percent : 1f - percent);
				_audioSrcVideo.volume = percent; // percent is in range [0, 1]
			}
			if (_audioSrcBgLoop)
			{
				_audioSrcBgLoop.volume = 1f - percent;
			}

			elapsedTime += Time.deltaTime;

			yield return null;
		}

		if (play == false) // Aim is Pause
		{
			//OverrideTargetFPS(_playerInstance, endingFPS);
			SetTargetFramerate(endingFPS);
			SetPause(true);

			if (_audioSrcVideo)
			{
				_audioSrcVideo.Pause();

				// If it gets unpaused, volume is already set to 1.
				_audioSrcVideo.volume = 1.0f;

				// Set Audio Track position to the time, where the video is currently (If it gets unpaused quickly, it is already on the correct position):
				_audioSrcVideo.time = Mathf.Max(0f, _audioSrcVideo.time - FadeFunctions.FallingQuarticParabolaIntegral * duration);
			}
			if (_audioSrcBgLoop)
			{
				_audioSrcBgLoop.volume = 1f;
			}
		}
		else // Aim is Play
		{
			//OverrideTargetFPS(_playerInstance, endingFPS);
			SetTargetFramerate(endingFPS);

			if (_audioSrcVideo)
			{
				//_audioSrc.UnPause(); // Not needed here, It was already unpaused.
				_audioSrcVideo.volume = 1.0f;
			}
			if (_audioSrcBgLoop)
			{
				_audioSrcBgLoop.volume = 0f;
			}
		}

		_isFading = false;
	}

	//public void OverrideTargetFPS(float speed)
	//{
	//	if (_playerInstance != IntPtr.Zero)
	//	{
	//		OverrideTargetFPS(_playerInstance, speed);
	//	}
	//}

	public void ToggleFadedPlayPause(bool play, float duration = 2f)
	{
		StartCoroutine(ToggleFadedPlayPauseRoutine(play, duration));
	}

	public float GetCurrentPlayTime()
	{
		if (_playerInstance != IntPtr.Zero)
		{
			return GetCurrentPlayingTime(_playerInstance);
		}

		return 0f; // TODO: Maybe with bool return and out return value for playing time...
	}

	public bool SetTargetPlayTime(float targetTime)
	{
		if (_playerInstance != IntPtr.Zero)
		{
			OverrideTargetPlayingTime(_playerInstance, targetTime);
			_targetPlayTime = targetTime;

			if (_shallPause)
			{
				_shallPauseAt = targetTime;
			}

			return true;
		}

		return false;
	}

	public void Resume()
	{
		SetPause(false);

		if (_audioSrcVideo)
		{
			_audioSrcVideo.UnPause();
		}

		if (_audioSrcBgLoop)
		{
			_audioSrcBgLoop.volume = 0f;
		}
	}

	public void Pause()
	{
		SetPause(true);

		if (_audioSrcVideo)
		{
			_audioSrcVideo.Pause();
		}

		if (_audioSrcBgLoop)
		{
			_audioSrcBgLoop.volume = 1f;
		}
	}

	// This needs to be called before calling Play().
	public void InitializePlayer(string filename, PathType videoPathType, float framerate, AudioClip audioClip, int maxQueue, StereoVideoMode stereoMode, bool invertLeftRight, bool videoIsUpsideDown, bool pauseAtStart = true, bool loopVideo = false)
	{
		// If player was already initialized, destroy the old instance:
		if (_playerInstance != IntPtr.Zero)
		{
			_command.IssuePluginEventAndData(GetRenderEventFunc(), -1, _playerInstance); // -1 means destroy this player instance in the C++ plugin.
			Graphics.ExecuteCommandBuffer(_command);
			_command.Clear();

			_playerInstance = IntPtr.Zero;

			_playState = PlayState.UNINITIALIZED;

			// In case the license only supports one video at a time, it is necessary to wait here for one frame.
			// Else, if we immediately create a new sequencer, the old license is not yet return and the application is not allowed to create a "second one".
			// This is not necessary, if the license does not have this limitation, so we do not wait for a frame here.
		}

		// Set player flags:
		_pauseAtStart = pauseAtStart;
		_filename = filename;
		_videoPathType = videoPathType;
		_loopVideo = loopVideo;
		_framerate = Mathf.Max(1f, framerate); // framerate must not be 0.
		_stereoMode = stereoMode;
		_invertLeftRight = invertLeftRight;
		_videoIsUpsideDown = videoIsUpsideDown;
		_maxQueue = maxQueue;

		// Create a player (sequencer) instance:
		_playerInstance = CreateSequencer(AbsoluteFilePath, _numberOfThreads, _videoBufferSize, _maxQueue, _shouldLog);

		int currentErrorCode = GetCurrentErrorCode();
		if (currentErrorCode != 0)
		{
			Debug.LogError("Error Code " + currentErrorCode + " appeared after calling CreateSequencer.");
			return;
		}

		int playersVideoWidth = GetVideoWidth();
		int playersVideoHeight = GetVideoHeight();
		ChromaSubsampling playersChromaSubsampling = (ChromaSubsampling)GetVideoChromaSubsampling();

		// Check if textures need to be reinitialized with new width and height:
		bool oldTexturesCanBeUsed = (_textureY != null && _textureY.width == playersVideoWidth && _textureY.height == playersVideoHeight && _chromaSubsampling == playersChromaSubsampling);
		if (oldTexturesCanBeUsed == false || _textureY == null || _textureU == null || _textureV == null)
		{
			// Destroy Textures if they are already created and create new ones with the set width and height:
			DestroyTextures();
			CreateTextures(playersVideoWidth, playersVideoHeight, playersChromaSubsampling);
			_chromaSubsampling = playersChromaSubsampling;
		}

		// Pass texture pointer to the plugin and initailize the player:
		InitPlayer(_playerInstance, _textureY.GetNativeTexturePtr(), _textureU.GetNativeTexturePtr(), _textureV.GetNativeTexturePtr(), (int)_format);

		_playState = PlayState.INITIALIZED;

		// Load other audio clip:
		if (_audioSrcVideo)
		{
			if (audioClip != null)
			{
				_audioSrcVideo.clip = audioClip;
			}
		}
	}

	public void Play() // bool initPlayer, bool oldTexturesCanBeUsed = false)
	{
		// Check if player is initialized:
		if (_playerInstance == IntPtr.Zero || _playState == PlayState.UNINITIALIZED)
		{
			Debug.LogError("Play cannot be called with uninitialized player. Call InitializePlayer before.");
			return;
		}

		// Shall the Video be started explicitly or automatically:
		PauseAfterFirstFrame(_playerInstance, _pauseAtStart);

		// Plays the video (or pauses it at the first frame, if _pauseAtStart is true):
		Play(_playerInstance, _framerate, _loopVideo);
		_playState = (_pauseAtStart ? PlayState.PAUSED : PlayState.PLAYING);

		if (_audioSrcVideo)
		{
			_audioSrcVideo.Play();

			if (_pauseAtStart == true)
			{
				_audioSrcVideo.Pause();
			}
		}

		Debug.Log("Playing Video: " + AbsoluteFilePath);
	}

	private void CreateTextures(int width, int height, ChromaSubsampling chromaSubsampling)
	{
		int widthUV = width;
		int heightUV = height;
		if (chromaSubsampling == ChromaSubsampling._420)
		{
			heightUV = height / 2;
			widthUV = width / 2;
		}
		else if (chromaSubsampling == ChromaSubsampling._422)
		{
			widthUV = width / 2;
		}

		_textureY = new Texture2D(width, height, _format, false);
		_textureY.wrapMode = (_clampTextures == true ? TextureWrapMode.Clamp : TextureWrapMode.Repeat);
		_textureY.Apply();

		_textureU = new Texture2D(widthUV, heightUV, _format, false);
		_textureU.wrapMode = (_clampTextures == true ? TextureWrapMode.Clamp : TextureWrapMode.Repeat);
		_textureU.Apply();

		_textureV = new Texture2D(widthUV, heightUV, _format, false);
		_textureV.wrapMode = (_clampTextures == true ? TextureWrapMode.Clamp : TextureWrapMode.Repeat);
		_textureV.Apply();

		if (OnTexturesChanged != null)
		{
			foreach (TexturesChangedDelegate TexturesChanged in OnTexturesChanged.GetInvocationList())
			{
				try
				{
					TexturesChanged(this);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}

	private string AbsoluteFilePath
	{
		get
		{
			string absolutePath = null;

			switch (_videoPathType)
			{
				case PathType.AbsolutePath:
					absolutePath = _filename;
					break;
				case PathType.StreamingAssets:
					absolutePath = Application.streamingAssetsPath + "/" + _filename;
					break;
				default:
					Debug.LogWarning("The chosen VideoPathType is not implemented to be accessed in AbsoluteFilePath.", gameObject);
					break;
			}

			//Debug.Log("Absolute Path is set to: " + absolutePath);
			return absolutePath;
		}
	}

	// @param msDeltaTime: Time to change the current play time about in milli seconds
	public void UpdateVideoTargetTime(float msDeltaTime)
	{
		float applyTime = msDeltaTime * (_targetFramerate / _framerate);

		_targetTime += applyTime;
	}

	public float GetUpdatedVideoTargetTime(float msDeltaTime)
	{
		float addTime = msDeltaTime * (_targetFramerate / _framerate);

		return GetCurrentPlayTime() + addTime;
	}

	public void SetTargetFramerate(float targetFramerate)
	{
		_targetFramerate = targetFramerate;
	}

	public void Mute()
	{
		if (_audioSrcVideo)
		{
			_audioSrcVideo.volume = 0f;
		}
		if (_audioSrcBgLoop)
		{
			_audioSrcBgLoop.volume = 0f;
		}
	}

	public void Unmute()
	{
		if (IsPaused() == false) // If video is playing unmute the video audio:
		{
			if (_audioSrcVideo)
			{
				_audioSrcVideo.volume = 1f;
			}
		}
		else // If video is paused unmute the background loop:
		{
			if (_audioSrcBgLoop)
			{
				_audioSrcBgLoop.volume = 1f;
			}
		}
	}

	public float GetPlaySpeed()
	{
		return _targetFramerate / _framerate;
	}
}
