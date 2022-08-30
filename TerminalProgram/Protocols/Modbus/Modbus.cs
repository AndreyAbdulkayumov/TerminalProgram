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
        public string LengthOfData;
        public byte[] Data;
    }

    public class Modbus
    {
        /// <summary>
        /// Полином для расчета CRC.
        /// </summary>
        public ushort Polynom { get; } = 0xA001;
        /// <summary>
        /// Номер Slave устройства, с которым будет происходить обмен данными. По умолчанию равно 0, значит вызов широковещательный.
        /// </summary>
        public byte SlaveID { get; set; } = 0;

        private static bool IsBusy = false;

        private IConnection Device = null;

        public Modbus(IConnection ConnectedDevice)
        {
            Device = ConnectedDevice;
        }

        public void WriteRegister(UInt16 PackageNumber, UInt16 Address, UInt16 Data, 
            out DEVICE_RESPONSE Response, int NumberOfBytes, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] TX;
                byte TX_Bytes = 5;  // PDU Length

                switch (ModbusType)
                {
                    case TypeOfModbus.ASCII:
                        TX_Bytes += 5;    // Префикс (1 Б) + SlaveID (2 Б) + Суффикс (2 Б)
                        break;

                    case TypeOfModbus.RTU:
                        TX_Bytes += 1;    // SlaveID (1 Б)
                        break;

                    case TypeOfModbus.TCP:
                        TX_Bytes += 7;    // ID транзакции (2 Б) + ID протокола (2 Б, равно 0) + Длина остатка пакета (2 Б) + SlaveID (1 Б)
                        break;

                    default:
                        throw new Exception("Выбрана неизвестная разновидность Modbus.");
                }

                if (CRC_IsEnable)
                {
                    TX_Bytes += 2;
                }


                TX = new byte[TX_Bytes];

                byte[] RX = new byte[20];

                byte[] AddressArray = BitConverter.GetBytes(Address);
                byte[] DataArray = BitConverter.GetBytes(Data);
                byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);

                if (ModbusType == TypeOfModbus.TCP)
                {
                    // Номер транзакции
                    TX[0] = PackageNumberArray[1];
                    TX[1] = PackageNumberArray[0];
                    // Modbus ID
                    TX[2] = 0x00;
                    TX[3] = 0x00;
                    // Длина PDU
                    TX[4] = 0x00;
                    TX[5] = 0x06;
                    // Slave ID
                    TX[6] = SlaveID;
                    // Write 1 register
                    TX[7] = 0x06;
                    // address 
                    TX[8] = AddressArray[1];
                    TX[9] = AddressArray[0];
                    // value
                    TX[10] = DataArray[1];
                    TX[11] = DataArray[0];
                    // CRC
                    if (CRC_IsEnable)
                    {
                        byte[] CRC = CRC_16.Calculate(TX, Polynom);
                        TX[12] = CRC[1];
                        TX[13] = CRC[0];
                    }
                }

                else if (ModbusType == TypeOfModbus.RTU)
                {
                    // Slave ID
                    TX[0] = SlaveID;
                    // Write 1 register
                    TX[1] = 0x06;
                    // address 
                    TX[2] = AddressArray[1];
                    TX[3] = AddressArray[0];
                    // value
                    TX[4] = DataArray[1];
                    TX[5] = DataArray[0];
                    // CRC
                    if (CRC_IsEnable)
                    {
                        byte[] CRC = CRC_16.Calculate(TX, Polynom);
                        TX[6] = CRC[1];
                        TX[7] = CRC[0];
                    }
                }

                Device.Send(TX);

                Device.Receive(RX);

                Response = DecodingOfArray(0x06, RX, NumberOfBytes);

                IsBusy = false;
            }
            
            catch(Exception error)
            {
                IsBusy = false;
                throw new Exception(error.Message);
            }
        }

        public UInt16 ReadRegister(UInt16 PackageNumber, UInt16 Address,
            out DEVICE_RESPONSE Response, int NumberOfBytes, TypeOfModbus ModbusType, bool CRC_IsEnable)
        {
            try
            {
                while (IsBusy) ;

                IsBusy = true;

                byte[] TX;

                if (CRC_IsEnable)
                {
                    TX = new byte[14];
                }

                else
                {
                    TX = new byte[12];
                }

                byte[] RX = new byte[20];

                byte[] AddressArray = BitConverter.GetBytes(Address);
                byte[] PackageNumberArray = BitConverter.GetBytes(PackageNumber);
                byte[] NumberOfBytesArray = BitConverter.GetBytes(NumberOfBytes);

                if (ModbusType == TypeOfModbus.TCP)
                {
                    // Номер транзакции
                    TX[0] = PackageNumberArray[1];
                    TX[1] = PackageNumberArray[0];
                    // Modbus ID
                    TX[2] = 0x00;
                    TX[3] = 0x00;
                    // Длина PDU
                    TX[4] = 0x00;
                    TX[5] = 0x06;
                    // Slave ID
                    TX[6] = SlaveID;
                    // Read 1 register
                    TX[7] = 0x04;
                    // address 
                    TX[8] = AddressArray[1];
                    TX[9] = AddressArray[0];
                    // value
                    TX[10] = NumberOfBytesArray[1];
                    TX[11] = NumberOfBytesArray[0];
                    // CRC
                    if (CRC_IsEnable)
                    {
                        byte[] CRC = CRC_16.Calculate(TX, Polynom);
                        TX[12] = CRC[1];
                        TX[13] = CRC[0];
                    }
                }
                
                else if (ModbusType == TypeOfModbus.RTU)
                {
                    // Slave ID
                    TX[0] = SlaveID;
                    // Read 1 register
                    TX[1] = 0x04;
                    // address 
                    TX[2] = AddressArray[1];
                    TX[3] = AddressArray[0];
                    // value
                    TX[4] = NumberOfBytesArray[1];
                    TX[5] = NumberOfBytesArray[0];
                    // CRC
                    if (CRC_IsEnable)
                    {
                        byte[] CRC = CRC_16.Calculate(TX, Polynom);
                        TX[6] = CRC[1];
                        TX[7] = CRC[0];
                    }
                }

                Device.Send(TX);

                Device.Receive(RX);

                Response = DecodingOfArray(0x04, RX, NumberOfBytes);

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

        private DEVICE_RESPONSE DecodingOfArray(int command, byte [] massive, int NumberOfBytes)
        {
            DEVICE_RESPONSE Decoding = new DEVICE_RESPONSE();

            byte[] temp = new byte[2];

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

            if (Decoding.Command > 128)
            {
                int FunctionCode = Decoding.Command - 128;

                Decoding.Data = new byte[NumberOfBytes];

                Array.Copy(massive, 8, Decoding.Data, 0, 1);

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

            if (command == 0x04)
            {
                Decoding.LengthOfData = Convert.ToString(massive[8]);

                Decoding.Data = new byte[NumberOfBytes];

                Array.Copy(massive, 9, Decoding.Data, 0, NumberOfBytes);
            }

            else if (command == 0x06)
            {
                Decoding.LengthOfData = "Not measured";

                Decoding.Data = new byte[NumberOfBytes];

                Array.Copy(massive, 10, Decoding.Data, 0, NumberOfBytes);
            }

            else
            {
                throw new Exception("Неизвестный код Modbus команды");
            }

            Decoding.Data = ReverseArray(Decoding.Data);

            return Decoding;
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
