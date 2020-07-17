using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StereoVideoMode = ImmersifyPlugin.StereoVideoMode;

// This Script needs to be attached to a camera, because it needs to receive the OnPreRender callbacks.
[RequireComponent(typeof(Camera))]
public class EquirectangularUvController : MonoBehaviour
{
	private static readonly int _textureTransID = Shader.PropertyToID("_TextureTrans");
	
	private Camera _camera;

	[SerializeField, Space]
	private bool _invertLeftRight = false; // If e.g. the left side of the video contains the right video part or the top half of the video contains the right video part, set this true to apply the video correctly.
	[SerializeField]
	private bool _videoIsUpsideDown = false; // If the video is upside down, set this true to invert the scaling (and therfor correct the video).
	[SerializeField]
	private StereoVideoMode _stereoVideoMode = StereoVideoMode.MONO;
	[SerializeField]
	private MeshRenderer _videoMeshRenderer = null;

	private Material _equirectangularMaterial;

	private ImmersifyCmdConfigMgr _config;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		_equirectangularMaterial = _videoMeshRenderer.material;
	}

	private void Start()
	{
		_config = ImmersifyCmdConfigMgr.Instance as ImmersifyCmdConfigMgr;
		_stereoVideoMode = _config.stereoMode;

		if (_stereoVideoMode == StereoVideoMode.MONO)
		{
			DisplayMonoVideo();
		}
		else
		{
			// Just setup the first frame: 
			// If the video is stereo, but the viewer is mono, just the left video is displayed.
			DisplayLeftEyeVideo();
		}
	}

	private void Update()
	{
		// Just for demo & test reasons:
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if (_invertLeftRight == false)
			{
				DisplayLeftEyeVideo();
			}
			else // _invertLeftRight == true
			{
				DisplayRightEyeVideo();
			}
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			if (_invertLeftRight == false)
			{
				DisplayRightEyeVideo();
			}
			else // _invertLeftRight == true
			{
				DisplayLeftEyeVideo();
			}
		}
	}

	private void OnPreRender()
	{
		// If Video is stereo:
		// > If Editor: Display only left eye (half uv)
		// > If Build: 
		// > > If vrmode stereo: Display stereo
		// > > If vrmode none: Display only left eye (half uv) // vrmode none equals editor!
		// If Video is mono: Display fullscreen uv[0, 1]

		if (_stereoVideoMode == StereoVideoMode.MONO)
		{
			DisplayMonoVideo();
			//Debug.Log("DisplayMonoVideo");
		}
		else // Video is stereo (top-bottom or side-by-side)
		{
			if (string.IsNullOrEmpty(UnityEngine.XR.XRSettings.loadedDeviceName)) // If vrmode is none. (This is the case in editor or if explitly wanted in build via commandline arguments)
			{
				DisplayLeftEyeVideo();
				//Debug.Log("DisplayLeftEyeVideo (-vrmode none)");
			}
			else // vrmode is stereo (in this case we expect non-hmd stereo)
			{
				if (_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
				{
					if (_invertLeftRight == false)
					{
						DisplayLeftEyeVideo();
					}
					else // _invertLeftRight == true
					{
						DisplayRightEyeVideo();
					}
				}
				else if (_camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
				{
					if (_invertLeftRight == false)
					{
						DisplayRightEyeVideo();
					}
					else // _invertLeftRight == true
					{
						DisplayLeftEyeVideo();
					}
				}
			}
		}

		//Debug.Log("Cam Eye: " + _camera.stereoActiveEye);
		//Debug.Log("Loaded device: " + UnityEngine.XR.XRSettings.loadedDeviceName);
	}

	private void ApplyTextureTransformation(float tilingX, float tilingY, float offsetX, float offsetY)
	{
		Vector4 textureTrans = new Vector4(tilingX, tilingY, offsetX, offsetY);

		if (_videoIsUpsideDown == true)
		{
			textureTrans.y *= -1f;
		}

		_equirectangularMaterial.SetVector(_textureTransID, textureTrans);
	}

	private void DisplayMonoVideo()
	{
		ApplyTextureTransformation(1f, 1f, 0f, 0f);
	}

	private void DisplayLeftEyeVideo()
	{
		if(_stereoVideoMode == StereoVideoMode.TOP_BOTTOM)
		{
			// TilingY = 0.5 (half height) and OffsetY = 0 => Bottom Half of the video.
			ApplyTextureTransformation(1f, 0.5f, 0f, 0f);
		}
		else if(_stereoVideoMode == StereoVideoMode.LEFT_RIGHT)
		{
			// TilingX = 0.5 (half width) and OffsetX = 0 => Left Half of the video.
			ApplyTextureTransformation(0.5f, 1.0f, 0f, 0f);
		}
	}

	private void DisplayRightEyeVideo()
	{
		if (_stereoVideoMode == StereoVideoMode.TOP_BOTTOM)
		{
			// TilingY = 0.5 (half height) and OffsetY = 0.5 => Top Half of the video.
			ApplyTextureTransformation(1f, 0.5f, 0f, 0.5f);
		}
		else if (_stereoVideoMode == StereoVideoMode.LEFT_RIGHT)
		{
			// TilingX = 0.5 (half width) and OffsetX = 0.5 => Right Half of the video.
			ApplyTextureTransformation(0.5f, 1.0f, 0.5f, 0f);
		}
	}
}
