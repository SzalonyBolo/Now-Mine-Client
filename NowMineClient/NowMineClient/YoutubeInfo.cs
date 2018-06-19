﻿using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowMineClient
{
    public class YoutubeInfo
    { 
        public string id;
        public string title;
        public string channelName;
        public Thumbnail thumbnail;
        public string userName;
        public int userId;
        public string color;
    }

    public class MusicData : YoutubeInfo
    {
        public int qPos;
    }
    //public class Thumbnail
    //{
    //    public string ETag;
    //    public long height;
    //    public string url;
    //    public long width;
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
        public MusicPiece MusicPiece { get; set; }
        public int QPos { get; set; }
        public PiecePosArgs(MusicPiece musicPiece, int qPos)
        {
            this.MusicPiece = musicPiece;
            MusicPiece.GestureRecognizers.Clear();
            this.QPos = qPos;
        }
    }
}
