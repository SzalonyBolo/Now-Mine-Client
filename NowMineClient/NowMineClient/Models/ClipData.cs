using NowMineClient.Helpers;
using NowMineCommon.Models;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace NowMineClient.Models
{
    public class ClipData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ClipData(ClipQueued clipQueued)
        {
            var user = UserStore.Users.Where(u => u.Id == clipQueued.UserID).First();
            User = user;
            ClipInfo = clipQueued;
            QueueID = clipQueued.QueueID;
        }

        public ClipData(ClipInfo info, User user)
        {
            ClipInfo = info;
            User = user;
        }

        public ClipInfo ClipInfo { get; set; }

        public User User { get; set; }

        public string Title { get => ClipInfo.Title; }

        public string ChannelName { get => ClipInfo.ChannelName; }

        public string UserName { get => User.Name; }

        public int UserID { get => User.Id; }

        public Uri Image { get => ClipInfo.Thumbnail; }

        public Color FrameColor { get => User.UserColor; }

        public bool DeleteVisibility { get; set; } = false;

        public uint QueueID { get; set; }

        //override
        public override bool Equals(object obj)
        {
            ClipData item = obj as ClipData;

            if (item == null)
                return false;

            return QueueID == item.QueueID;
        }

        public override int GetHashCode()
        {
            return QueueID.GetHashCode();
        }
    }
}
