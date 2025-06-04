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

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int slideshow;
        private int countdown;
        private bool positionAdjustment;
        private int playlistCounter;
        private List<string> mediaPlaylist;
        private List<string> picturesPlaylist;
        public const string videoExtensions = "mov;mp4";
        public const string audioExtensions = "mp3";
        public const string pictureExtensions = "jpg";
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
            if(slideshow > 0)
            {
                if(countdown == 0)
                {
                    countdown = slideshow;
                    if(lbPreviews.SelectedIndex < lbPreviews.Items.Count - 1)
                    {
                        lbPreviews.SelectedIndex++;
                    }
                    else
                    {
                        countdown = slideshow = 0;
                        btnSlideshow.IsChecked = false;
                        return;
                    }
                }
                countdown--;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
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
            tbFilename.Text = System.IO.Path.GetFileName(fileName);
            mePlayer.Source = new Uri(mediaPlaylist[0]);
            playlistCounter = 0;
            slProgress.IsEnabled = true;
            slProgress.Value = 0;
            mePlayer.Play();
        }

        private void btnVideo_Click(object sender, RoutedEventArgs e)
        {
            spSlideshow.Visibility = Visibility.Collapsed;
            imPicture.Source = null;
            if(mePlayer.Source != null && mePlayer.Source.AbsolutePath.EndsWith("mp3"))
            {
                MediaChangeReset();
            }
           
        }

        private void btnAudio_Click(object sender, RoutedEventArgs e)
        {
            if (mePlayer.Source != null && mePlayer.Source.AbsolutePath.EndsWith("mov"))
            {
                MediaChangeReset();
            }  
        }
        private void MediaChangeReset()
        {
            mePlayer.Source = null;
            btnPause.IsChecked = false;
            btnPause.IsEnabled = false;
            slProgress.Value = 0;
            slProgress.IsEnabled = false;
            playlistCounter = 0;
            mediaPlaylist.Clear();
            picturesPlaylist.Clear();
            tbProgressTime.Text = "0:00:00/0:00:00";
            tbFilename.Text = "Nazev souboru";
        }

        private void btnPause_Checked(object sender, RoutedEventArgs e)
        {
            btnPause.Content = "Play";
            mePlayer.Pause();
        }

        private void btnPause_Unchecked(object sender, RoutedEventArgs e)
        {
            btnPause.Content = "Pause";
            mePlayer.Play();
        }

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mePlayer.Volume = slVolume.Value;
        }

        private void slProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            positionAdjustment = true;
        }

        private void slProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
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

        private void btnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (btnVideo.IsChecked != null)
            {
                PlaylistWindow playlistWindow = new PlaylistWindow((bool)btnVideo.IsChecked);
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
                            tbPicturesCount.Text = "Počet obrázků: " + picturesPlaylist.Count; 
                        }
                        else
                        {
                            spSlideshow.Visibility = Visibility.Collapsed;
                            lbPreviews.Visibility = Visibility.Collapsed;
                        }
                        imPicture.Source = new BitmapImage(new Uri(picturesPlaylist[0]));
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

        private void mePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if ((playlistCounter) == mediaPlaylist.Count -1) return;
            mePlayer.Source = new Uri(mediaPlaylist[++playlistCounter]);
            slProgress.Value = 0;
            tbFilename.Text = System.IO.Path.GetFileName(mediaPlaylist[playlistCounter]);
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            if(playlistCounter == 0) return;
            mePlayer.Source = new Uri(mediaPlaylist[--playlistCounter]);
            slProgress.Value = 0;
            tbFilename.Text = System.IO.Path.GetFileName(mediaPlaylist[playlistCounter]);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (playlistCounter >= mediaPlaylist.Count-1) return;
            mePlayer.Source = new Uri(mediaPlaylist[++playlistCounter]);
            slProgress.Value = 0;
            tbFilename.Text = System.IO.Path.GetFileName(mediaPlaylist[playlistCounter]);
        }

        private void lbPreviews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbPreviews.SelectedItem != null)
            {
                imPicture.Source = new BitmapImage(new Uri((string)lbPreviews.SelectedItem));
            }
        }

        private void tbxDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (tbxDelay.Text != "") return;
            Regex delay = new Regex("[^1-9]");
            e.Handled = delay.IsMatch(e.Text);
        }

        private void btnSlideshow_Click(object sender, RoutedEventArgs e)
        {
            
            if (btnSlideshow.IsChecked !=null && btnSlideshow.IsChecked == true)
            {
                if (!int.TryParse(tbxDelay.Text, out slideshow))
                {
                    btnSlideshow.IsEnabled = false;
                    slideshow = 0;
                }
            }
            else
            {
                slideshow = 0;
            }
        }
    }
}