using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NowMineClient.Models;
using NowMineClient.Views;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace NowMineClient.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeletePopup : PopupPage, INotifyPropertyChanged
    {
        public event EventHandler YesClickedEvent;
        public ClipData ClipToDelete { get; set;}
        SemaphoreSlim _semaphore;

        public DeletePopup(ClipData clipData, SemaphoreSlim _semaphorePopup)
		{
            InitializeComponent();
            ClipToDelete = clipData;
            _semaphore = _semaphorePopup;
            var clipView = new ClipControl();
            clipView.BindingContext = ClipToDelete;
            clipView.IconPanelVisibility = false;
            ClipGridRow.Children.Add(clipView);
        }

        protected void ClosePopup(object sender, EventArgs e)
        {
            Navigation.PopPopupAsync();
        }

        private void YesButtonClicked(object sender, EventArgs e)
        {
            YesClickedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            _semaphore.Release();
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
    }
}