using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Player
{
    public class PlaylistCollectionsFile
    {
        public ObservableCollection<Playlist> picturePlaylists {  get; set; }
        public ObservableCollection<Playlist> audioPlaylists { get; set; }
        public ObservableCollection<Playlist> videoPlaylists { get; set; }
        public PlaylistCollectionsFile()
        {
            picturePlaylists = new ObservableCollection<Playlist>();
            audioPlaylists = new ObservableCollection<Playlist>();
            videoPlaylists = new ObservableCollection<Playlist>();
        }
    }
}
