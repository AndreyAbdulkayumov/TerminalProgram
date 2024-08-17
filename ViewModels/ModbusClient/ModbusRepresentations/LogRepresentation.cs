namespace ViewModels.ModbusClient.ModbusRepresentations
{
    internal static class LogRepresentation
    {
        public static string GetData(DateTime Request_ExecutionTime, string RequestString, DateTime Response_ExecutionTime, string ResponseString)
        {
            string Data = string.Empty;

            if (RequestString != string.Empty)
            {
                Data += Request_ExecutionTime.ToString("HH : mm : ss . fff") + "   ->   " + RequestString;
            }

            if (ResponseString != string.Empty)
            {
                if (RequestString != string.Empty)
                {
                    Data += "\n";
                }

                Data += Response_ExecutionTime.ToString("HH : mm : ss . fff") + "   <-   " + ResponseString;
            }

            return Data;
        }
    }
}
