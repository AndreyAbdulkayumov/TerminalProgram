using MessageBox_Core;
using ViewModels.ModbusClient.DataTypes;

namespace ViewModels.ModbusClient.ModbusRepresentations
{
    internal static class BinaryRepresentation
    {
        public static List<BinaryRepresentation_ItemData>? GetData(ModbusDataDisplayed data, Action<string,
            MessageType> messageBox, Func<string, Task> copyToClipboard)
        {
            var words = data.Data?.Select(e => Convert.ToString(e, 2).PadLeft(16, '0'));

            if (words == null)
                return null;

            var items = new List<BinaryRepresentation_ItemData>();

            ushort currentAddress = data.Address;

            foreach (string element in words)
            {
                items.Add(GetBinaryRepresentation(currentAddress, element, messageBox, copyToClipboard));
                currentAddress += 1;
            }

            return items;
        }

        private static BinaryRepresentation_ItemData GetBinaryRepresentation(ushort address, string inputData, 
            Action<string, MessageType> messageBox, Func<string, Task> copyToClipboard)
        {
            char[] bitsValue = inputData.ToCharArray();

            var itemGroup = new List<BinaryDataItemGroup>();

            var items = new List<BinaryDataItem>();

            for (int i = 0; i < bitsValue.Length; i++)
            {
                var item = new BinaryDataItem()
                {
                    Bit = bitsValue[i].ToString(),
                    IsChange = false
                };

                items.Add(item);

                if ((i + 1) % 4 == 0)
                {
                    itemGroup.Add(new BinaryDataItemGroup(items.ToArray()));
                    items.Clear();
                }
            }

            return new BinaryRepresentation_ItemData(
                address: "0x" + address.ToString("X2"),
                binaryData: itemGroup.ToArray(),
                messageBox, 
                copyToClipboard);
        }
    }
}
