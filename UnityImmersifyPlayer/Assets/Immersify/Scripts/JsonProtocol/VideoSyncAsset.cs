using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;

public class VideoSyncAsset : AssetIdType
{
	public VideoSyncAsset()
	{
		EventIdentifier = EventIdentifierVideo.VIDEO_SYNC;
	}
	
	[SerializeField]
	private float _targetSyncTime;

	public float TargetSyncTime
	{
		get { return _targetSyncTime; }
		set { _targetSyncTime = value; }
	}
}
