using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ViewModels.ModbusClient.WriteFields
{
    public class MultipleRegisters_VM : ReactiveObject, IWriteField_VM
    {
        private ObservableCollection<ModbusClient_WriteData_VM> _writeDataCollection = new ObservableCollection<ModbusClient_WriteData_VM>();

        public ObservableCollection<ModbusClient_WriteData_VM> WriteDataCollection
        {
            get => _writeDataCollection;
            set => this.RaiseAndSetIfChanged(ref _writeDataCollection, value);
        }

        public ReactiveCommand<Unit, Unit> Command_AddRegister { get; }


        public MultipleRegisters_VM()
        {
            Command_AddRegister = ReactiveCommand.Create(() =>
            {
                WriteDataCollection.Add(new ModbusClient_WriteData_VM(
                    canRemove: true,
                    startAddressAddition: WriteDataCollection.Count,
                    data: 0,
                    dataFormat: "hex",
                    removeItemHandler: RemoveWriteDataItem
                    ));
            });
        }

        public UInt16[] GetData()
        {
            return WriteDataCollection.Select(x => x.Data).ToArray();
        }

        private void RemoveWriteDataItem(Guid selectedId)
        {
            int AddressCounter = 0;

            var newCollection = WriteDataCollection
                .Where(e => e.Id != selectedId)
                .ToList();

            newCollection.ForEach(e =>
            {
                e.StartAddressAddition = "+" + AddressCounter.ToString();

                AddressCounter++;
            });

            WriteDataCollection.Clear();
            WriteDataCollection.AddRange(newCollection);
        }
    }
}
