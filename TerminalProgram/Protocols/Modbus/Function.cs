using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalProgram.Protocols.Modbus
{
    public abstract class ModbusFunction
    {
        public string DisplayedName;
        public string DisplayedNumber;
        public byte Number;
    }

    public class ModbusReadFunction : ModbusFunction
    {

    }

    public class ModbusWriteFunction : ModbusFunction
    {

    }

    
    public static class Function
    {
        public static readonly ModbusReadFunction ReadCoilStatus = new ModbusReadFunction()
        {
            DisplayedName = "0x01 Чтение регистров флагов",
            DisplayedNumber = "0x01 (чтение)",
            Number = 0x01
        };

        public static readonly ModbusReadFunction ReadDiscreteInputs = new ModbusReadFunction()
        {
            DisplayedName = "0x02 Чтение дискретных входов",
            DisplayedNumber = "0x02 (чтение)",
            Number = 0x02
        };

        public static readonly ModbusReadFunction ReadHoldingRegisters = new ModbusReadFunction()
        {
            DisplayedName = "0x03 Чтение регистров хранения",
            DisplayedNumber = "0x03 (чтение)",
            Number = 0x03
        };

        public static readonly ModbusReadFunction ReadInputRegisters = new ModbusReadFunction()
        {
            DisplayedName = "0x04 Чтение входных регистров",
            DisplayedNumber = "0x04 (чтение)",
            Number = 0x04
        };

        public static readonly ModbusReadFunction[] AllReadFunctions =
        {
            ReadCoilStatus,
            ReadDiscreteInputs,
            ReadHoldingRegisters,
            ReadInputRegisters
        };

        public static readonly ModbusWriteFunction ForceSingleCoil = new ModbusWriteFunction()
        {
            DisplayedName = "0x05 Запись одного флага",
            DisplayedNumber = "0x05 (запись)",
            Number = 0x05
        };

        public static readonly ModbusWriteFunction PresetSingleRegister = new ModbusWriteFunction()
        {
            DisplayedName = "0x06 Запись одного регистра",
            DisplayedNumber = "0x06 (запись)",
            Number = 0x06
        };

        public static readonly ModbusWriteFunction PresetMultipleRegister = new ModbusWriteFunction()
        {
            DisplayedName = "0x10 Запись нескольких регистров",
            DisplayedNumber = "0x10 (запись)",
            Number = 0x10
        };

        public static readonly ModbusWriteFunction[] AllWriteFunctions =
        {
            ForceSingleCoil,
            PresetSingleRegister,
            PresetMultipleRegister
        };
    }
}
