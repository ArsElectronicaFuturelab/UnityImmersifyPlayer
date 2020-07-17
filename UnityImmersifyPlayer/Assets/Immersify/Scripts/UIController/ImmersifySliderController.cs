using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImmersifySliderController : MonoBehaviour
{
	[SerializeField]
	private ImmersifyPlugin _immersifyPlugin = null;

	[SerializeField]
	private Slider _slider = null;

	private bool _sliderIsMovedManually = false;

	private void Awake()
	{
		if(_slider == null)
		{
			_slider = GetComponent<Slider>();
		}
	}

	private void Update()
	{
		// Only do this, if slider is currently not moved via hand:
		if (_sliderIsMovedManually == false)
		{
			long videoDuration = _immersifyPlugin.GetVideoDurationInMS();
			float currentPlayingTime = _immersifyPlugin.GetCurrentPlayTime();
			_slider.value = currentPlayingTime / (float)(videoDuration);

			Debug.Log("(currentPlayingTime) " + currentPlayingTime + " / " + videoDuration + " (videoDuration) || Video is finished: " + _immersifyPlugin.IsFinished());
		}
	}

	public void OnBeginSliderDrag()
	{
		_sliderIsMovedManually = true;

		Debug.Log("OnBeginSliderDrag");
	}

	public void OnEndSliderDrag()
	{
		_sliderIsMovedManually = false;

		Debug.Log("OnEndSliderDrag: " + _slider.value);

		if (_immersifyPlugin.GetSeekingIsSupported() == true)
		{
			long videoDuration = _immersifyPlugin.GetVideoDurationInMS();
			int videoTargetMS = (int)(videoDuration * _slider.value);
			_immersifyPlugin.SeekToMS(videoTargetMS);

			if(_immersifyPlugin.IsFinished())
			{
				_immersifyPlugin.Play();
			}
		}
	}
}
