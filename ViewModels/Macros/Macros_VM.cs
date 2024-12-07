using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ViewModels.Macros
{
    public class Macros_VM : ReactiveObject
    {
        private ObservableCollection<MacrosViewItem> _items = new ObservableCollection<MacrosViewItem>();
        public ObservableCollection<MacrosViewItem> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public ReactiveCommand<Unit, Unit> Command_AddMacros { get; set; }


        private readonly IMessageBox _messageBox;

        public Macros_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;

            Command_AddMacros = ReactiveCommand.Create(CreateMacros);
        }

        private void CreateMacros()
        {
            var rand = new Random();

            int number = rand.Next(0, 65345);

            Items.Add(new MacrosViewItem($"macros {number}", () => _messageBox.Show($"Это номер {number}", MessageType.Warning)));
        }
    }
}
