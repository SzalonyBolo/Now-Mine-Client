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

        public delegate void UDPQueuedEventHandler(ClipData clip, int qPos);
        public event UDPQueuedEventHandler UDPQueued;

        public delegate void DeletePieceEventHandler(uint queueID);
        public event DeletePieceEventHandler DeletePiece;

        public delegate void PlayedNowEventHandler(int qPos);
        public event PlayedNowEventHandler PlayedNow;

        public event EventHandler RenderQueue;

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
        public TCPConnector TcpConnector
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
            var response = await TcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        internal async Task<bool> ChangeColor(byte[] newColor)
        {
            string newColorString = Convert.ToBase64String(newColor);
            string request = JsonMessageBuilder.GetDataCommandRequest(CommandType.ChangeColor, newColorString);
            var response = await TcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        internal async Task<bool> SendDeletePiece(ClipData clipData)
        {
            string request = JsonMessageBuilder.GetDataCommandRequest(CommandType.DeleteClip, clipData.QueueID);
            var response = await TcpConnector.getData(request, serverAddress);

            return JsonMessageBuilder.GetSuccess(response);
        }

        protected void OnServerConnected()
        {
            ServerConnected?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeletepiece(uint queueIDToDelete)
        {
            DeletePiece?.Invoke(queueIDToDelete);
        }

        protected void OnPlayedNow(int qPos)
        {
            PlayedNow?.Invoke(qPos);
        }

        protected virtual void OnUDPQueued(ClipQueued piece, int qPos)
        {
            ClipData mPiece = new ClipData(piece);
            UDPQueued?.Invoke(mPiece, qPos);
        }

        internal async Task<IList<User>> GetUsers()
        {
            try
            {
                var request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.GetUsers);
                var response = await TcpConnector.getData(request, serverAddress);
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

        internal void StartListeningUDP()
        {
            udpConnector.serverAddress = serverAddress;
            udpConnector.MessegeReceived += UDPMessageReceived;
            udpConnector.receiveBroadcastUDP();
        }

        private async void UDPMessageReceived(object source, MessegeEventArgs args)
        {
            byte[] bytes = args.Messege;
            string msg = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Debug.WriteLine(string.Format("UDP Received: {0}", msg));

            if(bytes.Length < sizeof(uint) + sizeof(int))  //Command + eventID
            {
                Debug.WriteLine("Message to short!");
                return;
            }

            uint receivedEventID = BitConverter.ToUInt32(bytes, bytes.Length - sizeof(uint));
            if (EventManager.CheckCurrentEventID(receivedEventID))
            {
                Console.WriteLine(string.Format("EventID further! GetEvents getting send!"));
                var request = JsonMessageBuilder.GetDataCommandRequest<uint>(CommandType.GetEvents, EventManager.ActualEventID);
                var response = await TcpConnector.getData(request, serverAddress);
                var eventList = JsonMessageBuilder.GetStandardResponseData<List<EventItem>>(response, CommandType.GetEvents);
                Debug.WriteLine("Got Event list with {0} items", eventList.Count);
                EventManager.DoEvents(eventList);
                return;
            }

            Array.Copy(bytes, 0, bytes, 0, bytes.Length - sizeof(uint));

            //string command = msg.Substring(0, msg.IndexOf(':'));
            int commandInt = BitConverter.ToInt32(bytes, 0);
            CommandType command = (CommandType)commandInt;
            int startIndex = sizeof(int);
            switch (command)
            {
                case CommandType.QueueClip:
                    using (MemoryStream ms = new MemoryStream(bytes, startIndex, bytes.Length - startIndex))
                    using (BsonReader reader = new BsonReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ClipQueued musidData = serializer.Deserialize<ClipQueued>(reader);
                        if (musidData.UserID == User.DeviceUser.Id)
                        {
                            Debug.WriteLine("UDP/ Omiting mine queued piece");
                            return;
                        }
                        Debug.WriteLine(string.Format("UDP/ Adding to Queue {0}", musidData.Title));
                        OnUDPQueued(musidData, musidData.QPos); 
                    }
                    break;

                case CommandType.DeleteClip:
                    //int qPosDelete = int.Parse(msg.Substring(msg.IndexOf(':') + 1));
                    //to int
                    uint queueIDToDelete = BitConverter.ToUInt32(bytes, startIndex);
                    Debug.WriteLine("UDP/ Deleting Clip with QueueID: {0}", queueIDToDelete);
                    OnDeletepiece(queueIDToDelete);
                    break;

                case CommandType.PlayNow:
                    int qPos = BitConverter.ToInt32(bytes, startIndex);
                    //int qPosPlayedNow = int.Parse(msg.Substring(msg.IndexOf(':') + 1));
                    Debug.WriteLine("UDP/ Play Now with qPos: {0}", qPos);
                    OnPlayedNow(qPos);
                    break;

                case CommandType.PlayNext:
                    Debug.WriteLine("UDP/ Played Next");
                    OnPlayedNow(0);
                    break;

                case CommandType.ChangeName:
                    int userID = BitConverter.ToInt32(bytes, startIndex);
                    string userName = BitConverter.ToString(bytes, sizeof(int));
                    Debug.WriteLine("UDP/ Changing name userId: {0} to {1}", userID, userName);
                    if (!string.IsNullOrEmpty(userName))
                    {
                        User.Users[userID].Name = userName;
                        RenderQueue?.Invoke(this, new EventArgs());
                    }
                    break;

                case CommandType.ChangeColor:
                    byte[] colorBytes = new byte[3];
                    int byteIndex = startIndex;
                    Array.Copy(bytes, startIndex, colorBytes, 0, sizeof(byte) * 3);
                    var userId = BitConverter.ToInt32(bytes, startIndex + (sizeof(byte) * 3));
                    Debug.WriteLine("UDP/ Changing color userId: {0} to {1}", userId, colorBytes);
                    User.Users[userId].UserColor = colorBytes;
                    
                    RenderQueue?.Invoke(this, new EventArgs());
                    break;

                case CommandType.ServerShutdown:
                    Debug.WriteLine("UDP/ Server Shutdown");
                    Device.BeginInvokeOnMainThread(() => { App.Current.MainPage = new ViewModels.ServerCheckPage(); });
                    break;

                case CommandType.QueueReshufle:
                    Debug.WriteLine("UDP/ Queue Reshufle");
                    break;

                default:
                    Debug.WriteLine("UDP/ Cannot interpret right...");
                    break;
            }
        }

        internal async Task<int> SendToQueue(ClipData data)
        {
            var QueueRequest = JsonMessageBuilder.GetDataCommandRequest(CommandType.QueueClip, data.ClipInfo);
            var response = await TcpConnector.getData(QueueRequest, serverAddress);

            uint queueID;
            int qPos;
            bool success = JsonMessageBuilder.GetQueueClipResponseData(response, out queueID, out qPos);
            data.QueueID = queueID;
            return qPos;
        }

        internal async Task<bool> SendPlayNext()
        {
            var Request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.PlayNext);
            var answer = await TcpConnector.getData(Request, serverAddress);
            Debug.WriteLine("TCP/ Sending Play Next");
            return JsonMessageBuilder.GetSuccess(answer);
            //return BitConverter.ToBoolean(answer, 0);
        }

        public async Task<IList<ClipQueued>> GetQueue()
        {
            try
            {
                var request = JsonMessageBuilder.GetStandardCommandRequest(CommandType.GetQueue);
                var response = await TcpConnector.getData(request, serverAddress);
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

        public async Task<bool> ListenAndCallServer()
        {
            TcpConnector.MessegeReceived += OnServerFound;
            try
            {
                await TcpConnector.WaitForFirstConnection();
                Debug.WriteLine("Awaiting for Connection");
                
                while(string.IsNullOrEmpty(serverAddress))
                {
                    await udpConnector.SendBroadcastUDP("NowMine!");
                    Debug.WriteLine("Sending \"NowMine!\" to Broadcast UDP");
                    await Task.Delay(3000);
                }

                return true;
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("Error in ListenAndCallServer: {0}", e.Message));
                return false;
            }
        }

        private void OnServerFound(object source, MessegeEventArgs args)
        {
            string messege = Encoding.UTF8.GetString(args.Messege, 0, args.Messege.Length);
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
            TcpConnector.MessegeReceived -= OnServerFound;
        }

        public bool IsWifi()
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