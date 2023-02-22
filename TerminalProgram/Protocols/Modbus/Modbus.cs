using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TerminalProgram.Protocols.Modbus
{
    public enum TypeOfModbus
    {
        TCP,
        RTU,
        ASCII
    }

    public struct ModbusResponse
    {
        // Только для Modbus TCP
        public UInt16 OperationNumber;
        public UInt16 ProtocolID;
        public UInt16 LengthOfPDU; 
        
        // Общая часть для всех типов Modbus протокола
        public byte SlaveID;

        // PDU - Protocol Data Unit
        public byte Command;
        public int LengthOfData;
        public byte[] Data;
    }

    public static class ModbusCommand
    {
        public static readonly int ReadInputRegisters = 0x04;

        public static readonly int PresetSingleRegister = 0x06;
    }

    public class Modbus
    {
        /// <summary>
        /// Полином для расчета CRC.
        /// </summary>
        public ushort Polynom { get; } = 0xA001;
        /// <summary>
        /// Номер Slave устройства, с которым будет происходить обмен данными. 
        /// По умолчанию равно 0, значит вызов широковещательный.
        /// </summary>
        public byte SlaveID { get; set; } = 0;

        private static bool IsBusy = false;

        private IConnection Device = null;

        public Modbus(IConnection ConnectedDevice)
        {
            Device = ConnectedDevice;
        }

        public void WriteRegister(UInt16 Address, UInt16 Data, 
            out ModbusResponse Response, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[20];

                byte[] TX = ModbusMessage.Create_WriteType(ModbusType, SlaveID, Address, Data, CRC_IsEnable, Polynom);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                Response = ModbusMessage.Decoding(ModbusType, ModbusCommand.PresetSingleRegister, RX);

                IsBusy = false;
            }
            
            catch(Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }

        public UInt16 ReadRegister(UInt16 Address, out ModbusResponse Response, 
            int NumberOfRegisters, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[20];

                byte[] TX = ModbusMessage.Create_ReadType(ModbusType, SlaveID, Address, NumberOfRegisters, CRC_IsEnable, Polynom);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                Response = ModbusMessage.Decoding(ModbusType, ModbusCommand.ReadInputRegisters, RX);

                UInt16 result = (UInt16)BitConverter.ToInt16(Response.Data, 0);

                IsBusy = false;

                return result;
            }

            catch (Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }
    }
}
