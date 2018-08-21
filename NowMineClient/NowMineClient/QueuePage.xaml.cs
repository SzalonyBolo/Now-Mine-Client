using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowMineClient.Network;
using Xamarin.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace NowMineClient
{
    public partial class QueuePage : ContentPage
    {
        //Network network;
        public ServerConnection serverConnection;
        private List<MusicPiece> _queue;
        public List<MusicPiece> Queue
        {
            get
            {
                if (_queue == null)
                    _queue = new List<MusicPiece>();
                return _queue;
            }
            set
            {
                _queue = value;
            }
        }
        public QueuePage(ServerConnection serverConnection)
        {
            InitializeComponent();
            this.serverConnection = serverConnection;
            this.Title = "Kolejka";
        }

        private void renderQueue()
        {
            Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Clear(); }); 
            if (Queue.Count > 0 && Queue.First().Info.userId == User.DeviceUser.Id)
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = true; BtnPlayNext.IsEnabled = true; });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = false; BtnPlayNext.IsEnabled = false; });
            }
            foreach (MusicPiece musicPiece in Queue)
            {
                var musicPieceUser= User.Users.Where(u => u.Id == musicPiece.Info.userId).First();
                musicPiece.FrameColor = musicPieceUser.getColor();
                //Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicPiece.copy()); });
                Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicPiece); });
            }

        }

        public async Task getQueue()
        {
            Debug.WriteLine("Get Queue!");
            IList<YoutubeInfo> infos = await serverConnection.getQueueTCP();
            if (infos == null)
            {
                sltQueue.Children.Add(new Label() { Text = "Nie dogadałem się z serwerem :/" } );
            }
            else
            {
                foreach (YoutubeInfo info in infos)
                {
                    var musicPiece = new MusicPiece(info);
                    //musicPiece.FrameColor = User.Users.Where(u => u.Id == info.userId).First().getColor();
                    Queue.Add(musicPiece);
                }
                renderQueue();
            }
        }

        internal async Task getUsers()
        {
            Debug.WriteLine("Get Users!");
            User.Users = new List<User>(await serverConnection.getUsers());
        }

        public async void SuccessfulQueued(object s, PiecePosArgs e)
        {
            //await getQueue();
            int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
            if (qPos <= Queue.Count)
                Queue.Insert(qPos, e.MusicPiece);
            else
                await getQueue();
            renderQueue();
        }

        internal void PlayedNow(object s, GenericEventArgs<int> e)
        {
            int qPos = e.EventData == -1 ? Queue.Count : e.EventData;
            //if (Queue.Count > 0)
            
            Queue.RemoveAt(0);
            MusicPiece playingNow = Queue.ElementAt(qPos);
            Queue.Insert(0, playingNow);
            Queue.RemoveAt(qPos);
            renderQueue();
            
        }

        internal void PlayedNext(object s, GenericEventArgs<int> e)
        {
            Queue.RemoveAt(0);  
            renderQueue();            
        }

        internal void DeletePiece(object s, GenericEventArgs<int> e)
        {
            Queue.RemoveAt(e.EventData);
            Device.BeginInvokeOnMainThread(() => { renderQueue(); });
        }

        internal void QueueReveiced(object s, PiecePosArgs e)
        {
            int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
            Queue.Insert(qPos, e.MusicPiece);
            renderQueue();
        }

        private async void BtnPlayNext_Clicked(object sender, EventArgs e)
        {
            bool answer = await serverConnection.SendPlayNext();
        }
    }
}
