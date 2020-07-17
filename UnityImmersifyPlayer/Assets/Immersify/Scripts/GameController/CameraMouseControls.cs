﻿using UnityEngine;
using System.Collections.Generic;

// Camera Movement ("CamerControls.cs") copied from: https://github.com/adrenak/UniVRMedia

public class CameraMouseControls : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY = 0,
		MouseX = 1,
		MouseY = 2
	}

	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 5f;
	public float sensitivityY = 5f;

	public float minimumX = -360f;
	public float maximumX = 360f;

	public float minimumY = -60f;
	public float maximumY = 60f;

	float rotationX = 0f;
	float rotationY = 0f;

	private List<float> rotArrayX = new List<float>();
	float rotAverageX = 0f;

	private List<float> rotArrayY = new List<float>();
	float rotAverageY = 0f;

	public float frameCounter = 2f;

	Quaternion originalRotation;

	void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		if (axes == RotationAxes.MouseXAndY)
		{
			rotAverageY = 0f;
			rotAverageX = 0f;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;

			rotArrayY.Add(rotationY);
			rotArrayX.Add(rotationX);

			if (rotArrayY.Count >= frameCounter)
			{
				rotArrayY.RemoveAt(0);
			}
			if (rotArrayX.Count >= frameCounter)
			{
				rotArrayX.RemoveAt(0);
			}

			for (int j = 0; j < rotArrayY.Count; j++)
			{
				rotAverageY += rotArrayY[j];
			}
			for (int i = 0; i < rotArrayX.Count; i++)
			{
				rotAverageX += rotArrayX[i];
			}

			rotAverageY /= rotArrayY.Count;
			rotAverageX /= rotArrayX.Count;

			rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
			rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

			Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
			Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

			transform.localRotation = originalRotation * xQuaternion * yQuaternion;
		}
		else if (axes == RotationAxes.MouseX)
		{
			rotAverageX = 0f;

			rotationX += Input.GetAxis("Mouse X") * sensitivityX;

			rotArrayX.Add(rotationX);

			if (rotArrayX.Count >= frameCounter)
			{
				rotArrayX.RemoveAt(0);
			}
			for (int i = 0; i < rotArrayX.Count; i++)
			{
				rotAverageX += rotArrayX[i];
			}
			rotAverageX /= rotArrayX.Count;

			rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

			Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
			transform.localRotation = originalRotation * xQuaternion;
		}
		else
		{
			rotAverageY = 0f;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

			rotArrayY.Add(rotationY);

			if (rotArrayY.Count >= frameCounter)
			{
				rotArrayY.RemoveAt(0);
			}
			for (int j = 0; j < rotArrayY.Count; j++)
			{
				rotAverageY += rotArrayY[j];
			}
			rotAverageY /= rotArrayY.Count;

			rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

			Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
			transform.localRotation = originalRotation * yQuaternion;
		}
#endif
	}

	void Start()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb)
			rb.freezeRotation = true;
		originalRotation = transform.localRotation;
#endif
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;
		if ((angle >= -360f) && (angle <= 360f))
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}
}