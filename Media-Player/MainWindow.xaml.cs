using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IOPath = System.IO.Path;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private LinearGradientBrush playlistBG;
        private int slideshowInterval;
        private int countdown;
        private bool positionAdjustment;
        private int mediaIndex;
        private int pictureIndex;
        private int MediaIndex
        {
            get => mediaIndex;
            set
            {
                mediaIndex = value;
                MediaIndexChanged();
            }
        }
        public int PictureIndex
        {
            get => pictureIndex;
            set
            {
                pictureIndex = value;
                PictureIndexChanged();
                OnPropertyChanged(nameof(PictureIndex));
            }
        }
        private List<string> mediaPlaylist;
        private List<string> picturePlaylist;
        MessageBoxResult continueAfterError = 0;
        public const string videoExtensions = "mov;mp4;avi;wmv";
        public const string audioExtensions = "mp3"; // přípona použita jako podmínka v metodě BtnOpen_Click !
        public const string pictureExtensions = "jpg;png;gif;tif;bmp";
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            playlistBG = (LinearGradientBrush)FindResource("playlistBackground");
            mediaPlaylist = new List<string>();
            picturePlaylist = new List<string>();
            mePlayer.Volume = slVolume.Value;
            DataContext = this;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if(mePlayer.Source !=null && mePlayer.NaturalDuration.HasTimeSpan && !positionAdjustment)
            {
                slProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                slProgress.Value = mePlayer.Position.TotalSeconds;
            }
            if(slideshowInterval > 0)
            {
                if(countdown < 1)
                {
                    countdown = slideshowInterval;
                    if(PictureIndex < picturePlaylist.Count - 1)
                    {
                        PictureIndex++;
                    }
                    else
                    {
                        StopSlideshow();
                        return;
                    }
                }
                countdown--;
            }
            continueAfterError = 0;
        }

        private void StopSlideshow()
        {
            countdown = slideshowInterval = 0;
            btnSlideshow.IsChecked = false;
            tbxDelay.IsEnabled = true;
        }

        private void btnAudio_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaPlaylist == null) return;
            MediaChangeReset();
        }

        private void btnVideo_Checked(object sender, RoutedEventArgs e)
        {
            PicturesPreviewReset();
            MediaChangeReset();
        }
        
        private void MediaChangeReset()
        {
            mePlayer.Source = null;
            MediaControlsReset();
            mediaIndex = -1;
            mediaPlaylist.Clear();
            tbProgressTime.Text = "0:00:00/0:00:00";
            CombineFilenames();
        }

        private void MediaControlsReset()
        {
            btnPause.IsChecked = false;
            btnPause.IsEnabled = false;
            slProgress.Value = 0;
            slProgress.IsEnabled = false;
        }

        private void PicturesPreviewReset()
        {
            StopSlideshow();
            spSlideshow.Visibility = Visibility.Collapsed;
            pictureIndex = -1;
            picturePlaylist.Clear();
            lbPreviews.Items.Refresh();
            lbPreviews.Visibility = Visibility.Collapsed;
            imPicture.Source = null;
            CombineFilenames();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (btnVideo.IsChecked == true )
            {
                ofd.Filter = GetFilterString(videoExtensions);
                if(ofd.ShowDialog() == true)
                {
                    mediaPlaylist.Add(ofd.FileName);
                    SetPanelAndPlay();
                    mediaPlaylist.Clear();
                }
            }
            else
            {
                ofd.Filter = GetFilterString(audioExtensions) + "|" + GetFilterString(pictureExtensions);
                if( ofd.ShowDialog() == true )
                {
                    if(ofd.FileName.EndsWith("mp3"))
                    {
                        mediaPlaylist.Add(ofd.FileName);
                        SetPanelAndPlay();
                        mediaPlaylist.Clear();
                    }
                    else
                    {
                        PicturesPreviewReset();
                        TryShowPicture(ofd.FileName);
                    }
                }
            }
        }

        public static string GetFilterString(string extensions)
        {
            string[] extensionsArray = extensions.Split(';');
            string filterString = string.Empty;
            foreach (string extension in extensionsArray)
            {
                filterString += "*." + extension + ";";
            }
            filterString = filterString.Trim(';');
            return filterString + "|" + filterString;
        }

        private void BtnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (btnVideo.IsChecked != null)
            {
                PlaylistWindow playlistWindow = new PlaylistWindow((bool)btnVideo.IsChecked, playlistBG);
                playlistWindow.Closing += PlaylistWindow_Closing;
                if (playlistWindow.ShowDialog() == true)
                {
                    if (playlistWindow.FinalResult.Last() == "(obrázky)")
                    {
                        PicturesPreviewReset();
                        playlistWindow.FinalResult.Remove(playlistWindow.FinalResult.Last());
                        picturePlaylist = playlistWindow.FinalResult;
                        lbPreviews.ItemsSource = picturePlaylist;
                        if (picturePlaylist.Count > 1)
                        {
                            spSlideshow.Visibility = Visibility.Visible;
                            PictureIndex = 0;
                            lbPreviews.Visibility = Visibility.Visible;
                            tbPicturesCount.Text = picturePlaylist.Count + "x ";
                        }
                        if (picturePlaylist.Count == 1)
                        {
                            TryShowPicture(picturePlaylist[0]);
                        }
                    }
                    else
                    {
                        playlistWindow.FinalResult.Remove(playlistWindow.FinalResult.Last());
                        mediaPlaylist = playlistWindow.FinalResult;
                        if (mediaPlaylist.Count > 0)
                        {
                            SetPanelAndPlay();
                        }
                    }
                }
            }
        }

        private void SetPanelAndPlay()
        {
            btnPause.IsChecked = false;
            btnPause.IsEnabled = true;
            slProgress.IsEnabled = true;
            MediaIndex = 0;
            mePlayer.Play();
        }

        private void PlaylistWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender != null)
            {
                ((PlaylistWindow)sender).SerializePlaylists();
            }
        }

        private void TryShowPicture(string picturePath)
        {
            try
            {
                imPicture.Source = new BitmapImage(new Uri(picturePath));
                CombineFilenames();
            }
            catch
            {
                ShowErrorMessage(true, picturePath, picturePlaylist, PictureIndex, StopSlideshow, PicturesPreviewReset);
            }
        }

        private void mePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string mediaPath = mePlayer.Source.ToString();
            mePlayer.Source = null;
            CombineFilenames();
            ShowErrorMessage(false, mediaPath, mediaPlaylist, MediaIndex, MoveMediaToEnd, MediaChangeReset);
        }

        private void ShowErrorMessage(bool isPicture, string path, List<string> playlist,int index, Action endPlayback, Action reset)
        {
            if (playlist.Count == 0)
            {
                MessageBox.Show("Soubor poškozen, nelze spustit..." + Environment.NewLine + path);
                reset();
            }
            else if (index == playlist.Count - 1)
            {
                MessageBox.Show("Poslední položka playlistu nenalezena či nelze spustit..." + Environment.NewLine + path);
                playlist.RemoveAt(index);
                lbPreviews.Items.Refresh();
                SetOriginalIndex(isPicture, playlist.Count - 1);
                endPlayback();
            }
            else
            {
                if (continueAfterError == 0)
                {
                    continueAfterError = MessageBox.Show("Některé z položek Playlistu nenalezeny či je nelze spustit..."
                                                        + Environment.NewLine + $"Od: {path}" + Environment.NewLine
                                                        + "Přejete si pokračovat?", "Chyba spuštění", MessageBoxButton.YesNo);
                    if (continueAfterError == MessageBoxResult.No)
                    {
                        reset();
                        continueAfterError = 0;
                        return;
                    }
                }
                playlist.RemoveAt(index);
                lbPreviews.Items.Refresh();
                SetOriginalIndex(isPicture, index);
            }
        }

        private void CombineFilenames()
        {
            if (mePlayer.Source != null)
            {
                if (imPicture.Source != null)
                {
                    tbFilename.Text = IOPath.GetFileName(mePlayer.Source.ToString()) + "/" + IOPath.GetFileName(imPicture.Source.ToString());
                }
                else
                {
                    tbFilename.Text = IOPath.GetFileName(mePlayer.Source.ToString());
                }
            }
            else if (imPicture.Source != null)
            {
                tbFilename.Text = IOPath.GetFileName(imPicture.Source.ToString());
            }
            else
            {
                tbFilename.Text = "Název souboru";
            }
        }

        private void SetOriginalIndex(bool isPicture, int Value)
        {
            if (isPicture)
            {
                PictureIndex = Value;
            }
            else
            {
                MediaIndex = Value;
            }
        }

        private void BtnPause_Checked(object sender, RoutedEventArgs e)
        {
            imPauseContent.Source = new BitmapImage(new Uri(@"/Resources/Images/Play.png", UriKind.Relative));
            mePlayer.Pause();
        }

        private void BtnPause_Unchecked(object sender, RoutedEventArgs e)
        {
            imPauseContent.Source = new BitmapImage(new Uri(@"/Resources/Images/Pause.png", UriKind.Relative));
            mePlayer.Play();
        }

        private void NextPlaylistItem(object sender, RoutedEventArgs e)
        {
            ChangePlaylistItem(mediaPlaylist.Count - 1, 1);
        }

        private void LastPlaylistItem(object sender, RoutedEventArgs e)
        {
            ChangePlaylistItem(0, -1);
        }

        private void ChangePlaylistItem(int limitPosition, int increment)
        {
            if (MediaIndex == limitPosition || MediaIndex == -1) return;
            MediaIndex += increment;
        }

        private void MediaIndexChanged()
        {
            if (MediaIndex >= 0)
            {
                mePlayer.Source = new Uri(mediaPlaylist[mediaIndex]);
                CombineFilenames();
            }
            else
            {
                MediaControlsReset();
            }
        }

        private void MoveMediaToEnd()
        {
            if (MediaIndex >= 0)
            {
                slProgress.Value = slProgress.Maximum - 0.1;
            }
            else
            {
                MediaChangeReset();
            }
        }

        private void SlProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            positionAdjustment = true;
        }

        private void SlProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(slProgress.Value);
            positionAdjustment = false;
        }

        private void slProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!positionAdjustment)
            {
                mePlayer.Position = TimeSpan.FromSeconds(slProgress.Value);
            }
            tbProgressTime.Text = TimeSpan.FromSeconds(slProgress.Value).ToString(@"h\:mm\:ss") + "/" 
                                + TimeSpan.FromSeconds(slProgress.Maximum).ToString(@"h\:mm\:ss");
        }

        private void SlVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mePlayer.Volume = slVolume.Value;
        }

        private void PictureIndexChanged()
        {
            if (PictureIndex >= 0)
            {
                TryShowPicture(picturePlaylist[pictureIndex]);
            }
        }

        private void TbxDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (tbxDelay.Text != "")
            {
                e.Handled = true;
                return;
            }
            Regex delay = new Regex("[^1-9]");
            e.Handled = delay.IsMatch(e.Text);
        }

        private void tbxDelay_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbxDelay.Text = string.Empty;
        }

        private void tbxDelay_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (tbxDelay.Text == string.Empty)
            {
                tbxDelay.Text = "5";
            }
        }

        private void BtnSlideshow_Click(object sender, RoutedEventArgs e)
        {
            if (btnSlideshow.IsChecked !=null && btnSlideshow.IsChecked == true)
            {
                if (int.TryParse(tbxDelay.Text, out slideshowInterval))
                {
                    tbxDelay.IsEnabled = false;
                }
                else
                {
                    tbxDelay.IsEnabled = true;
                    btnSlideshow.IsChecked = false;
                    slideshowInterval = 0;
                    tbxDelay.Text = "5";
                }
            }
            else
            {
                tbxDelay.IsEnabled = true;
                slideshowInterval = 0;
            }
        }

        private void HideOrShowPanel(object sender, RoutedEventArgs e)
        {
            Visibility backupVisibility = dpControlPanel.Visibility;
            dpControlPanel.Visibility = btnShowPanel.Visibility;
            btnShowPanel.Visibility = backupVisibility;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ChangeColors(object sender, RoutedEventArgs e)
        {
            AppColors appColors = new AppColors((SolidColorBrush)((Button)sender).Background);
            LinearGradientBrush leftBGResource = (LinearGradientBrush)FindResource("leftSideBackground");
            leftBGResource.GradientStops.Clear();
            foreach (GradientStop stop in appColors.leftBG)
            {
                leftBGResource.GradientStops.Add(stop);
            }
            LinearGradientBrush rightBGResource = (LinearGradientBrush)FindResource("rightSideBackground");
            rightBGResource.GradientStops.Clear();
            foreach (GradientStop stop in appColors.rightBG)
            {
                rightBGResource.GradientStops.Add(stop);
            }
            playlistBG.GradientStops.Clear();
            foreach (GradientStop stop in appColors.playlistBG)
            {
                playlistBG.GradientStops.Add(stop);
            }
        }
    }
}