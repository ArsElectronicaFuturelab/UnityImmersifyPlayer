using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtension
{
	// Based on ReverseNormals.cs from Joachim Ante here : http://wiki.unity3d.com/index.php/ReverseNormals
	public static void InvertNormals(this Mesh mesh)
	{
		// Calculate normals
		Vector3[] normals = mesh.normals;
		for (int ii = 0; ii < normals.Length; ii++)
		{
			normals[ii] = -normals[ii];
		}
		mesh.normals = normals;

		// Create triangles
		for (int mm = 0; mm < mesh.subMeshCount; mm++)
		{
			int[] triangles = mesh.GetTriangles(mm);
			for (int ii = 0; ii < triangles.Length; ii += 3)
			{
				int temp = triangles[ii + 0];
				triangles[ii + 0] = triangles[ii + 1];
				triangles[ii + 1] = temp;
			}
			mesh.SetTriangles(triangles, mm);
		}
	}
}
