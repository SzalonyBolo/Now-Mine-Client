using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NowMineClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClipControl : ContentView, INotifyPropertyChanged
    {
        public ClipControl()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty IconDeleteCommandProperty =
            BindableProperty.Create(
                propertyName: nameof(IconDeleteCommand),
                returnType: typeof(ICommand),
                declaringType: typeof(ClipControl),
                defaultValue: null);

        public ICommand IconDeleteCommand
        {
            get { return (ICommand)GetValue(IconDeleteCommandProperty); }
            set { SetValue(IconDeleteCommandProperty, value); }
        }

        public static readonly BindableProperty IconPanelVisibilityProperty =
            BindableProperty.Create(
                propertyName: nameof(IconPanelVisibility),
                returnType: typeof(bool),
                declaringType: typeof(ClipControl),
                defaultValue: false);

        public bool IconPanelVisibility {
            get { return (bool)GetValue(IconPanelVisibilityProperty); }
            set { SetValue(IconPanelVisibilityProperty, value); }
        }
    }
}