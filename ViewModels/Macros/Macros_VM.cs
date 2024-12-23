using Core.Models.Settings;
using Core.Models.Settings.FileTypes;
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

        private IEnumerable<IMacros>? _allMacros;

        private readonly IMessageBox _messageBox;
        private readonly Func<IEnumerable<string?>?, Task<object?>> _openCreateMacrosWindow;

        private readonly Model_Settings _settings;

        public Macros_VM(IMessageBox messageBox, Func<IEnumerable<string?>?, Task<object?>> openCreateMacrosWindow)
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
            _allMacros = null;

            switch (mode)
            {
                case ApplicationWorkMode.NoProtocol:
                    _allMacros = BuildNoProtocolMacros();
                    break;

                case ApplicationWorkMode.ModbusClient:
                    _allMacros = BuildModbusMacros();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<MacrosNoProtocolItem>? BuildNoProtocolMacros()
        {
            var macros = _settings.ReadAllNoProtocolMacros();

            if (macros.Items == null || macros.Items.Count == 0)
            {
                return null;
            }

            foreach (var element in macros.Items)
            {
                IMacrosContext _macrosContext = new NoProtocolMacrosItemContext(element);

                BuildMacrosItem(_macrosContext.CreateContext());
            }

            return macros.Items;
        }

        private IEnumerable<MacrosModbusItem>? BuildModbusMacros()
        {
            var macros = _settings.ReadAllModbusMacros();

            if (macros.Items == null || macros.Items.Count == 0)
            {
                return null;
            }

            foreach (var element in macros.Items)
            {
                IMacrosContext _macrosContext = new ModbusMacrosItemContext(element);

                BuildMacrosItem(_macrosContext.CreateContext());
            }

            return macros.Items;
        }

        public async Task CreateMacros()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            IEnumerable<string?>? allMacrosNames = _allMacros?.Select(e => e.Name);

            var result = await _openCreateMacrosWindow(allMacrosNames);

            if (result == null)
            {
                return;
            }

            IMacrosContext _macrosContext;

            if (result is MacrosNoProtocolItem noProtocolInfo)
            {
                _macrosContext = new NoProtocolMacrosItemContext(noProtocolInfo);
                _settings.SaveNoProtocolMacros(noProtocolInfo);
            }

            else if (result is MacrosModbusItem modbusInfo)
            {
                _macrosContext = new ModbusMacrosItemContext(modbusInfo);
                _settings.SaveModbusMacros(modbusInfo);
            }

            else
            {
                throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
            }

            // На случай если режим будет изменен во время создания нового макроса
            if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            {
                BuildMacrosItem(_macrosContext.CreateContext());
            }
        }

        private void DeleteMacros(string name)
        {
            switch (CommonUI_VM.CurrentApplicationWorkMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    _settings.DeleteNoProtocolMacros(name);
                    break;

                case ApplicationWorkMode.ModbusClient:
                    _settings.DeleteModbusMacros(name);
                    break;

                default:
                    throw new NotImplementedException();
            }

            UpdateWorkspace(CommonUI_VM.CurrentApplicationWorkMode);
        }

        private void BuildMacrosItem(MacrosData itemData)
        {
            Items.Add(new MacrosViewItem(itemData.Name, itemData.Action, DeleteMacros, _messageBox));
        }
    }
}
