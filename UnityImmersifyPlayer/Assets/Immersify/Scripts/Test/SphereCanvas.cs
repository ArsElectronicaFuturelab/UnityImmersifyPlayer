using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCanvas : MonoBehaviour
{
	[SerializeField]
	private Vector3 _sphereScale = new Vector3(-100f, 100f, 100f);
	[SerializeField]
	private bool _invertNormals = true;

	private void Awake()
	{
		MeshFilter sphereMesh = GetComponent<MeshFilter>();
		if (sphereMesh != null)
		{
			transform.localScale = _sphereScale;

			if (_invertNormals == true)
			{
				sphereMesh.mesh.InvertNormals();
			}
		}
		else
		{
			Debug.LogError("SphereCanvas could not do its job, because there was no mesh to modify.");
		}
	}
}
