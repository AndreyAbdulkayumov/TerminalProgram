namespace Core.Clients.DataTypes
{
    public class DataFromDevice : EventArgs
    {
        public readonly byte[] RX;

        public DataFromDevice(int RX_ArrayLength)
        {
            RX = new byte[RX_ArrayLength];
        }
    }
}
