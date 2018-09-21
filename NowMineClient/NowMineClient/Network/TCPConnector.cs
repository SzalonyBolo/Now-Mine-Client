using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NowMineClient.Models;
using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NowMineClient.Network
{
    public class MessegeEventArgs : EventArgs
    {
        public byte[] Messege { get; set; }
    }

    public class TCPConnector
    {
        public delegate void MessegeTCPventHandler(object source, MessegeEventArgs args);
        public event MessegeTCPventHandler MessegeReceived;

        private TcpSocketClient _tcpClient;
        public TcpSocketClient tcpClient
        {
            get
            {
                if (_tcpClient == null)
                    _tcpClient = new TcpSocketClient();                        
                return _tcpClient;
            }
        }

        private TcpSocketListener _tcpListener;
        public TcpSocketListener tcpListener
        {
            get
            {
                if (_tcpListener == null)
                {
                    _tcpListener = new TcpSocketListener();
                }
                return _tcpListener;
            }
        }

        private bool isSending = false;

        private async void FirstConnection(object sender, Sockets.Plugin.Abstractions.TcpSocketListenerConnectEventArgs e)
        {
            Debug.WriteLine("TCP/ Host connected!");
            var messageBuffer = new byte[8];
            await e.SocketClient.ReadStream.ReadAsync(messageBuffer, 0, 8);

            //var client = e.SocketClient;
            //var bytesRead = -1;
            //var buf = new byte[1];
            //string message = "";
            //List<byte> bytes = new List<byte>();
            //while (bytesRead != 0)
            //{
            //    bytesRead = await e.SocketClient.ReadStream.ReadAsync(buf, 0, 1);
            //    if (bytesRead > 0)
            //    {
            //        Debug.WriteLine(buf[0]);
            //        message += System.Text.Encoding.UTF8.GetString(buf, 0, 1);
            //        bytes.Add(buf[0]);
            //    }
            //}
            //Checking if te first 4 bytes are the host address
            var connectedAddress = e.SocketClient.RemoteAddress.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if((int)messageBuffer[i] != int.Parse(connectedAddress[i]))
                {
                    Debug.WriteLine("TCP/ Connected Someone else then server. Disconnecting!");
                    await e.SocketClient.DisconnectAsync();
                    //await _tcpListener.StopListeningAsync();
                    return;
                }
            }
            int deviceUserID = BitConverter.ToInt32(messageBuffer, 4);

            User.InitializeDeviceUser(deviceUserID);
            Debug.WriteLine("TCP/ RECEIVED Connection from: {0}:{1}", e.SocketClient.RemoteAddress, e.SocketClient.RemotePort);
            using (var ms = new MemoryStream())
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, User.DeviceUser, typeof(User));

                int dvcusSize = ms.ToArray().Length;
                await e.SocketClient.WriteStream.WriteAsync(BitConverter.GetBytes(dvcusSize), 0, 4);
                Debug.WriteLine("TCP/ Sending to server Device User Size: {0}", dvcusSize);
                await e.SocketClient.WriteStream.FlushAsync();

                await e.SocketClient.WriteStream.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
                Debug.WriteLine("TCP/ Sending to server Device User: {0}", Convert.ToBase64String(ms.ToArray()));
            }
            await e.SocketClient.WriteStream.FlushAsync();

            int intOk = (byte)e.SocketClient.ReadStream.ReadByte();
            //bool okCheck = BitConverter.ToBoolean(okByte, 0);
            //if ok check....

            OnMessegeTCP(messageBuffer);
            await _tcpListener.StopListeningAsync();
        }


        protected virtual void OnMessegeTCP(byte[] bytes)
        {
            MessegeReceived?.Invoke(this, new MessegeEventArgs() { Messege = bytes });
        }

        public async Task waitForFirstConnection()
        {
            Debug.WriteLine("TCP: Starting Listening!");
            tcpListener.ConnectionReceived += FirstConnection;
            await tcpListener.StartListeningAsync(4444);
        }

        public async Task<byte[]> getData(string message, string serverAddress)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                return await getData(data, serverAddress);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in TCP/GetData {0}", e.Message);
                return new byte[0];
            }
        }

        public async Task<byte[]> getData(byte[] message, string serverAddress)
        {
            try
            {
                Debug.WriteLine("Sending {0} to {1}!", message, serverAddress);
                await tcpClient.ConnectAsync(serverAddress, 4444);
                await tcpClient.WriteStream.WriteAsync(message, 0, message.Length);
                int readByte = 0;
                List<byte> queue = new List<byte>();
                while (readByte != -1)
                {
                    readByte = tcpClient.ReadStream.ReadByte();
                    Debug.WriteLine("TCP/ Rec: {0}", readByte);
                    queue.Add((byte)readByte);
                }
                await tcpClient.DisconnectAsync();
                return queue.ToArray();
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception in TCP/GetData {0}", e.Message);
                return new byte[0];
            }
            
        }

        public async Task<byte[]> SendQueueData(byte[] data, string serverAddress)
        {
            if (isSending)
            {
                return null;
            }
            try
            {
                isSending = true;
                Debug.WriteLine("Sending {0} to {1}!", Encoding.UTF8.GetString(data, 0, data.Length), serverAddress);
                byte[] queueString = Encoding.UTF8.GetBytes("Queue: ");
                byte[] message = new byte[queueString.Length + data.Length];
                System.Buffer.BlockCopy(queueString, 0, message, 0, queueString.Length);
                System.Buffer.BlockCopy(data, 0, message, queueString.Length, data.Length);
                await tcpClient.ConnectAsync(serverAddress, 4444);
                Debug.WriteLine("Connected!");
                await tcpClient.WriteStream.WriteAsync(message, 0, message.Length);
                //await tcpClient.WriteStream.WriteAsync(data, 0, data.Length);
                await tcpClient.WriteStream.FlushAsync();
                int readByte = 0;
                List<byte> response = new List<byte>();
                while (readByte != -1)
                {
                    readByte = tcpClient.ReadStream.ReadByte();
                    Debug.WriteLine("TCP/ Rec: {0}", readByte);
                    response.Add((byte)readByte);
                }
                isSending = false;
                await tcpClient.DisconnectAsync();
                return response.ToArray();
            }
            catch (Exception ex)
            {
                isSending = false;
                Debug.WriteLine("Message:{0} Data:{1} Source:{2}", ex.Message, ex.Data, ex.Source);
                return null;
            }
        }


        public async Task stopWaitingForFirstConnection()
        {
            tcpListener.ConnectionReceived -= FirstConnection;
            Debug.WriteLine("TCP: Stopping Listening!");
            await tcpListener.StopListeningAsync();
        }
    }
}