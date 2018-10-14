using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace NowMineClient.Models
{
    public class ClipData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ClipInfo ClipInfo;
        readonly User User;
        //public ICommand RemoveCommand { get; set; }

        //ClipData(User user)
        //{
        //    this.User = user;
        //}

        public ClipData(ClipQueued clipQueued)
        {
            User user = User.Users.Where(u => u.Id == clipQueued.UserID).First();
            this.User = user;
            ClipInfo = clipQueued;
            QueueID = clipQueued.QueueID;
            //RemoveCommand = new Command(OnDeleteClicked);
            OnPropertyChanged("RemoveCommand");
            //OnPropertyChanged("FrameColor");
        }

        public ClipData(ClipInfo info, User user)
        {
            ClipInfo = info;
            User = user;
        }

        public string Title
        {
            get
            {
                return this.ClipInfo?.Title;
            }
        }

        public string ChannelName
        {
            get
            {
                return this.ClipInfo?.ChannelName;
            }
        }

        public string UserName
        {
            get
            {
                return this.User?.Name;
            }
        }

        public int UserID
        {
            get
            {
                return this.User.Id;
            }
        }

        public Uri Image
        {
            get
            {
                return this.ClipInfo?.Thumbnail;
            }
        }

        //private Color _frameColor;
        public Color FrameColor
        {
            get
            {
                //if (_frameColor == null)
                //{
                //    _frameColor = Color.Black;
                //}
                return User.GetColor();
                //return _frameColor;
            }
            //set
            //{
            //    if (value != _frameColor)
            //    {
            //        _frameColor = value;
            //        //this.MusicPieceFrame.BorderColor = _frameColor;
            //        //this.MusicPieceFrame.BackgroundColor = _frameColor;
            //        //this.MusicPieceFrame.OutlineColor = _frameColor;
            //        //PropertyChanged?.Invoke(this, "FrameColor");
            //        //OnPropertyChanged("MusicPieceFrame");
            //        //OnPropertyChanged("BorderColor");
            //    }
            //}
        }
        public bool DeleteVisibility { get; set; } = false;

        //private void btnDelete_Clicked(object sender, EventArgs e)
        //{
        //    DeleteClicked?.Invoke(this, EventArgs.Empty);
        //}

        //private void OnDeleteClicked()
        //{
        //    DeleteClicked?.Invoke(this, EventArgs.Empty);
        //}

        //public delegate void DeletePieceClicked(object o, EventArgs e);
        //public event DeletePieceClicked DeleteClicked;

        public uint QueueID { get; set; }

        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        //public void ClearSubscriptions()
        //{
        //    DeleteClicked = null;
        //}

        public void DeleteOnTap(object sender, EventArgs e)
        {

        }
    }
}
