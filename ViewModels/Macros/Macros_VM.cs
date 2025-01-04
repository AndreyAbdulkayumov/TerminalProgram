using Core.Models.Settings;
using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ViewModels.Macros.DataTypes;
using ViewModels.Macros.MacrosItemContext;

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

        public ReactiveCommand<Unit, Unit> Command_Import { get; set; }
        public ReactiveCommand<Unit, Unit> Command_Export { get; set; }
        public ReactiveCommand<Unit, Unit> Command_CreateMacros { get; set; }

        private MacrosNoProtocol? _noProtocolMacros;
        private MacrosModbus? _modbusMacros;

        private List<string> _allMacrosNames = new List<string>();

        private readonly IMessageBox _messageBox;
        private readonly Func<EditMacrosParameters, Task<object?>> _openEditMacrosWindow;
        private readonly Func<string, Task<string?>> _getFolderPath;
        private readonly Func<string, Task<string?>> _getFilePath;

        private readonly Model_Settings _settings;

        public Macros_VM(
            IMessageBox messageBox, 
            Func<EditMacrosParameters, Task<object?>> openEditMacrosWindow,
            Func<string, Task<string?>> getFolderPath_Handler,
            Func<string, Task<string?>> getFilePath_Handler)
        {
            _messageBox = messageBox;
            _openEditMacrosWindow = openEditMacrosWindow;
            _getFolderPath = getFolderPath_Handler;
            _getFilePath = getFilePath_Handler;

            _settings = Model_Settings.Model;

            Command_Import = ReactiveCommand.CreateFromTask(ImportMacros);
            Command_Import.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка при импорте макросов.\n\n{error.Message}", MessageType.Error));

            Command_Export = ReactiveCommand.CreateFromTask(ExportMacros);
            Command_Export.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка при экспорте макроса.\n\n{error.Message}", MessageType.Error));

            Command_CreateMacros = ReactiveCommand.CreateFromTask(CreateMacros);
            Command_CreateMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка при создании макроса.\n\n{error.Message}", MessageType.Error));

            CommonUI_VM.ApplicationWorkModeChanged += CommonUI_VM_ApplicationWorkModeChanged;

            InitUI();
        }

        private void InitUI()
        {
            ModeName = GetModeName(CommonUI_VM.CurrentApplicationWorkMode);
            UpdateWorkspace(CommonUI_VM.CurrentApplicationWorkMode);
        }

        private string GetValidMacrosFileName()
        {
            if (_noProtocolMacros != null)
            {
                return _settings.FilePath_Macros_NoProtocol;
            }

            else if (_modbusMacros != null)
            {
                return _settings.FilePath_Macros_Modbus;
            }

            throw new Exception("Не выбран режим.");
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
            var macros = _settings.ReadOrCreateDefaultMacros<MacrosNoProtocol>();

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
            var macros = _settings.ReadOrCreateDefaultMacros<MacrosModbus>();

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

        private async Task CreateMacros()
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

            var content = await _openEditMacrosWindow(new EditMacrosParameters(null, null, _allMacrosNames));

            if (content == null)
            {
                return;
            }

            IMacrosContext macrosContext;

            if (content is MacrosNoProtocolItem noProtocolContent)
            {
                macrosContext = new NoProtocolMacrosItemContext(noProtocolContent);
                (currentMacros as MacrosNoProtocol)?.Items?.Add(noProtocolContent);
                _settings.SaveMacros(_noProtocolMacros);
            }

            else if (content is MacrosModbusItem modbusContent)
            {
                macrosContext = new ModbusMacrosItemContext(modbusContent);
                (currentMacros as MacrosModbus)?.Items?.Add(modbusContent);
                _settings.SaveMacros(_modbusMacros);
            }

            else
            {
                throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
            }
            
            // На случай если режим будет изменен во время создания нового макроса
            if (currentMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
            {
                if (content is MacrosNoProtocolItem)
                {
                    _noProtocolMacros = currentMacros as MacrosNoProtocol;
                }

                else if (content is MacrosModbusItem)
                {
                    _modbusMacros = currentMacros as MacrosModbus;
                }

                else
                {
                    throw new NotImplementedException($"Поддержка режима {currentMode} не реализована.");
                }

                BuildMacrosItem(macrosContext.CreateContext());
            }
        }

        private async Task EditMacros(string name)
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            object? currentMacros, initData;

            switch (currentMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    currentMacros = _noProtocolMacros;
                    initData = _noProtocolMacros?.Items?.Find(e => e.Name == name);
                    break;

                case ApplicationWorkMode.ModbusClient:
                    currentMacros = _modbusMacros;
                    initData = _modbusMacros?.Items?.Find(e => e.Name == name);
                    break;

                default:
                    throw new NotImplementedException();
            }

            object? content = await _openEditMacrosWindow(new EditMacrosParameters(name, initData, _allMacrosNames));

            if (content == null)
            {
                return;
            }

            IMacrosContext macrosContext;

            if (content is MacrosNoProtocolItem noProtocolContent)
            {
                macrosContext = new NoProtocolMacrosItemContext(noProtocolContent);

                var item = _noProtocolMacros?.Items?.First(item => item.Name == name);

                if (item != null)
                {
                    item.MacrosEncoding = noProtocolContent.MacrosEncoding;
                    item.Name = noProtocolContent.Name;
                    item.Message = noProtocolContent.Message;
                    item.EnableCR = noProtocolContent.EnableCR;
                    item.EnableLF = noProtocolContent.EnableLF;
                    item.IsByteString = noProtocolContent.IsByteString;

                    _settings.SaveMacros(_noProtocolMacros);
                }
            }

            else if (content is MacrosModbusItem modbusContent)
            {
                macrosContext = new ModbusMacrosItemContext(modbusContent);

                var item = _modbusMacros?.Items?.First(item => item.Name == name);

                if (item != null)
                {
                    item.Name = modbusContent.Name;
                    item.SlaveID = modbusContent.SlaveID;
                    item.Address = modbusContent.Address;
                    item.FunctionNumber = modbusContent.FunctionNumber;
                    item.WriteInfo = modbusContent.WriteInfo;
                    item.NumberOfReadRegisters = modbusContent.NumberOfReadRegisters;
                    item.CheckSum_IsEnable = modbusContent.CheckSum_IsEnable;

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
                MacrosData data = macrosContext.CreateContext();

                ChangeMacrosItem(name, data, content);
            }
        }

        private void ChangeMacrosItem(string oldName, MacrosData newData, object macrosContent)
        {
            IMacrosItem? item;

            if (macrosContent is MacrosNoProtocolItem)
            {
                item = _noProtocolMacros?.Items?.Find(macros => macros.Name == oldName);
            }

            else if (macrosContent is MacrosModbusItem)
            {
                item = _modbusMacros?.Items?.Find(macros => macros.Name == oldName);
            }

            else
            {
                throw new NotImplementedException($"Поддержка режима {CommonUI_VM.CurrentApplicationWorkMode} не реализована.");
            }

            if (item != null)
            {
                item.Name = newData.Name;
            }

            var viewItem = Items.First(macros => macros.Title == oldName);

            if (viewItem != null)
            {
                viewItem.Title = newData.Name;
                viewItem.ClickAction = newData.Action;

                _allMacrosNames = Items.Select(item => item.Title).ToList();
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

        private async Task ImportMacros()
        {
            ApplicationWorkMode workMode = CommonUI_VM.CurrentApplicationWorkMode;

            string modeName = GetModeName(workMode);

            if (await _messageBox.ShowYesNoDialog(
                "Внимание!!!\n\n" +
                $"При импорте файла макросов для режима \"{modeName}\" старые макросы будут удалены без возможности восстановления.\n\n" +
                "Продолжить?",
                MessageType.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            string? macrosFilePath = await _getFilePath($"Выбор файла для импорта макросов режима \"{modeName}\".");

            if (macrosFilePath != null)
            {
                string fileName = Path.GetFileName(macrosFilePath);

                string macrosValidFilePath = GetValidMacrosFileName();
                string validFileName = Path.GetFileName(macrosValidFilePath);

                if (fileName != validFileName)
                {
                    throw new Exception($"Некорректное имя файла макроса.\nОжидается имя \"{validFileName}\".");
                }

                try
                {
                    switch (workMode)
                    {
                        case ApplicationWorkMode.NoProtocol:
                            var macrosNoProtocol = _settings.ReadMacros<MacrosNoProtocol>(macrosFilePath);
                            break;

                        case ApplicationWorkMode.ModbusClient:
                            var macrosModbus = _settings.ReadMacros<MacrosModbus>(macrosFilePath);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                catch (Exception)
                {
                    throw new Exception("Нарушена целостность файла.");
                }

                _settings.DeleteFile(macrosValidFilePath);

                _settings.CopyFile(macrosFilePath, macrosValidFilePath);

                // На случай если режим будет изменен во время импорта
                if (workMode.Equals(CommonUI_VM.CurrentApplicationWorkMode))
                {
                    UpdateWorkspace(workMode);
                }

                _messageBox.Show($"Файл с макросами для режима \"{modeName}\" успешно импортирован!", MessageType.Information);
            }
        }

        private async Task ExportMacros()
        {
            string? outputFilePath = await _getFolderPath("Выбор папки для экспорта файла макросов.");

            if (outputFilePath != null)
            {
                string macrosFileName = GetValidMacrosFileName();

                string outputFileName = Path.Combine(outputFilePath, Path.GetFileName(macrosFileName));

                _settings.CopyFile(macrosFileName, outputFileName);

                _messageBox.Show($"Экспорт прошел успешно!\n\nПуть к файлу:\n{outputFileName}", MessageType.Information);
            }
        }
    }
}
