using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using NowMineCommon;

namespace NowMineClient.Models
{
    public class User : NowMineCommon.Models.BaseUser
    {
        public static User DeviceUser { get; set; } = null;

        private static List<User> _users;
        public static List<User> Users
        {
            get
            {
                if (_users == null)
                    _users = new List<User>();
                return _users.Concat(new List<User>() { DeviceUser }).ToList();
            }

            set
            {
                _users = value;
            }
        }

        public static void InitializeDeviceUser(int id)
        {
            User user = new User();
            user.Id = id;
            if (Application.Current.Properties.ContainsKey("UserColor"))
            {

                var color = Application.Current.Properties["UserColor"] as byte[];
                if (color.Length == 3)
                    user.UserColor = color;
            }
            if (Application.Current.Properties.ContainsKey("UserName"))
            {
                var name = Application.Current.Properties["UserName"] as string;
                if (!string.IsNullOrEmpty(name))
                    user.Name = name;
            }
            else
            {
                user.Name = Device.Idiom.ToString();
            }
            DeviceUser = user;
        }

        internal Color GetColor()
        {
            return Color.FromRgb(UserColor[0], UserColor[1], UserColor[2]);
        }
    }
}
