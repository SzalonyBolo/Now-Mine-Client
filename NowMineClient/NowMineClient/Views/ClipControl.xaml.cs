using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace NowMineClient.Views
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClipControl : ContentView, INotifyPropertyChanged
    {
        public ClipControl()
        {
            InitializeComponent();
        }

        public void OnDeleteClicked(object o, EventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        public delegate void DeletePieceClicked(object o, EventArgs e);
        public event DeletePieceClicked DeleteClicked;
    }
}
