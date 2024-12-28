namespace Core.Models.NoProtocol.DataTypes
{
    public class NoProtocolDataReceivedEventArgs : EventArgs
    {
        public readonly byte[] RawData;
        public readonly string[]? DataWithDebugInfo;
        public int DataIndex = 0;

        public NoProtocolDataReceivedEventArgs(byte[] rawData)
        {
            RawData = rawData;
        }

        public NoProtocolDataReceivedEventArgs(byte[] rawData, string[]? dataWithDebugInfo, int dataIndex)
        {
            RawData = rawData;
            DataWithDebugInfo = dataWithDebugInfo;
            DataIndex = dataIndex;
        }
    }
}
