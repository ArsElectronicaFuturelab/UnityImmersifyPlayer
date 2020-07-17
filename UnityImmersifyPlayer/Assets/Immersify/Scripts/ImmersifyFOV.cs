using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmersifyFOV : MonoBehaviour
{
#pragma warning disable 0649 // Ignore Unassigned Variable Warning
	[SerializeField]
	private Camera[] _cameras;
#pragma warning restore 0649

	[SerializeField]
	private float _minFOV = 20f;
	[SerializeField]
	private float _maxFOV = 70f;
	[SerializeField]
	private float _defaultFOV = 60f;

	private bool _resetInProgress = false;

	public void IncreaseFOV(float changeValue)
	{
		if (_cameras.Length == 0)
		{
			Debug.LogWarning("You wanted to change FOV but no camera is referenced. Aborting operation.");
			return;
		}

		float fov = Mathf.Clamp(_cameras[0].fieldOfView + changeValue, _minFOV, _maxFOV);

		foreach (Camera cam in _cameras)
		{
			cam.fieldOfView = fov;
		}
	}

	public void ResetFOV(float time = 0.5f)
	{
		if (_cameras.Length == 0)
		{
			Debug.LogWarning("You wanted to change FOV but no camera is referenced. Aborting operation.");
			return;
		}

		// If reset is not in progress already and the camera does not have the default FOV already:
		if (_resetInProgress == false && _cameras[0].fieldOfView.CompareTo(_defaultFOV) != 0f)
		{
			StartCoroutine(ResetFovOverTime(time));
		}
	}

	private IEnumerator ResetFovOverTime(float duration)
	{
		_resetInProgress = true;

		float startValue = _cameras[0].fieldOfView;
		float curTime = 0f;

		while (curTime < duration)
		{
			curTime += Time.smoothDeltaTime;
			float percent = Mathf.Clamp01(curTime / duration);

			foreach (Camera cam in _cameras)
			{
				cam.fieldOfView = Mathf.Lerp(startValue, _defaultFOV, percent);
			}

			yield return null;
		}

		foreach (Camera cam in _cameras)
		{
			cam.fieldOfView = _defaultFOV;
		}

		_resetInProgress = false;
	}
}
