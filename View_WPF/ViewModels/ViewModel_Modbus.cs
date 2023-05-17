using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Models;
using Core.Models.Modbus;
using ReactiveUI;

namespace View_WPF.ViewModels
{
    public class ViewModel_Modbus : ReactiveObject
    {
        #region Properties



        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> Command_Write { get; }
        public ReactiveCommand<Unit, Unit> Command_Read { get; }

        #endregion

        private readonly ConnectedHost Model;

        private readonly ViewMessage Message;
        private readonly StateUI_Connected SetUI_Connected;
        private readonly StateUI_Disconnected SetUI_Disconnected;

        public ViewModel_Modbus(
            ViewMessage MessageBox,
            StateUI_Connected UI_Connected_Handler,
            StateUI_Disconnected UI_Disconnected_Handler)
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected?.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            SetUI_Disconnected?.Invoke();
        }
    }
}
