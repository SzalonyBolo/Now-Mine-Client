using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using NowMineClient;
using Android.Net;
using NowMineClient.Droid;
using Android.Util;
using Android.Net.Wifi;
using Java.Net;


[assembly: Dependency(typeof(NetworkConnection))]
namespace NowMineClient.Droid
{
    public class NetworkConnection : INetworkConnection
    {
        public bool IsConnected { get; set; }
        public void CheckNetworkConnection()
        {
            var connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
            {
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }
        }

        public string GetBroadcastAddress()
        {

            Log.Debug("UDP", "Getting broadcast adress");
            WifiManager wifi = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            DhcpInfo dhcp = wifi.DhcpInfo;
            // handle null somehow

            int broadcast = (dhcp.IpAddress & dhcp.Netmask) | ~dhcp.Netmask;
            byte[] quads = new byte[4];
            for (int k = 0; k < 4; k++)
                quads[k] = (byte)((broadcast >> k * 8) & 0xFF);
            Log.Debug("UDP", "Adrress is: " + InetAddress.GetByAddress(quads));
            return InetAddress.GetByAddress(quads).ToString();
        }
    }
}