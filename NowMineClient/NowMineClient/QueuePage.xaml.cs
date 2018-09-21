using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowMineClient.Network;
using Xamarin.Forms;
using System.Diagnostics;
using System.ComponentModel;
using NowMineCommon.Models;
using NowMineClient.Models;

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

        private void RenderQueue()
        {
            Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Clear(); }); 
            if (Queue.Count > 0 && Queue.First().Info.UserID == User.DeviceUser.Id)
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = true; BtnPlayNext.IsEnabled = true; });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = false; BtnPlayNext.IsEnabled = false; });
            }
            foreach (MusicPiece musicPiece in Queue)
            {
                var musicPieceUser= User.Users.Where(u => u.Id == musicPiece.Info.UserID).First();
                musicPiece.FrameColor = musicPieceUser.GetColor();
                //Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicPiece.copy()); });
                if (musicPiece.Info.UserID == User.DeviceUser.Id)
                {
                    musicPiece.DeleteVisible = true;
                    musicPiece.DeleteClicked += ShowDeleteComfromtation;
                }
                Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicPiece); });
            }

        }
        #region PopupAbomination
        private void ShowDeleteComfromtation(object o, EventArgs e)
        {
            MusicPiece musicPiece = o as MusicPiece;
            if (musicPiece == null)
                return;
            sltQueue.IsVisible = false;
            sltDeletePopup.IsVisible = true;
            _toDelete = musicPiece;
            btnYesPopupDelete.Clicked += BtnYesPopupDelete_Clicked;
        }

        MusicPiece _toDelete;

        private async void BtnYesPopupDelete_Clicked(object sender, EventArgs e)
        {
            if (_toDelete == null)
                return;
            bool responde = await serverConnection.SendDeletePiece(_toDelete);
            if (responde)
            {
                Queue.Remove(_toDelete);
                RenderQueue();
            }
            sltDeletePopup.IsVisible = false;
            sltQueue.IsVisible = true;
        }

        private void btnNoPopupDelete_Clicked(object sender, EventArgs e)
        {
            sltDeletePopup.IsVisible = false;
            sltQueue.IsVisible = true;
        }
        #endregion

        public async Task getQueue()
        {
            Debug.WriteLine("Get Queue!");
            IList<ClipQueued> infos = await serverConnection.getQueueTCP();
            if (infos == null)
            {
                sltQueue.Children.Add(new Label() { Text = "Nie dogadałem się z serwerem :/" } );
            }
            else
            {
                foreach (ClipQueued info in infos)
                {
                    var musicPiece = new MusicPiece(info);
                    //musicPiece.FrameColor = User.Users.Where(u => u.Id == info.userId).First().getColor();
                    Queue.Add(musicPiece);
                }
                RenderQueue();
            }
        }

        internal async Task getUsers()
        {
            Debug.WriteLine("Get Users!");
            User.Users = new List<User>(await serverConnection.getUsers());
        }

        public async void SuccessfulQueued(object s, PiecePosArgs e)
        {
            try
            {
                //await getQueue();
                int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
                if (qPos <= Queue.Count)
                    Queue.Insert(qPos, e.MusicPiece);
                else
                    await getQueue();
                RenderQueue();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error on Successful Queue: ", ex.Message);
            }
        }

        internal void PlayedNow(object s, GenericEventArgs<int> e)
        {
            int qPos = e.EventData == -1 ? Queue.Count : e.EventData;
            //if (Queue.Count > 0)
            
            Queue.RemoveAt(0);
            MusicPiece playingNow = Queue.ElementAt(qPos);
            Queue.Insert(0, playingNow);
            Queue.RemoveAt(qPos);
            RenderQueue();
            
        }

        internal void PlayedNext(object s, GenericEventArgs<int> e)
        {
            Queue.RemoveAt(0);  
            RenderQueue();            
        }

        internal void DeletePiece(object s, GenericEventArgs<int> e)
        {
            Queue.RemoveAt(e.EventData);
            Device.BeginInvokeOnMainThread(() => { RenderQueue(); });
        }

        internal void QueueReveiced(object s, PiecePosArgs e)
        {
            int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
            Queue.Insert(qPos, e.MusicPiece);
            RenderQueue();
        }

        private async void BtnPlayNext_Clicked(object sender, EventArgs e)
        {
            bool answer = await serverConnection.SendPlayNext();
        }
    }
}
