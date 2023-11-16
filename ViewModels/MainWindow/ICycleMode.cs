using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.MainWindow
{
    internal interface ICycleMode
    {
        event EventHandler<EventArgs>? DeviceIsDisconnected;

        void SourceWindowClosingAction();

        void Start_Stop_Handler();
    }
}
