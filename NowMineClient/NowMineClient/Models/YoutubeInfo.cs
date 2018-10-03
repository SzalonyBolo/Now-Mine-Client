using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowMineClient.Models
{
    //public class YoutubeInfo
    //{ 
    //    public string id;
    //    public string title;
    //    public string channelName;
    //    public Thumbnail thumbnail;
    //    public string userName;
    //    public int userId;
    //    //public string color;
    //}

    //public class ClipData : YoutubeInfo
    //{
    //    public int qPos;
    //}

    public class GenericEventArgs<T> : EventArgs
    {
        public T EventData { get; private set; }

        public GenericEventArgs(T EventData)
        {
            this.EventData = EventData;
        }
    }

    public class PiecePosArgs : EventArgs
    {
        public ClipData ClipData { get; set; }
        public int QPos { get; set; }
        public PiecePosArgs(ClipData clipData, int qPos)
        {
            this.ClipData = clipData;
            //ClipData.GestureRecognizers.Clear();
            this.QPos = qPos;
        }
    }
}
