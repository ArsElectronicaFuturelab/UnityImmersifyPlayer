using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sender = DeepSpace.Udp.UdpReceiver.ReceivedMessage.Sender;

public class FloorNetworkVideoHandler : MonoBehaviour
{
	private Dictionary<string, UdpVideoSync> _udpVideoSyncDict = new Dictionary<string, UdpVideoSync>();

	protected virtual void OnEnable()
	{
		JsonConverterVideo.ReceivedVideoLoadMessage += HandleVideoLoadAsset;
		JsonConverterVideo.ReceivedVideoStartPauseMessage += HandleVideoStartPauseAsset;
		JsonConverterVideo.ReceivedVideoSyncMessage += HandleVideoSyncAsset;
	}

	protected virtual void OnDisable()
	{
		JsonConverterVideo.ReceivedVideoLoadMessage -= HandleVideoLoadAsset;
		JsonConverterVideo.ReceivedVideoStartPauseMessage -= HandleVideoStartPauseAsset;
		JsonConverterVideo.ReceivedVideoSyncMessage -= HandleVideoSyncAsset;
	}

	public void RegisterVideoPlayerForSync(UdpVideoSync udpVideoSync)
	{
		_udpVideoSyncDict.Add(udpVideoSync.networkId, udpVideoSync);
	}

	public void HandleVideoSyncAsset(VideoSyncAsset videoSyncAsset, Sender sender)
	{
		UdpVideoSync udpVideoSync;
		if (_udpVideoSyncDict.TryGetValue(videoSyncAsset.Asset_Id, out udpVideoSync))
		{
			udpVideoSync.SyncVideo(videoSyncAsset);
		}
	}

	public void HandleVideoStartPauseAsset(VideoStartPauseAsset videoStartPauseAsset, Sender sender)
	{
		UdpVideoSync udpVideoSync;
		if (_udpVideoSyncDict.TryGetValue(videoStartPauseAsset.Asset_Id, out udpVideoSync))
		{
			udpVideoSync.StartPauseVideo(videoStartPauseAsset);
		}
	}

	public void HandleVideoLoadAsset(VideoLoadAsset videoLoadAsset, Sender sender)
	{
		UdpVideoSync udpVideoSync;
		if (_udpVideoSyncDict.TryGetValue(videoLoadAsset.Asset_Id, out udpVideoSync))
		{
			udpVideoSync.LoadVideo(videoLoadAsset);
		}
	}
}
