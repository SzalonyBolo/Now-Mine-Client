using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NowMineClient.Helpers;
using NowMineClient.Models;
using NowMineClient.OSSpecific;
using NowMineCommon.Enums;
using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMineClient.Network
{
    public class ServerConnection
    {
        public string serverAddress { get; set; }

        public delegate void ServerConnectedEventHandler(object s, EventArgs e);
        public event ServerConnectedEventHandler ServerConnected;

        public delegate void UDPQueuedEventHandler(object s, PiecePosArgs e);
        public event UDPQueuedEventHandler UDPQueued;

        public delegate void DeletePieceEventHandler(object s, GenericEventArgs<int> e);
        public event DeletePieceEventHandler DeletePiece;

        public delegate void PlayedNowEventHandler(object s, GenericEventArgs<int> e);
        public event PlayedNowEventHandler PlayedNow;

        //public event EventHandler PlayedNext;

        private UDPConnector _udpConnector;
        public UDPConnector udpConnector
        {
            get
            {
                if (_udpConnector == null)
                {
                    _udpConnector = new UDPConnector();
                }
                return _udpConnector;
            }
        }

        private TCPConnector _tcpConnector;
        public TCPConnector tcpConnector
        {
            get
            {
                if (_tcpConnector == null)
                {
                    _tcpConnector = new TCPConnector();
                }
                return _tcpConnector;
            }
        }

        internal async Task<bool> ChangeName(string newUserName)
        {

            string request = JsonMessageBuilder.GetDataCommandRequest(CommandType.ChangeName, newUserName);
            var response = await tcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        internal async Task<bool> ChangeColor(byte[] newColor)
        {
            string newColorString = System.Convert.ToBase64String(newColor);
            string request = JsonMessageBuilder.GetDataCommandRequest(CommandType.ChangeColor, newColorString);
            var response = await tcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        internal async Task<bool> SendDeletePiece(ClipData clipData)
        {
            string request = JsonMessageBuilder.GetDataCommandRequest(CommandType.DeleteClip, clipData.QueueID);
            var response = await tcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        protected void OnServerConnected()
        {
            ServerConnected?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeletepiece(int qPos)
        {
            DeletePiece?.Invoke(this, new GenericEventArgs<int>(qPos));
        }

        protected void OnPlayedNow(int qPos)
        {
            PlayedNow?.Invoke(this, new GenericEventArgs<int>(qPos));
        }


        protected virtual void OnUDPQueued(ClipQueued piece)
        {

            ClipData mPiece = new ClipData(piece);
            UDPQueued?.Invoke(this, new PiecePosArgs(mPiece, piece.QPos));
        }

        internal async Task<IList<User>> getUsers()
        {
            try
            {
                var request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.GetUsers);
                var response = await tcpConnector.getData(request, serverAddress);
                List<User> users = JsonMessageBuilder.GetStandardResponseData<List<User>>(response, CommandType.GetUsers);
                Debug.WriteLine("Got User list with {0} items", users.Count);
                return users;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Data: {0}; message: {1}", e.Data, e.Message);
                return new List<User>();
            }
        }

        public async Task<bool> ConnectToServer()
        {
            tcpConnector.MessegeReceived += OnServerFound;

            bool _serverFound = false;
            while (!_serverFound)
            {
                _serverFound = await FindServer();
            }
            return false;
        }

        public async Task<bool> FindServer()
        {
            await tcpConnector.waitForFirstConnection();
            Debug.WriteLine("Sending \"NowMine!\" to Broadcast UDP");
            await udpConnector.sendBroadcastUdp("NowMine!");
            Debug.WriteLine("Awaiting for Connection");
            
            if (string.IsNullOrEmpty(serverAddress))
            {
                return true;
            }
            return false;
        }

        internal void startListeningUDP()
        {
            udpConnector.serverAddress = serverAddress;
            udpConnector.MessegeReceived += UDPMessageReceived;
            udpConnector.receiveBroadcastUDP();
        }

        private void UDPMessageReceived(object source, MessegeEventArgs args)
        {
            byte[] bytes = args.Messege;
            string msg = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Debug.WriteLine("UDP Received: {0}", msg);
            string command = msg.Substring(0, msg.IndexOf(':'));
            int startIndex = Encoding.UTF8.GetBytes(command + ": ").Length;
            switch (command)
            {   
                case "Queue":
                    using (MemoryStream ms = new MemoryStream(bytes, startIndex, bytes.Length - startIndex))
                    using (BsonReader reader = new BsonReader(ms))
                    {
                        //reader.ReadRootValueAsArray = true;
                        JsonSerializer serializer = new JsonSerializer();
                        ClipQueued musidData = serializer.Deserialize<ClipQueued>(reader);
                        if (musidData.UserID == User.DeviceUser.Id)
                        {
                            return;
                        }
                        Debug.WriteLine("UDP/ Adding to Queue {0}", musidData.Title);
                        OnUDPQueued(musidData); 
                    }
                    break;

                case "Delete":
                    int qPosDelete = int.Parse(msg.Substring(msg.IndexOf(':') + 1));
                    //to int
                    OnDeletepiece(qPosDelete);
                    break;

                case "PlayedNow":
                    int qPosPlayedNow = BitConverter.ToInt32(bytes, startIndex);
                    //int qPosPlayedNow = int.Parse(msg.Substring(msg.IndexOf(':') + 1));
                    OnPlayedNow(qPosPlayedNow);
                    break;

                //case "PlayedNext":
                //    OnPlayedNext();
                //    break;

                default:
                    Debug.WriteLine("UDP/ Cannot interpret right...");
                    break;
            }
                
        }

        internal async Task<int> SendToQueue(ClipData data)
        {
            var QueueRequest = JsonMessageBuilder.GetDataCommandRequest(CommandType.QueueClip, data.ClipInfo);
            var response = await tcpConnector.getData(QueueRequest, serverAddress);

            uint queueID;
            int qPos;
            bool success = JsonMessageBuilder.GetQueueClipResponseData(response, out queueID, out qPos);
            data.QueueID = queueID;
            return qPos;
            //}
        }

        internal async Task<bool> SendPlayNext()
        {
            var Request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.PlayNext);
            var answer = await tcpConnector.getData(Request, serverAddress);
            return JsonMessageBuilder.GetSuccess(answer);
            //return BitConverter.ToBoolean(answer, 0);
        }


        public async Task<IList<ClipQueued>> GetQueue()
        {
            //tcpConnector.MessegeReceived += OnQueueReceived;
            try
            {
                var request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.GetQueue);
                var response = await tcpConnector.getData(request, serverAddress);
                List<ClipQueued> clips = JsonMessageBuilder.GetStandardResponseData<List<ClipQueued>>(response, CommandType.GetQueue);
                Debug.WriteLine("Got User list with {0} items", clips.Count);
                return clips;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Data: {0}; message: {1}", e.Data, e.Message);
                return null;
            }
        }

        private void OnServerFound(object source, MessegeEventArgs args)
        {
            string messege = System.Text.Encoding.UTF8.GetString(args.Messege, 0, args.Messege.Length);
            var ipAddressBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                ipAddressBuilder.Append(args.Messege[i]);
                if (i != 3)
                    ipAddressBuilder.Append('.');
            }
            serverAddress = ipAddressBuilder.ToString();
            //tutaj sprawdzanie czy to ip itd
            //int userID = BitConverter.ToInt32(args.Messege, 4);
            //User.InitializeDeviceUser(userID);
            OnServerConnected();
            tcpConnector.MessegeReceived -= OnServerFound;
        }

        public bool isWifi()
        {
            Debug.WriteLine("NET: Getting Wifi status");
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckNetworkConnection();
            bool status = networkConnection.IsConnected;
            Debug.WriteLine("NET: Wifi status is {0}", status);
            return status;
        }
    }
}
