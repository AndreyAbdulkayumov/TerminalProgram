using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests.Modbus
{
    public static class ModbusField
    {
        public static byte[] Get_Address(UInt16 Address)
        {
            byte[] ArrayBytes = BitConverter.GetBytes(Address);

            if (ArrayBytes.Length != 2)
            {
                throw new Exception("Поле адреса имеет недопустимое количество байт: " + ArrayBytes.Length);
            }

            return ArrayBytes;
        }

        public static byte[] Get_NumberOfRegisters(UInt16 NumberOfRegisters)
        {
            byte[] ArrayBytes = BitConverter.GetBytes(NumberOfRegisters);

            if (ArrayBytes.Length != 2)
            {
                throw new Exception("Поле количества регистров имеет недопустимое количество байт: " + ArrayBytes.Length);
            }

            return ArrayBytes;
        }

        public static byte[] Get_WriteData(UInt16[] Data)
        {
            List<byte> ListBytes = new List<byte>();

            byte[] Temp;

            foreach (UInt16 element in Data)
            {
                Temp = BitConverter.GetBytes(element);

                ListBytes.Add(Temp[1]);
                ListBytes.Add(Temp[0]);
            }

            if (ListBytes.Count != Data.Length * 2)
            {
                throw new Exception("Поле данных имеет недопустимое количество байт: " + ListBytes.Count);
            }

            return ListBytes.ToArray();
        }
    }
}
