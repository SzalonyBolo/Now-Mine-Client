using Sockets.Plugin;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NowMineClient.Network
{
    public class UDPConnector
    {
        public event Action<byte[]> MessegeReceived;

        public string ServerAddress { get; set; }

        private UdpSocketClient _udpClient;
        public UdpSocketClient UdpClient
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
        public UdpSocketReceiver UdpReceiver
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

        public void ReceiveBroadcastUDP(int port = 1234)
        {
            UdpReceiver.MessageReceived += UdpReceiver_MessageReceived;
            UdpReceiver.StartListeningAsync(port);
        }

        private void UdpReceiver_MessageReceived(object sender, Sockets.Plugin.Abstractions.UdpSocketMessageReceivedEventArgs e)
        {
            if (e.RemoteAddress == ServerAddress)
                MessegeReceived?.Invoke(e.ByteData);
        }
    }
}
