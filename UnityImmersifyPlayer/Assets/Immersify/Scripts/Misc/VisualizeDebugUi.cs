using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizeDebugUi : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _uiElements = new List<GameObject>();
	[SerializeField]
	private KeyCode _triggerKey = KeyCode.D;
	[SerializeField]
	private bool _isActive = false;

	private void Start()
	{
		foreach (GameObject element in _uiElements)
		{
			element.SetActive(_isActive);
		}
	}

	private void Update()
	{
		if(Input.GetKeyDown(_triggerKey))
		{
			_isActive = !_isActive;

			foreach (GameObject element in _uiElements)
			{
				element.SetActive(_isActive);
			}
		}
	}
}
