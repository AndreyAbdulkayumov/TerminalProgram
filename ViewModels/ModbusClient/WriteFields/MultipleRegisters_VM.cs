using Core.Models.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using ViewModels.FloatNumber;
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

        private readonly Model_Settings SettingsFile;

        public MultipleRegisters_VM()
        {
            SettingsFile = Model_Settings.Model;

            Command_AddRegister = ReactiveCommand.Create(() =>
            {
                int addressAddition;

                if (WriteDataCollection.Count == 0)
                {
                    addressAddition = 0;
                }

                else
                {
                    int formatAddition = WriteDataCollection.Last().SelectedDataFormat == ModbusDataFormatter.DataFormatName_float ? 2 : 1;
                    addressAddition = WriteDataCollection.Last().StartAddressAddition + formatAddition;
                }

                WriteDataCollection.Add(new MultipleRegisters_Item(
                    startAddressAddition: addressAddition,
                    removeItemHandler: RemoveWriteDataItem
                    ));

                WriteDataCollection.Last().RequestToUpdateAddresses += Item_RequestToUpdateAddresses;
            });
        }

        public WriteData GetData()
        {
            if (WriteDataCollection.Count == 0)
            {
                return new WriteData(Array.Empty<byte>(), 0);
            }

            FloatNumberFormat floatFormat = FloatHelper.GetFloatNumberFormatOrDefault(SettingsFile.Settings?.FloatNumberFormat);

            int registerCounter = 0;

            byte[] data = WriteDataCollection.SelectMany(x =>
            {
                if (x.DataFormat == NumberStyles.Float)
                {
                    registerCounter += 2;

                    return FloatHelper.GetBytesFromFloatNumber(x.FloatData, floatFormat);
                }

                registerCounter++;

                return BitConverter.GetBytes(x.Data);
            })
            .ToArray();

            return new WriteData(data, registerCounter);
        }

        private void Item_RequestToUpdateAddresses(object? sender, RequestToUpdateAddressesArgs e)
        {
            int itemIndex = WriteDataCollection.ToList().FindIndex(item => item.Id == e.ItemId);

            if (itemIndex < WriteDataCollection.Count - 1)
            {
                UpdateAddresses();
            }
        }

        private void RemoveWriteDataItem(Guid selectedId)
        {
            int itemIndex = WriteDataCollection.ToList().FindIndex(item => item.Id == selectedId);

            WriteDataCollection.RemoveAt(itemIndex);

            UpdateAddresses();
        }        

        private void UpdateAddresses()
        {
            int AddressCounter = 0;

            foreach (var item in WriteDataCollection)
            {
                item.StartAddressAddition = AddressCounter;

                if (item.SelectedDataFormat == ModbusDataFormatter.DataFormatName_float)
                {
                    AddressCounter += 2;
                    continue;
                }

                AddressCounter += 1;
            }
        }        
    }
}
