using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS SCRIPT IS NOT IN USE - JUST TEST!
[RequireComponent(typeof(ImmersifyPlugin))]
public class EquirectangularVideo : MonoBehaviour
{
	private ImmersifyPlugin _immersifyPlugin;

#pragma warning disable 0649 // Ignore Unassigned Variable Warning
	[SerializeField]
	private Material _blitMat;
#pragma warning restore 0649

	//private RenderTexture _equirectRenderTex;
	[SerializeField]
	private RenderTexture _cubeMapLeftEye; // Contains the current video CubeMap for the left (or mono) eye.
	//[SerializeField]
	//private RenderTexture _cubeMapRightEye; // Contains the current video CubeMap for the right eye.
	
	private void Awake()
	{
		_immersifyPlugin = GetComponent<ImmersifyPlugin>();
	}

	void Start()
	{
		//_immersifyPlugin
		//_equirectRenderTex = new RenderTexture(4096, 2048, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		// TODO: Apply image to render texture and apply this render texture to the cubemap -> skybox.


		// TODO: THIS WILL NOT WORK, TEXTURE WILL ALWAYS BE -1 AT THIS TIME.
		// However, this script is not in use...
		_cubeMapLeftEye = new RenderTexture(_immersifyPlugin.GetVideoWidth(), _immersifyPlugin.GetVideoWidth(), 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

		// TODO
		//_blitMat.SetTexture("_UVTex", _immersifyPlugin.TextureUV);
	}
	
	void LateUpdate()
	{
		//_cubeMapLeftEye.ConvertToEquirect(_equirectRenderTex, Camera.MonoOrStereoscopicEye.Mono);



		Graphics.Blit(_immersifyPlugin.TextureY, _cubeMapLeftEye, _blitMat);

		Camera cam = Camera.main;
		cam.RenderToCubemap(_cubeMapLeftEye, 63, Camera.MonoOrStereoscopicEye.Mono);
	}
}
