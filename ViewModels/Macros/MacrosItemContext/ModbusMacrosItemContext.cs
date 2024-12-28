using Core.Models.Modbus.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using ViewModels.ModbusClient;

namespace ViewModels.Macros.MacrosItemContext
{
    internal class ModbusMacrosItemContext : IMacrosContext
    {
        private readonly MacrosModbusItem _content;

        public ModbusMacrosItemContext(MacrosModbusItem content)
        {
            _content = content;
        }

        public MacrosData CreateContext()
        {
            Func<Task> action = async () =>
            {
                if (ModbusClient_VM.Instance == null)
                {
                    return;
                }

                var modbusFunction = Function.AllFunctions.Single(x => x.Number == _content.FunctionNumber);

                if (modbusFunction != null)
                {
                    if (modbusFunction is ModbusReadFunction readFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Read(_content.SlaveID, _content.Address, readFunction, _content.NumberOfReadRegisters, _content.CheckSum_IsEnable);
                    }

                    else if (modbusFunction is ModbusWriteFunction writeFunction)
                    {
                        await ModbusClient_VM.Instance.Modbus_Write(
                            _content.SlaveID, 
                            _content.Address,
                            writeFunction, 
                            _content.WriteInfo?.WriteBuffer,
                            _content.WriteInfo != null ? _content.WriteInfo.NumberOfWriteRegisters : 0, 
                            _content.CheckSum_IsEnable
                            );
                    }

                    else
                    {
                        throw new Exception("Выбранна неизвестная Modbus функция");
                    }
                }
            };

            return new MacrosData(_content.Name, action);
        }
    }
}
