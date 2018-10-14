using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowMineClient.Network;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using NowMineCommon.Models;
using NowMineClient.Models;
using NowMineClient.OSSpecific;
using NowMine.APIProviders;
using NowMineClient.Views;

namespace NowMineClient.ViewModels
{
    //[XamlCompilation (XamlCompilationOptions.Compile)]
    public partial class YoutubeSearchPage : ContentPage
    {
        private readonly YouTubeProvider youtubeProvider;
        private readonly ServerConnection serverConnection;

        public delegate void SuccessfulQueuedEventHandler(object s, PiecePosArgs e);
        public event SuccessfulQueuedEventHandler SuccessfulQueued;

        public YoutubeSearchPage(ServerConnection serverConnection)
        {
            InitializeComponent();
            youtubeProvider = new YouTubeProvider();
            this.serverConnection = serverConnection;
            //searchButton.Clicked += searchButton_Click;
            this.Title = "Kolejkuj";
            //this.BackgroundColor = Color.Purple;
            this.BackgroundColor = Color.White;
        }

        public void entSearch_Completed(object s, EventArgs e)
        {
            var entSearch = (Entry)s;
            string searchString = entSearch.Text;
            var infos = youtubeProvider.GetSearchClipInfos(searchString);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += AddToQueue_Tapped;
            searchBoard.Children.Clear();
            foreach (var yi in infos)
            {
                //new ClipQueued(yi, 0, User.DeviceUser.Id);
                var musicData = new ClipData(yi, User.DeviceUser);
                ClipControl musicPiece = new ClipControl();
                //musicPiece.FrameColor = User.DeviceUser.GetColor();
                musicPiece.BindingContext = musicData;
                musicPiece.GestureRecognizers.Add(tapGestureRecognizer);
                searchBoard.Children.Add(musicPiece);
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
            SuccessfulQueued?.Invoke(this, new PiecePosArgs(clipData, qPos));
        }
    }
}
