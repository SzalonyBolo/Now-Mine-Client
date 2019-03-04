using NowMineClient.Models;
using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NowMineClient.Network
{
    interface IServerConnection
    {
        event EventHandler ServerConnected;
        event Action<ClipData, int> UDPQueued;
        event Action<uint> DeletePiece;
        event Action<int> PlayedNow;
        event EventHandler RenderQueue;
        Task<bool> ListenAndCallServer();
        void StartListeningUDP();
        Task<bool> ChangeName(string newUserName);
        Task<bool> ChangeColor(byte[] newColor);
        Task<bool> SendDeletePiece(ClipData clipData);
        Task<IList<User>> GetUsers();
        Task<IList<ClipQueued>> GetQueue();
        Task<int> SendToQueue(ClipData data);
        Task<bool> SendPlayNext();
    }
}
