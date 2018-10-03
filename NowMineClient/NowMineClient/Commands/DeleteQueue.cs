using NowMineClient.Models;
using NowMineClient.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace NowMineClient.Commands
{
    //class DeleteQueue : ICommand
    //{
    //    private ServerConnection serverConnection;
    //    public DeleteQueue(ServerConnection serverConnection)
    //    {
    //        this.serverConnection = serverConnection;
    //    }

    //    public event EventHandler CanExecuteChanged;

    //    public bool CanExecute(object parameter)
    //    {
    //        return true;
    //    }

    //    public async void Execute(object parameter)
    //    {
    //        var musicData = parameter as ClipData;
    //        bool responde = await serverConnection.SendDeletePiece(musicData);
    //        if (responde)
    //        {
    //            Queue.Remove(_toDelete);
    //            RenderQueue();
    //        }
    //        sltDeletePopup.IsVisible = false;
    //        sltQueue.IsVisible = true;
    //    }
    //}
}
