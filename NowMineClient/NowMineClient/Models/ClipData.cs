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

        public ClipData(ClipQueued clipQueued)
        {
            User user = User.Users.Where(u => u.Id == clipQueued.UserID).First();
            this.User = user;
            ClipInfo = clipQueued;
            QueueID = clipQueued.QueueID;
            //OnPropertyChanged("RemoveCommand");
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

        public Color FrameColor
        {
            get
            {
                return User.GetColor();
            }
        }
        public bool DeleteVisibility { get; set; } = false;

        public uint QueueID { get; set; }

        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(null, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var item = obj as ClipData;

            if (item == null)
                return false;

            return this.QueueID == item.QueueID;
        }

        public override int GetHashCode()
        {
            return this.QueueID.GetHashCode();
        }
    }
}
