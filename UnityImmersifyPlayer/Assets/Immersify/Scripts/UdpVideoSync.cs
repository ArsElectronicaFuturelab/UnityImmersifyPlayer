using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace;
using DeepSpace.Udp;

public class UdpVideoSync : MonoBehaviour
{
	public UdpManager udpManager;
	public FloorNetworkVideoHandler floorNetworkVideoHandler;
	public string networkId; // TODO: Get this from an ID Manager, etc.
	public ImmersifyPlugin _videoPlugin;

	private UdpCmdConfigMgr _configMgr;

	private void Awake()
	{
		_configMgr = UdpCmdConfigMgr.Instance as UdpCmdConfigMgr;
	}

	private void Start()
	{
		floorNetworkVideoHandler.RegisterVideoPlayerForSync(this);
	}

	private void Update()
	{
		if (_configMgr.applicationType == CmdConfigManager.AppType.WALL)
		{
			// Sending the sync data via UDP:
			VideoSyncAsset videoSyncAsset = new VideoSyncAsset();
			videoSyncAsset.Asset_Id = networkId;
			videoSyncAsset.TargetSyncTime = _videoPlugin.GetCurrentPlayTime() + (_configMgr.networkFrameDelay * Time.smoothDeltaTime);
			udpManager.SenderToFloor.AddMessage(JsonUtility.ToJson(videoSyncAsset));
		}
	}

	// This is called from the FloorManager, that receives the sent VideoSyncAsset Messages.
	public void SyncVideo(VideoSyncAsset videoSyncAsset)
	{
		_videoPlugin.SetTargetPlayTime(videoSyncAsset.TargetSyncTime);
	}

	public void DoStartPauseSynced(VideoStartPauseAsset.VideoActionEnum videoAction)
	{
		if (_configMgr.applicationType == CmdConfigManager.AppType.WALL)
		{
			// Send command to floor:
			VideoStartPauseAsset videoStartPauseAsset = new VideoStartPauseAsset();
			videoStartPauseAsset.Asset_Id = networkId;
			videoStartPauseAsset.VideoAction = videoAction;
			udpManager.SenderToFloor.AddMessage(JsonUtility.ToJson(videoStartPauseAsset));

			if (_configMgr.networkFrameDelay > 0)
			{
				StartCoroutine(DoVideoActionAfter(_configMgr.networkFrameDelay, videoStartPauseAsset));
			}
			else
			{
				StartPauseVideo(videoStartPauseAsset);
			}
		}
	}

	private IEnumerator DoVideoActionAfter(int frameAmount, VideoStartPauseAsset videoStartPauseAsset)
	{
		for (int ii = 0; ii < frameAmount; ++ii)
		{
			yield return null; // Wait for one frame.
		}

		StartPauseVideo(videoStartPauseAsset);
	}

	public void StartPauseVideo(VideoStartPauseAsset videoStartPauseAsset)
	{
		switch (videoStartPauseAsset.VideoAction)
		{
			case VideoStartPauseAsset.VideoActionEnum.START:
				_videoPlugin.Resume();
				break;
			case VideoStartPauseAsset.VideoActionEnum.PAUSE:
				_videoPlugin.Pause();
				break;
			case VideoStartPauseAsset.VideoActionEnum.START_OVER_TIME:
				_videoPlugin.ToggleFadedPlayPause(true);
				break;
			case VideoStartPauseAsset.VideoActionEnum.PAUSE_OVER_TIME:
				_videoPlugin.ToggleFadedPlayPause(false);
				break;
			case VideoStartPauseAsset.VideoActionEnum.STOP:
				Debug.LogError("STOP is not implemented!");
				break;
			default:
				Debug.LogWarning("StartPauseVideo: Video Action \"" + videoStartPauseAsset.ToString() + "\" is not implemented.");
				break;
		}
	}

	public void LoadVideo(VideoLoadAsset videoLoadAsset)
	{
		// Wall only:
		if (_configMgr.applicationType == CmdConfigManager.AppType.WALL)
		{
			// Sending the sync data via UDP:
			videoLoadAsset.Asset_Id = networkId;
			udpManager.SenderToFloor.AddMessage(JsonUtility.ToJson(videoLoadAsset));
		}

		// Wall and Floor:
		_videoPlugin.InitializePlayer(videoLoadAsset.Path,
									  videoLoadAsset.PathType,
									  videoLoadAsset.Framerate,
									  audioClip: null,
									  videoLoadAsset.MaxQueue,
									  videoLoadAsset.StereoMode,
									  videoLoadAsset.InvertLeftRight,
									  videoLoadAsset.VideoIsUpsideDown,
									  pauseAtStart: true,
									  loopVideo: false);
	}
}
