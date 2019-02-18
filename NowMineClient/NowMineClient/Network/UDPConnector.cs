using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowMineClient.Network
{
    public class UDPConnector
    {
        public delegate void MessageUDPEventHandler(object source, MessegeEventArgs args);
        public event MessageUDPEventHandler MessegeReceived;

        protected virtual void OnMessageUDP(byte[] bytes)
        {
            MessegeReceived?.Invoke(this, new MessegeEventArgs() { Messege = bytes });
        }

        public string serverAddress { get; set; }

        private UdpSocketClient _udpClient;
        public UdpSocketClient udpClient
        {
            get
            {
                using (_udpClient = new UdpSocketClient())
                {
                    return _udpClient;
                }
            }
        }

        private UdpSocketReceiver _udpReceiver;
        public UdpSocketReceiver udpReceiver
        {
            get
            {
                if (_udpReceiver == null)
                    _udpReceiver = new UdpSocketReceiver();
                return _udpReceiver;
            }
        }

        public async Task SendBroadcastUDP(string message, int port = 1234)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            string address = "255.255.255.255";
            try
            {
                var uc = new UdpSocketClient();
                await uc.SendToAsync(data, address, port);
                uc.Dispose();
                Debug.WriteLine("UDP: Sent {0} to {1}:{2}", message, address, port);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("UDP/ Errir in SendBroadcastUDP",e.Message));
            }
        }

        public void receiveBroadcastUDP(int port = 1234)
        {
            udpReceiver.MessageReceived += UdpReceiver_MessageReceived;
            udpReceiver.StartListeningAsync(port);
        }

        private void UdpReceiver_MessageReceived(object sender, Sockets.Plugin.Abstractions.UdpSocketMessageReceivedEventArgs e)
        {
            if (e.RemoteAddress == serverAddress)
                OnMessageUDP(e.ByteData);
        }
    }
}
