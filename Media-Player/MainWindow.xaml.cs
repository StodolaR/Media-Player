using Microsoft.Win32;
using System.IO;
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
    public partial class MainWindow : Window
    {
        private int slideshowInterval;
        private int countdown;
        private bool positionAdjustment;
        private int playlistIndex;
        private List<string> mediaPlaylist;
        private List<string> picturesPlaylist;
        public const string videoExtensions = "mov;mp4;avi;wmv";
        public const string audioExtensions = "mp3";
        public const string pictureExtensions = "jpg;png;gif;tif;bmp";
        public MainWindow()
        {
            InitializeComponent();
            mediaPlaylist = new List<string>();
            picturesPlaylist = new List<string>();
            mePlayer.Volume = slVolume.Value;
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
                if(countdown == 0)
                {
                    countdown = slideshowInterval;
                    if(lbPreviews.SelectedIndex < lbPreviews.Items.Count - 1)
                    {
                        lbPreviews.SelectedIndex++;
                        CombineFilenames(IOPath.GetFileName((string)lbPreviews.SelectedItem));
                    }
                    else
                    {
                        countdown = slideshowInterval = 0;
                        btnSlideshow.IsChecked = false;
                        return;
                    }
                }
                countdown--;
            }
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
                    SetPanelAndPlay(ofd.FileName);
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
                        SetPanelAndPlay(ofd.FileName);
                    }
                    else
                    {
                        picturesPlaylist.Clear();
                        lbPreviews.ItemsSource = picturesPlaylist;
                        lbPreviews.Items.Refresh();
                        imPicture.Source = new BitmapImage(new Uri(ofd.FileName));
                        CombineFilenames(IOPath.GetFileName(ofd.FileName));
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
        private void SetPanelAndPlay(string fileName)
        {
            btnPause.IsChecked = false;
            btnPause.IsEnabled = true;
            mePlayer.Source = new Uri(mediaPlaylist[0]);
            CombineFilenames(IOPath.GetFileName(fileName));
            playlistIndex = 0;
            slProgress.IsEnabled = true;
            slProgress.Value = 0;
            mePlayer.Play();
        }
        private void CombineFilenames(string fileName)
        {
            if (imPicture.Source != null && mePlayer.Source != null)
            {
                tbFilename.Text = IOPath.GetFileName(mePlayer.Source.ToString()) + "/" + IOPath.GetFileName(imPicture.Source.ToString());
            }
            else
            {
                tbFilename.Text = fileName;
            }
        }
        private void btnAudio_Checked(object sender, RoutedEventArgs e)
        {
            if (mediaPlaylist == null) return;
            MediaChangeReset();
        }

        private void btnVideo_Checked(object sender, RoutedEventArgs e)
        {
            spSlideshow.Visibility = Visibility.Collapsed;
            picturesPlaylist.Clear();
            lbPreviews.Items.Refresh();
            imPicture.Source = null;
            MediaChangeReset();
        }
        private void MediaChangeReset()
        {
            mePlayer.Source = null;
            btnPause.IsChecked = false;
            btnPause.IsEnabled = false;
            slProgress.Value = 0;
            slProgress.IsEnabled = false;
            playlistIndex = 0;
            mediaPlaylist.Clear();
            tbProgressTime.Text = "0:00:00/0:00:00";
            tbFilename.Text = "Nazev souboru";
            
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

        private void SlVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mePlayer.Volume = slVolume.Value;
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
            tbProgressTime.Text = TimeSpan.FromSeconds(slProgress.Value).ToString(@"h\:mm\:ss") + "/" + TimeSpan.FromSeconds(slProgress.Maximum).ToString(@"h\:mm\:ss");
        }

        private void BtnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (btnVideo.IsChecked != null)
            {
                PlaylistWindow playlistWindow = new PlaylistWindow((bool)btnVideo.IsChecked);
                playlistWindow.Closing += PlaylistWindow_Closing;
                if (playlistWindow.ShowDialog() == true)
                {
                    if(playlistWindow.FinalResult.Last() == "(obrázky)")
                    {
                        playlistWindow.FinalResult.Remove(playlistWindow.FinalResult.Last());
                        picturesPlaylist = playlistWindow.FinalResult;
                        if(picturesPlaylist.Count > 1)
                        {
                            spSlideshow.Visibility = Visibility.Visible;
                            lbPreviews.ItemsSource = picturesPlaylist;
                            lbPreviews.SelectedIndex = 0;
                            lbPreviews.Visibility = Visibility.Visible;
                            tbPicturesCount.Text = picturesPlaylist.Count + "x "; 
                        }
                        else
                        {
                            spSlideshow.Visibility = Visibility.Collapsed;
                            lbPreviews.Visibility = Visibility.Collapsed;
                        }
                        imPicture.Source = new BitmapImage(new Uri(picturesPlaylist[0]));
                        CombineFilenames(IOPath.GetFileName(picturesPlaylist[0]));
                    }
                    else
                    {
                        playlistWindow.FinalResult.Remove(playlistWindow.FinalResult.Last());
                        mediaPlaylist = playlistWindow.FinalResult;
                        SetPanelAndPlay(mediaPlaylist[0]);
                    }
                }
            }  
        }

        private void PlaylistWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender != null)
            {
                ((PlaylistWindow)sender).SerializePlaylists();
            }
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
            if ((playlistIndex) == limitPosition) return;
            playlistIndex += increment;
            mePlayer.Source = new Uri(mediaPlaylist[playlistIndex]);
            slProgress.Value = 0;
            CombineFilenames(IOPath.GetFileName(mediaPlaylist[playlistIndex]));
        }

        private void LbPreviews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbPreviews.SelectedItem != null)
            {
                imPicture.Source = new BitmapImage(new Uri((string)lbPreviews.SelectedItem));
                CombineFilenames(IOPath.GetFileName((string)lbPreviews.SelectedItem));
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
                    btnSlideshow.IsEnabled = false;
                    slideshowInterval = 0;
                }
            }
            else
            {
                tbxDelay.IsEnabled = true;
                slideshowInterval = 0;
            }
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            dpControlPanel.Visibility = Visibility.Collapsed;
            btnShowPanel.Visibility = Visibility.Visible;
        }

        private void BtnShowPanel_Click(object sender, RoutedEventArgs e)
        {
            dpControlPanel.Visibility=Visibility.Visible;
            btnShowPanel.Visibility=Visibility.Collapsed;
        }

        private void tbxDelay_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbxDelay.Text = string.Empty;
        }

        private void tbxDelay_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(tbxDelay.Text == string.Empty)
            {
                tbxDelay.Text = "5";
            }
        }
    }
}