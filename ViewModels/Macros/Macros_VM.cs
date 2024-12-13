using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ViewModels.Macros
{
    public class Macros_VM : ReactiveObject
    {
        private const string ModeName_NoProtocol = "Без протокола";
        private const string ModeName_ModbusClient = "Modbus";

        private string _modeName;

        public string ModeName
        {
            get => _modeName;
            set => this.RaiseAndSetIfChanged(ref _modeName, value);
        }

        private ObservableCollection<MacrosViewItem> _items = new ObservableCollection<MacrosViewItem>();

        public ObservableCollection<MacrosViewItem> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public ReactiveCommand<Unit, Unit> Command_CreateMacros { get; set; }

        private readonly IMessageBox _messageBox;
        private readonly Func<Task> _openCreateMacrosWindow;

        public Macros_VM(IMessageBox messageBox, Func<Task> openCreateMacrosWindow)
        {
            _messageBox = messageBox;
            _openCreateMacrosWindow = openCreateMacrosWindow;

            ModeName = GetModeName(CommonUI_VM.CurrentApplicationWorkMode);

            Command_CreateMacros = ReactiveCommand.CreateFromTask(CreateMacros);

            CommonUI_VM.ApplicationWorkModeChanged += CommonUI_VM_ApplicationWorkModeChanged;
        }

        private string GetModeName(ApplicationWorkMode type)
        {
            switch (type)
            {
                case ApplicationWorkMode.NoProtocol:
                    return ModeName_NoProtocol;

                case ApplicationWorkMode.ModbusClient:
                    return ModeName = ModeName_ModbusClient;

                default:
                    throw new NotImplementedException();
            }
        }

        private void CommonUI_VM_ApplicationWorkModeChanged(object? sender, ApplicationWorkMode e)
        {
            ModeName = GetModeName(e);
        }

        public async Task CreateMacros()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            await _openCreateMacrosWindow();

            // На случай если режим будет изменен при добавлении макроса
            if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            {
                var rand = new Random();

                int number = rand.Next(0, 65345);

                Items.Add(new MacrosViewItem($"macros {number}", () => _messageBox.Show($"Это номер {number}", MessageType.Warning)));
            }            
        }
    }
}
