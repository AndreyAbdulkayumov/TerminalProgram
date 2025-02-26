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
                    if (command.Content == null)
                        continue;

                    var modbusFunction = Function.AllFunctions.Single(x => x.Number == command.Content.FunctionNumber);

                    if (modbusFunction != null)
                    {
                        if (modbusFunction is ModbusReadFunction readFunction)
                        {
                            await ModbusClient_VM.Instance.Modbus_Read(command.Content.SlaveID, command.Content.Address, readFunction, command.Content.NumberOfReadRegisters, command.Content.CheckSum_IsEnable);
                        }

                        else if (modbusFunction is ModbusWriteFunction writeFunction)
                        {
                            await ModbusClient_VM.Instance.Modbus_Write(
                                command.Content.SlaveID,
                                command.Content.Address,
                                writeFunction,
                                command.Content.WriteInfo?.WriteBuffer,
                                command.Content.WriteInfo != null ? command.Content.WriteInfo.NumberOfWriteRegisters : 0,
                                command.Content.CheckSum_IsEnable
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
