using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using NowMineClient.Models;
using NowMineClient.Views;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeletePopup : PopupPage, INotifyPropertyChanged
    {
        ICommand DeleteYesCommand;
        ICommand DeleteNoCommand;

        public event EventHandler YesClickedEvent;
        public ClipData ClipToDelete { get; set;}

        public DeletePopup(ClipData clipData)
		{
            InitializeComponent();
            ClipToDelete = clipData;
            DeleteNoCommand = new Command(ClosePopup);
            DeleteYesCommand = new Command(OnYesClickedEvevent);
            OnPropertyChanged("DeleteNoCommand");
            OnPropertyChanged("DeleteYesCommand");
            //OnPropertyChanged("ClipToDelete");
            var clipView = new ClipControl();
            ClipToDelete.DeleteVisibility = false;
            clipView.BindingContext = ClipToDelete;
            ClipGridRow.Children.Add(clipView);
        }

        protected void OnYesClickedEvevent()
        {
            YesClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected void ClosePopup()
        {
            Navigation.PopPopupAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ### Methods for supporting animations in your popup page ###

        // Invoked before an animation appearing
        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
        }

        // Invoked after an animation appearing
        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        // Invoked before an animation disappearing
        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        // Invoked after an animation disappearing
        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
        }

        protected override Task OnAppearingAnimationBeginAsync()
        {
            return base.OnAppearingAnimationBeginAsync();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return base.OnAppearingAnimationEndAsync();
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return base.OnDisappearingAnimationBeginAsync();
        }

        protected override Task OnDisappearingAnimationEndAsync()
        {
            return base.OnDisappearingAnimationEndAsync();
        }

        // ### Overrided methods which can prevent closing a popup page ###

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            return base.OnBackButtonPressed();
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked
            return base.OnBackgroundClicked();
        }

        private void YesButtonClicked(object sender, EventArgs e)
        {
            OnYesClickedEvevent();
        }

        private void NoButtonClicked(object sender, EventArgs e)
        {
            ClosePopup();
        }
    }
}