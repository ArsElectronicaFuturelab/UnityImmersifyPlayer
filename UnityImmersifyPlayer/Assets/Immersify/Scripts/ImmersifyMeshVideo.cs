using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmersifyMeshVideo : MonoBehaviour
{
	[SerializeField]
	private ImmersifyPlugin _immersifyPlugin;
	[SerializeField]
	private MeshRenderer _meshRenderer = null;

	private Material _videoMaterial;
	private readonly int _visibilityID = Shader.PropertyToID("_Visibility");
	bool _isVisible = false;

	private void Awake()
	{
		TryToFindComponentsAutomatically();

		if (_meshRenderer != null)
		{
			_videoMaterial = _meshRenderer.material;
		}

		FadeToBlack(time: 0f);
	}

	private void OnValidate()
	{
		TryToFindComponentsAutomatically();
	}

	private void Start()
	{
		TexturesGotChanged(_immersifyPlugin);
	}

	private void OnEnable()
	{
		ImmersifyPlugin.OnTexturesChanged += TexturesGotChanged;
	}

	private void OnDisable()
	{
		ImmersifyPlugin.OnTexturesChanged -= TexturesGotChanged;
	}

	private void Update()
	{
		if (_isVisible == false)
		{
			if (_immersifyPlugin.IsReady() == true)
			{
				FadeToTexture();
			}
		}

		// Demo Controls:
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_immersifyPlugin.SetPause(!_immersifyPlugin.IsPaused());
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			_immersifyPlugin.ToggleFadedPlayPause(false, 2);
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			_immersifyPlugin.ToggleFadedPlayPause(true, 2);
		}
		else if (Input.GetKeyDown(KeyCode.M)) // Saving all 3 textures to disk as png.
		{
			// For tests only:
			byte[] pngDataY = _immersifyPlugin.TextureY.DeCompress().EncodeToPNG();
			byte[] pngDataU = _immersifyPlugin.TextureU.DeCompress().EncodeToPNG();
			byte[] pngDataV = _immersifyPlugin.TextureV.DeCompress().EncodeToPNG();

			string timeString = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			System.IO.File.WriteAllBytes(Application.dataPath + "/../" + timeString + "_pngDataY.png", pngDataY);
			System.IO.File.WriteAllBytes(Application.dataPath + "/../" + timeString + "_pngDataU.png", pngDataU);
			System.IO.File.WriteAllBytes(Application.dataPath + "/../" + timeString + "_pngDataV.png", pngDataV);

			Debug.Log("Saved PNGs.");
		}
	}

	private void OnDestroy()
	{
		if (_meshRenderer)
		{
			_meshRenderer.material.mainTexture = null;
		}
	}

	private void TexturesGotChanged(ImmersifyPlugin plugin)
	{
		if (plugin != null && plugin == _immersifyPlugin)
		{
			if (_meshRenderer)
			{
				_meshRenderer.material.SetTexture("_TexY", _immersifyPlugin.TextureY);
				_meshRenderer.material.SetTexture("_TexU", _immersifyPlugin.TextureU);
				_meshRenderer.material.SetTexture("_TexV", _immersifyPlugin.TextureV);
			}
		}
	}

	private void TryToFindComponentsAutomatically()
	{
		if (_immersifyPlugin == null)
		{
			_immersifyPlugin = GetComponent<ImmersifyPlugin>();
		}

		if (_meshRenderer == null)
		{
			_meshRenderer = GetComponent<MeshRenderer>();
		}
	}

	public MeshRenderer GetMeshRenderer()
	{
		return _meshRenderer;
	}

	public ImmersifyPlugin GetImmersifyPlugin()
	{
		return _immersifyPlugin;
	}
	public void FadeToBlack(float time = 1.0f)
	{
		StartCoroutine(FadeTexture(aimVisibility: 0f, time));
		_isVisible = false;
	}

	public void FadeToTexture(float time = 1.0f)
	{
		StartCoroutine(FadeTexture(aimVisibility: 1f, time));
		_isVisible = true;
	}

	private IEnumerator FadeTexture(float aimVisibility, float time)
	{
		if (_videoMaterial.HasProperty(_visibilityID))
		{
			float startVisibilty = _videoMaterial.GetFloat(_visibilityID);
			float curVisibilty;
			float curTime = 0f;

			while (curTime < time)
			{
				curTime += Time.smoothDeltaTime;
				curVisibilty = Mathf.Lerp(startVisibilty, aimVisibility, curTime / time);
				_videoMaterial.SetFloat(_visibilityID, curVisibilty);

				yield return null;
			}

			_videoMaterial.SetFloat(_visibilityID, aimVisibility);
		}
	}
}

// For test only:
public static class DecompressExtensionTexture2D
{
	public static Texture2D DeCompress(this Texture2D source)
	{
		RenderTexture renderTex = RenderTexture.GetTemporary(
					source.width,
					source.height,
					0,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear);

		Graphics.Blit(source, renderTex);
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = renderTex;
		Texture2D readableText = new Texture2D(source.width, source.height);
		readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		readableText.Apply();
		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary(renderTex);
		return readableText;
	}
}
