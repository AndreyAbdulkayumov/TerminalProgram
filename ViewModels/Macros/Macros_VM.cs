using Core.Models.Settings;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ViewModels.Macros.MacrosItem;

namespace ViewModels.Macros
{
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

        private readonly Model_Settings _settings;

        public Macros_VM(IMessageBox messageBox, Func<Task> openCreateMacrosWindow)
        {
            _messageBox = messageBox;
            _openCreateMacrosWindow = openCreateMacrosWindow;

            _settings = Model_Settings.Model;

            Command_CreateMacros = ReactiveCommand.CreateFromTask(CreateMacros);
            Command_CreateMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка при создании макроса.\n\n{error.Message}", MessageType.Error));

            CommonUI_VM.ApplicationWorkModeChanged += CommonUI_VM_ApplicationWorkModeChanged;

            Init();
        }

        private void Init()
        {
            ModeName = GetModeName(CommonUI_VM.CurrentApplicationWorkMode);
            UpdateWorkspace(CommonUI_VM.CurrentApplicationWorkMode);
        }

        private string GetModeName(ApplicationWorkMode mode)
        {
            switch (mode)
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

            UpdateWorkspace(e);
        }

        private void UpdateWorkspace(ApplicationWorkMode mode)
        {
            Items.Clear();

            switch (mode)
            {
                case ApplicationWorkMode.NoProtocol:
                    BuildNoProtocolMacros();
                    break;

                case ApplicationWorkMode.ModbusClient:
                    BuildModbusMacros();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildNoProtocolMacros()
        {
            var noProtocolMacros = _settings.ReadAllNoProtocolMacros();

            if (noProtocolMacros.Items != null && noProtocolMacros.Items.Count > 0)
            {
                foreach (var element in noProtocolMacros.Items)
                {
                    IMacrosContext _macrosContext =
                        new NoProtocolMacrosContext(
                            element.Name,
                            element.Message,
                            element.EnableCR,
                            element.EnableLF);

                    BuildMacrosItem(_macrosContext.CreateContext());
                }
            }            
        }

        private void BuildModbusMacros()
        {
            var modbusMacros = _settings.ReadAllModbusMacros();

            if (modbusMacros.Items != null && modbusMacros.Items.Count > 0)
            {
                foreach (var element in modbusMacros.Items)
                {
                    IMacrosContext _macrosContext =
                        new ModbusMacrosContext(
                            element.Name,
                            element.SlaveID,
                            element.Address,
                            element.FunctionNumber,
                            element.WriteBuffer,
                            element.NumberOfRegisters,
                            element.CheckSum_IsEnable);

                    BuildMacrosItem(_macrosContext.CreateContext());
                }
            }            
        }

        public async Task CreateMacros()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;
            
            await _openCreateMacrosWindow();

            //var rand = new Random();

            //int number = rand.Next(0, 65345);

            //IMacrosContext _macrosContext;

            //switch (currentMode)
            //{
            //    case ApplicationWorkMode.NoProtocol:
            //        _macrosContext = new NoProtocolMacrosContext(number.ToString(), "test message", true, true);
            //        _settings.SaveNoProtocolMacros(number.ToString(), "test message", true, true);
            //        break;

            //    case ApplicationWorkMode.ModbusClient:
            //        _macrosContext = new ModbusMacrosContext(number.ToString(), 1, 2, 3, null, 2, false);
            //        _settings.SaveModbusMacros(number.ToString(), 1, 2, 3, null, 2, false);
            //        break;

            //    default:
            //        throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
            //}

            //// На случай если режим будет изменен во время создания нового макроса
            //if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            //{
            //    BuildMacrosItem(_macrosContext.CreateContext());
            //}
        }

        private void BuildMacrosItem(MacrosData itemData)
        {
            Items.Add(new MacrosViewItem(itemData.Name, itemData.Action, _messageBox));
        }
    }
}
