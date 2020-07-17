using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace DeepSpace.Udp
{
	public class UdpManager : MonoBehaviour
	{
		// Add more UdpSender here if there are e.g. external Servers that shall receive data.
		[SerializeField]
		private UdpSender _senderToWall;
		[SerializeField]
		private UdpSender _senderToFloor;
		[SerializeField]
		private UdpReceiver _receiver;

		public UdpSender SenderToWall
		{
			get { return _senderToWall; }
			private set { _senderToWall = value; }
		}

		public UdpSender SenderToFloor
		{
			get { return _senderToFloor; }
			private set { _senderToFloor = value; }
		}

		public UdpReceiver Receiver
		{
			get { return _receiver; }
			private set { _receiver = value; }
		}

		private void Start()
		{
			CmdConfigManager config = CmdConfigManager.Instance;
			if (config == null)
			{
				Debug.LogWarning("Missing CmdConfigManager. Is Main Scene loaded?"
					+ "\nUsing default values from UDP Sender and Receiver components instead of configured values.");
			}

			UdpCmdConfigMgr configMgr = UdpCmdConfigMgr.Instance as UdpCmdConfigMgr;

			// Depending on the own state, this manager does not need a sender to itself, so it will be destroyed.
			// Additionally, the receiver is beeing configured.
			switch (configMgr.applicationType)
			{
				case CmdConfigManager.AppType.WALL:
					int wallUdpPort = configMgr.udpReceivingPort; // This port should be configured depending on application type.
					Receiver.ActivateReceiver(wallUdpPort);
					Destroy(SenderToWall.gameObject);
					SenderToWall = null;
					break;
				case CmdConfigManager.AppType.FLOOR:
					int floorUdpPort = configMgr.udpReceivingPort; // This should be configured depending on application type.
					Receiver.ActivateReceiver(floorUdpPort);
					Destroy(SenderToFloor.gameObject);
					SenderToFloor = null;
					break;
			}

			// All not disabled senders need to be configured and activated:
			if (SenderToWall != null)
			{
				string wallIp = configMgr.udpAddress; // SenderToWall.IpAddress
				int wallUdpPort = configMgr.udpSendingPort; // SenderToWall.Port;
				SenderToWall.ActivateSender(wallIp, wallUdpPort);
			}
			if (SenderToFloor != null)
			{
				string floorIp = configMgr.udpAddress; // SenderToFloor.IpAddress
				int floorUdpPort = configMgr.udpSendingPort; // SenderToFloor.Port
				SenderToFloor.ActivateSender(floorIp, floorUdpPort);
			}
		}
	}
}