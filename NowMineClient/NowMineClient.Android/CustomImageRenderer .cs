using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using NowMineClient;
using NowMineClient.Droid;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using Android.Graphics.Drawables;
using NowMineClient.Models;

[assembly: ExportRenderer(typeof(CustomImage), typeof(CustomImageRenderer))]
namespace NowMineClient.Droid
{
#pragma warning disable 0618
    public class CustomImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Image> e)
        {
            base.OnElementChanged(e);

            var customImage = e.NewElement as CustomImage;

            var thisImage = Control as ImageView;

            thisImage.Touch += (object sender, TouchEventArgs args) =>
            {
                ImageView iv = (ImageView)sender;

                this.BuildDrawingCache(true);
                Bitmap bitmap = this.GetDrawingCache(true);
                if (bitmap == null)
                {
                    bitmap = iv.GetDrawingCache(true);
                    if (bitmap == null)
                    {
                        bitmap = ((BitmapDrawable)sender).Bitmap;
                    }
                }

                if (args.Event.Action == MotionEventActions.Down)
                {
                    customImage.OnPressed(getHexValue((int)args.Event.RawX, (int)args.Event.RawY, bitmap, customImage));
                }
                else if (args.Event.Action == MotionEventActions.Move)
                {
                    customImage.OnPressed(getHexValue((int)args.Event.RawX, (int)args.Event.RawY, bitmap, customImage));
                }
                else if (args.Event.Action == MotionEventActions.Up)
                {
                    customImage.OnPressed(getHexValue((int)args.Event.RawX, (int)args.Event.RawY, bitmap, customImage));
                }
            };
        }

        public string getHexValue(int rX, int rY, Bitmap bitmap, CustomImage customImage)
        {
            string hex = "#000000";
            int VisualObjectX = (int)this.GetX();
            int VisualObjectY = (int)this.GetY();
            //var scalex = this.ScaleX;
            //var scaley = this.ScaleY;
            //var top = this.Top;
            //var mheight = this.MeasuredHeightAndState;
            VisualObjectY += 80;
            //if (rX < (bitmap.Width + VisualObjectX) && rY < (bitmap.Height + VisualObjectY))
            if (rX - VisualObjectX < bitmap.Width && rY - VisualObjectY < bitmap.Height)
            {
                
                int color = bitmap.GetPixel(rX - VisualObjectX, rY - VisualObjectY);
                hex = $"#{Android.Graphics.Color.GetRedComponent(color):X2}{Android.Graphics.Color.GetGreenComponent(color):X2}{Android.Graphics.Color.GetBlueComponent(color):X2}";
            }
            else
            {
                hex = customImage.HEXValue;
            }

            return hex;
        }
    }
}