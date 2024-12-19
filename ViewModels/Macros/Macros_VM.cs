using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ViewModels.ModbusClient;
using Core.Models.Modbus;
using ViewModels.NoProtocol;

namespace ViewModels.Macros
{
    public class MacrosItemData
    {
        public readonly string Name;
        public readonly Func<Task> Action;

        public MacrosItemData(string name, Func<Task> action)
        {
            Name = name;
            Action = action;
        }
    }

    public class Macros_VM : ReactiveObject
    {
        private const string ModeName_NoProtocol = "Без протокола";
        private const string ModeName_ModbusClient = "Modbus";

        private string? _modeName;

        public string? ModeName
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
            Command_CreateMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка при создании макроса.\n\n{error.Message}", MessageType.Error));

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

                MacrosItemData itemData;

                switch (currentMode)
                {
                    case ApplicationWorkMode.NoProtocol:
                        itemData = CreateNoProtocolMacros(number.ToString(), "test message", true, true);
                        break;

                    case ApplicationWorkMode.ModbusClient:
                        itemData = CreateModbusMacros(number.ToString(), 1, 2, 3, null, 2, false);
                        break;

                    default:
                        throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
                }

                BuildMacrosItem(itemData);
            }
        }

        private MacrosItemData CreateNoProtocolMacros(string macrosName, string message, bool enableCR, bool enableLF)
        {
            Func<Task> action = async () =>
            {
                if (NoProtocol_VM.Instance == null)
                {
                    return;
                }

                await NoProtocol_VM.Instance.NoProtocol_Send(false, message, enableCR, enableLF);
            };

            return new MacrosItemData(macrosName, action);
        }

        private MacrosItemData CreateModbusMacros(string macrosName, byte slaveID, ushort address, int functionNumber, byte[]? writeBuffer, int numberOfRegisters, bool checkSum_IsEnable)
        {
            Func<Task> action = async () =>
            {
                if (ModbusClient_VM.Instance == null)
                {
                    return;
                }

                var modbusFunction = Function.AllFunctions.Single(x => x.Number == functionNumber);

                if (modbusFunction != null)
                {
                    if (modbusFunction is ModbusReadFunction readFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Read(slaveID, address, readFunction, numberOfRegisters, checkSum_IsEnable);
                    }

                    else if (modbusFunction is ModbusWriteFunction writeFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Write(slaveID, address, writeFunction, writeBuffer, numberOfRegisters, checkSum_IsEnable);
                    }
                    
                    else
                    {
                        throw new Exception("Выбранна неизвестная Modbus функция");
                    }
                }                
            };

            return new MacrosItemData(macrosName, action);
        }

        private void BuildMacrosItem(MacrosItemData itemData)
        {
            Items.Add(new MacrosViewItem($"macros {itemData.Name}", itemData.Action, _messageBox));
        }
    }
}
