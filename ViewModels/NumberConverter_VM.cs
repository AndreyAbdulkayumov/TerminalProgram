using MessageBox_Core;
using ReactiveUI;

namespace ViewModels
{
    public class NumberConverter_VM : ReactiveObject
    {
        private string? _decFormat;

        public string? DecFormat
        {
            get => _decFormat;
            set => this.RaiseAndSetIfChanged(ref _decFormat, value);
        }

        private string? _hexFormat;

        public string? HexFormat
        {
            get => _hexFormat;
            set => this.RaiseAndSetIfChanged(ref _hexFormat, value);
        }

        private string? _binFormat;

        public string? BinFormat
        {
            get => _binFormat;
            set => this.RaiseAndSetIfChanged(ref _binFormat, value);
        }

        private string? _floatFormat;

        public string? FloatFormat
        {
            get => _floatFormat;
            set => this.RaiseAndSetIfChanged(ref _floatFormat, value);
        }

        private string? _doubleFormat;

        public string? DoubleFormat
        {
            get => _doubleFormat;
            set => this.RaiseAndSetIfChanged(ref _doubleFormat, value);
        }

        private readonly IMessageBox _messageBox;

        public NumberConverter_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;

            this.WhenAnyValue(x => x.DecFormat, x => x.HexFormat, x => x.BinFormat)
                .Subscribe(x => UpdateAllValues());
        }

        private void UpdateAllValues()
        {
            
        }
    }
}
