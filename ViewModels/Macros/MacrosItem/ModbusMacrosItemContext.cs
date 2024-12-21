using Core.Models.Modbus;
using Core.Models.Settings.FileTypes;
using ViewModels.ModbusClient;

namespace ViewModels.Macros.MacrosItem
{
    internal class ModbusMacrosItemContext : IMacrosContext
    {
        private string _macrosName;
        private byte _slaveID;
        private ushort _address;
        private int _functionNumber;
        private byte[]? _writeBuffer;
        private int _numberOfRegisters;
        private bool _checkSum_IsEnable;

        public ModbusMacrosItemContext(MacrosModbusItem info)
        {
            _macrosName = info.Name;
            _slaveID = info.SlaveID;
            _address = info.Address;
            _functionNumber = info.FunctionNumber;
            _writeBuffer = info.WriteBuffer;
            _numberOfRegisters = info.NumberOfRegisters;
            _checkSum_IsEnable = info.CheckSum_IsEnable;
        }

        public MacrosData CreateContext()
        {
            Func<Task> action = async () =>
            {
                if (ModbusClient_VM.Instance == null)
                {
                    return;
                }

                var modbusFunction = Function.AllFunctions.Single(x => x.Number == _functionNumber);

                if (modbusFunction != null)
                {
                    if (modbusFunction is ModbusReadFunction readFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Read(_slaveID, _address, readFunction, _numberOfRegisters, _checkSum_IsEnable);
                    }

                    else if (modbusFunction is ModbusWriteFunction writeFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Write(_slaveID, _address, writeFunction, _writeBuffer, _numberOfRegisters, _checkSum_IsEnable);
                    }

                    else
                    {
                        throw new Exception("Выбранна неизвестная Modbus функция");
                    }
                }
            };

            return new MacrosData(_macrosName, action);
        }
    }
}
