using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StereoVideoMode = ImmersifyPlugin.StereoVideoMode;

// This Script needs to be attached to a camera, because it needs to receive the OnPreRender callbacks.
// It is able to control the stereo rendering of multiple videos depending on their stereo settings.
[RequireComponent(typeof(Camera))]
public class VideoUvController : MonoBehaviour
{
	private static readonly int _textureTransID = Shader.PropertyToID("_TextureTrans");

	private static readonly int _textureIdY = Shader.PropertyToID("_TexY");
	private static readonly int _textureIdU = Shader.PropertyToID("_TexU");
	private static readonly int _textureIdV = Shader.PropertyToID("_TexV");

	private class ImmersifyPlayer
	{
		public ImmersifyPlayer(ImmersifyMeshVideo immersifyMeshVideo)
		{
			_meshVideo = immersifyMeshVideo;
			_plugin = immersifyMeshVideo.GetImmersifyPlugin();
			_videoPlaneMaterial = immersifyMeshVideo.GetMeshRenderer().material;
		}

		private ImmersifyMeshVideo _meshVideo;
		private ImmersifyPlugin _plugin;
		private Material _videoPlaneMaterial;

		private ImmersifyMeshVideo MeshVideo
		{
			get { return _meshVideo; }
		}

		public ImmersifyPlugin Plugin
		{
			get { return _plugin; }
		}

		public Material VideoPlaneMaterial
		{
			get { return _videoPlaneMaterial; }
		}

		public StereoVideoMode StereoMode
		{
			get { return _plugin.StereoMode; }
		}

		public bool InvertLeftRight
		{
			get { return _plugin.InvertLeftRight; }
		}

		public bool VideoIsUpsideDown
		{
			get { return _plugin.VideoIsUpsideDown; }
		}
	}

	private Camera _camera;
	private List<ImmersifyPlayer> _immersifyPlayerList = new List<ImmersifyPlayer>();

	[SerializeField]
	private List<ImmersifyMeshVideo> _immersifyMeshVideoList = new List<ImmersifyMeshVideo>();


	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void Start()
	{
		foreach (ImmersifyMeshVideo meshVideo in _immersifyMeshVideoList)
		{
			ImmersifyPlayer player = new ImmersifyPlayer(meshVideo);
			_immersifyPlayerList.Add(player);

			if (player.StereoMode == StereoVideoMode.MONO)
			{
				DisplayMonoVideo(player);
			}
			else
			{
				// Just setup the first frame: 
				// If the video is stereo, but the viewer is mono, just the left video is displayed.
				DisplayLeftEyeVideo(player);
			}
		}
	}

	private void Update()
	{
		// Just for demo & test reasons:
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			foreach (ImmersifyPlayer player in _immersifyPlayerList)
			{
				if (player.InvertLeftRight == false)
				{
					DisplayLeftEyeVideo(player);
				}
				else // player.InvertLeftRight == true
				{
					DisplayRightEyeVideo(player);
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			foreach (ImmersifyPlayer player in _immersifyPlayerList)
			{
				if (player.InvertLeftRight == false)
				{
					DisplayRightEyeVideo(player);
				}
				else // player.InvertLeftRight == true
				{
					DisplayLeftEyeVideo(player);
				}
			}
		}
	}

	private void OnPreRender()
	{
		foreach (ImmersifyPlayer player in _immersifyPlayerList)
		{
			// If Video is stereo:
			// > If Editor: Display only left eye (half uv)
			// > If Build: 
			// > > If vrmode stereo: Display stereo
			// > > If vrmode none: Display only left eye (half uv) // vrmode none equals editor!
			// If Video is mono: Display fullscreen uv[0, 1]

			if (player.StereoMode == StereoVideoMode.MONO)
			{
				DisplayMonoVideo(player);
			}
			else // Video is stereo (top-bottom or side-by-side)
			{
				if (string.IsNullOrEmpty(UnityEngine.XR.XRSettings.loadedDeviceName)) // If vrmode is none. (This is the case in editor or if explitly wanted in build via commandline arguments)
				{
					DisplayLeftEyeVideo(player);
				}
				else // vrmode is stereo (in this case we expect non-hmd stereo)
				{
					if (_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
					{
						if (player.InvertLeftRight == false)
						{
							DisplayLeftEyeVideo(player);
						}
						else // player.InvertLeftRight == true
						{
							DisplayRightEyeVideo(player);
						}
					}
					else if (_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
					{
						if (player.InvertLeftRight == false)
						{
							DisplayRightEyeVideo(player);
						}
						else // player.InvertLeftRight == true
						{
							DisplayLeftEyeVideo(player);
						}
					}
				}
			}
		}

		//Debug.Log("Cam Eye: " + _camera.stereoActiveEye);
		//Debug.Log("Loaded device: " + UnityEngine.XR.XRSettings.loadedDeviceName);
	}

	private void ApplyTextureTransformation(ImmersifyPlayer player, float tilingX, float tilingY, float offsetX, float offsetY)
	{
		//Vector4 textureTrans = new Vector4(tilingX, tilingY, offsetX, offsetY);
		Vector2 textureOffset = new Vector2(offsetX, offsetY);
		Vector2 textureScale = new Vector2(tilingX, tilingY);

		if (player.VideoIsUpsideDown == true)
		{
			//textureTrans.y *= -1f;
			textureScale.y *= -1f;
		}

		//_videoPlaneMaterial.SetVector(_textureTransID, textureTrans);

		player.VideoPlaneMaterial.SetTextureOffset(_textureIdY, textureOffset);
		player.VideoPlaneMaterial.SetTextureOffset(_textureIdU, textureOffset);
		player.VideoPlaneMaterial.SetTextureOffset(_textureIdV, textureOffset);

		player.VideoPlaneMaterial.SetTextureScale(_textureIdY, textureScale);
		player.VideoPlaneMaterial.SetTextureScale(_textureIdU, textureScale);
		player.VideoPlaneMaterial.SetTextureScale(_textureIdV, textureScale);
	}

	private void DisplayMonoVideo(ImmersifyPlayer player)
	{
		ApplyTextureTransformation(player, 1f, 1f, 0f, 0f);
	}

	private void DisplayLeftEyeVideo(ImmersifyPlayer player)
	{
		if (player.StereoMode == StereoVideoMode.TOP_BOTTOM)
		{
			// TilingY = 0.5 (half height) and OffsetY = 0 => Bottom Half of the video.
			ApplyTextureTransformation(player, 1f, 0.5f, 0f, 0f);
		}
		else if (player.StereoMode == StereoVideoMode.LEFT_RIGHT)
		{
			// TilingX = 0.5 (half width) and OffsetX = 0 => Left Half of the video.
			ApplyTextureTransformation(player, 0.5f, 1.0f, 0f, 0f);
		}
	}

	private void DisplayRightEyeVideo(ImmersifyPlayer player)
	{
		if (player.StereoMode == StereoVideoMode.TOP_BOTTOM)
		{
			// TilingY = 0.5 (half height) and OffsetY = 0.5 => Top Half of the video.
			ApplyTextureTransformation(player, 1f, 0.5f, 0f, 0.5f);
		}
		else if (player.StereoMode == StereoVideoMode.LEFT_RIGHT)
		{
			// TilingX = 0.5 (half width) and OffsetX = 0.5 => Right Half of the video.
			ApplyTextureTransformation(player, 0.5f, 1.0f, 0.5f, 0f);
		}
	}
}
