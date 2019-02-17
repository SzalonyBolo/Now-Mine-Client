using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NowMineClient.Droid.Renderers;
using NowMineClient.OSSpecific;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FontAwesomeIcon), typeof(FontAwesomeIconRenderer))]
namespace NowMineClient.Droid.Renderers
{
#pragma warning disable CS0618 // Type or member is obsolete
    class FontAwesomeIconRenderer : LabelRenderer
    {
       protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
    {
        base.OnElementChanged(e);
        if (e.OldElement == null)
        {
            //The ttf in /Assets is CaseSensitive, so name it FontAwesome.ttf
            Control.Typeface = Typeface.CreateFromAsset(Forms.Context.Assets, FontAwesomeIcon.Typeface + ".ttf");
        }
    }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}