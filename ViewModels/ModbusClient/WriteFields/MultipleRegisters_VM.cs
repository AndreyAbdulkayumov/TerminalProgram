using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
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

        public bool HasValidationErrors => WriteDataCollection.Any(e => e.HasErrors);
        public string? ValidationMessage => string.Join("\n\n",
            WriteDataCollection
                .Select((field, index) => new { Field = field, Index = index + 1 })
                .Where(item => item.Field.HasErrors)
                .SelectMany(item =>
                    item.Field.ActualErrors.Select(element => $"[Поле записи данных №{item.Index}]\n{item.Field.GetFullErrorMessage(element.Key)}")
                )
        );

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
            int registerCounter = 0;

            byte[] data = WriteDataCollection.SelectMany(x => 
            {
                if (x.DataFormat == NumberStyles.Float)
                {
                    registerCounter += 2;

                    byte[] floatBytes = BitConverter.GetBytes(x.FloatData);

                    for(int i = 0; i < floatBytes.Length - 1; i += 2)
                    {
                        SwapBytes(ref floatBytes[i], ref floatBytes[i + 1]);
                    }

                    return floatBytes;
                }

                registerCounter++;

                return new byte[]
                {
                    (byte)(x.Data & 0xFF),       // младший байт
                    (byte)((x.Data >> 8) & 0xFF) // старший байт
                };
            })
            .ToArray();

            return new WriteData(data, registerCounter);
        }

        private void SwapBytes(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
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
