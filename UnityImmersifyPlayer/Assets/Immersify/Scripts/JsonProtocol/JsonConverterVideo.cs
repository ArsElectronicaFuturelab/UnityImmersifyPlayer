using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;
using DeepSpace.Udp;
using System;

public class JsonConverterVideo : JsonConverter
{
	public delegate void ReceivedVideoSyncMessageDelegated(VideoSyncAsset videoSyncAsset, UdpReceiver.ReceivedMessage.Sender sender);
	public delegate void ReceivedVideoStartPauseMessageDelegated(VideoStartPauseAsset videoStartPauseAsset, UdpReceiver.ReceivedMessage.Sender sender);
	public delegate void ReceivedVideoLoadMessageDelegated(VideoLoadAsset videoLoadAsset, UdpReceiver.ReceivedMessage.Sender sender);

	public static event ReceivedVideoSyncMessageDelegated ReceivedVideoSyncMessage;
	public static event ReceivedVideoStartPauseMessageDelegated ReceivedVideoStartPauseMessage;
	public static event ReceivedVideoLoadMessageDelegated ReceivedVideoLoadMessage;


	protected override void ReceivedPlainMessage(string jsonString, UdpReceiver.ReceivedMessage.Sender sender)
	{
		// You can implement the parsing of your json based received data here.
		// Have a look into the base implementation in JsonConverter.cs to see a possible way how to do this.

		// If you cannot use the received data, call the base method to pass on the DevKit internal json messages.
		base.ReceivedPlainMessage(jsonString, sender);
	}

	protected override void ReceivedEventMessage(string jsonString, uint eventIdentifier, UdpReceiver.ReceivedMessage.Sender sender)
	{
		switch(eventIdentifier)
		{
			case EventIdentifierVideo.VIDEO_SYNC:
				VideoSyncAsset videoSyncAsset = JsonUtility.FromJson<VideoSyncAsset>(jsonString);
				CallReceivedVideoSyncMessage(videoSyncAsset, sender);
				break;
			case EventIdentifierVideo.VIDEO_START_PAUSE:
				VideoStartPauseAsset videoStartPauseAsset = JsonUtility.FromJson<VideoStartPauseAsset>(jsonString);
				CallReceivedVideoStartPauseMessage(videoStartPauseAsset, sender);
				break;
			case EventIdentifierVideo.VIDEO_LOAD:
				VideoLoadAsset videoLoadAsset = JsonUtility.FromJson<VideoLoadAsset>(jsonString);
				CallReceivedVideoLoadMessage(videoLoadAsset, sender);
				break;
			default:
				base.ReceivedEventMessage(jsonString, eventIdentifier, sender);
				break;
		}
	}

	private void CallReceivedVideoStartPauseMessage(VideoStartPauseAsset videoStartPauseAsset, UdpReceiver.ReceivedMessage.Sender sender)
	{
		if (ReceivedVideoStartPauseMessage != null)
		{
			foreach (ReceivedVideoStartPauseMessageDelegated Callack in ReceivedVideoStartPauseMessage.GetInvocationList())
			{
				try
				{
					Callack(videoStartPauseAsset, sender);
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}

	private void CallReceivedVideoSyncMessage(VideoSyncAsset videoSyncAsset, UdpReceiver.ReceivedMessage.Sender sender)
	{
		if (ReceivedVideoSyncMessage != null)
		{
			foreach (ReceivedVideoSyncMessageDelegated Callack in ReceivedVideoSyncMessage.GetInvocationList())
			{
				try
				{
					Callack(videoSyncAsset, sender);
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}

	private void CallReceivedVideoLoadMessage(VideoLoadAsset videoLoadAsset, UdpReceiver.ReceivedMessage.Sender sender)
	{
		if (ReceivedVideoLoadMessage != null)
		{
			foreach (ReceivedVideoLoadMessageDelegated Callack in ReceivedVideoLoadMessage.GetInvocationList())
			{
				try
				{
					Callack(videoLoadAsset, sender);
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}	
}
