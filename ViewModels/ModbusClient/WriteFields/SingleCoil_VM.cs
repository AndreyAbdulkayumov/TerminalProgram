using ReactiveUI;
using ViewModels.Validation;

namespace ViewModels.ModbusClient.WriteFields
{
    public class SingleCoil_VM : ReactiveObject, IWriteField_VM
    {
        private bool _logic_Zero;

        public bool Logic_Zero
        {
            get => _logic_Zero;
            set => this.RaiseAndSetIfChanged(ref _logic_Zero, value);
        }

        private bool _logic_One;

        public bool Logic_One
        {
            get => _logic_One;
            set => this.RaiseAndSetIfChanged(ref _logic_One, value);
        }

        // У этого элемента нет полей ввода, поэтому он не может иметь ошибок валидации
        public bool HasValidationErrors => false;   
        public string? ValidationMessage => null;

        // 0xFF00
        private const byte LogicOne_Value_High = 0xFF;
        private const byte LogicOne_Value_Low = 0x00;

        private const byte LogicZero_Value_High = 0;
        private const byte LogicZero_Value_Low = 0;


        public SingleCoil_VM()
        {
            Logic_One = true;
        }

        public WriteData GetData()
        {
            byte[] data = Logic_One ? 
                [LogicOne_Value_Low, LogicOne_Value_High] : 
                [LogicZero_Value_Low, LogicZero_Value_High];

            return new WriteData(data, 1);
        }
    }
}
