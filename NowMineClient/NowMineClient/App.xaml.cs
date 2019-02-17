using NowMineClient.Network;
using NowMineClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMineClient
{
    public partial class App : Application
    {

        //private ServerConnection _serverConnection { set; get; }
        //internal ServerConnection serverConnection
        //{
        //    get
        //    {
        //        if (_serverConnection == null)
        //            _serverConnection = new ServerConnection();
        //        return _serverConnection;
        //    }
        //}

        public App()
        {
            InitializeComponent();

            //MainPage = new NowMineClient.MainPage();
            var serverCheckPage = new ServerCheckPage();
            //serverConnection.ServerConnected += serverCheckPage.ServerConnected;

            MainPage = serverCheckPage;

            //var task = serverConnection.ConnectToServer();
            //task.RunSynchronously();
            //var result = task.Result;
            //var task = Task.Run(async () => result = await serverConnection.ConnectToServer());
            //if (result)
///            serverConnection.ConnectToServer().Start();

            
            //MainPage = new Page1();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
