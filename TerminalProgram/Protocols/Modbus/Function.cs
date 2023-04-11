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

        public ModbusFunction(string DisplayedName, string DisplayedNumber, byte Number)
        {
            this.DisplayedName = DisplayedName;
            this.DisplayedNumber = DisplayedNumber;
            this.Number = Number;
        }
    }

    public class ModbusReadFunction : ModbusFunction
    {
        public ModbusReadFunction(string DisplayedName, string DisplayedNumber, byte Number) : 
            base (DisplayedName, DisplayedNumber, Number)
        {

        }
    }

    public class ModbusWriteFunction : ModbusFunction
    {
        public ModbusWriteFunction(string DisplayedName, string DisplayedNumber, byte Number) :
            base(DisplayedName, DisplayedNumber, Number)
        {

        }
    }

    
    public static class Function
    {
        public static readonly ModbusReadFunction ReadCoilStatus = 
            new ModbusReadFunction(
                "0x01 Чтение регистров флагов",
                "0x01 (чтение)",
                0x01);

        public static readonly ModbusReadFunction ReadDiscreteInputs =
            new ModbusReadFunction(
                "0x02 Чтение дискретных входов",
                "0x02 (чтение)",
                0x02);

        public static readonly ModbusReadFunction ReadHoldingRegisters =
            new ModbusReadFunction(
                "0x03 Чтение регистров хранения",
                "0x03 (чтение)",
                0x03);

        public static readonly ModbusReadFunction ReadInputRegisters = 
            new ModbusReadFunction(
                "0x04 Чтение входных регистров",
                "0x04 (чтение)",
                0x04);

        public static readonly ModbusReadFunction[] AllReadFunctions =
        {
            ReadCoilStatus,
            ReadDiscreteInputs,
            ReadHoldingRegisters,
            ReadInputRegisters
        };

        public static readonly ModbusWriteFunction ForceSingleCoil =
            new ModbusWriteFunction(
                "0x05 Запись одного флага",
                "0x05 (запись)",
                0x05);

        public static readonly ModbusWriteFunction PresetSingleRegister =
            new ModbusWriteFunction(
                "0x06 Запись одного регистра",
                "0x06 (запись)",
                0x06);

        public static readonly ModbusWriteFunction PresetMultipleRegister =
            new ModbusWriteFunction(
                "0x10 Запись нескольких регистров",
                "0x10 (запись)",
                0x10);

        public static readonly ModbusWriteFunction[] AllWriteFunctions =
        {
            ForceSingleCoil,
            PresetSingleRegister,
            PresetMultipleRegister
        };
    }
}
