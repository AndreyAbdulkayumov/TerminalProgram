﻿using System.Text;

namespace Core.Models.Modbus.Message
{
    public static class CheckSum
    {
        /* Принцип расчета CRC:
        * 1) задаем значение из единиц регистру Register = 0xFFFF, в этом значении будет сохраняться наш рассчитанный по каждому байту сообщения CRC.
        * 2) задаем нужный нам полином, в нашем случае это 0xA001
        * 3) запускаем цикл, по количеству байт в сообщении(сообщение всегда идет без полученного CRC, потому что расчитывается заново всегда)
        * 3-1) В цикле 3: делим регистр через xor на выбранный байт сообщения(начиная с первого)
        * 3-2) В цикле 3: запускаем вложенный цикл на 8 шагов(по количеству бит в байте),
        * в котором узнаем, является ли старший бит единицей:
        * Если является — сдвигаем регистр вправо на один бит, и делим по xor на полином
        * Если не является единицей — просто сдвигаем регистр вправо.
        *
        * По окончанию вложенного цикла происходит возврат к основному циклу, в котором последовательность повторяется для следующего байта в сообщении.
        *
        * По окончанию основного цикла содержимое регистра присваивается массиву CRC младшим байтом вперед и выдается в виде результата функции
        */
        /// <summary>
        /// Метод расчета CRC с указанным полиномом
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static byte[] Calculate_CRC16(byte[] Message, UInt16 Polynom = 0xA001)
        {            
            ushort Register = 0xFFFF; // создаем регистр, в котором будем сохранять высчитанный CRC
            //ushort Polynom = 0xA001; // Указываем полином, он может быть как 0xA001(старший бит справа), так и его реверс 0x8005(старший бит слева, здесь не рассматривается), при сдвиге вправо используется 0xA001

            // Два последних элемента массива предназначены для хранения байтов CRC.
            // Поэтому их не учитываем в расчете.
            for (int i = 0; i < Message.Length - 2; i++) // для каждого байта в принятом\отправляемом сообщении проводим следующие операции(байты сообщения без принятого CRC)
            {
                Register = (ushort)(Register ^ Message[i]); // Делим через XOR регистр на выбранный байт сообщения(от младшего к старшему)

                for (int j = 0; j < 8; j++) // для каждого бита в выбранном байте делим полученный регистр на полином
                {
                    if ((ushort)(Register & 0x01) == 1) //если старший бит равен 1 то
                    {
                        Register = (ushort)(Register >> 1); //сдвигаем на один бит вправо
                        Register = (ushort)(Register ^ Polynom); //делим регистр на полином по XOR
                    }
                    else //если старший бит равен 0 то
                    {
                        Register = (ushort)(Register >> 1); // сдвигаем регистр вправо
                    }
                }
            }

            // выдаваемый массив CRC
            byte[] CRC16 = new byte[2];

            CRC16[0] = (byte)(Register & 0x00FF); // присваеваем младший байт 
            CRC16[1] = (byte)(Register >> 8); // присваеваем старший байт

            return CRC16;
        }

        public static byte[] Calculate_LRC8(byte[] MainPart)
        {
            byte LRC8 = 0;

            foreach (byte Element in MainPart)
            {
                LRC8 += Element;
            }

            LRC8 = (byte)((LRC8 ^ 0xFF) + 1);

            string LRC8_String = LRC8.ToString("X2");

            char[] LRC8_Array = new char[2];

            LRC8_Array[0] = LRC8_String.First();
            LRC8_Array[1] = LRC8_String.Last();

            return Encoding.ASCII.GetBytes(LRC8_Array);
        }
    }
}
