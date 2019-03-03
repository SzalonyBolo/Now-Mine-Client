using System.ComponentModel;
using Xamarin.Forms;

namespace NowMineClient.Models
{
    public class User : NowMineCommon.Models.BaseUser, INotifyPropertyChanged
    {
        public Color UserColor
        {
            get
            {
                return Color.FromRgb(ColorBytes[0], ColorBytes[1], ColorBytes[2]);
            }
        }

        protected override void OnPropertyChanged(string propName)
        {
            base.OnPropertyChanged(propName);
            if (propName.Equals(nameof(ColorBytes)))
                OnPropertyChanged(nameof(UserColor));
        }
    }
}
