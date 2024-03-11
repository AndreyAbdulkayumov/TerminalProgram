using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_Modbus : ReactiveObject
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

        public ViewModel_Tab_Modbus()
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
