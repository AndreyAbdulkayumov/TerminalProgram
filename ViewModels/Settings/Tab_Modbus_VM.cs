﻿using ReactiveUI;
using System.Reactive.Linq;

namespace ViewModels.Settings
{
    public class Tab_Modbus_VM : ReactiveObject
    {
        private string _writeTimeout = string.Empty;

        public string WriteTimeout
        {
            get => _writeTimeout;
            set => this.RaiseAndSetIfChanged(ref _writeTimeout, value);
        }

        private string _readTimeout = string.Empty;

        public string ReadTimeout
        {
            get => _readTimeout;
            set => this.RaiseAndSetIfChanged(ref _readTimeout, value);
        }

        public Tab_Modbus_VM()
        {
            this.WhenAnyValue(x => x.WriteTimeout)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt16 _))
                .Subscribe(result => WriteTimeout = result);

            this.WhenAnyValue(x => x.ReadTimeout)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt16 _))
                .Subscribe(result => ReadTimeout = result);
        }
    }
}