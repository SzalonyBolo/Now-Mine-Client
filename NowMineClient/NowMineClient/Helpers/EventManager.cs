using NowMineClient.Models;
using NowMineCommon.Enums;
using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NowMineClient.Helpers
{
    public static class EventManager
    {
        public static uint ActualEventID { get; set; }

        public delegate void QueuedEventHandler(ClipData clip, int qPos);
        public static event QueuedEventHandler QueuedPiece;

        public delegate void DeletePieceEventHandler(uint queueID);
        public static event DeletePieceEventHandler DeletedPiece;

        public delegate void PlayedNowEventHandler(int qPos);
        public static event PlayedNowEventHandler PlayedNow;

        public static bool CheckCurrentEventID(uint eventID)
        {
            if (eventID == ActualEventID -1)
            {
                ActualEventID++;
                return true;
            }
            if (eventID == ActualEventID)
                return true;
            return false;
        }

        internal static void DoEvents(List<EventItem> eventList)
        {
            if (eventList == null || eventList.Count == 0)
            {
                return;
            }
            foreach(var e in eventList)
            {
                switch(e.commandType)
                {
                    case CommandType.QueueClip:
                        ClipQueued clip = e.Data as ClipQueued;
                        var user = User.Users.Find(u => u.Id == clip.UserID);
                        QueuedPiece?.Invoke(new ClipData(clip, user), clip.QPos);
                        break;

                    case CommandType.DeleteClip:
                        uint queueID = (uint)e.Data;
                        DeletedPiece?.Invoke(queueID);
                        break;

                    case CommandType.PlayNow:
                        int qPos = (int)e.Data;
                        PlayedNow?.Invoke(qPos);
                        break;

                    default:
                        Debug.WriteLine("Event Manager/ Cannot interpret event right...");
                        break;
                }
            }
            ActualEventID = eventList[eventList.Count - 1].EventID;
        }
    }
}
