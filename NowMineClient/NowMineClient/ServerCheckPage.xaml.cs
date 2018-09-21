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

namespace NowMineClient
{
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


        public ServerCheckPage()
        {
            InitializeComponent();
            //if (serverConnection.isWifi())
            var wifi = Plugin.Connectivity.Abstractions.ConnectionType.WiFi;
            var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
            if (connectionTypes.Contains(wifi))
            {
                lblMain.Text = "Wyszukiwanie Serwera Now Mine!";
                searchServer();
            }
            else
            {
                lblMain.Text = "Połącz się z siecią wifi w której działa Now Mine!";
                CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
            }
        }

        private void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            //todo
            //updejtować property i tyle kurwa
            var wifi = Plugin.Connectivity.Abstractions.ConnectionType.WiFi;
            var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
            if (connectionTypes.Contains(wifi))
            {
                CrossConnectivity.Current.ConnectivityChanged -= Current_ConnectivityChanged;
                lblMain.Text = "Wyszukiwanie Serwera Now Mine!";
                searchServer();
            }
        }

        private async void searchServer()
        {
            try
            {
                serverConnection.ServerConnected += ServerConnected;
                await serverConnection.ConnectToServer();
            }
            catch(Exception e)
            {
                
            }
        }

        private async void ServerConnected(object s, EventArgs e)
        {
            serverConnection.ServerConnected -= ServerConnected;
            Debug.WriteLine("GUI: Open Queue Page!");
            //Device.BeginInvokeOnMainThread(() => { lblMain.Text = "Znaleziono Serwer!"; });
            var tabbedPage = new TabbedPage();
            var queuePage = new QueuePage(serverConnection);
            await queuePage.getUsers();
            await queuePage.getQueue();
            

            var ytSearchPage = new YoutubeSearchPage(serverConnection);
            ytSearchPage.SuccessfulQueued += queuePage.SuccessfulQueued;

            serverConnection.UDPQueued += queuePage.SuccessfulQueued;
            serverConnection.DeletePiece += queuePage.DeletePiece;
            serverConnection.PlayedNow += queuePage.PlayedNow;
            serverConnection.startListeningUDP();

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
