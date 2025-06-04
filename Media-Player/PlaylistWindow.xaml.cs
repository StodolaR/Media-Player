using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : Window, INotifyPropertyChanged
    {
        private PlaylistCollectionsFile allPlaylists;
        private ObservableCollection<Playlist> actualPlaylists;
        private Playlist? selectedPlaylist;

        public ObservableCollection<Playlist> ActualPlaylists 
        {
            get => actualPlaylists;
            set
            {
                actualPlaylists = value;
                OnPropertyChanged(nameof(ActualPlaylists));
            }
        }
        public Playlist? SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                selectedPlaylist = value;
                OnPropertyChanged(nameof(SelectedPlaylist));
            }
        }
        public List<string> FinalResult { get; set; }
        public PlaylistWindow(bool IsVideoPlaylist)
        {
            InitializeComponent();
            allPlaylists = DeserializePlaylists();
            actualPlaylists = new ObservableCollection<Playlist>();
            if (IsVideoPlaylist)
            {
                ActualPlaylists = GetVideoPlaylists();
            }
            else
            {
                ActualPlaylists = GetAudioPlaylists();
            }
            FinalResult = new List<string>();
            DataContext = this;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private PlaylistCollectionsFile DeserializePlaylists()
        {
            try
            {
                if(File.Exists("playlists.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PlaylistCollectionsFile));
                    using(StreamReader sr = new StreamReader("playlists.xml"))
                    {
                        PlaylistCollectionsFile collections = new PlaylistCollectionsFile();
                        object? collectionsObject = serializer.Deserialize(sr);
                        if (collectionsObject != null)
                        {
                            collections = (PlaylistCollectionsFile)collectionsObject;
                        }
                        return collections;
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message + ex.InnerException);
            }
            return new PlaylistCollectionsFile();
        }
        private ObservableCollection<Playlist> GetVideoPlaylists()
        {
            tbPlaylistType.Text = "(video)";
            btnPictures.Visibility = Visibility.Collapsed;
            return allPlaylists.videoPlaylists;
        }
        private ObservableCollection<Playlist> GetAudioPlaylists()
        {
            tbPlaylistType.Text = "(audio)";
            return allPlaylists.audioPlaylists;
        }
        private ObservableCollection<Playlist> GetPicturePlaylists()
        {
            tbPlaylistType.Text = "(obrázky)";
            return allPlaylists.picturePlaylists;
        }

        private void BtnPictures_Click(object sender, RoutedEventArgs e)
        {
            if(btnPictures.IsChecked == true)
            {
                ActualPlaylists = GetPicturePlaylists();
            }
            else
            {
                 ActualPlaylists = GetAudioPlaylists();
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            popNewName.IsOpen = true;
            popNewName.Tag = "New";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(tbNewName.Text))
            {
                if(actualPlaylists.Any(x => x.Name == tbNewName.Text))
                {
                    tbNameExist.Visibility = Visibility.Visible;
                    return;
                }
                tbNameExist.Visibility = Visibility.Collapsed;
                if((string)popNewName.Tag == "New")
                {
                    Playlist newPlaylist = new Playlist(tbNewName.Text);
                    ActualPlaylists.Add(newPlaylist);
                    SelectedPlaylist = newPlaylist;
                }
                else if ((string)popNewName.Tag == "Rename" && SelectedPlaylist != null)
                {
                    SelectedPlaylist.Name = tbNewName.Text;
                }
            }
            tbNewName.Text = string.Empty;
            popNewName.IsOpen = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            tbNameExist.Visibility = Visibility.Collapsed;
            tbNewName.Text = string.Empty;
            popNewName.IsOpen = false;
        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedPlaylist != null)
            {
                popNewName.IsOpen = true;
                popNewName.Tag = "Rename";
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist != null)
            {
                ActualPlaylists.Remove(SelectedPlaylist);
            }
        }

        private void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist == null) return;   
            OpenFileDialog ofd = new OpenFileDialog();
            switch(tbPlaylistType.Text)
            {
                case "(video)" : ofd.Filter = MainWindow.GetFilterString(MainWindow.videoExtensions); break;
                case "(audio)": ofd.Filter = MainWindow.GetFilterString(MainWindow.audioExtensions); break;
                case "(obrázky)": ofd.Filter = MainWindow.GetFilterString(MainWindow.pictureExtensions); break;
            }
            if(ofd.ShowDialog() == true)
            {
                if (rbAddToEnd.IsChecked == true || lbFilenames.SelectedItem == null)
                {
                    SelectedPlaylist.Paths.Add(ofd.FileName);
                }
                else
                {
                    SelectedPlaylist.Paths.Insert(lbFilenames.SelectedIndex, ofd.FileName);
                }
            }
        }

        private void ChooseStartIndex(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "btnFromBegin" || lbFilenames.SelectedItem == null)
            {
                GetFinalPlaylist(0);
            }
            else
            {
                GetFinalPlaylist(lbFilenames.SelectedIndex);
            }
        }

        private void GetFinalPlaylist(int firstIndex)
        {
            if (selectedPlaylist != null)
            {
                for (int i = firstIndex; i < selectedPlaylist.Paths.Count; i++)
                {
                    if (!selectedPlaylist.Paths[i].StartsWith("["))
                    {
                        FinalResult.Add(selectedPlaylist.Paths[i]);
                    }
                }
                FinalResult.Add(tbPlaylistType.Text);
                this.DialogResult = true;
            }
            this.Close();
        }
        public void SerializePlaylists()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(allPlaylists.GetType());
                using (StreamWriter sw = new StreamWriter("playlists.xml"))
                {
                    serializer.Serialize(sw, allPlaylists);
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message + ex.InnerException);
            }
        }

        private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist == null) return;
            OpenFolderDialog ofold = new OpenFolderDialog();
            if (ofold.ShowDialog() == true)
            {
                string[] filesInFolder = Directory.GetFiles(ofold.FolderName);
                string[] searchedExtensions = new string[0];
                switch (tbPlaylistType.Text)
                {
                    case "(video)": searchedExtensions = MainWindow.videoExtensions.Split(';'); break;
                    case "(audio)": searchedExtensions = MainWindow.audioExtensions.Split(';'); break;
                    case "(obrázky)": searchedExtensions = MainWindow.pictureExtensions.Split(';'); break;
                }
                int insertIndex;
                if (rbAddToEnd.IsChecked == true || lbFilenames.SelectedItem == null)
                {
                    insertIndex = SelectedPlaylist.Paths.Count;
                }
                else
                {
                    insertIndex = lbFilenames.SelectedIndex;
                }
                SelectedPlaylist.Paths.Insert(insertIndex++, "[SLOŽKA] " + ofold.FolderName);
                foreach (string file in filesInFolder)
                {
                    foreach(string extension in searchedExtensions)
                    {
                        if (file.EndsWith(extension))
                        {
                            SelectedPlaylist.Paths.Insert(insertIndex++, file);
                            break;
                        }
                    }
                }
                SelectedPlaylist.Paths.Insert(insertIndex, "[KONEC SLOŽKY]");
            }
        }

        private void BtnRemovePath_Click(object sender, RoutedEventArgs e)
        {
            if (lbFilenames.SelectedItem == null || SelectedPlaylist == null ||((string)lbFilenames.SelectedItem).EndsWith("]")) return;
            int index = lbFilenames.SelectedIndex;
            if (((string)lbFilenames.SelectedItem).StartsWith("["))
            {
                int folderRemoved = 1;
                SelectedPlaylist.Paths.RemoveAt(index);
                while (folderRemoved > 0)
                {
                    if (SelectedPlaylist.Paths[index].StartsWith("[SLOŽKA"))
                    {
                        folderRemoved++;
                    }
                    else if (SelectedPlaylist.Paths[index].StartsWith("[KONEC"))
                    {
                        folderRemoved--;
                    }
                    SelectedPlaylist.Paths.RemoveAt(index);
                }
            }
            else
            {
                SelectedPlaylist.Paths.RemoveAt(index);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            //if(lbFilenames.SelectedIndex == 0 || ) return;
        }
    }
}
