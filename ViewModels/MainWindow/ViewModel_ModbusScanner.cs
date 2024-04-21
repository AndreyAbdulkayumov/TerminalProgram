using Core.Models;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.MainWindow
{
    public class ViewModel_ModbusScanner : ReactiveObject
    {
        private bool _searchInProcess = false;

        public bool SearchInProcess
        {
            get => _searchInProcess;
            set => this.RaiseAndSetIfChanged(ref _searchInProcess, value);
        }

        private string _slavesAddresses = string.Empty;

        public string SlavesAddresses
        {
            get => _slavesAddresses;
            set => this.RaiseAndSetIfChanged(ref _slavesAddresses, value);
        }

        private string _pdu_SearchRequest = string.Empty;

        public string PDU_SearchRequest
        {
            get => _pdu_SearchRequest;
            set => this.RaiseAndSetIfChanged(ref _pdu_SearchRequest, value);
        }

        public string PDU_SearchRequest_Default
        {
            get => "03 00 01 00 02";
        }

        private int _pollingPeriod;

        public int PollingPeriod
        {
            get => _pollingPeriod;
            set => this.RaiseAndSetIfChanged(ref _pollingPeriod, value);
        }

        private bool _searchAllSlaves;

        public bool SearchAllSlaves
        {
            get => _searchAllSlaves;
            set => this.RaiseAndSetIfChanged(ref _searchAllSlaves, value);
        }

        private const string ButtonContent_Start = "Начать поиск";
        private const string ButtonContent_Stop = "Остановить поиск";

        private string _actionButtonContent = ButtonContent_Start;

        public string ActionButtonContent
        {
            get => _actionButtonContent;
            set => this.RaiseAndSetIfChanged(ref _actionButtonContent, value);
        }

        private int _progressBar_Value;

        public int ProgressBar_Value
        {
            get => _progressBar_Value;
            set => this.RaiseAndSetIfChanged(ref _progressBar_Value, value);
        }

        // Минимальное значение адреса устройства Modbus.
        // Не берем в учет широковещательный адрес (SlaveId = 0).
        public int ProgressBar_Minimun
        {
            get => 1;
        }

        // Максимальное значение адреса устройства Modbus.
        public int ProgressBar_Maximun
        {
            get => 255;
        }

        public ReactiveCommand<Unit, Unit> Command_Start_Stop_Search { get; }

        private readonly Action<string, MessageType> MessageBox;

        private readonly ConnectedHost Model;

        private Task? SearchTask;
        private CancellationTokenSource? SearchCancel;


        public ViewModel_ModbusScanner(Action<string, MessageType> MessageBox)
        {
            this.MessageBox = MessageBox;

            Model = ConnectedHost.Model;

            Command_Start_Stop_Search = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SearchInProcess)
                {
                    await StopPolling();

                    return;
                }

                StartPolling();       
            });
            Command_Start_Stop_Search.ThrownExceptions.Subscribe(error => MessageBox.Invoke(error.Message, MessageType.Error));

            // Значения по умолчанию
            PollingPeriod = 100;
            SearchAllSlaves = false;
        }

        private void StartPolling()
        {
            ActionButtonContent = ButtonContent_Stop;
            SearchInProcess = true;

            SlavesAddresses = string.Empty;

            SearchCancel = new CancellationTokenSource();

            SearchTask = Task.Run(() => SearchDevices(SearchCancel.Token));
        }

        private async Task StopPolling()
        {
            SearchCancel?.Cancel();

            if (SearchTask != null)
            {
                await Task.WhenAny(SearchTask);
            }

            StopPolling_UI_Actions();
        }

        private void StopPolling_UI_Actions()
        {
            ActionButtonContent = ButtonContent_Start;
            SearchInProcess = false;

            ProgressBar_Value = ProgressBar_Minimun;
        }

        private async Task SearchDevices(CancellationToken TaskCancel)
        {
            try
            {
                for (int i = ProgressBar_Minimun; i <= ProgressBar_Maximun; i++)
                {
                    SlavesAddresses += "Slave ID:\n" +
                        "dec:   " + i + "\n" +
                        "hex:   " + i.ToString("X2") + "\n\n";

                    ProgressBar_Value++;

                    await Task.Delay(PollingPeriod, TaskCancel);
                }
            }

            catch (OperationCanceledException)
            {
                
            }

            catch (Exception error)
            {
                MessageBox.Invoke(error.Message, MessageType.Error);
            }

            finally
            {
                StopPolling_UI_Actions();
            }
        }
    }
}
