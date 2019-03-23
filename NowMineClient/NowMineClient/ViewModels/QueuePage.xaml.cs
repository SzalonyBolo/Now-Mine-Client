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
using Xamarin.Forms.Xaml;
using NowMineClient.Helpers;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QueuePage : ContentPage
    {
        private readonly IServerConnection serverConnection = DependencyService.Get<IServerConnection>();
        
        private ObservableCollection<ClipData> _queue;
        public ObservableCollection<ClipData> Queue
        {
            get
            {
                if (_queue == null)
                    _queue = new ObservableCollection<ClipData>();
                return _queue;
            }
            set
            {
                _queue = value;
            }
        }

        private SemaphoreSlim _semaphorePopup = new SemaphoreSlim(1);

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new Command(async () => await ExecuteRefreshCommand())); }
        }

        private ICommand _deleteClipCommand;
        public ICommand DeleteClipCommand
        {
            get { return _deleteClipCommand ?? (_deleteClipCommand = new Command<ClipData>(async (o) => await ShowDeletePopup(o))); }
        }

        bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                if (_isRefreshing == value)
                    return;

                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public QueuePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async Task ShowDeletePopup(ClipData clipData)
        {
            await _semaphorePopup.WaitAsync();
            var deletePopup = new DeletePopup(clipData, _semaphorePopup);
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
                Queue.Clear();
                if (infos == null)
                {
                    //Queue.Children.Add(new Label() { Text = "Nie dogadałem się z serwerem :/" });
                }
                else
                {
                    foreach (ClipQueued info in infos)
                    {
                        var musicPiece = new ClipData(info);
                        //musicPiece.FrameColor = UserStore.Users.Where(u => u.Id == info.userId).First().getColor();
                        Queue.Add(musicPiece);
                    }
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
            UserStore.Users = new ObservableCollection<User>(await serverConnection.GetUsers());
        }

        public async void SuccessfulQueued(ClipData clip, int qPos)
        {
            try
            {
                if (qPos <= Queue.Count)
                    Queue.Insert(qPos, clip);
                else
                    await GetQueue();
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
        }

        internal async void PlayedNext()
        {
            if (Queue.Count > 0)
                Queue.RemoveAt(0);
            else
                await GetQueue();
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
            //Device.BeginInvokeOnMainThread(() => { RenderQueue(); });
        }

        internal void QueueReveiced(ClipData clip, int qPos)
        {
            //int qPos = e.QPos == -1 ? Queue.Count : e.QPos;
            Queue.Insert(qPos, clip);
        }

        private async void BtnPlayNext_Clicked(object sender, EventArgs e)
        {
            bool answer = await serverConnection.SendPlayNext();
        }

        private async Task ExecuteRefreshCommand()
        {
            await serverConnection.GetUsers();
            await serverConnection.GetQueue();
            IsRefreshing = false;
        }
    }
}