﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TerminalProgram.Protocols.Modbus
{

    public static class CRC_16
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
        public static byte[] Calculate(byte[] Message, ushort Polynom)
        {
            // выдаваемый массив CRC
            byte[] CRC = new byte[2];
            ushort Register = 0xFFFF; // создаем регистр, в котором будем сохранять высчитанный CRC
            //ushort Polynom = 0xA001; // Указываем полином, он может быть как 0xA001(старший бит справа), так и его реверс 0x8005(старший бит слева, здесь не рассматривается), при сдвиге вправо используется 0xA001

            for (int i = 0; i < Message.Length; i++) // для каждого байта в принятом\отправляемом сообщении проводим следующие операции(байты сообщения без принятого CRC)
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
                        
            CRC[0] = (byte)(Register & 0x00FF); // присваеваем младший байт 
            CRC[1] = (byte)(Register >> 8); // присваеваем старший байт

            return CRC;
        }
    }
}
