using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NowMineClient.Models;
using NowMineClient.OSSpecific;
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
            var messageString = "ChangeName " + newUserName;

            byte[] bytes = await tcpConnector.getData(messageString, serverAddress);
            bool answer = BitConverter.ToBoolean(bytes, 0);
            if (answer)
            {
                Application.Current.Properties["UserName"] = newUserName;
                await Application.Current.SavePropertiesAsync();
            }

            return answer;
        }

        internal async Task<bool> ChangeColor(byte[] NewColor)
        {
            //var messageString = "ChangeColor ";
            var CommandString = Encoding.UTF8.GetBytes("ChangeColor ");
            var Message = new byte[CommandString.Length + NewColor.Length];

            System.Buffer.BlockCopy(CommandString, 0, Message, 0, CommandString.Length);
            System.Buffer.BlockCopy(NewColor, 0, Message, CommandString.Length, NewColor.Length);

            byte[] answer = await tcpConnector.getData(Message, serverAddress);
            bool isColorChanged = BitConverter.ToBoolean(answer, 0);
            if (isColorChanged)
            {
                User.DeviceUser.UserColor = NewColor;
                Application.Current.Properties["UserColor"] = NewColor;
                await Application.Current.SavePropertiesAsync();
            }
            return isColorChanged;
        }

        internal async Task<bool> SendDeletePiece(ClipData clipData)
        {
            var messageString = "DeletePiece " + clipData.ClipInfo.ID;

            byte[] bytes = await tcpConnector.getData(messageString, serverAddress);
            bool answer = BitConverter.ToBoolean(bytes, 0);

            return answer;
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

        //protected void OnPlayedNext()
        //{
        //    PlayedNext?.Invoke(this, EventArgs.Empty);
        //}

        protected virtual void OnUDPQueued(ClipQueued piece)
        {

            ClipData mPiece = new ClipData(piece);
            UDPQueued?.Invoke(this, new PiecePosArgs(mPiece, piece.QPos));
        }

        internal async Task<IList<User>> getUsers()
        {
            try
            {
                byte[] bQueue = await tcpConnector.getData("GetUsers", serverAddress);
                using (MemoryStream ms = new MemoryStream(bQueue))
                using (BsonReader reader = new BsonReader(ms))
                {
                    reader.ReadRootValueAsArray = true;
                    JsonSerializer serializer = new JsonSerializer();
                    IList<User> users = serializer.Deserialize<IList<User>>(reader);
                    Debug.WriteLine("Got User list with {0} items", users.Count);
                    return users;
                }
                //await tcpConnector.receiveTCP();


            }
            catch (Exception e)
            {
                Debug.WriteLine("Data: {0}; message: {1}", e.Data, e.Message);
                return null;
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

        internal async Task<int> SendToQueue(ClipInfo info)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, info, typeof(ClipInfo));
                Debug.WriteLine("Sending to Queue: {0}", Convert.ToBase64String(ms.ToArray()));
                byte[] response = await tcpConnector.SendQueueData(ms.ToArray(), serverAddress);

                return BitConverter.ToInt32(response, 0);
            }
        }

        internal async Task<bool> SendPlayNext()
        {
            byte[] answer = await tcpConnector.getData("PlayNext", serverAddress);
            return BitConverter.ToBoolean(answer, 0);
        }


        public async Task<IList<ClipQueued>> getQueueTCP()
        {
            //tcpConnector.MessegeReceived += OnQueueReceived;
            try
            {
                byte[] bQueue = await tcpConnector.getData("GetQueue", serverAddress);
                if (0 != BitConverter.ToInt32(bQueue, 0))
                {
                    using (MemoryStream ms = new MemoryStream(bQueue))
                    using (BsonReader reader = new BsonReader(ms))
                    {
                        reader.ReadRootValueAsArray = true;
                        JsonSerializer serializer = new JsonSerializer();
                        IList<ClipQueued> ytInfos = serializer.Deserialize<IList<ClipQueued>>(reader);
                        Debug.WriteLine("Got Queue with {0} items", ytInfos.Count);
                        return ytInfos;
                    }
                    //todo sending back if its work
                    //await tcpConnector.receiveTCP();
                }
                else
                    return new List<ClipQueued>();
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
