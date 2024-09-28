using Core.Clients;

namespace Core.Models.NoProtocol
{
    public class CycleModeParameters
    {
        public string? Message;

        public bool Message_CR_Enable = false;
        public bool Message_LF_Enable = false;

        public bool Response_Date_Enable = false;
        public bool Response_Time_Enable = false;

        public bool Response_String_Start_Enable = false;
        public string? Response_String_Start;

        public bool Response_String_End_Enable = false;
        public string? Response_String_End;

        public bool Response_CR_Enable = false;
        public bool Response_LF_Enable = false;
    }

    public class Model_NoProtocol
    {
        public double CycleMode_Period
        {
            get => CycleModeTimer.Interval; 
            set => CycleModeTimer.Interval = value;
        }

        public event EventHandler<string>? Model_DataReceived;
        public event EventHandler<string>? Model_ErrorInReadThread;
        public event EventHandler<string>? Model_ErrorInCycleMode;

        private IConnection? _client;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private CycleModeParameters? _cycleModeInfo;

        private const string SpaceString = "  ";

        private string[]? _outputArray;
        private int _resultIndex;

        private DateTime _outputDateTime;

        private bool _dateTime_IsUsed = false;

        private bool _date_IsUsed = false;

        private bool _time_IsUsed = false;
        private int _timeIndex;

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

                _outputArray[_resultIndex] = ConnectedHost.GlobalEncoding.GetString(e.RX);

                Model_DataReceived?.Invoke(this, string.Concat(_outputArray));
            }

            else
            {
                Model_DataReceived?.Invoke(this, ConnectedHost.GlobalEncoding.GetString(e.RX));
            }
        }

        private void Client_ErrorInReadThread(object? sender, string e)
        {
            CycleMode_Stop();

            Model_ErrorInReadThread?.Invoke(this, e);            
        }

        public async Task Send(string? stringMessage, bool CR_Enable, bool LF_Enable)
        {
            if (_client == null)
            {
                throw new Exception("Клиент не инициализирован.");
            }

            if (stringMessage == null || stringMessage == String.Empty)
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

        public async void CycleMode_Start(CycleModeParameters info)
        {
            _cycleModeInfo = info;

            _outputArray = CreateOutputBuffer(info);

            await Send(info.Message,
                info.Message_CR_Enable,
                info.Message_LF_Enable);

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
                RX.Add(SpaceString);

                _resultIndex += 2;

                _dateTime_IsUsed = true;

                _date_IsUsed = true;

                _timeIndex += 2;
            }

            if (info.Response_Time_Enable)
            {
                RX.Add(String.Empty);   // Элемент для времени
                RX.Add(SpaceString);

                _resultIndex += 2;

                _dateTime_IsUsed = true;

                _time_IsUsed = true;
            }

            if (info.Response_String_Start_Enable)
            {
                // Элемент для пользовательской строки в начале
                RX.Add((info.Response_String_Start == null ? String.Empty : info.Response_String_Start) + SpaceString);
                _resultIndex++;
            }

            // Элемент для принимаемых данных
            RX.Add(String.Empty);

            if (info.Response_String_End_Enable)
            {
                // Элемент для пользовательской строки в конце
                RX.Add(SpaceString + (info.Response_String_End == null ? String.Empty : info.Response_String_End));
            }

            // Два элемента для специальных символов

            if (info.Response_CR_Enable)
            {
                RX.Add("\r");
            }

            if (info.Response_LF_Enable)
            {
                RX.Add("\n");
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
                if (_cycleModeInfo != null)
                {
                    await Send(_cycleModeInfo.Message,
                        _cycleModeInfo.Message_CR_Enable,
                        _cycleModeInfo.Message_LF_Enable);
                }
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
