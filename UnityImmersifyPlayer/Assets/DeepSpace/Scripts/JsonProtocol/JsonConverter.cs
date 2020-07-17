using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.Udp;

namespace DeepSpace.JsonProtocol
{
	public class JsonConverter : MonoBehaviour
	{
		public delegate void ReceivedTransformAssetDelegated(TransformAsset transformAsset, UdpReceiver.ReceivedMessage.Sender sender);
		public delegate void ReceivedSpawnAssetDelegated(SpawnAsset spawnAsset, UdpReceiver.ReceivedMessage.Sender sender);
		public delegate void ReceivedFieldOfViewMessageDelegated(FieldOfViewAsset fovAsset, UdpReceiver.ReceivedMessage.Sender sender);
		public delegate void ReceivedSettingMessageDelegated(SettingMessage settingMessage, UdpReceiver.ReceivedMessage.Sender sender);

		public static event ReceivedTransformAssetDelegated ReceivedTransformAsset;
		public static event ReceivedSpawnAssetDelegated ReceivedSpawnAsset;
		public static event ReceivedFieldOfViewMessageDelegated ReceivedFieldOfViewMessage;
		public static event ReceivedSettingMessageDelegated ReceivedSettingMessage;


		[SerializeField]
		protected UdpManager _udpManager;

		private void Update()
		{
			List<UdpReceiver.ReceivedMessage> messages = _udpManager.Receiver.GetReceivedMessages();
			for (int ii = 0; ii < messages.Count; ++ii)
			{
				// Removing Message from queue before parsing it. This prevents an endless loop, if a non catched exception occurs.
				UdpReceiver.ReceivedMessage curMessage = messages[ii];
				messages.RemoveAt(ii);

				// Now parse the message:
				ReceivedPlainMessage(curMessage.jsonMsg, curMessage.sender);
				
				ii--;
			}

			messages.Clear();
		}

		protected virtual void ReceivedPlainMessage(string jsonString, UdpReceiver.ReceivedMessage.Sender sender)
		{
			try
			{
				EventType eventType = JsonUtility.FromJson<EventType>(jsonString);
				if (eventType.EventIdentifier != EventIdentifierBase.UNKNOWN)
				{
					ReceivedEventMessage(jsonString, eventType.EventIdentifier, sender);
				}
				else
				{
					Debug.LogWarning("Received JSON Message of unknown event type.");
				}
			}
			catch (System.ArgumentException exception)
			{
				Debug.LogException(exception);
				Debug.LogError("The json string, that led to an exception:\n" + jsonString);
			}
		}

		protected virtual void ReceivedEventMessage(string jsonString, uint eventIdentifier, UdpReceiver.ReceivedMessage.Sender sender)
		{
			//Debug.Log("Received JSON: " + jsonString);

			switch (eventIdentifier)
			{
				case EventIdentifierBase.ASSET_TRANSFORM:
					TransformAsset transformAsset = JsonUtility.FromJson<TransformAsset>(jsonString);
					CallReceivedTransformAsset(transformAsset, sender);
					break;
				case EventIdentifierBase.ASSET_SPAWN:
					SpawnAsset spawnAsset = JsonUtility.FromJson<SpawnAsset>(jsonString);
					CallReceivedSpawnAsset(spawnAsset, sender);
					break;
				case EventIdentifierBase.ASSET_FOV:
					FieldOfViewAsset fovAsset = JsonUtility.FromJson<FieldOfViewAsset>(jsonString);
					CallReceivedFieldOfViewMessage(fovAsset, sender);
					break;
				case EventIdentifierBase.SETTING_MESSAGE:
					SettingMessage settingMessage = JsonUtility.FromJson<SettingMessage>(jsonString);
					if(settingMessage.valueType == SettingMessage.ValueType.FLOAT)
					{
						SettingMessageFloat settingMessageFloat = JsonUtility.FromJson<SettingMessageFloat>(jsonString);
						CallReceivedSettingMessage(settingMessageFloat, sender);
					}
					else
					{
						Debug.LogWarning("Setting Message was received, but value type has not yet been implemented. This message is dismissed.");
					}
					break;
				default:
					Debug.LogWarning("Received Event of Type " + EventIdentifierBase.GetIdentifierName(eventIdentifier) + "(" + eventIdentifier + ") but it was not handled.");
					break;
			}
		}

		private void CallReceivedTransformAsset(TransformAsset transformAsset, UdpReceiver.ReceivedMessage.Sender sender)
		{
			if (ReceivedTransformAsset != null)
			{
				foreach (ReceivedTransformAssetDelegated Callack in ReceivedTransformAsset.GetInvocationList())
				{
					try
					{
						Callack(transformAsset, sender);
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		private void CallReceivedSpawnAsset(SpawnAsset spawnAsset, UdpReceiver.ReceivedMessage.Sender sender)
		{
			if (ReceivedSpawnAsset != null)
			{
				foreach (ReceivedSpawnAssetDelegated Callack in ReceivedSpawnAsset.GetInvocationList())
				{
					try
					{
						Callack(spawnAsset, sender);
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		private void CallReceivedFieldOfViewMessage(FieldOfViewAsset fovAsset, UdpReceiver.ReceivedMessage.Sender sender)
		{
			if (ReceivedFieldOfViewMessage != null)
			{
				foreach (ReceivedFieldOfViewMessageDelegated Callack in ReceivedFieldOfViewMessage.GetInvocationList())
				{
					try
					{
						Callack(fovAsset, sender);
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		private void CallReceivedSettingMessage(SettingMessage settingMessage, UdpReceiver.ReceivedMessage.Sender sender)
		{
			if (ReceivedSettingMessage != null)
			{
				foreach (ReceivedSettingMessageDelegated Callack in ReceivedSettingMessage.GetInvocationList())
				{
					try
					{
						Callack(settingMessage, sender);
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}
	}
}