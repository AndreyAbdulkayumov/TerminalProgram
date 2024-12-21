using System.Text;
using System.Text.RegularExpressions;

namespace ViewModels.Helpers
{
    public static class StringByteConverter
    {
        public static string GetMessageString(string message, bool isBytesString, Encoding encoding)
        {
            if (isBytesString)
            {
                return string.Join(" ", encoding.GetBytes(message).Select(x => x.ToString("X2"))); ;
            }

            return encoding.GetString(StringToBytes(message));
        }

        public static byte[] StringToBytes(string message)
        {
            message = message.Replace(" ", string.Empty);

            byte[] bytesToSend = new byte[message.Length / 2 + message.Length % 2];

            string byteString;

            for (int i = 0; i < bytesToSend.Length; i++)
            {
                if (i * 2 + 2 > message.Length)
                    byteString = "0" + message.Last();
                else
                    byteString = message.Substring(i * 2, 2);

                bytesToSend[i] = Convert.ToByte(byteString, 16);
            }
            return bytesToSend;
        }

        public static string GetValidatedByteString(string bytesString)
        {
            return Regex.Replace(bytesString, @"[^0-9a-fA-F\s]", string.Empty).ToUpper();
        }
    }
}
