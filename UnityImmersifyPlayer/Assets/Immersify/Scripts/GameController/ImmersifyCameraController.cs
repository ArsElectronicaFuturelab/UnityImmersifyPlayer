using UnityEngine;
using System.Collections;
using DeepSpace;

public class ImmersifyCameraController : MonoBehaviour
{
	public float rotationSpeed = 2.0f;

	public AnimationCurve _rotationCorrectionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private ImmersifyPlugin _immersifyPlugin = null;
	[SerializeField]
	private ImmersifyFOV _fovController = null;

	private ControllerInputManager _controllerInput;
	private Transform _transform;

	private void Start()
	{
		_transform = transform;
		_controllerInput = ControllerInputManager.Instance;
		_controllerInput.RegisterButtonDownCallback(OnButtonDown);

		Cursor.visible = false;
	}

	private void Update()
	{
		Vector2 leftJoystick = _controllerInput.LeftJoystick;
		Vector2 rightJoystick = _controllerInput.RightJoystick;
		Vector2 dPad = _controllerInput.DPad;

		// Look around (Left, Right, Up, Down) with the right Joystick
		if (rightJoystick.x.CompareTo(0f) != 0f)
		{
			_transform.Rotate(Vector3.up, rightJoystick.x * rotationSpeed, Space.World);
		}
		if (rightJoystick.y.CompareTo(0f) != 0f)
		{
			_transform.Rotate(Vector3.right, -rightJoystick.y * rotationSpeed, Space.Self);
		}

		if (dPad.y.CompareTo(0f) != 0f)
		{
			if (_fovController != null)
			{
				_fovController.IncreaseFOV(-dPad.y);
			}
		}
		if (dPad.x.CompareTo(0f) != 0f)
		{
			if (_fovController != null)
			{
				if (dPad.x > 0.9f)
				{
					_fovController.ResetFOV();
				}
			}
		}
	}

	private void OnButtonDown(ControllerInputManager.Button button)
	{
		// Rotate to normal horizontal with R3:
		if (button == ControllerInputManager.Button.R3)
		{
			StartCoroutine(RotateToHorizon(0.5f));
		}

		// Video Controls:
		if (button == ControllerInputManager.Button.START)
		{
			if(_immersifyPlugin.IsPaused() == true)
			{
				// Resume video and audio:
				_immersifyPlugin.Resume();
			}
			else
			{
				// Pause video and audio:
				_immersifyPlugin.Pause();
			}
		}
		else if (button == ControllerInputManager.Button.A)
		{
			_immersifyPlugin.ToggleFadedPlayPause(false, 2);
		}
		else if (button == ControllerInputManager.Button.B)
		{
			_immersifyPlugin.ToggleFadedPlayPause(true, 2);
		}
	}

	private IEnumerator RotateToHorizon(float time)
	{
		float curTime = 0.0f;

		Vector3 forward = _transform.forward;
		forward.y = 0.0f;
		forward.Normalize();

		Quaternion startRotation = _transform.localRotation;
		Quaternion aimRotation = Quaternion.LookRotation(forward, Vector3.up);

		while (curTime <= time)
		{
			_transform.localRotation = Quaternion.Slerp(startRotation, aimRotation, _rotationCorrectionCurve.Evaluate(curTime / time));

			curTime += Time.smoothDeltaTime;
			yield return null;
		}

		// fix rotation finally...
		_transform.localRotation = aimRotation;
	}
}