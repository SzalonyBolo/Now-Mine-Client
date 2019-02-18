using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowMineClient.Network;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using NowMineClient.OSSpecific;
using NowMineClient.Helpers;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerCheckPage : ContentPage
    {
        private ServerConnection _serverConnection { set; get; }
        internal ServerConnection serverConnection
        {
            get
            {
                if (_serverConnection == null)
                    _serverConnection = new ServerConnection();
                return _serverConnection;
            }
        }
        IEnumerable<ConnectionType> connectionTypes = CrossConnectivity.Current.ConnectionTypes;


        public ServerCheckPage()
        {
            InitializeComponent();
            
            //var coolLabel = new FontAwesomeIcon(FontAwesomeIcon.Icon.Gear);
            //sltMain.Children.Add(coolLabel);
            var wifi = ConnectionType.WiFi;
            if (connectionTypes.Contains(wifi))
            {
                lblMain.Text = "Wyszukiwanie Serwera Now Mine!";
                AwaitForServer();
            }
            else
            {
                lblMain.Text = "Połącz się z siecią wifi w której działa Now Mine!";
                CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
            }
        }

        private void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            //todo
            //updejtować property i tyle kurwa
            var wifi = ConnectionType.WiFi;
            connectionTypes = CrossConnectivity.Current.ConnectionTypes;
            if (connectionTypes.Contains(wifi))
            {
                CrossConnectivity.Current.ConnectivityChanged -= Current_ConnectivityChanged;
                lblMain.Text = "Wyszukiwanie Serwera Now Mine!";
                AwaitForServer();
            }
        }

        private async void AwaitForServer()
        {
            try
            {
                serverConnection.ServerConnected += OnServerConnected;
                await serverConnection.ListenAndCallServer();
            }
            catch(Exception e)
            {
                Debug.WriteLine("Error on SearchServer");
            }
        }

        public async void OnServerConnected(object s, EventArgs e)
        {
            serverConnection.ServerConnected -= OnServerConnected;
            Debug.WriteLine("GUI: Open Queue Page!");
            //Device.BeginInvokeOnMainThread(() => { lblMain.Text = "Znaleziono Serwer!"; });
            var tabbedPage = new TabbedPage();
            var queuePage = new QueuePage(serverConnection);
            await queuePage.GetUsers();
            await queuePage.GetQueue();
            
            var ytSearchPage = new YoutubeSearchPage(serverConnection);
            ytSearchPage.SuccessfulQueued += queuePage.SuccessfulQueued;

            serverConnection.UDPQueued += queuePage.SuccessfulQueued;
            serverConnection.DeletePiece += queuePage.DeletePiece;
            serverConnection.PlayedNow += queuePage.PlayedNow;
            serverConnection.RenderQueue += queuePage.OnRenderQueue;

            EventManager.QueuedPiece += queuePage.SuccessfulQueued;
            EventManager.DeletedPiece += queuePage.DeletePiece;
            EventManager.PlayedNow += queuePage.PlayedNow;

            serverConnection.StartListeningUDP();

            var userConfigPage = new UserConfigPage(serverConnection);

            tabbedPage.Children.Add(userConfigPage);
            tabbedPage.Children.Add(queuePage);
            tabbedPage.Children.Add(ytSearchPage);
            //tabbedPage.Height;

            tabbedPage.BarBackgroundColor = Color.Purple;
            tabbedPage.BarTextColor = Color.White;
            
            Device.BeginInvokeOnMainThread(() => { App.Current.MainPage = tabbedPage; });
        }
    }
}
