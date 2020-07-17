using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;

public class VideoStartPauseAsset : AssetIdType
{
	public enum VideoActionEnum
	{
		START,
		START_OVER_TIME,
		PAUSE,
		PAUSE_OVER_TIME,
		STOP
	}

	public VideoStartPauseAsset()
	{
		EventIdentifier = EventIdentifierVideo.VIDEO_START_PAUSE;
	}
	
	[SerializeField]
	private int _videoActionInt;

	public VideoActionEnum VideoAction
	{
		get { return (VideoActionEnum)_videoActionInt; }
		set { _videoActionInt = (int)value; }
	}
}
