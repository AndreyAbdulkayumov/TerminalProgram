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

        private IConnection? Client;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private CycleModeParameters? CycleModeInfo;

        private string[]? OutputArray;
        private int ResultIndex;

        private DateTime OutputDateTime;

        private bool DateTime_IsNeed = false;

        private bool Output_Date = false;

        private bool Output_Time = false;
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

        private void Client_ErrorInReadThread(object? sender, string e)
        {
            Model_ErrorInReadThread?.Invoke(this, e);
        }

        private void Client_DataReceived(object? sender, DataFromDevice e)
        {
            if (OutputArray != null)
            {
                if (DateTime_IsNeed)
                {
                    OutputDateTime = DateTime.Now;

                    if (Output_Date)
                    {
                        OutputArray[0] = OutputDateTime.ToShortDateString();
                    }

                    if (Output_Time)
                    {
                        OutputArray[TimeIndex] = OutputDateTime.ToLongTimeString();
                    }
                }

                OutputArray[ResultIndex] = ConnectedHost.GlobalEncoding.GetString(e.RX);

                Model_DataReceived?.Invoke(this, string.Concat(OutputArray));
            }            
        }

        private void Host_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            Client = null;
        }

        public void Send(string? StringMessage, bool CR_Enable, bool LF_Enable)
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

            Client.Send(Message.ToArray(), Message.Count);
        }

        public void CycleMode_Start(CycleModeParameters Info)
        {
            CycleModeInfo = Info;

            List<string> RX = new List<string>();

            ResultIndex = 0;

            Output_Date = false;

            Output_Time = false;
            TimeIndex = 0;

            string SpaceString = "  ";

            if (Info.Response_Date_Enable)
            {
                RX.Add(String.Empty);
                RX.Add(SpaceString);

                ResultIndex += 2;

                DateTime_IsNeed = true;

                Output_Date = true;

                TimeIndex += 2;
            }

            if (Info.Response_Time_Enable)
            {
                RX.Add(String.Empty);
                RX.Add(SpaceString);

                ResultIndex += 2;

                DateTime_IsNeed = true;

                Output_Time = true;
            }

            if (Info.Response_String_Start_Enable)
            {
                RX.Add((Info.Response_String_Start == null ? "" : Info.Response_String_Start) + SpaceString);
                ResultIndex++;
            }

            RX.Add("");

            if (Info.Response_String_End_Enable)
            {
                RX.Add(SpaceString + (Info.Response_String_End == null ? "" : Info.Response_String_End));
            }

            if (Info.Response_CR_Enable)
            {
                RX.Add("\r");
            }

            if (Info.Response_LF_Enable)
            {
                RX.Add("\n");
            }

            OutputArray = RX.ToArray();

            Send(Info.Message,
                Info.Message_CR_Enable,
                Info.Message_LF_Enable);
                        
            CycleModeTimer.Start();
        }

        public void CycleMode_Stop()
        {
            CycleModeTimer.Stop();
        }

        private void CycleModeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (CycleModeInfo != null)
            {
                Send(CycleModeInfo.Message,
                    CycleModeInfo.Message_CR_Enable,
                    CycleModeInfo.Message_LF_Enable);
            }            
        }
    }
}
