using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Modbus.Message
{
    public class ModbusASCII_Message : ModbusMessage
    {
        public override string ProtocolName { get; } = "Modbus ASCII";

        public override byte[] CreateMessage(ModbusFunction Function, MessageData Data)
        {
            byte[] PDU = Modbus_PDU.Create(Function, Data);

            // В этом массиве содержится SlaveID + PDU
            byte[] MainPart = new byte[1 + PDU.Length];

            // Slave ID
            MainPart[0] = Data.SlaveID;

            Array.Copy(PDU, 0, MainPart, 1, PDU.Length);

            byte[] MainPart_ASCII = ConvertArrayToASCII(MainPart);

            byte[] TX;

            if (Data.CheckSum_IsEnable)
            {
                TX = new byte[5 + MainPart_ASCII.Length];
            }

            else
            {
                TX = new byte[3 + MainPart_ASCII.Length];
            }

            // Символ начала кадра (префикс)
            TX[0] = 0x3A;

            Array.Copy(MainPart_ASCII, 0, TX, 1, MainPart_ASCII.Length);

            // LRC8
            if (Data.CheckSum_IsEnable)
            {
                byte[] LRC8 = CheckSum.Calculate_LRC8(MainPart);
                TX[TX.Length - 4] = LRC8[0];
                TX[TX.Length - 3] = LRC8[1];
            }

            // Символы конца кадра
            TX[TX.Length - 2] = 0x0D;  // Предпоследний элемент
            TX[TX.Length - 1] = 0x0A;  // Последний элемент

            return TX;
        }

        public override ModbusResponse DecodingMessage(ModbusFunction Function, byte[] SourceArray)
        {
            int SizeOfArray = 0;

            for (int i = 0; i < SourceArray.Length; i++)
            {
                if (i + 1 <= SourceArray.Length)
                {
                    // Спец. символы 0x0D и 0x0A встречаются только в конце значимой части массива
                    if (SourceArray[i] == 0x0D && SourceArray[i + 1] == 0x0A) 
                    {
                        SizeOfArray = i + 2;
                        break;
                    }
                }
                
                else
                {
                    SizeOfArray = SourceArray.Length;
                    break;
                }
            }

            byte[] SplitArray = new byte[SizeOfArray];

            Array.Copy(SourceArray, 0, SplitArray, 0, SizeOfArray);

            byte[] MainPart = new byte[SplitArray.Length - 5];

            Array.Copy(SplitArray, 1, MainPart, 0, MainPart.Length);

            byte[] ConvertedArray = ConvertArrayToBytes(MainPart);

            ModbusResponse DecodingResponse = new ModbusResponse
            {
                SlaveID = ConvertedArray[0],
                Command = ConvertedArray[1]
            };

            CheckErrorCode(TypeOfModbus.ASCII, ref DecodingResponse, ConvertedArray);

            if (Function is ModbusReadFunction)
            {
                DecodingResponse.LengthOfData = ConvertedArray[2];

                if (DecodingResponse.LengthOfData == 0)
                {
                    throw new Exception("Длина информационной части пакета равна 0.\n" +
                        "Код функции: " + Function.Number.ToString() + "\n" +
                        "Возможно нарушение целостности пакета Modbus ASCII.");
                }

                DecodingResponse.Data = new byte[DecodingResponse.LengthOfData];

                // Согласно документации на протокол Modbus:
                // В ответном пакете Modbus ASCII на команды чтения
                // информационная часть начинается с 3 байта.

                Array.Copy(ConvertedArray, 3, DecodingResponse.Data, 0, DecodingResponse.LengthOfData);

                DecodingResponse.Data = ReverseLowAndHighBytes(DecodingResponse.Data);
            }

            else if (Function is ModbusWriteFunction)
            {
                DecodingResponse.LengthOfData = -1;
            }

            else
            {
                throw new Exception("Неподдерживаемый код Modbus команды (Код: " + Function.Number + ")");
            }

            return DecodingResponse;
        }

        private byte[] ConvertArrayToASCII(byte[] Bytes_Array)
        {
            // В Modbus ASCII один байт представлен двумя ASCII символами
            char[] ASCII_Array = new char[Bytes_Array.Length * 2];

            string Element;

            for (int i = 0; i < Bytes_Array.Length; i++)
            {
                Element = Bytes_Array[i].ToString("X2");  // Представление двух разрядов числа в шестнацатеричном виде

                ASCII_Array[i * 2] = Element.First();
                ASCII_Array[i * 2 + 1] = Element.Last();
            }

            return Encoding.ASCII.GetBytes(ASCII_Array);
        }

        private byte[] ConvertArrayToBytes(byte[] Array)
        {
            char[] Chars_Array = Encoding.ASCII.GetChars(Array);

            string[] JoinChars_Array = new string[Chars_Array.Length / 2];

            // В Modbus ASCII один байт представлен двумя ASCII символами
            for (int i = 0; i < JoinChars_Array.Length; i++)
            {
                JoinChars_Array[i] = string.Concat(Chars_Array[i * 2], Chars_Array[i * 2 + 1]);
            }

            byte[] Bytes_Array = JoinChars_Array.Where(x => x != null)
                .Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();

            return Bytes_Array;
        }
    }
}
