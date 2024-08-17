using ViewModels.ModbusClient.DataTypes;

namespace ViewModels.ModbusClient.ModbusRepresentations
{
    internal static class BinaryRepresentation
    {
        public static List<BinaryRepresentation_ItemData>? GetData(ModbusDataDisplayed Data)
        {
            var Words = Data.Data?.Select(e => Convert.ToString(e, 2).PadLeft(16, '0'));

            if (Words == null)
                return null;

            List<BinaryRepresentation_ItemData> Items = new List<BinaryRepresentation_ItemData>();

            ushort CurrentAddress = Data.Address;

            foreach (string element in Words)
            {
                Items.Add(GetBinaryRepresentation(CurrentAddress, element));
                CurrentAddress += 1;
            }

            return Items;
        }

        private static BinaryRepresentation_ItemData GetBinaryRepresentation(ushort Address, string InputData)
        {
            char[] BitsValue = InputData.ToCharArray();

            List<BinaryDataItemGroup> ItemGroup = new List<BinaryDataItemGroup>();

            List<BinaryDataItem> Items = new List<BinaryDataItem>();

            for (int i = 0; i < BitsValue.Length; i++)
            {
                BinaryDataItem Item = new BinaryDataItem()
                {
                    Bit = BitsValue[i].ToString(),
                    IsChange = false
                };

                Items.Add(Item);

                if ((i + 1) % 4 == 0)
                {
                    ItemGroup.Add(new BinaryDataItemGroup() { GroupData = Items.ToArray() });
                    Items.Clear();
                }
            }

            return new BinaryRepresentation_ItemData()
            {
                Address = "0x" + Address.ToString("X2"),
                BinaryData = ItemGroup.ToArray()
            };
        }
    }
}
