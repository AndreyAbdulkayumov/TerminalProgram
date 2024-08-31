﻿using Core.Models;
using Core.Models.Modbus;
using Core.Models.Modbus.Message;
using MessageBox_Core;
using ReactiveUI;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;

namespace ViewModels.ModbusClient
{
    public class ModbusScanner_VM : ReactiveObject
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

        private string? _pauseBetweenRequests;

        public string? PauseBetweenRequests
        {
            get => _pauseBetweenRequests;
            set => this.RaiseAndSetIfChanged(ref _pauseBetweenRequests, value);
        }

        private string _deviceReadTimeout = "Таймаут чтения ";

        public string DeviceReadTimeout
        {
            get => _deviceReadTimeout;
            set => this.RaiseAndSetIfChanged(ref _deviceReadTimeout, value);
        }

        private const string ButtonContent_Start = "Начать поиск";
        private const string ButtonContent_Stop = "Остановить поиск";

        private string _actionButtonContent = ButtonContent_Start;

        public string ActionButtonContent
        {
            get => _actionButtonContent;
            set => this.RaiseAndSetIfChanged(ref _actionButtonContent, value);
        }

        private string _currentSlaveID = string.Empty;

        public string CurrentSlaveID
        {
            get => _currentSlaveID;
            set => this.RaiseAndSetIfChanged(ref _currentSlaveID, value);
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

        public string ErrorMessageInUI
        {
            get => "Ни одно устройство не ответило.\nВозможно стоит повысить значение паузы между запросами.";
        }

        private bool _errorIsVisible = false;

        public bool ErrorIsVisible
        {
            get => _errorIsVisible;
            set => this.RaiseAndSetIfChanged(ref _errorIsVisible, value);
        }

        public ReactiveCommand<Unit, Unit> Command_Start_Stop_Search { get; }

        private readonly Action<string, MessageType> MessageBox;

        private readonly ConnectedHost Model;

        private Task? _searchTask;
        private CancellationTokenSource? _searchCancel;

        private uint _pauseBetweenRequests_ForWork;

        public ModbusScanner_VM(Action<string, MessageType> messageBox)
        {
            MessageBox = messageBox;

            Model = ConnectedHost.Model;

            DeviceReadTimeout += Model.Host_ReadTimeout.ToString() + " мс.";

            Command_Start_Stop_Search = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SearchInProcess)
                {
                    await StopPolling();

                    return;
                }

                StartPolling();
            });
            Command_Start_Stop_Search.ThrownExceptions.Subscribe(error => messageBox.Invoke(error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.PauseBetweenRequests)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberStyles.Number, out _pauseBetweenRequests_ForWork))
                .Subscribe(x => PauseBetweenRequests = x);

            // Значения по умолчанию
            PauseBetweenRequests = "100";
        }

        public async Task Close_EventHandler()
        {
            if (SearchInProcess && _searchTask != null)
            {
                _searchCancel?.Cancel();

                await Task.WhenAny(_searchTask);
            }
        }

        private void StartPolling()
        {
            if (PauseBetweenRequests == null || PauseBetweenRequests == string.Empty)
            {
                MessageBox.Invoke("Не задано значение паузы.", MessageType.Warning);
                return;
            }

            ActionButtonContent = ButtonContent_Stop;
            SearchInProcess = true;

            SlavesAddresses = string.Empty;

            ErrorIsVisible = false;

            _searchCancel = new CancellationTokenSource();

            _searchTask = Task.Run(() => SearchDevices(_searchCancel.Token));
        }

        private async Task StopPolling()
        {
            _searchCancel?.Cancel();

            if (_searchTask != null)
            {
                await Task.WhenAny(_searchTask);
            }

            StopPolling_UI_Actions();
        }

        private void StopPolling_UI_Actions()
        {
            ActionButtonContent = ButtonContent_Start;
            SearchInProcess = false;

            ProgressBar_Value = ProgressBar_Minimun;
        }

        private async Task SearchDevices(CancellationToken taskCancel)
        {
            try
            {
                ushort address = 0;
                int numberOfRegisters = 2;
                ModbusMessage modbusMessageType = new ModbusRTU_Message();
                ushort CRC16_Polynom = 0xA001;


                ModbusReadFunction readFunction = Function.AllReadFunctions.Single(x => x.Number == 3);

                var data = new ReadTypeMessage(
                    0,
                    address,
                    numberOfRegisters,
                    modbusMessageType is ModbusTCP_Message ? false : true,
                    CRC16_Polynom);

                ModbusOperationResult result;

                for (int i = ProgressBar_Minimun; i <= ProgressBar_Maximun; i++)
                {
                    try
                    {
                        data.SlaveID = (byte)i;

                        CurrentSlaveID = i + " (0x" + i.ToString("X2") + ")";

                        result = await Model.Modbus.ReadRegister(readFunction, data, modbusMessageType);

                        SlavesAddresses += "Slave ID:\n" +
                            "dec:   " + i + "\n" +
                            "hex:   " + i.ToString("X2") + "\n\n";
                    }

                    catch (TimeoutException)
                    {
                        continue;
                    }

                    catch (ModbusException)
                    {
                        SlavesAddresses += "Slave ID:\n" +
                            "dec:   " + i + "\n" +
                            "hex:   " + i.ToString("X2") + "\n\n";
                    }

                    finally
                    {
                        ProgressBar_Value++;

                        await Task.Delay((int)_pauseBetweenRequests_ForWork, taskCancel);
                    }
                }

                if (SlavesAddresses == string.Empty)
                {
                    ErrorIsVisible = true;
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