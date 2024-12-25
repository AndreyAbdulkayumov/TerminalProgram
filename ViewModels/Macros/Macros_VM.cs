using Core.Models.Settings;
using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ViewModels.Macros.DataTypes;
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

        private ObservableCollection<MacrosViewItem_VM> _items = new ObservableCollection<MacrosViewItem_VM>();

        public ObservableCollection<MacrosViewItem_VM> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public ReactiveCommand<Unit, Unit> Command_CreateMacros { get; set; }

        private MacrosNoProtocol? _noProtocolMacros;
        private MacrosModbus? _modbusMacros;

        private List<string?> _allMacrosNames = new List<string?>();

        private readonly IMessageBox _messageBox;
        private readonly Func<EditMacrosParameters, Task<object?>> _openEditMacrosWindow;

        private readonly Model_Settings _settings;

        public Macros_VM(IMessageBox messageBox, Func<EditMacrosParameters, Task<object?>> openEditMacrosWindow)
        {
            _messageBox = messageBox;
            _openEditMacrosWindow = openEditMacrosWindow;

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

            _noProtocolMacros = null;
            _modbusMacros = null;

            _allMacrosNames.Clear();

            switch (mode)
            {
                case ApplicationWorkMode.NoProtocol:
                    _noProtocolMacros = BuildNoProtocolMacros();
                    break;

                case ApplicationWorkMode.ModbusClient:
                    _modbusMacros = BuildModbusMacros();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private MacrosNoProtocol? BuildNoProtocolMacros()
        {
            var macros = _settings.ReadMacros<MacrosNoProtocol>();

            if (macros.Items == null || macros.Items.Count == 0)
            {
                return null;
            }

            foreach (var element in macros.Items)
            {
                IMacrosContext _macrosContext = new NoProtocolMacrosItemContext(element);

                BuildMacrosItem(_macrosContext.CreateContext());
            }

            return macros;
        }

        private MacrosModbus? BuildModbusMacros()
        {
            var macros = _settings.ReadMacros<MacrosModbus>();

            if (macros.Items == null || macros.Items.Count == 0)
            {
                return null;
            }

            foreach (var element in macros.Items)
            {
                IMacrosContext _macrosContext = new ModbusMacrosItemContext(element);

                BuildMacrosItem(_macrosContext.CreateContext());
            }

            return macros;
        }

        public async Task CreateMacros()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            object? currentMacros;

            switch (currentMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    currentMacros = _noProtocolMacros;
                    break;

                case ApplicationWorkMode.ModbusClient:
                    currentMacros = _modbusMacros;
                    break;

                default:
                    throw new NotImplementedException();

            }

            var result = await _openEditMacrosWindow(new EditMacrosParameters(null, null, _allMacrosNames));

            if (result == null)
            {
                return;
            }

            IMacrosContext _macrosContext;

            if (result is MacrosNoProtocolItem noProtocolInfo)
            {
                _macrosContext = new NoProtocolMacrosItemContext(noProtocolInfo);
                (currentMacros as MacrosNoProtocol)?.Items?.Add(noProtocolInfo);
                _settings.SaveMacros(_noProtocolMacros);
            }

            else if (result is MacrosModbusItem modbusInfo)
            {
                _macrosContext = new ModbusMacrosItemContext(modbusInfo);
                (currentMacros as MacrosModbus)?.Items?.Add(modbusInfo);
                _settings.SaveMacros(_modbusMacros);
            }

            else
            {
                throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
            }
            
            // На случай если режим будет изменен во время создания нового макроса
            if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            {
                if (result is MacrosNoProtocolItem)
                {
                    _noProtocolMacros = currentMacros as MacrosNoProtocol;
                }

                else if (result is MacrosModbusItem)
                {
                    _modbusMacros = currentMacros as MacrosModbus;
                }

                else
                {
                    throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
                }

                BuildMacrosItem(_macrosContext.CreateContext());
            }
        }

        private async Task EditMacros(string name)
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            object? result, currentMacros;

            switch (currentMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    currentMacros = _noProtocolMacros;
                    result = await _openEditMacrosWindow(new EditMacrosParameters(name, _noProtocolMacros?.Items?.Find(e => e.Name == name), _allMacrosNames));
                    break;

                case ApplicationWorkMode.ModbusClient:
                    currentMacros = _modbusMacros;
                    result = await _openEditMacrosWindow(new EditMacrosParameters(name, _modbusMacros?.Items?.Find(e => e.Name == name), _allMacrosNames));
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (result == null)
            {
                return;
            }

            IMacrosContext _macrosContext;

            if (result is MacrosNoProtocolItem noProtocolInfo)
            {
                _macrosContext = new NoProtocolMacrosItemContext(noProtocolInfo);

                var item = _noProtocolMacros?.Items?.First(item => item.Name == name);

                if (item != null)
                {
                    item.Name = noProtocolInfo.Name;
                    item.Message = noProtocolInfo.Message;
                    item.EnableCR = noProtocolInfo.EnableCR;
                    item.EnableLF = noProtocolInfo.EnableLF;

                    _settings.SaveMacros(_noProtocolMacros);
                }
            }

            else if (result is MacrosModbusItem modbusInfo)
            {
                _macrosContext = new ModbusMacrosItemContext(modbusInfo);

                var item = _modbusMacros?.Items?.First(item => item.Name == name);

                if (item != null)
                {
                    item.Name = modbusInfo.Name;
                    item.SlaveID = modbusInfo.SlaveID;
                    item.Address = modbusInfo.Address;
                    item.FunctionNumber = modbusInfo.FunctionNumber;
                    item.WriteBuffer = modbusInfo.WriteBuffer;
                    item.NumberOfRegisters = modbusInfo.NumberOfRegisters;
                    item.CheckSum_IsEnable = modbusInfo.CheckSum_IsEnable;

                    _settings.SaveMacros(_modbusMacros);
                }
            }

            else
            {
                throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
            }

            // На случай если режим будет изменен во время создания нового макроса
            if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            {
                string? resultMacrosName = (result as IMacrosItem)?.Name;

                if (string.IsNullOrWhiteSpace(resultMacrosName))
                {
                    throw new Exception("Ошибка редактирования макроса. Нет имени.");
                }

                IMacrosItem? item;

                if (result is MacrosNoProtocolItem)
                {
                    item = _noProtocolMacros?.Items?.Find(macros => macros.Name == name);
                }

                else if (result is MacrosModbusItem)
                {
                    item = _modbusMacros?.Items?.Find(macros => macros.Name == name);
                }

                else
                {
                    throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
                }

                if (item != null)
                {
                    item.Name = resultMacrosName;
                }

                var viewItem = Items.First(macros => macros.Title == name);

                if (viewItem != null)
                {
                    viewItem.Title = resultMacrosName;

                    // Обновление экшона !!!!!!!!!!!!!!!!!!!!!!!!!!
                }
            }
        }

        private void DeleteMacros(string name)
        {
            var itemToRemove = Items.First(item => item.Title == name);

            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }

            _allMacrosNames.Remove(name);

            switch (CommonUI_VM.CurrentApplicationWorkMode)
            {
                case ApplicationWorkMode.NoProtocol:

                    var noProtocolItem = _noProtocolMacros?.Items?.First(macros => macros.Name == name);

                    if (noProtocolItem != null)
                    {
                        _noProtocolMacros?.Items?.Remove(noProtocolItem);
                        _settings.SaveMacros(_noProtocolMacros);
                    }
                    
                    break;

                case ApplicationWorkMode.ModbusClient:

                    var modbusItem = _modbusMacros?.Items?.First(macros => macros.Name == name);

                    if (modbusItem != null)
                    {
                        _modbusMacros?.Items?.Remove(modbusItem);
                        _settings.SaveMacros(_modbusMacros);
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildMacrosItem(MacrosData itemData)
        {
            Items.Add(new MacrosViewItem_VM(itemData.Name, itemData.Action, EditMacros, DeleteMacros, _messageBox));
            _allMacrosNames.Add(itemData.Name);
        }
    }
}
