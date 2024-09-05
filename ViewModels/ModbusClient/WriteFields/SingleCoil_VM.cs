using ReactiveUI;

namespace ViewModels.ModbusClient.WriteFields
{
    public class SingleCoil_VM : ReactiveObject, IWriteField_VM
    {
        private bool _logic_One;

        public bool Logic_One
        {
            get => _logic_One;
            set => this.RaiseAndSetIfChanged(ref _logic_One, value);
        }

        private bool _logic_Zero;

        public bool Logic_Zero
        {
            get => _logic_Zero;
            set => this.RaiseAndSetIfChanged(ref _logic_Zero, value);
        }

        private const UInt16 LogicOne_Value = 65280;  // 0xFF00
        private const UInt16 LogicZero_Value = 0;

        public SingleCoil_VM()
        {
            Logic_One = true;
        }

        public UInt16[] GetData()
        {
            return Logic_One ? [LogicOne_Value] : [LogicZero_Value];
        }
    }
}
