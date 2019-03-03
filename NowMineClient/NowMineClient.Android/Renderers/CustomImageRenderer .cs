using Android.Views;
using Android.Widget;
using Xamarin.Forms;
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
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
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
                float rawX = args.Event.RawX;
                float rawY = args.Event.RawY;
                //System.Diagnostics.Debug.WriteLine(string.Format("RawX: {0} RawY: {1}", rawX, rawY));
                if (args.Event.Action == MotionEventActions.Down)
                {
                    customImage.OnPressed(GetHexValue(rawX, rawY, bitmap, customImage));
                }
                else if (args.Event.Action == MotionEventActions.Move)
                {
                    customImage.OnPressed(GetHexValue(rawX, rawY, bitmap, customImage));
                }
                else if (args.Event.Action == MotionEventActions.Up)
                {
                    customImage.OnPressed(GetHexValue(rawX, rawY, bitmap, customImage));
                }
            };
        }

        public string GetHexValue(float rX, float rY, Bitmap bitmap, CustomImage customImage)
        {
            string hex = string.Empty;
            int[] locationScreen = new int[2];
            this.GetLocationOnScreen(locationScreen);
            try
            {
                if ((rX - locationScreen[0]) < bitmap.Width && (rY - locationScreen[1])< bitmap.Height)
                {
                    int color = bitmap.GetPixel((int)(rX - locationScreen[0]), (int)(rY - locationScreen[1]));
                    hex = $"#{Android.Graphics.Color.GetRedComponent(color):X2}{Android.Graphics.Color.GetGreenComponent(color):X2}{Android.Graphics.Color.GetBlueComponent(color):X2}";
                }
                else
                {
                    hex = customImage.HEXValue;
                }
            }
            catch(System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error in GetHexValue: {0}", e.Message));
            }
            return hex;
        }
    }
}