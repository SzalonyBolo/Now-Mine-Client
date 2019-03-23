using NowMineClient.Helpers;
using NowMineClient.Models;
using NowMineClient.Network;
using NowMineClient.OSSpecific;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserConfigPage : ContentPage
    {
        private readonly IServerConnection serverConnection = DependencyService.Get<IServerConnection>();

        public UserConfigPage()
        {
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            BoxColorPicker.Color = UserStore.DeviceUser.UserColor;
            ChangeNameEntry.Text = UserStore.DeviceUser.Name;

            var colorPickerImage = new CustomImage
            {
                Source = "Resources/colorpicker.png",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Aspect = Aspect.AspectFit
            };
            colorPickerImage.Pressed += BoxColorPickerColorChange;
            colorPickerImage.Moved += BoxColorPickerColorChange;
            colorPickerImage.Released += BoxColorPickerColorChange;
            colorPickerImage.SizeChanged += (o, e) => { ColorPickerRow.HeightRequest = colorPickerImage.Height; }; //Workournd for image row size bug (bugzilla #55294)
            ColorPickerRow.Children.Add(colorPickerImage);

            BoxColorPicker.Color = UserStore.DeviceUser.UserColor;
        }
        private void BoxColorPickerColorChange(object sender, EventArgs args)
        {
            CustomImage ci = (CustomImage)sender;
            string hexColor = ci.HEXValue;

            if (!string.IsNullOrEmpty(hexColor))
            {
                BoxColorPicker.Color = Color.FromHex(hexColor);
            }
        }

        private async void ColorChange_BtnClicked(object sender, EventArgs e)
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
                UserStore.DeviceUser.ColorBytes = ColorsBytes;
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
            if (newUserName.Equals(UserStore.DeviceUser.Name))
                return;
            bool isLegal = await serverConnection.ChangeName(newUserName);
            Debug.WriteLine("New User Name Accepted: {0}", isLegal);
            if (isLegal)
            {
                entry.TextColor = Color.Green;
                UserStore.DeviceUser.Name = newUserName;
                Application.Current.Properties["UserName"] = newUserName;
                await Application.Current.SavePropertiesAsync();
                DependencyService.Get<IMessage>().LongAlert("Zmieniono nick");
            }
            else
            {
                entry.Text = UserStore.DeviceUser.Name;
                entry.TextColor = Color.Red;
                DependencyService.Get<IMessage>().LongAlert("Nie udało się zmienić nick");
            }
        }
    }
}
