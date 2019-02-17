//using NControl.Abstractions;
//using NGraphics;
using NowMineClient.Models;
using NowMineClient.Network;
using NowMineClient.OSSpecific;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserConfigPage : ContentPage
    {
        ServerConnection serverConnection;
        public UserConfigPage(ServerConnection serverConnection)
        {
            InitializeComponent();
            this.serverConnection = serverConnection;
            this.Title = "Kolejkujący";
            //var colorPicker = new ColorPicker();
            //stlConfigLayout.Children.Add(colorPicker);
            //ChangeNameEntry.Text = User.DeviceUser.Name;
            //BoxColorPicker.HeightRequest = 60;
            //BoxColorPicker.WidthRequest = this.Width - 40;
            var colorPickerImage = new CustomImage
            {
                Source = "Resources/colorpicker.png",
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand
            };
            colorPickerImage.Pressed += (sender, args) =>
            {
                CustomImage ci = (CustomImage)sender;
                string hexColor = ci.HEXValue;

                if (hexColor != "")
                {
                    BoxColorPicker.Color = Color.FromHex(hexColor);
                }
            };
            colorPickerImage.Moved += (sender, args) =>
            {
                CustomImage ci = (CustomImage)sender;
                string hexColor = ci.HEXValue;

                if (hexColor != "")
                {
                    BoxColorPicker.Color = Color.FromHex(hexColor);
                }
            };
            colorPickerImage.Released += (sender, args) =>
            {
                CustomImage ci = (CustomImage)sender;
                string hexColor = ci.HEXValue;

                if (hexColor != "")
                {
                    BoxColorPicker.Color = Color.FromHex(hexColor);
                }
            };
            sltConfigLayout.Children.Add(colorPickerImage);

            var BtnSubmit = new Button();
            BtnSubmit.HeightRequest = 40;
            BtnSubmit.WidthRequest = 80;
            BtnSubmit.Text = "Wyślij";
            BtnSubmit.Clicked += BtnSubmit_Clicked;

            BoxColorPicker.Color = User.DeviceUser.GetColor();

            sltConfigLayout.Children.Add(BtnSubmit);
        }

        private async void BtnSubmit_Clicked(object sender, EventArgs e)
        {
            var Color = BoxColorPicker.Color;
            var ColorsBytes = new byte[3];
            ColorsBytes[0] = (byte)(Color.R * 255);
            ColorsBytes[1] = (byte)(Color.G * 255);
            ColorsBytes[2] = (byte)(Color.B * 255);
            var response = await serverConnection.ChangeColor(ColorsBytes);
            if (response)
            {
                Application.Current.Properties["UserColor"] = ColorsBytes;
                await Application.Current.SavePropertiesAsync();
                DependencyService.Get<IMessage>().LongAlert("Zmieniono kolor");
            }
            else
            {
                DependencyService.Get<IMessage>().LongAlert("Nie udało sie zmienić koloru");
            }
        }

        private async void Entry_Completed(object sender, EventArgs e)
        {
            Entry entry = (Entry)sender;
            string newUserName = entry.Text;
            bool isLegal = await serverConnection.ChangeName(newUserName);
            Debug.WriteLine("New User Name Accepted: {0}", isLegal);
            if (isLegal)
            {
                entry.TextColor = Xamarin.Forms.Color.Green;
                User.DeviceUser.Name = newUserName;
                Application.Current.Properties["UserName"] = newUserName;
                await Application.Current.SavePropertiesAsync();
                DependencyService.Get<IMessage>().LongAlert("Zmieniono nick");
            }
            else
            {
                entry.Text = User.DeviceUser.Name;
                entry.TextColor = Xamarin.Forms.Color.Red;
                DependencyService.Get<IMessage>().LongAlert("Nie udało się zmienić nick");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BoxColorPicker.Color = User.DeviceUser.GetColor();
            ChangeNameEntry.Text = User.DeviceUser.Name;
        }
    }
}
