using Core.Models.Settings;

namespace ViewModels.FloatNumber
{
    public static class FloatHelper
    {
        public static byte[] GetBytesFromFloatNumber(float floatNumber, FloatNumberFormat floatFormat)
        {
            byte[] bytes = BitConverter.GetBytes(floatNumber);

            Array.Reverse(bytes); // т.к. в протоколе Modbus используется передача данных старшим байтом вперед.

            byte[] floatBytes;

            switch (floatFormat)
            {
                case FloatNumberFormat.AB_CD:
                    floatBytes = [bytes[0], bytes[1], bytes[2], bytes[3]];
                    break;

                case FloatNumberFormat.BA_DC:
                    floatBytes = [bytes[1], bytes[0], bytes[3], bytes[2]];
                    break;

                case FloatNumberFormat.CD_AB:
                    floatBytes = [bytes[2], bytes[3], bytes[0], bytes[1]];
                    break;

                case FloatNumberFormat.DC_BA:
                    floatBytes = [bytes[3], bytes[2], bytes[1], bytes[0]];
                    break;

                default:
                    throw new Exception("Неизвестный тип формата записи числа типа float.");
            }

            return floatBytes;
        }

        public static float GetFloatNumberFromBytes(byte[] bytes, FloatNumberFormat floatFormat)
        {
            byte[] formattedBytes;

            switch (floatFormat)
            {
                case FloatNumberFormat.AB_CD:
                    formattedBytes = [bytes[0], bytes[1], bytes[2], bytes[3]];
                    break;

                case FloatNumberFormat.BA_DC:
                    formattedBytes = [bytes[1], bytes[0], bytes[3], bytes[2]];
                    break;

                case FloatNumberFormat.CD_AB:
                    formattedBytes = [bytes[2], bytes[3], bytes[0], bytes[1]];
                    break;

                case FloatNumberFormat.DC_BA:
                    formattedBytes = [bytes[3], bytes[2], bytes[1], bytes[0]];
                    break;

                default:
                    throw new Exception("Неизвестный тип формата записи числа типа float.");
            }

            return BitConverter.ToSingle(formattedBytes, 0);
        }

        public static FloatNumberFormat GetFloatNumberFormatOrDefault(string? formatName)
        {
            switch (formatName)
            {
                case DeviceData.FloatWriteFormat_AB_CD:
                    return FloatNumberFormat.AB_CD;

                case DeviceData.FloatWriteFormat_BA_DC:
                    return FloatNumberFormat.BA_DC;

                case DeviceData.FloatWriteFormat_CD_AB:
                    return FloatNumberFormat.CD_AB;

                case DeviceData.FloatWriteFormat_DC_BA:
                    return FloatNumberFormat.DC_BA;

                default:
                    return FloatNumberFormat.BA_DC;
            }
        }
    }
}
