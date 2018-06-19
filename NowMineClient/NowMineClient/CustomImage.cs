using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMineClient
{
    public class CustomImage : Image
    {
        public event EventHandler Pressed;
        public event EventHandler Moved;
        public event EventHandler Released;

        public static readonly BindableProperty RGBProperty =
         BindableProperty.Create(propertyName: nameof(HEXValue),
             returnType: typeof(string),
             declaringType: typeof(CustomImage),
             defaultValue: "");

        public string HEXValue
        {
            get { return (string)GetValue(RGBProperty); }
            set { SetValue(RGBProperty, value); }
        }

        public virtual void OnPressed(string HEXValue)
        {
            this.HEXValue = HEXValue;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMoved(string HEXValue)
        {
            this.HEXValue = HEXValue;
            Moved?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnReleased(string HEXValue)
        {
            this.HEXValue = HEXValue;
            Released?.Invoke(this, EventArgs.Empty);
        }
    }
}
