using Core.Models.Modbus;
using ViewModels.ModbusClient;

namespace ViewModels.Macros.MacrosItem
{
    internal class ModbusMacrosContext : IMacrosContext
    {
        private string _macrosName;
        private byte _slaveID;
        private ushort _address;
        private int _functionNumber;
        private byte[]? _writeBuffer;
        private int _numberOfRegisters;
        private bool _checkSum_IsEnable;

        public ModbusMacrosContext(string macrosName, byte slaveID, ushort address, int functionNumber, byte[]? writeBuffer, int numberOfRegisters, bool checkSum_IsEnable)
        {
            _macrosName = macrosName;
            _slaveID = slaveID;
            _address = address;
            _functionNumber = functionNumber;
            _writeBuffer = writeBuffer;
            _numberOfRegisters = numberOfRegisters;
            _checkSum_IsEnable = checkSum_IsEnable;
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
