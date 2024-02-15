using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private IConnection? Client;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private CycleModeParameters? CycleModeInfo;

        private const string SpaceString = "  ";

        private string[]? OutputArray;
        private int ResultIndex;

        private DateTime OutputDateTime;

        private bool DateTime_IsUsed = false;

        private bool Date_IsUsed = false;

        private bool Time_IsUsed = false;
        private int TimeIndex;

        public Model_NoProtocol(ConnectedHost Host)
        {
            Host.DeviceIsConnect += Host_DeviceIsConnect;
            Host.DeviceIsDisconnected += Host_DeviceIsDisconnected;

            CycleModeTimer = new System.Timers.Timer(IntervalDefault);
            CycleModeTimer.Elapsed += CycleModeTimer_Elapsed;
        }
        
        private void Host_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice != null && e.ConnectedDevice.IsConnected)
            {
                Client = e.ConnectedDevice;

                Client.DataReceived += Client_DataReceived;
                Client.ErrorInReadThread += Client_ErrorInReadThread;
            }
        }

        private void Host_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            Client = null;
        }     

        private void Client_DataReceived(object? sender, DataFromDevice e)
        {
            if (OutputArray != null)
            {
                if (DateTime_IsUsed)
                {
                    OutputDateTime = DateTime.Now;

                    if (Date_IsUsed)
                    {
                        OutputArray[0] = OutputDateTime.ToShortDateString();
                    }

                    if (Time_IsUsed)
                    {
                        OutputArray[TimeIndex] = OutputDateTime.ToLongTimeString();
                    }
                }

                OutputArray[ResultIndex] = ConnectedHost.GlobalEncoding.GetString(e.RX);

                Model_DataReceived?.Invoke(this, string.Concat(OutputArray));
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

        public async Task Send(string? StringMessage, bool CR_Enable, bool LF_Enable)
        {
            if (Client == null)
            {
                throw new Exception("Клиент не инициализирован.");
            }

            if (StringMessage == null || StringMessage == String.Empty)
            {
                throw new Exception("Буфер для отправления пуст. Введите отправляемое значение.");
            }

            List<byte> Message = new List<byte>(ConnectedHost.GlobalEncoding.GetBytes(StringMessage));

            if (CR_Enable == true)
            {
                Message.Add((byte)'\r');
            }

            if (LF_Enable == true)
            {
                Message.Add((byte)'\n');
            }

            await Client.Send(Message.ToArray(), Message.Count);
        }

        public async void CycleMode_Start(CycleModeParameters Info)
        {
            CycleModeInfo = Info;

            OutputArray = CreateOutputBuffer(Info);

            await Send(Info.Message,
                Info.Message_CR_Enable,
                Info.Message_LF_Enable);

            CycleModeTimer.Start();
        }

        private string[] CreateOutputBuffer(CycleModeParameters Info)
        {
            List<string> RX = new List<string>();

            ResultIndex = 0;

            Date_IsUsed = false;

            Time_IsUsed = false;
            TimeIndex = 0;                       

            if (Info.Response_Date_Enable)
            {
                RX.Add(String.Empty);   // Элемент для даты
                RX.Add(SpaceString);

                ResultIndex += 2;

                DateTime_IsUsed = true;

                Date_IsUsed = true;

                TimeIndex += 2;
            }

            if (Info.Response_Time_Enable)
            {
                RX.Add(String.Empty);   // Элемент для времени
                RX.Add(SpaceString);

                ResultIndex += 2;

                DateTime_IsUsed = true;

                Time_IsUsed = true;
            }

            if (Info.Response_String_Start_Enable)
            {
                // Элемент для пользовательской строки в начале
                RX.Add((Info.Response_String_Start == null ? String.Empty : Info.Response_String_Start) + SpaceString);
                ResultIndex++;
            }

            // Элемент для принимаемых данных
            RX.Add(String.Empty);

            if (Info.Response_String_End_Enable)
            {
                // Элемент для пользовательской строки в конце
                RX.Add(SpaceString + (Info.Response_String_End == null ? String.Empty : Info.Response_String_End));
            }

            // Два элемента для специальных символов

            if (Info.Response_CR_Enable)
            {
                RX.Add("\r");
            }

            if (Info.Response_LF_Enable)
            {
                RX.Add("\n");
            }

            return RX.ToArray();
        }

        public void CycleMode_Stop()
        {
            CycleModeTimer.Stop();

            OutputArray = null;
        }

        private async void CycleModeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (CycleModeInfo != null)
                {
                    await Send(CycleModeInfo.Message,
                        CycleModeInfo.Message_CR_Enable,
                        CycleModeInfo.Message_LF_Enable);
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
