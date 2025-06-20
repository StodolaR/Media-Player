﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Player
{
    public class Playlist : INotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public ObservableCollection<string> Paths { get; set; }
        public Playlist()
        {
            this.name = string.Empty;
            Name = string.Empty;
            Paths = new ObservableCollection<string>();
        }
        public Playlist(string name)
        {
            this.name = name;
            Name = name;
            Paths = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
