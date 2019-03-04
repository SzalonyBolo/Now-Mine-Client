using System;
using System.Collections.Generic;
using System.Linq;
using NowMineClient.Network;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using NowMineClient.Helpers;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerCheckPage : ContentPage
    {
        private readonly IServerConnection serverConnection = DependencyService.Get<IServerConnection>();

        IEnumerable<ConnectionType> connectionTypes = CrossConnectivity.Current.ConnectionTypes;


        public ServerCheckPage()
        {
            InitializeComponent();
            var wifi = ConnectionType.WiFi;
            if (connectionTypes.Contains(wifi))
            {
                lblMain.Text = "Szukam Serwera";
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
                lblMain.Text = "Szukam Serwera";
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
                Debug.WriteLine(string.Format("Error on SearchServer: {0}", e.Message));
            }
        }

        public async void OnServerConnected(object s, EventArgs e)
        {
            serverConnection.ServerConnected -= OnServerConnected;
            Debug.WriteLine("GUI: Open Queue Page!");
            //Device.BeginInvokeOnMainThread(() => { lblMain.Text = "Znaleziono Serwer!"; });
            var tabbedPage = new TabbedPage();
            var queuePage = new QueuePage();
            await queuePage.GetUsers();
            await queuePage.GetQueue();

            //var primaryColor = Android.Resource.Attribute.ColorPrimary;

            var ytSearchPage = new YoutubeSearchPage();
            ytSearchPage.SuccessfulQueued += queuePage.SuccessfulQueued;

            serverConnection.UDPQueued += queuePage.SuccessfulQueued;
            serverConnection.DeletePiece += queuePage.DeletePiece;
            serverConnection.PlayedNow += queuePage.PlayedNow;
            serverConnection.RenderQueue += queuePage.OnRenderQueue;

            EventManager.QueuedPiece += queuePage.SuccessfulQueued;
            EventManager.DeletedPiece += queuePage.DeletePiece;
            EventManager.PlayedNow += queuePage.PlayedNow;

            serverConnection.StartListeningUDP();

            var userConfigPage = new UserConfigPage();
            //userConfigPage.UserConfigChanged += queuePage.OnRenderQueue;

            tabbedPage.Children.Add(userConfigPage);
            tabbedPage.Children.Add(queuePage);
            tabbedPage.Children.Add(ytSearchPage);
            //tabbedPage.Height;

            //tabbedPage.BarBackgroundColor = Color.Purple;
            //tabbedPage.BarTextColor = Color.White;
            
            Device.BeginInvokeOnMainThread(() => { App.Current.MainPage = tabbedPage; });
        }
    }
}
