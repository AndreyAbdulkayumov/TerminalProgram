using Core.Models.Modbus.DataTypes;
using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using ViewModels.ModbusClient;

namespace ViewModels.Macros.MacrosItemContext
{
    internal class ModbusMacrosItemContext : IMacrosContext
    {
        private readonly MacrosContent<MacrosCommandModbus> _content;

        public ModbusMacrosItemContext(MacrosContent<MacrosCommandModbus> content)
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

                if (_content.Commands == null)
                {
                    return;
                }

                foreach (var command in _content.Commands)
                {
                    var modbusFunction = Function.AllFunctions.Single(x => x.Number == command.FunctionNumber);

                    if (modbusFunction != null)
                    {
                        if (modbusFunction is ModbusReadFunction readFunction)
                        {
                            await ModbusClient_VM.Instance.Modbus_Read(command.SlaveID, command.Address, readFunction, command.NumberOfReadRegisters, command.CheckSum_IsEnable);
                        }

                        else if (modbusFunction is ModbusWriteFunction writeFunction)
                        {
                            await ModbusClient_VM.Instance.Modbus_Write(
                                command.SlaveID,
                                command.Address,
                                writeFunction,
                                command.WriteInfo?.WriteBuffer,
                                command.WriteInfo != null ? command.WriteInfo.NumberOfWriteRegisters : 0,
                                command.CheckSum_IsEnable
                                );
                        }

                        else
                        {
                            throw new Exception("Выбранна неизвестная Modbus функция");
                        }
                    }
                }                
            };

            return new MacrosData(_content.MacrosName, action);
        }
    }
}
