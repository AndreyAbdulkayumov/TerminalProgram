using Core.Models;
using Core.Models.NoProtocol;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TerminalProgram.Views;
using TerminalProgram.Views.Protocols;

namespace TerminalProgram.ViewModels.MainWindow
{
    internal class ViewModel_NoProtocol_CycleMode : ReactiveObject
    {
        #region Message

        private string _message_Content = String.Empty;

        public string Message_Content
        {
            get => _message_Content;
            set => this.RaiseAndSetIfChanged(ref _message_Content, value);
        }              

        private bool _message_CR = false;

        public bool Message_CR
        {
            get => _message_CR; 
            set => this.RaiseAndSetIfChanged(ref _message_CR, value);
        }

        private bool _message_LF = false;

        public bool Message_LF
        {
            get => _message_LF;
            set => this.RaiseAndSetIfChanged(ref _message_LF, value);
        }

        private int _message_Period_ms = 100;

        public int Message_Period_ms
        {
            get => _message_Period_ms;
            set => this.RaiseAndSetIfChanged(ref _message_Period_ms, value);
        }


        #endregion

        #region Response

        private bool _response_Date = false;

        public bool Response_Date
        {
            get => _response_Date;
            set => this.RaiseAndSetIfChanged(ref _response_Date, value);
        }

        private bool _response_Time = false;

        public bool Response_Time
        {
            get => _response_Time; 
            set => this.RaiseAndSetIfChanged(ref _response_Time, value);
        }

        private bool _response_String_Start_Enable = false;

        public bool Response_String_Start_Enable
        {
            get => _response_String_Start_Enable;
            set => this.RaiseAndSetIfChanged(ref _response_String_Start_Enable, value);
        }

        private string _response_String_Start = String.Empty;

        public string Response_String_Start
        {
            get => _response_String_Start;
            set => this.RaiseAndSetIfChanged(ref _response_String_Start, value);
        }

        private bool _response_String_End_Enable = false;

        public bool Response_String_End_Enable
        {
            get => _response_String_End_Enable;
            set => this.RaiseAndSetIfChanged(ref _response_String_End_Enable, value);
        }

        private string _response_String_End = String.Empty;

        public string Response_String_End
        {
            get => _response_String_End;
            set => this.RaiseAndSetIfChanged(ref _response_String_End, value);
        }

        private bool _response_CR = false;

        public bool Response_CR
        {
            get => _response_CR;
            set => this.RaiseAndSetIfChanged(ref _response_CR, value);
        }

        private bool _response_LF = false;

        public bool Response_LF
        {
            get => _response_LF;
            set => this.RaiseAndSetIfChanged(ref _response_LF, value);
        }

        #endregion

        #region Button

        private const string Button_Content_Start = "Начать опрос";
        private const string Button_Content_Stop = "Остановить опрос";

        private string _button_Content = Button_Content_Start;

        public string Button_Content
        {
            get => _button_Content;
            set => this.RaiseAndSetIfChanged(ref _button_Content, value);
        }

        public ReactiveCommand<Unit, Unit> Command_Start_Stop_Polling { get; }

        #endregion

        private bool IsStart = false;

        private readonly ConnectedHost Model;

        private readonly NoProtocol_CycleMode ParentWindow;
        private readonly Action<string, MessageType> Message;
        private readonly Action UI_State_Work;
        private readonly Action UI_State_Wait;

        public ViewModel_NoProtocol_CycleMode(
            NoProtocol_CycleMode ParentWindow,
            Action<string, MessageType> MessageBox,
            Action UI_State_Work,
            Action UI_State_Wait
            )
        {         
            this.ParentWindow = ParentWindow;

            Message = MessageBox;

            this.UI_State_Work = UI_State_Work;
            this.UI_State_Wait = UI_State_Wait;

            Model = ConnectedHost.Model;

            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_ErrorInCycleMode += NoProtocol_Model_ErrorInCycleMode;

            ParentWindow.Closing += (sender, e) => 
            { 
                Model.NoProtocol.CycleMode_Stop();
                Model.NoProtocol.Model_ErrorInCycleMode -= NoProtocol_Model_ErrorInCycleMode;
            };

            ParentWindow.KeyDown += ParentWindow_KeyDown_Handler;

            Command_Start_Stop_Polling = ReactiveCommand.Create(Start_Stop_Handler);
            Command_Start_Stop_Polling.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            this.UI_State_Wait.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            ParentWindow.Close();
        }

        private void ParentWindow_KeyDown_Handler(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        Start_Stop_Handler();
                        break;

                    case Key.Escape:
                        ParentWindow.Close();
                        break;
                }
            }

            catch (Exception error)
            {
                Message.Invoke(error.Message, MessageType.Error);
            }
        }

        private void NoProtocol_Model_ErrorInCycleMode(object? sender, string e)
        {
            Message.Invoke(e, MessageType.Error);
        }

        private void Start_Stop_Handler()
        {
            if (IsStart)
            {                
                Model.NoProtocol.CycleMode_Stop();

                UI_State_Wait.Invoke();

                Button_Content = Button_Content_Start;
                IsStart = false;
            }

            else
            {                
                Model.NoProtocol.CycleMode_Period = Message_Period_ms;

                CycleModeParameters Info = new CycleModeParameters()
                {
                    Message = Message_Content,

                    Message_CR_Enable = Message_CR,
                    Message_LF_Enable = Message_LF,

                    Response_Date_Enable = Response_Date,
                    Response_Time_Enable = Response_Time,

                    Response_String_Start_Enable = this.Response_String_Start_Enable,
                    Response_String_Start = this.Response_String_Start,

                    Response_String_End_Enable = this.Response_String_End_Enable,
                    Response_String_End = this.Response_String_End,

                    Response_CR_Enable = Response_CR,
                    Response_LF_Enable = Response_LF,
                };

                Model.NoProtocol.CycleMode_Start(Info);

                UI_State_Work.Invoke();

                Button_Content = Button_Content_Stop;
                IsStart = true;
            }
        }        
    }
}
