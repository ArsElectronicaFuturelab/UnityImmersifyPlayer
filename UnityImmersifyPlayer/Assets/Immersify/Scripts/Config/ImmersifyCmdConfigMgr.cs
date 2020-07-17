using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DeepSpace;

public class ImmersifyCmdConfigMgr : UdpCmdConfigMgr
{
	// Values for your own configuration...
	[Space]
	public ImmersifyPlugin.PathType videoPathType = ImmersifyPlugin.PathType.AbsolutePath;
	//public EquirectangularUvController.StereoVideoMode stereoVideoMode = EquirectangularUvController.StereoVideoMode.MONO; // TODO: Implement this.
	public string videoFile = "";
	//public int videoWidth = 0;
	//public int videoHeight = 0;
	public float videoFramerate = 30f;
	//public ImmersifyPlugin.ChromaSubsampling chromaSubsampling = ImmersifyPlugin.ChromaSubsampling._422;
	public int maxQueue = 16;
	public ImmersifyPlugin.StereoVideoMode stereoMode = ImmersifyPlugin.StereoVideoMode.MONO;
	public bool invertLeftRight = false;
	public bool videoIsUpsideDown = false;
	[Space]
	[SerializeField]
	protected ImmersifyPlugin.PathType _audioPathType = ImmersifyPlugin.PathType.AbsolutePath;
	protected bool _audioPathTypeGotSetExplicitly = false;
	public string audioFile = string.Empty;
	protected AudioClip _audioClip = null;

	public AudioClip AudioClip
	{
		get { return _audioClip; }
	}

	public bool AudioClipIsLoading
	{
		get;
		private set;
	} = false;

	public ImmersifyPlugin.PathType AudioPathType
	{
		get
		{
#if UNITY_EDITOR
			return _audioPathType;
#else
			return (_audioPathTypeGotSetExplicitly == true ? _audioPathType : videoPathType);
#endif
		}
	}

	protected override void ParseArgument(string key, string value)
	{
		key = key.ToLower();
		value = value.ToLower();

		if (key.Equals("-videopathtype"))
		{
			if (value.Equals("streamingassets"))
			{
				videoPathType = ImmersifyPlugin.PathType.StreamingAssets;
			}
			else if (value.Equals("absolutepath"))
			{
				videoPathType = ImmersifyPlugin.PathType.AbsolutePath;
			}
		}
		else if (key.Equals("-audiopathtype"))
		{
			if (value.Equals("streamingassets"))
			{
				_audioPathType = ImmersifyPlugin.PathType.StreamingAssets;
				_audioPathTypeGotSetExplicitly = true;
			}
			else if (value.Equals("absolutepath"))
			{
				_audioPathType = ImmersifyPlugin.PathType.AbsolutePath;
				_audioPathTypeGotSetExplicitly = true;
			}
		}
		else if (key.Equals("-videofile"))
		{
			videoFile = value;
			Debug.Log("Videofile got set: " + value);
		}
		else if (key.Equals("-audiofile"))
		{
			audioFile = value;
			Debug.Log("Audiofile got set: " + value);
		}
		//else if (key.Equals("-videowidth"))
		//{
		//	int resultWidth;
		//	if (ValueToInt(value, out resultWidth))
		//	{
		//		videoWidth = resultWidth;
		//	}
		//}
		//else if (key.Equals("-videoheight"))
		//{
		//	int resultHeight;
		//	if (ValueToInt(value, out resultHeight))
		//	{
		//		videoHeight = resultHeight;
		//	}
		//}
		else if (key.Equals("-videoframerate"))
		{
			float resultFramerate;
			if (ValueToFloat(value, out resultFramerate))
			{
				videoFramerate = resultFramerate;
			}
		}
		//else if(key.Equals("-chromasubsampling"))
		//{
		//	int resultSubsampling;
		//	if (ValueToInt(value, out resultSubsampling))
		//	{
		//		switch (resultSubsampling)
		//		{
		//			case 444:
		//				chromaSubsampling = ImmersifyPlugin.ChromaSubsampling._444;
		//				break;
		//			case 422:
		//				chromaSubsampling = ImmersifyPlugin.ChromaSubsampling._422;
		//				break;
		//			case 420:
		//				chromaSubsampling = ImmersifyPlugin.ChromaSubsampling._420;
		//				break;
		//			default:
		//				Debug.LogWarning("Could not handle chroma subsampling " + resultSubsampling);
		//				break;
		//		}
		//	}
		//}
		else if(key.Equals("-maxqueue"))
		{
			int resultMaxQueue;
			if(ValueToInt(value, out resultMaxQueue))
			{
				maxQueue = Mathf.Max(1, resultMaxQueue); // maxQueue must not be 0 or negative.
			}
		}
		else if(key.Equals("-stereomode"))
		{
			if (value.Equals("mono"))
			{
				stereoMode = ImmersifyPlugin.StereoVideoMode.MONO;
			}
			else if (value == "topbottom")
			{
				stereoMode = ImmersifyPlugin.StereoVideoMode.TOP_BOTTOM;
			}
			else if (value == "sidebyside")
			{
				stereoMode = ImmersifyPlugin.StereoVideoMode.LEFT_RIGHT;
			}
		}
		else if(key.Equals("-invertleftright"))
		{
			bool resultValue;
			if(ValueToBool(value, out resultValue))
			{
				invertLeftRight = resultValue;
			}
		}
		else if (key.Equals("-videoisupsidedown"))
		{
			bool resultValue;
			if (ValueToBool(value, out resultValue))
			{
				videoIsUpsideDown = resultValue;
			}
		}
		else
		{
			base.ParseArgument(key, value);
		}
	}

	protected override void FinishedParsingArguments()
	{
		if (string.IsNullOrEmpty(audioFile) == false) // If AudioFile was set via command line arguments:
		{
			// Distinct between endings. (mp3, ogg, wav), implement your own if neccesary.
			// NOTE: MPEG wont work for license reasons, see https://answers.unity.com/questions/1532988/unitywebrequestmultimedia-to-get-audioclip-from-ap.html
			AudioType audioType = AudioType.UNKNOWN;
			string fileEnding = audioFile.Substring(Mathf.Max(0, audioFile.Length - 4)).ToLower();
			if (fileEnding.EndsWith(".mp3") || fileEnding.EndsWith(".mp2") || fileEnding.EndsWith(".aac"))
			{
				audioType = AudioType.MPEG;
			}
			else if(fileEnding.EndsWith(".ogg"))
			{
				audioType = AudioType.OGGVORBIS;
			}
			else if(fileEnding.EndsWith(".wav"))
			{
				audioType = AudioType.WAV;
			}

			string filePath = audioFile;
			// If the audio path type is not an absolute path, we need to change the filePath:
			if (AudioPathType == ImmersifyPlugin.PathType.StreamingAssets)
			{
				filePath = Application.streamingAssetsPath + "/" + audioFile;
			}

			// Download audio from hard drive:
			StartCoroutine(DownloadAudioFile(filePath, audioType));
		}

		base.FinishedParsingArguments();
	}

	private IEnumerator DownloadAudioFile(string filepath, AudioType audioType)
	{
		Debug.Log("Audiofile is going to be loaded...");
		AudioClipIsLoading = true;

		using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(filepath, audioType))
		{
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				Debug.Log(webRequest.error);
			}
			else
			{
				_audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
			}
		}

		AudioClipIsLoading = false;
		Debug.Log("Audiofile got loaded " + (_audioClip != null ? "successfully" : "unsuccessfully") + ".");
	}
}
