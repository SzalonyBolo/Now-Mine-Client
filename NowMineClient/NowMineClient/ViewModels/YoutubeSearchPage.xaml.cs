using System;
using NowMineClient.Network;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using NowMineClient.Models;
using NowMineClient.OSSpecific;
using NowMine.APIProviders;
using NowMineClient.Views;
using NowMineClient.Helpers;
using System.Linq;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YoutubeSearchPage : ContentPage
    {
        private readonly YouTubeProvider youtubeProvider;
        private readonly ServerConnection serverConnection;

        public delegate void SuccessfulQueuedEventHandler(ClipData clip, int qPos);
        public event SuccessfulQueuedEventHandler SuccessfulQueued;

        public YoutubeSearchPage(ServerConnection serverConnection)
        {
            InitializeComponent();
            youtubeProvider = new YouTubeProvider();
            this.serverConnection = serverConnection;
            //searchButton.Clicked += searchButton_Click;
            this.Title = "Kolejkuj";
            this.BackgroundColor = Color.White;
        }

        public async void EntSearch_Completed(object s, EventArgs e)
        {
            var entSearch = (Entry)s;
            string searchString = entSearch.Text;
            if (string.IsNullOrEmpty(searchString))
                return;
            searchBoard.Children.Clear();
            loadingLogo.IsVisible = true;
            var infos = await youtubeProvider.GetSearchClipInfos(searchString);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += AddToQueue_Tapped;
            loadingLogo.IsVisible = false;
            foreach (var yi in infos)
            {
                var musicData = new ClipData(yi, UserStore.DeviceUser);
                ClipControl musicPiece = new ClipControl();
                musicPiece.BindingContext = musicData;
                musicPiece.GestureRecognizers.Add(tapGestureRecognizer);
                Device.BeginInvokeOnMainThread(() => { searchBoard.Children.Add(musicPiece); });
            }
        }

        private async void AddToQueue_Tapped(object sender, EventArgs e)
        {
            try
            {
                var musicPiece = (ClipControl)sender;
                var clipData = musicPiece.BindingContext as ClipData;
                int qPos = await serverConnection.SendToQueue(clipData);
                Debug.WriteLine("Sended to server {0}; Position on queue {1}", clipData.Title, qPos);
                if (qPos != -2)
                {
                    OnSuccessfulQueued(clipData, qPos);
                    DependencyService.Get<IMessage>().LongAlert(String.Format("Zakolejkowano {0}", clipData.Title));
                }
                else
                {
                    Debug.WriteLine("Something gone wronge with queue");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception in AddToQueue_Tapped {0}", ex.Message);
            }
        }

        protected virtual void OnSuccessfulQueued(ClipData clipData, int qPos)
        {
            SuccessfulQueued?.Invoke(clipData, qPos);
        }
    }
}
