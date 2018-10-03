using NowMineClient.Models;
using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NowMineClient.Views
{
    public partial class ClipControl : ContentView, INotifyPropertyChanged
    {
        //ICommand RemoveCommand;

        public ClipControl()
        {
            InitializeComponent();
            //RemoveCommand = new Command(asdf);
        }

        //public void asdf()
        //{
        //    Console.WriteLine("asdfasdf");
        //}
    }
}
