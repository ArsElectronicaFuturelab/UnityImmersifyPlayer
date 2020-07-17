using UnityEngine;
using System.Collections;
using DeepSpace.JsonProtocol;

namespace DeepSpace
{
	public class WallNetworkHandler : MonoBehaviour
	{
		public void HandleTransformAsset(TransformAsset transformAsset)
		{
			// Implement this if you want to make it possible to sync a transform, that has been changed on the floor, with the wall.
		}
	}
}