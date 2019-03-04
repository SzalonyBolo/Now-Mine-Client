using NowMineClient.ViewModels;
using Xamarin.Forms;

namespace NowMineClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new ServerCheckPage();
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
