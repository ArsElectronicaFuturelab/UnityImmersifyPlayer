using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ImmersifyFramerate : MonoBehaviour
{
#pragma warning disable 0649 // Ignore Unassigned Variable Warning
	[SerializeField]
	private ImmersifyPlugin _immersifyPlugin;
#pragma warning restore 0649

	[SerializeField]
	private TextMeshProUGUI _uiText;
	[SerializeField]
	private float _fpsUpdateInterval = 0.5f;
	
	private float _lastPlayedVideoFrames = 0;
	private float _timeleft; // Left time for current interval

	private void Awake()
	{
		if (_uiText == null)
		{
			_uiText = GetComponent<TextMeshProUGUI>();
		}
	}

	private void Start()
	{
		_timeleft = _fpsUpdateInterval;

		_lastPlayedVideoFrames = _immersifyPlugin.GetCurrentPlayTime() / _immersifyPlugin.Framerate; ;
	}

	private void Update()
	{
		_timeleft -= Time.deltaTime;

		if (_timeleft <= 0f)
		{
			float curPlayedVideoFrames = _immersifyPlugin.GetCurrentPlayTime() / 1000f * _immersifyPlugin.Framerate;
			float diffPlayedVideoFrames = curPlayedVideoFrames - _lastPlayedVideoFrames;
			float videoFPS = diffPlayedVideoFrames / _fpsUpdateInterval;

			//Debug.Log("Played time: " + _immersifyPlugin.GetCurrentPlayTime());
			//Debug.Log("Played frames: " + curPlayedVideoFrames);

			// Display two fractional digits (f2 format)
			_uiText.text = string.Format("{0:F2} FPS (Video)", videoFPS);
			
			if (videoFPS < _immersifyPlugin.Framerate * 0.25f)
			{
				_uiText.color = Color.red;
			}
			else if (videoFPS < _immersifyPlugin.Framerate * 0.5f)
			{
				_uiText.color = Color.yellow;
			}
			else
			{
				_uiText.color = Color.green;
			}

			_lastPlayedVideoFrames = curPlayedVideoFrames;
			_timeleft = _fpsUpdateInterval;
		}
	}
}
