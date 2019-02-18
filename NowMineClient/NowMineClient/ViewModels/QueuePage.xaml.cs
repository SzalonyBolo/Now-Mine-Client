using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NowMineClient.Network;
using Xamarin.Forms;
using System.Diagnostics;
using NowMineCommon.Models;
using NowMineClient.Models;
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
            foreach (var musicData in Queue.ToList())
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
            deletePopup.YesClickedEvent -= DeletePopupYesClicked;
            var clipData = deletePopup.ClipToDelete;
            var response = await serverConnection.SendDeletePiece(clipData);
            await Navigation.PopPopupAsync();
            if (response)
            {
                DependencyService.Get<IMessage>().LongAlert(String.Format("Usunięto {0}", clipData.Title));
                //Queue.Remove(clipData); - UDP recived
                //RenderQueue();
            }
            else
            {
                DependencyService.Get<IMessage>().LongAlert(String.Format("Nie udało się usunąć kawałka z kolejki"));
                await GetQueue();
            }
        }

        public async Task GetQueue()
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
                    Queue.Clear();
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

        internal async Task GetUsers()
        {
            Debug.WriteLine("Get Users!");
            User.Users = new List<User>(await serverConnection.GetUsers());
        }

        public async void SuccessfulQueued(ClipData clip, int qPos)
        {
            try
            {
                //await getQueue();
                //int qPos = e.QPos == -1 ? Queue.Count : qPos
                if (qPos <= Queue.Count)
                    Queue.Insert(qPos, clip);
                else
                    await GetQueue();
                RenderQueue();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("Error on Successful Queue: {0}", ex.Message));
            }
        }

        internal async void PlayedNow(int qPos)
        {
            //int qPos = e.EventData == -1 ? Queue.Count : e.EventData;
            //if (Queue.Count > 0)
            if (Queue.Count > 0)
                Queue.RemoveAt(0);
            else
                await GetQueue();
            if (Queue.Count > 0 && qPos != 0)
            {
                var playingNow = Queue.ElementAt(qPos);
                Queue.Insert(0, playingNow);
                Queue.RemoveAt(qPos);
            }
            else
                await GetQueue();
            RenderQueue();
            
        }

        internal async void PlayedNext()
        {
            if (Queue.Count > 0)
                Queue.RemoveAt(0);
            else
                await GetQueue();
            RenderQueue();            
        }

        internal void DeletePiece(uint queueID)
        {
            //Queue.RemoveAt(e.EventData);
            foreach (var q in Queue)
            {
                if (q.QueueID == queueID)
                {
                    Queue.Remove(q);
                    break;
                }
            }
            Device.BeginInvokeOnMainThread(() => { RenderQueue(); });
        }

        internal void QueueReveiced(ClipData clip, int qPos)
        {
            //int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
            Queue.Insert(qPos, clip);
            RenderQueue();
        }

        private async void BtnPlayNext_Clicked(object sender, EventArgs e)
        {
            bool answer = await serverConnection.SendPlayNext();
        }

        public void OnRenderQueue(object s, EventArgs e)
        {
            RenderQueue();
        }
    }
}
