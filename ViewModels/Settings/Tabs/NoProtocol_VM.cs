﻿using ReactiveUI;
using System.Collections.ObjectModel;

namespace ViewModels.Settings.Tabs
{
    public class NoProtocol_VM : ReactiveObject
    {
        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            "ASCII", "UTF-8", "UTF-32", "Unicode"
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string _selectedEncoding = string.Empty;

        public string SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }


        public NoProtocol_VM()
        {

        }
    }
}
