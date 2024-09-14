using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ViewModels.ModbusClient.WriteFields.DataItems;

namespace ViewModels.ModbusClient.WriteFields
{
    public class MultipleRegisters_VM : ReactiveObject, IWriteField_VM
    {
        private ObservableCollection<MultipleRegisters_Item> _writeDataCollection = new ObservableCollection<MultipleRegisters_Item>();

        public ObservableCollection<MultipleRegisters_Item> WriteDataCollection
        {
            get => _writeDataCollection;
            set => this.RaiseAndSetIfChanged(ref _writeDataCollection, value);
        }

        public ReactiveCommand<Unit, Unit> Command_AddRegister { get; }


        public MultipleRegisters_VM()
        {
            Command_AddRegister = ReactiveCommand.Create(() =>
            {
                WriteDataCollection.Add(new MultipleRegisters_Item(
                    canRemove: true,
                    startAddressAddition: WriteDataCollection.Count,
                    data: 0,
                    dataFormat: "hex",
                    removeItemHandler: RemoveWriteDataItem
                    ));
            });
        }

        public WriteData GetData()
        {
            byte[] data = WriteDataCollection.SelectMany(x => new byte[]
            {
                (byte)(x.Data & 0xFF),       // младший байт
                (byte)((x.Data >> 8) & 0xFF) // старший байт
            })
            .ToArray();

            return new WriteData(data, data.Length);
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
