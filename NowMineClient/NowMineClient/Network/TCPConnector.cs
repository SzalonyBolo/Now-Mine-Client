using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NowMineClient.Models;
using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using NowMineClient.Helpers;

namespace NowMineClient.Network
{
    public class TCPConnector
    {
        public delegate void MessegeTCPventHandler(object source, MessegeEventArgs args);
        public event MessegeTCPventHandler MessegeReceived;

        private TcpSocketClient _tcpClient;
        public TcpSocketClient TcpClient
        {
            get
            {
                if (_tcpClient == null)
                    _tcpClient = new TcpSocketClient();                        
                return _tcpClient;
            }
        }

        private TcpSocketListener _tcpListener;
        public TcpSocketListener TcpListener
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

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        private async void FirstConnection(object sender, Sockets.Plugin.Abstractions.TcpSocketListenerConnectEventArgs socket)
        {
            Debug.WriteLine("TCP/ Host connected!");
            var messageBuffer = new byte[8];
            await socket.SocketClient.ReadStream.ReadAsync(messageBuffer, 0, 8);
            //Checking if te first 4 bytes are the host address
            var connectedAddress = socket.SocketClient.RemoteAddress.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if((int)messageBuffer[i] != int.Parse(connectedAddress[i]))
                {
                    Debug.WriteLine("TCP/ Connected Someone else then server. Disconnecting!");
                    await socket.SocketClient.DisconnectAsync();
                    return;
                }
            }
            int deviceUserID = BitConverter.ToInt32(messageBuffer, 4);

            UserStore.InitializeDeviceUser(deviceUserID);
            Debug.WriteLine("TCP/ Received first Connection from: {0}:{1}", socket.SocketClient.RemoteAddress, socket.SocketClient.RemotePort);
            using (var ms = new MemoryStream())
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, UserStore.DeviceUser, typeof(User));

                int dvcusSize = ms.ToArray().Length;
                await socket.SocketClient.WriteStream.WriteAsync(BitConverter.GetBytes(dvcusSize), 0, 4);
                Debug.WriteLine("TCP/ Sending to server Device User Size: {0}", dvcusSize);
                await socket.SocketClient.WriteStream.FlushAsync();

                await socket.SocketClient.WriteStream.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
                Debug.WriteLine(string.Format("TCP/ Sending to server Device User: {0}", Convert.ToBase64String(ms.ToArray())));
            }
            await socket.SocketClient.WriteStream.FlushAsync();

            int intOk = (byte)socket.SocketClient.ReadStream.ReadByte();
            //bool okCheck = BitConverter.ToBoolean(okByte, 0);
            //todo if ok check....

            OnMessegeTCP(messageBuffer);
            try
            {
                await TcpListener.StopListeningAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("TCP/ StopListenAsync: {0}", e.Message));
            }
        }


        protected virtual void OnMessegeTCP(byte[] bytes)
        {
            MessegeReceived?.Invoke(this, new MessegeEventArgs() { Message = bytes });
        }

        public async Task WaitForFirstConnection()
        {
            Debug.WriteLine("TCP: Starting Listening!");
            TcpListener.ConnectionReceived += FirstConnection;
            await TcpListener.StartListeningAsync(4444);
        }

        public async Task<string> GetData(string message, string serverAddress)
        {
            try
            {
                byte[] request = Encoding.UTF8.GetBytes(message);
                Debug.WriteLine(string.Format("Sending {0}", message));
                var response = await GetData(request, serverAddress);
                var data = Encoding.UTF8.GetString(response);
                Debug.WriteLine(string.Format("Got response: {0}", response));
                return Regex.Replace(data, @"[^\u0020-\u007E]", string.Empty);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Exception in TCP/GetData {0}", e.Message));
                return string.Empty;
            }
        }

        public async Task<byte[]> GetData(byte[] message, string serverAddress)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                //Debug.WriteLine("Sending {0} to {1}!", Convert.ToBase64String(message), serverAddress);
                await TcpClient.ConnectAsync(serverAddress, 4444);
                await TcpClient.WriteStream.WriteAsync(message, 0, message.Length);

                int readByte = 0;
                List<byte> answer = new List<byte>();
                while (readByte != -1)
                {
                    readByte = TcpClient.ReadStream.ReadByte();
                    //Debug.WriteLine("TCP/ Rec: {0}", readByte);
                    answer.Add((byte)readByte);
                }
                await TcpClient.DisconnectAsync();
                _semaphoreSlim.Release();
                return answer.ToArray();
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("Exception in TCP/GetData {0}", e.Message));
                throw e;
            }
        }
    }

    public class MessegeEventArgs : EventArgs
    {
        public byte[] Message { get; set; }
    }
}