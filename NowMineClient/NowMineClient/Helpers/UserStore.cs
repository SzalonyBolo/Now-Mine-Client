using NowMineClient.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace NowMineClient.Helpers
{
    class UserStore
    {
        public static User DeviceUser { get; set; } = null;

        private static ObservableCollection<User> _users;
        public static ObservableCollection<User> Users
        {
            get
            {
                if (_users == null)
                    _users = new ObservableCollection<User>();
                if (!_users.Contains(DeviceUser))
                    _users.Add(DeviceUser);
                return _users;
            }

            set
            {
                var newUsers = value;
                var devUser = newUsers.Where(u => u.Id == DeviceUser.Id).First();
                newUsers.Remove(devUser);
                newUsers.Add(DeviceUser);
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
                    user.ColorBytes = color;
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
    }
}
