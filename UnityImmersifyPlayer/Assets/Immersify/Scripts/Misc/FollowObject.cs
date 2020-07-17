using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
	private Transform _trans = null;

	[SerializeField]
	private Transform _followTrans = null;

	private void Awake()
	{
		_trans = transform;
	}

	private void LateUpdate()
	{
		if(_followTrans != null)
		{
			_trans.position = _followTrans.position;
		}
	}
}
