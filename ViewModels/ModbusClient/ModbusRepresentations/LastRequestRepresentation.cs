using ViewModels.ModbusClient.DataTypes;

namespace ViewModels.ModbusClient.ModbusRepresentations
{
    internal static class LastRequestRepresentation
    {
        public static RequestResponseField_ItemData[] GetData(string[] RequestBytes, string[] ResponseBytes)
        {
            int MaxLength = RequestBytes.Length > ResponseBytes.Length ? RequestBytes.Length : ResponseBytes.Length;

            RequestResponseField_ItemData[] Items = new RequestResponseField_ItemData[MaxLength];

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new RequestResponseField_ItemData();
                Items[i].ItemNumber = (i + 1).ToString();
            }

            for (int i = 0; i < RequestBytes.Length; i++)
            {
                Items[i].RequestDataType = i.ToString() + "X";
                Items[i].RequestData = RequestBytes[i];
            }

            for (int i = 0; i < ResponseBytes.Length; i++)
            {
                Items[i].ResponseDataType = i.ToString() + "Y";
                Items[i].ResponseData = ResponseBytes[i];
            }

            return Items;
        }
    }
}
