using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMineClient
{
    class User
    {
        public string Name { get; set; }
        public int Id { get; set; }
        private byte[] _color;
        public byte[] UserColor
        {
            get
            {
                if (_color == null)
                {
                    _color = new byte[3];
                    Random random = new Random();
                    random.NextBytes(_color);
                }
                    
                return _color;
            }
            set
            {
                _color = value;
            }
        }

        public Color getColor()
        {
            return Color.FromRgb(UserColor[0], UserColor[1], UserColor[2]);
        }
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
            //Random rnd = new Random();
            //for (int i = 3; i < 3; i++)
            //{
            //    user.UserColor[i] = (byte)rnd.Next(0, 255);
            //}
            user.Name = Device.Idiom.ToString();
            DeviceUser = user;
        }
    }
}
