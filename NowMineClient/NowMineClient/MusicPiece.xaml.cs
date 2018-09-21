using NowMineClient.Models;
using NowMineCommon.Models;
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
                    this.MusicPieceFrame.BorderColor = _frameColor;
                    //this.MusicPieceFrame.BackgroundColor = _frameColor;
                    //this.MusicPieceFrame.OutlineColor = _frameColor;
                    OnPropertyChanged("FrameColor");
                    //OnPropertyChanged("MusicPieceFrame");
                    //OnPropertyChanged("BorderColor");
                }
            }
        }

        private ClipQueued _info;
        public ClipQueued Info
        {
            get { return _info; }
            set
            {
                _info = value;
                Title = _info.Title;
                ChannelName = _info.ChannelName;
                UserName = User.Users.Where(u => u.Id == _info.UserID).First().Name;


                setImage = _info.Thumbnail.ToString();
                //HeightRequest = 150;
            }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { userName = value;
                lbluserName.Text = userName;
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

        //todo
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
        private bool _deleteVisible = false;

        public bool DeleteVisible
        {
            get { return _deleteVisible; }
            set
            {
                _deleteVisible = value;
                btnDelete.IsVisible = _deleteVisible;
            }
        }


        public MusicPiece()
        {
            InitializeComponent();
        }

        // to the SearchBar
        public MusicPiece(ClipQueued inf)
        {
            InitializeComponent();
            this._info = inf;
            lblTitle.Text = inf.Title;
            lblChannelName.Text = inf.ChannelName;
            setImage = inf.Thumbnail.ToString();
            //lbluserName.Text = inf.userName;
            lbluserName.Text = User.Users.Where(u => u.Id == inf.UserID).First().Name;
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
            musicPiece.lblTitle.Text = _info.Title;
            musicPiece.lblChannelName.Text = _info.ChannelName;
            musicPiece.setImage = _info.Thumbnail.ToString();
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

        private void btnDelete_Clicked(object sender, EventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        public delegate void DeletePieceClicked(object o, EventArgs e);
        public event DeletePieceClicked DeleteClicked;
    }
}
