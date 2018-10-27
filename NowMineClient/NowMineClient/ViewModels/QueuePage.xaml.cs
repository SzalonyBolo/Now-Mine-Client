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
using NowMineClient.ViewModels;
using Rg.Plugins.Popup.Extensions;
using NowMineClient.OSSpecific;
using NowMineClient.Views;
using Xamarin.Forms.Xaml;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QueuePage : ContentPage
    {
        //Network network;
        public ServerConnection serverConnection;
        private List<ClipData> _queue;
        public List<ClipData> Queue
        {
            get
            {
                if (_queue == null)
                    _queue = new List<ClipData>();
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
            if (Queue.Count > 0 && Queue.First().UserID == User.DeviceUser.Id)
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = true; BtnPlayNext.IsEnabled = true; });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => { BtnPlayNext.IsVisible = false; BtnPlayNext.IsEnabled = false; });
            }
            foreach (var musicData in Queue)
            {
                //var musicPieceUser= User.Users.Where(u => u.Id == musicData.UserID).First();
                //musicData.FrameColor = musicPieceUser.GetColor();
                //Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicPiece.copy()); });
                
                if (musicData.UserID== User.DeviceUser.Id)
                {
                    musicData.DeleteVisibility = true;
                    //musicData.DeleteClicked += ShowDeleteComfromtation;
                }
                ClipControl musicControl = new ClipControl();
                musicControl.BindingContext = musicData;
                //musicData.ClearSubscriptions();
                musicControl.DeleteClicked += ShowDeletePopup;
                Device.BeginInvokeOnMainThread(() => { sltQueue.Children.Add(musicControl); });
            }
        }

        private async void ShowDeletePopup(object o, EventArgs e)
        {
            //var clipData = o as ClipData;
            var clipView = o as ClipControl;
            var clipData = clipView.BindingContext as ClipData;
            var deletePopup = new DeletePopup(clipData);
            //await Navigation.PushModalAsync(new DeletePopup());
            deletePopup.YesClickedEvent += DeletePopupYesClicked;
            
            await Navigation.PushPopupAsync(deletePopup);
         }

        private async void DeletePopupYesClicked(object o, EventArgs e)
        {
            var deletePopup = o as DeletePopup;
            var clipData = deletePopup.ClipToDelete;
            var response = await serverConnection.SendDeletePiece(clipData);
            await Navigation.PopPopupAsync();
            if (response)
            {
                DependencyService.Get<IMessage>().LongAlert(String.Format("Usunięto {0}", clipData.Title));
                Queue.Remove(clipData);
                RenderQueue();
            }
            else
            {
                DependencyService.Get<IMessage>().ShortAlert(String.Format("Nie udało się usunąć kawałka z kolejki"));
                await getQueue();
            }
        }

        public async Task getQueue()
        {
            try
            {
                Debug.WriteLine("Get Queue!");
                IList<ClipQueued> infos = await serverConnection.GetQueue();
                if (infos == null)
                {
                    sltQueue.Children.Add(new Label() { Text = "Nie dogadałem się z serwerem :/" });
                }
                else
                {
                    foreach (ClipQueued info in infos)
                    {
                        var musicPiece = new ClipData(info);
                        //musicPiece.FrameColor = User.Users.Where(u => u.Id == info.userId).First().getColor();
                        Queue.Add(musicPiece);
                    }
                    RenderQueue();
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(String.Format("Exception in GetQueue {0}", e.Message));
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
                    Queue.Insert(qPos, e.ClipData);
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
            var playingNow = Queue.ElementAt(qPos);
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
            Queue.Insert(qPos, e.ClipData);
            RenderQueue();
        }

        private async void BtnPlayNext_Clicked(object sender, EventArgs e)
        {
            bool answer = await serverConnection.SendPlayNext();
        }
    }
}
