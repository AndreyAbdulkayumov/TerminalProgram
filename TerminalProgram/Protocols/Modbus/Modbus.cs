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

    public struct DEVICE_RESPONSE
    {
        public UInt16 OperationNumber;
        public UInt16 ProtocolID;
        public UInt16 LengthOfPDU;  // PDU - Protocol Data Unit
        public byte SlaveID;
        public byte Command;
        public int LengthOfData;
        public byte[] Data;
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

        public void WriteRegister(UInt16 PackageNumber, UInt16 Address, UInt16 Data, 
            out DEVICE_RESPONSE Response, int NumberOfRegisters, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[20];

                byte[] TX = ModbusMessage.Create_WriteType(ModbusType, SlaveID, Address, Data, CRC_IsEnable, Polynom);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                // Т.к. регистры 16 битные.
                Response = DecodingOfArray(ModbusType, 0x06, RX, NumberOfRegisters * 2); 

                IsBusy = false;
            }
            
            catch(Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }

        public UInt16 ReadRegister(UInt16 PackageNumber, UInt16 Address,
            out DEVICE_RESPONSE Response, int NumberOfRegisters, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] RX = new byte[20];

                byte[] TX = ModbusMessage.Create_ReadType(ModbusType, SlaveID, Address, NumberOfRegisters, CRC_IsEnable, Polynom);

                Device.Send(TX, TX.Length);

                Device.Receive(RX);

                // Т.к. регистры 16 битные.
                Response = DecodingOfArray(ModbusType, 0x04, RX, NumberOfRegisters * 2); 

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

        private DEVICE_RESPONSE DecodingOfArray(TypeOfModbus ModbusType, int command, byte [] massive, int NumberOfBytes)
        {
            DEVICE_RESPONSE Decoding = new DEVICE_RESPONSE();

            byte[] temp = new byte[2];

            if (ModbusType == TypeOfModbus.TCP)
            {
                temp[0] = massive[1];
                temp[1] = massive[0];
                Decoding.OperationNumber = (UInt16)BitConverter.ToInt16(temp, 0);
                temp[0] = massive[3];
                temp[1] = massive[2];
                Decoding.ProtocolID = (UInt16)BitConverter.ToInt16(temp, 0);
                temp[0] = massive[5];
                temp[1] = massive[4];
                Decoding.LengthOfPDU = (UInt16)BitConverter.ToInt16(temp, 0);
                Decoding.SlaveID = massive[6];
                Decoding.Command = massive[7];
            }

            else if (ModbusType == TypeOfModbus.RTU)
            {
                Decoding.SlaveID = massive[0];
                Decoding.Command = massive[1];                
            }

            else
            {
                throw new Exception("Неизвестный тип Modbus протокола. " + ModbusType);
            }

            CheckErrorCode(ModbusType, ref Decoding, massive);

            if (command == 0x04)
            {
                if (ModbusType == TypeOfModbus.TCP)
                {
                    Decoding.LengthOfData = massive[8];
                }
                
                else if (ModbusType == TypeOfModbus.RTU)
                {
                    Decoding.LengthOfData = massive[2];
                }

                else
                {
                    throw new Exception("Неизвестный тип Modbus протокола. " + ModbusType);
                }

                Decoding.Data = new byte[Decoding.LengthOfData];

                if (ModbusType == TypeOfModbus.TCP)
                {
                    Array.Copy(massive, 9, Decoding.Data, 0, Decoding.LengthOfData);
                }

                else
                {
                    Array.Copy(massive, 3, Decoding.Data, 0, Decoding.LengthOfData);
                }
                
                Decoding.Data = ReverseArray(Decoding.Data);
            }

            else if (command == 0x06)
            {
                Decoding.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неизвестный код Modbus команды");
            }

            return Decoding;
        }

        private void CheckErrorCode(TypeOfModbus ModbusType, ref DEVICE_RESPONSE Decoding, byte[] massive)
        {
            if (Decoding.Command > 128)
            {
                int FunctionCode = Decoding.Command - 128;

                Decoding.Data = new byte[1]; // Код ошибки занимает 1 байт

                if (ModbusType == TypeOfModbus.TCP)
                {
                    Array.Copy(massive, 8, Decoding.Data, 0, 1);
                }
                
                else
                {
                    Array.Copy(massive, 3, Decoding.Data, 0, 1);
                }

                switch (Decoding.Data[0])
                {
                    case 1:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Принятый код функции не может быть обработан (Код 1).");

                    case 2:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Адрес данных, указанный в запросе, недоступен (Код 2).");

                    case 3:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Значение, содержащееся в поле данных запроса, является недопустимой величиной (Код 3).");

                    case 4:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Невосстанавливаемая ошибка имела место, " +
                            "пока ведомое устройство пыталось выполнить затребованное действие (Код 4).");

                    case 5:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство приняло запрос и обрабатывает его, " +
                            "но это требует много времени. " +
                            "Этот ответ предохраняет ведущее устройство от генерации ошибки тайм-аута (Код 5).");

                    case 6:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство занято обработкой команды. " +
                            "Ведущее устройство должно повторить сообщение позже, когда ведомое освободится. (Код 6).");

                    case 7:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство не может выполнить программную функцию, заданную в запросе. " +
                            "Этот код возвращается для неуспешного программного запроса, " +
                            "использующего функции с номерами 13 или 14. " +
                            "Ведущее устройство должно запросить диагностическую информацию " +
                            "или информацию об ошибках от ведомого (Код 7).");

                    case 8:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Ведомое устройство при чтении расширенной памяти обнаружило ошибку контроля четности. " +
                            "Master может повторить запрос позже, " +
                            "но обычно в таких случаях требуется ремонт оборудования (Код 8).");

                    case 10:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Шлюз неправильно настроен или перегружен запросами (Код 10).");

                    case 11:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Ошибка Modbus: " +
                            "Slave устройства нет в сети или от него нет ответа (Код 11).");

                    default:
                        throw new Exception("Код функции: " + FunctionCode.ToString() + "\n" +
                            "Неизвестная ошибка Modbus (Код " + Decoding.Data[0] + ")");
                }
            }
        }

        static byte[] ReverseArray(byte[] SourceArray)
        {
            byte[] temp = new byte[SourceArray.Length];

            for(int i = 0; i < SourceArray.Length; i++)
            {
                temp[i] = SourceArray[SourceArray.Length - 1 - i];
            }

            return temp;
        }        
    }
}
