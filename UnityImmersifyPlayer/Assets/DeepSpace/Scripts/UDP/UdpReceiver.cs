using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DeepSpace.Udp
{
	public class UdpReceiver : UdpBase
	{
		public class ReceivedMessage
		{
			public enum Sender
			{
				WALL,
				FLOOR,
			}

			public ReceivedMessage(string jsonMsg, Sender sender)
			{
				this.jsonMsg = jsonMsg;
				this.sender = sender;
			}

			public string jsonMsg; // The received message.
			public Sender sender; // Where the received message came from.
		}

		// If this is true, the UDP listener will listen to any IP
		// If this is false, the UDP listener will only listen to the "_remoteIpAddress"
		[SerializeField]
		private bool _listenToAllAdresses = true;

		private List<ReceivedMessage> _receivedMessages = new List<ReceivedMessage>();
		private string _uncompletedMessage = string.Empty;

		private IPAddress _remoteIp = null;

		private UdpManager _udpManager;

		private void Awake()
		{
			_udpManager = transform.parent.GetComponent<UdpManager>();
		}

		// This should be called from outside. Without calling this, the receiver will not receive anything.
		public void ActivateReceiver(int port)
		{
			Port = port;

			EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_socket.Blocking = false;
			//_socket.ExclusiveAddressUse = false;
			//_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_socket.Bind(ipEndPoint);
		}

		private void Update()
		{
			if (_socket != null)
			{
				while (_socket.Available > 0)
				{
					Receive();
				}
			}
		}

		private void Receive()
		{
			try
			{
				byte[] _receiveBuffer = new byte[_socket.Available]; // Read everything, that is available at once!
				EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
				_socket.ReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ref ipEndPoint);

				if (_listenToAllAdresses == false)
				{
					IPAddress connectedAddress = ((IPEndPoint)ipEndPoint).Address;

					if (_remoteIp == null) // Only happens the first time or if _listenToAllAdresses was set to false during runtime.
					{
						_remoteIp = IPAddress.Parse(_remoteIpAddress);
					}

					if (connectedAddress.Equals(_remoteIp) == false)
					{
						Debug.LogError("Got network package from unaccepted endpoint (" + connectedAddress.ToString() + "). Expected endpoint is " + _remoteIpAddress + "."
							+ "\nPackage is not going to be read!");
						return;
					}
				}

				if (_receiveBuffer != null && _receiveBuffer.Length > 0)
				{
					string receivedJson = System.Text.Encoding.UTF8.GetString(_receiveBuffer);
					if (receivedJson.Trim()[0] != '{') // This is not perfect but can prevent making an initial data loss becomming a catastrophy.
					{
						receivedJson = _uncompletedMessage + receivedJson;
					}
					_uncompletedMessage = string.Empty;

					// Check if it is only one or more JSON Messages and check if each JSON Message is valid:
					List<string> jsonEvents = new List<string>();
					int startIndex = 0;
					int bracketCount = 0;
					bool toggleQuote = false; // Used to ignore curly braces inside of quotes, e.g. "{" or "}".
					for (int ii = 0; ii < receivedJson.Length; ++ii)
					{
						if (receivedJson[ii] == '"')
						{
							// Check the amount of backslashes, because the quote might be escaped, e.g. "\""
							int backSlashCount = 0;
							while(ii - (backSlashCount + 1) >= 0)
							{
								if(receivedJson[ii - (backSlashCount + 1)] == '\\')
								{
									backSlashCount++;
								}
								else
								{
									break;
								}
							}

							if(backSlashCount % 2 == 0) // For even backslashes (0, 2, 4) -> change quote toggle.
							{
								toggleQuote = !toggleQuote;
							}
						}
						if (toggleQuote == false) // Only count curcly braces, if they are not under quotes:
						{
							if (receivedJson[ii] == '{')
							{
								bracketCount++;
							}
							else if (receivedJson[ii] == '}')
							{
								bracketCount--;
							}
						}

						if (bracketCount == 0)
						{
							string jsonStr = receivedJson.Substring(startIndex, (ii - startIndex + 1));
							if (jsonStr[0] == '{' && jsonStr[jsonStr.Length - 1] == '}')
							{
								jsonEvents.Add(jsonStr);
							}
							startIndex = ii + 1;
						}
					}
					if (bracketCount != 0)
					{
						_uncompletedMessage = receivedJson.Substring(startIndex, receivedJson.Length - startIndex);
						Debug.LogWarning("Receiver incomplete JSON string: " + _uncompletedMessage);
					}

					// Check, where the message is from:
					string remoteAddress = ((IPEndPoint)ipEndPoint).Address.ToString();
					//Debug.Log("Received Message from Address " + remoteAddress);

					List<ReceivedMessage> receivedMessages = new List<ReceivedMessage>();

					if (_udpManager.SenderToWall != null && remoteAddress == _udpManager.SenderToWall.IpAddress)
					{
						// Received message from wall...
						foreach (string msg in jsonEvents)
						{
							receivedMessages.Add(new ReceivedMessage(msg, ReceivedMessage.Sender.WALL));
						}
					}
					else if (_udpManager.SenderToFloor != null && _udpManager.SenderToFloor.IpAddress == remoteAddress)
					{
						// Received message from floor...
						foreach (string msg in jsonEvents)
						{
							receivedMessages.Add(new ReceivedMessage(msg, ReceivedMessage.Sender.FLOOR));
						}
					}
					else
					{
						Debug.LogError("Message came from unknown source. Message will be ignored!");
						return;
					}

					_receivedMessages.AddRange(receivedMessages);
				}
			}
			catch (Exception receiveException)
			{
				Debug.LogException(receiveException);
			}
		}

		public List<ReceivedMessage> GetReceivedMessages()
		{
			return _receivedMessages; // Reference!
		}
	}
}