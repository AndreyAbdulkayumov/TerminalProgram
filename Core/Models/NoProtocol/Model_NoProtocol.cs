using Core.Clients;
using System.Text;

namespace Core.Models.NoProtocol
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

    public class Model_NoProtocol
    {
        public double CycleMode_Period
        {
            get => CycleModeTimer.Interval; 
            set => CycleModeTimer.Interval = value;
        }

        public event EventHandler<NoProtocolDataReceivedEventArgs>? Model_DataReceived;
        public event EventHandler<string>? Model_ErrorInReadThread;
        public event EventHandler<string>? Model_ErrorInCycleMode;

        public Encoding HostEncoding => ConnectedHost.GlobalEncoding;

        private IConnection? _client;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private CycleModeParameters? _cycleModeInfo;

        private string[]? _outputArray;
        private int _resultIndex;

        private DateTime _outputDateTime;

        private bool _dateTime_IsUsed = false;

        private bool _date_IsUsed = false;

        private bool _time_IsUsed = false;
        private int _timeIndex;

        private string? CycleMessage;

        public Model_NoProtocol(ConnectedHost host)
        {
            host.DeviceIsConnect += Host_DeviceIsConnect;
            host.DeviceIsDisconnected += Host_DeviceIsDisconnected;

            CycleModeTimer = new System.Timers.Timer(IntervalDefault);
            CycleModeTimer.Elapsed += CycleModeTimer_Elapsed;
        }
        
        private void Host_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice != null && e.ConnectedDevice.IsConnected)
            {
                _client = e.ConnectedDevice;

                _client.DataReceived += Client_DataReceived;
                _client.ErrorInReadThread += Client_ErrorInReadThread;
            }
        }

        private void Host_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            _client = null;
        }     

        private void Client_DataReceived(object? sender, DataFromDevice e)
        {
            if (_outputArray != null)
            {
                if (_dateTime_IsUsed)
                {
                    _outputDateTime = DateTime.Now;

                    if (_date_IsUsed)
                    {
                        _outputArray[0] = _outputDateTime.ToShortDateString();
                    }

                    if (_time_IsUsed)
                    {
                        _outputArray[_timeIndex] = _outputDateTime.ToLongTimeString();
                    }
                }

                Model_DataReceived?.Invoke(this, new NoProtocolDataReceivedEventArgs(e.RX, _outputArray, _resultIndex));

                return;
            }

            Model_DataReceived?.Invoke(this, new NoProtocolDataReceivedEventArgs(e.RX));
        }

        private void Client_ErrorInReadThread(object? sender, string e)
        {
            CycleMode_Stop();

            Model_ErrorInReadThread?.Invoke(this, e);            
        }

        public async Task SendString(string? stringMessage, bool CR_Enable, bool LF_Enable)
        {
            if (_client == null)
            {
                throw new Exception("Клиент не инициализирован.");
            }

            if (string.IsNullOrEmpty(stringMessage))
            {
                throw new Exception("Буфер для отправления пуст. Введите отправляемое значение.");
            }

            var Message = new List<byte>(ConnectedHost.GlobalEncoding.GetBytes(stringMessage));

            if (CR_Enable == true)
            {
                Message.Add((byte)'\r');
            }

            if (LF_Enable == true)
            {
                Message.Add((byte)'\n');
            }

            await _client.Send(Message.ToArray(), Message.Count);
        }

        public async Task SendBytes(byte[]? bytes)
        {
            if (_client == null)
            {
                throw new Exception("Клиент не инициализирован.");
            }

            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("Буфер для отправления пуст. Введите отправляемое значение.");
            }

            await _client.Send(bytes, bytes.Length);
        }

        public async Task CycleMode_Start(CycleModeParameters info)
        {
            _cycleModeInfo = info;

            _outputArray = CreateOutputBuffer(info);

            if (_cycleModeInfo.IsByteString)
            {
                await SendBytes(info.MessageBytes);
            }

            else
            {
                CycleMessage = ConnectedHost.GlobalEncoding.GetString(info.MessageBytes);

                await SendString(CycleMessage,
                    info.Message_CR_Enable,
                    info.Message_LF_Enable);
            }

            CycleModeTimer.Start();
        }

        private string[] CreateOutputBuffer(CycleModeParameters info)
        {
            var RX = new List<string>();

            _resultIndex = 0;

            _date_IsUsed = false;

            _time_IsUsed = false;
            _timeIndex = 0;                       

            if (info.Response_Date_Enable)
            {
                RX.Add(String.Empty);   // Элемент для даты

                _resultIndex++;

                _dateTime_IsUsed = true;

                _date_IsUsed = true;

                _timeIndex++;
            }

            if (info.Response_Time_Enable)
            {
                RX.Add(String.Empty);   // Элемент для времени

                _resultIndex++;

                _dateTime_IsUsed = true;

                _time_IsUsed = true;
            }

            if (info.Response_String_Start_Enable)
            {
                // Элемент для пользовательской строки в начале
                RX.Add(info.Response_String_Start == null ? String.Empty : info.Response_String_Start);
                _resultIndex++;
            }

            // Элемент для принимаемых данных
            RX.Add(String.Empty);

            if (info.Response_String_End_Enable)
            {
                // Элемент для пользовательской строки в конце
                RX.Add(info.Response_String_End == null ? String.Empty : info.Response_String_End);
            }

            // Два элемента для специальных символов

            if (info.Response_NextLine_Enable)
            {
                RX.Add(Environment.NewLine);
            }

            return RX.ToArray();
        }

        public void CycleMode_Stop()
        {
            CycleModeTimer.Stop();

            _outputArray = null;
        }

        private async void CycleModeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_cycleModeInfo == null)
                {
                    throw new Exception("Нет данных для отправки.");
                }

                if (_cycleModeInfo.IsByteString)
                {
                    await SendBytes(_cycleModeInfo.MessageBytes);
                    return;
                }

                await SendString(CycleMessage,
                    _cycleModeInfo.Message_CR_Enable,
                    _cycleModeInfo.Message_LF_Enable);
            }

            catch(Exception error)
            {
                CycleMode_Stop();

                Model_ErrorInCycleMode?.Invoke(this, "Ошибка отправки команды в цикличном опросе.\n\n" + error.Message +
                    "\n\nОпрос остановлен.");
            }                        
        }
    }
}
