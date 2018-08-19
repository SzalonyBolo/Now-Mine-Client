using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMineClient
{
    public partial class MusicPiece : ContentView, INotifyPropertyChanged
    {

        private DateTime created { get; set; }
        private DateTime played { get; set; }
        private Color _frameColor;
        public Color FrameColor {
            get
            {
                if (_frameColor == null)
                {
                    _frameColor = Color.Black;
                }
                return _frameColor;
            }
            set
            {
                if (value != _frameColor)
                {
                    _frameColor = value;
                    //OnPropertyChanged("FrameColor");
                    this.MusicPieceFrame.BorderColor = _frameColor;
                }
            }
        }

        private YoutubeInfo _info;
        public YoutubeInfo Info
        {
            get { return _info; }
            set
            {
                _info = value;
                Title = _info.title;
                ChannelName = _info.channelName;
                //setImage = _info.thumbnail.url;
                //HeightRequest = 150;
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                lblTitle.Text = value;
            }
        }

        private string _channelName;
        public string ChannelName
        {
            get
            {
                return _channelName;
            }
            set
            {
                _channelName = value;
                lblChannelName.Text = value;
            }
        }

        private string setImage
        {
            set
            {

                //BitmapImage bmp = new BitmapImage(new Uri(value, UriKind.RelativeOrAbsolute));
                //imgMain.Source = bmp;
                imgMain.Source = ImageSource.FromUri(new Uri(value));
                if (imgMain.Height != -1)
                    HeightRequest = imgMain.Height;
                else
                    HeightRequest = 100;
            }
        }

        public MusicPiece()
        {
            InitializeComponent();
        }

        // to the SearchBar
        public MusicPiece(YoutubeInfo inf)
        {
            InitializeComponent();
            this._info = inf;
            lblTitle.Text = inf.title;
            lblChannelName.Text = inf.channelName;
            setImage = inf.thumbnail.Url;
            lbluserName.Text = inf.userName;
            this.MinimumHeightRequest = 100;
            //lbluserName.Text = Visibility.Hidden;
            created = DateTime.Now;
        }

        //to Queue
        //public MusicPiece(YoutubeInfo inf, User user)
        //{
        //    InitializeComponent();
        //    this._info = inf;
        //    lblTitle.Content = _info.title;
        //    lblChannelName.Content = _info.channelName;
        //    setImage = _info.thumbnail.Url;
        //    created = DateTime.Now;
        //    lbluserName.Content = user.name;
        //}

        public MusicPiece copy()
        {
            MusicPiece musicPiece = new MusicPiece();
            musicPiece.InitializeComponent();
            musicPiece._info = this._info;
            musicPiece.lblTitle.Text = _info.title;
            musicPiece.lblChannelName.Text = _info.channelName;
            musicPiece.setImage = _info.thumbnail.Url;
            musicPiece.created = DateTime.Now;
            musicPiece.lbluserName.Text = lbluserName.Text;
            musicPiece.FrameColor = FrameColor;
            return musicPiece;
        }

        //internal void nowPlayingVisual()
        //{
        //    SolidColorBrush redBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        //    this.border.BorderBrush = redBrush;
        //}

        //internal void userColorBrush(User user)
        //{
        //    SolidColorBrush userBrush = new SolidColorBrush(user.getColor());
        //    this.border.BorderBrush = userBrush;
        //    this.dropShadowEffect.Color = user.getColor();
        //    this.recBackground.Fill = userBrush;
        //    this.recBackground.Opacity = 0.3d;
        //}

        //internal void historyVisual()
        //{
        //    SolidColorBrush greyBrush = new SolidColorBrush(Color.FromRgb(111, 111, 111));
        //    this.border.BorderBrush = greyBrush;
        //    this.lblTitle.BorderBrush = greyBrush;
        //    this.lblChannelName.BorderBrush = greyBrush;
        //}

        public void setPlayedDate()
        {
            this.played = DateTime.Now;
        }
    }
}
