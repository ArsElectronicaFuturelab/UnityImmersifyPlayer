using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquirectangularImageTest : MonoBehaviour
{
	[SerializeField]
	private Texture2D _demoImage;

	private RenderTexture _equirectRenderTex;
	[SerializeField]
	private RenderTexture _cubeMapLeftEye; // Contains the current video CubeMap for the left (or mono) eye.
	[SerializeField]
	private RenderTexture _cubeMapRightEye; // Contains the current video CubeMap for the right eye.


	private void Start()
	{
		//UnityEngine.Rendering.TextureDimension.Cube;

		Cubemap cubeMap = new Cubemap(4096,	TextureFormat.RGB24, false);
		Debug.Log(cubeMap.dimension);

		//_demoImage
		//_cubeMapLeftEye.ConvertToEquirect(_equirectRenderTex, Camera.MonoOrStereoscopicEye.Mono);
	}
}
