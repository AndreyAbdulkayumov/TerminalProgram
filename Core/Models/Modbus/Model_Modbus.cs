using Core.Clients;
using Core.Models.Modbus.Message;

namespace Core.Models.Modbus
{
    public struct ModbusResponse
    {
        // Только для Modbus TCP
        public UInt16 OperationNumber;
        public UInt16 ProtocolID;
        public UInt16 LengthOfPDU;

        // Общая часть для всех типов Modbus протокола
        public byte SlaveID;

        // PDU - Protocol Data Unit
        public byte Command;
        public int LengthOfData;
        public byte[] Data;
    }

    public class ModbusOperationResult
    {
        public byte[]? ReadedData;

        public ModbusActionDetails? Details;
    }

    public class Model_Modbus
    {
        public double CycleMode_Period
        {
            get => CycleModeTimer.Interval;
            set => CycleModeTimer.Interval = value;
        }

        public event EventHandler<string>? Model_ErrorInCycleMode;

        private static bool IsBusy = false;       

        private IConnection? _device;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private Func<Task>? _readRegisterInCycleMode;

        public Model_Modbus(ConnectedHost host)
        {
            host.DeviceIsConnect += Host_DeviceIsConnect;
            host.DeviceIsDisconnected += Host_DeviceIsDisconnected;

            CycleModeTimer = new System.Timers.Timer(IntervalDefault);
            CycleModeTimer.Elapsed += CycleModeTimer_Elapsed;
        }

        private void Host_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice != null && e.ConnectedDevice.IsConnected)
            {
                _device = e.ConnectedDevice;
            }
        }

        private void Host_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            _device = null;
        }

        public async Task<ModbusOperationResult> WriteRegister(ModbusWriteFunction writeFunction, MessageData dataForWrite, ModbusMessage message)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] TX = Array.Empty<byte>();
            byte[] RX = Array.Empty<byte>();

            var Result = new ModbusOperationResult();

            ModbusOperationInfo? TX_Info = null, RX_Info = null;

            try
            {
                if (_device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }

                TX = message.CreateMessage(writeFunction, dataForWrite);

                TX_Info = await _device.Send(TX, TX.Length);

                RX_Info = await _device.Receive();

                if (RX_Info.ResponseBytes != null && RX_Info.ResponseBytes.Length > 0)
                {
                    RX = RX_Info.ResponseBytes;

                    ModbusResponse Data = message.DecodingMessage(writeFunction, RX);
                }

                else
                {
                    throw new TimeoutException();
                }
            }

            catch (ModbusException error)
            {
                throw new ModbusException(
                    errorObject: error,
                    requestBytes: TX.Length > 0 ? TX : Array.Empty<byte>(),
                    responseBytes: GetOutputRX(RX, RX.Length),
                    request_ExecutionTime: TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                    response_ExecutionTime: RX_Info != null ? RX_Info.ExecutionTime : new DateTime()
                    );
            }

            catch (TimeoutException)
            {
                string errorMessage = "Хост не ответил.";

                if (_device != null)
                {
                    errorMessage += "\n\nТаймаут записи: " + _device.WriteTimeout + " мс." + "\n" +
                        "Таймаут чтения: " + _device.ReadTimeout + " мс.";
                }

                throw new TimeoutException(errorMessage);
            }

            catch (Exception error)
            {
                throw new Exception(error.Message,
                    new ModbusExceptionInfo()
                    {
                        Details = new ModbusActionDetails()
                        {
                            RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                            ResponseBytes = GetOutputRX(RX, RX.Length),

                            Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                            Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : new DateTime()
                        }
                    });
            }

            finally
            {
                Result.Details = new ModbusActionDetails()
                {
                    RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes = GetOutputRX(RX, RX.Length),

                    Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                    Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : new DateTime()
                };
            }

            IsBusy = false;

            return Result;
        }

        private byte[] GetOutputRX(byte[] RX, int length)
        {
            var outputArray = new byte[length];

            Array.Copy(RX, 0, outputArray, 0, outputArray.Length);

            return outputArray;
        }

        public async Task<ModbusOperationResult> ReadRegister(ModbusReadFunction readFunction, MessageData dataForRead, ModbusMessage message)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] TX = Array.Empty<byte>();
            byte[] RX = Array.Empty<byte>();

            var Result = new ModbusOperationResult();

            ModbusOperationInfo? TX_Info = null, RX_Info = null;

            try
            {
                if (_device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }
                
                TX = message.CreateMessage(readFunction, dataForRead);

                TX_Info = await _device.Send(TX, TX.Length);

                RX_Info = await _device.Receive();

                if (RX_Info.ResponseBytes != null && RX_Info.ResponseBytes.Length > 0)
                {
                    RX = RX_Info.ResponseBytes;

                    ModbusResponse DeviceResponse = message.DecodingMessage(readFunction, RX);

                    Result.ReadedData = DeviceResponse.Data;
                }
                
                else
                {
                    throw new TimeoutException();
                }
            }

            catch (ModbusException error)
            {
                throw new ModbusException(
                    errorObject:   error,
                    requestBytes:  TX.Length > 0 ? TX : Array.Empty<byte>(),
                    responseBytes: GetOutputRX(RX, RX.Length),
                    request_ExecutionTime: TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                    response_ExecutionTime: RX_Info != null ? RX_Info.ExecutionTime : new DateTime()                    
                    );
            }

            catch (TimeoutException)
            {
                string errorMessage = "Хост не ответил.";

                if (_device != null)
                {
                    errorMessage += "\n\nТаймаут записи: " + _device.WriteTimeout + " мс." + "\n" +
                        "Таймаут чтения: " + _device.ReadTimeout + " мс.";
                }

                throw new TimeoutException(errorMessage);
            }

            catch (Exception error)
            {
                throw new Exception(error.Message, 
                    new ModbusExceptionInfo()
                    {
                        Details = new ModbusActionDetails()
                        {
                            RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                            ResponseBytes = GetOutputRX(RX, RX.Length),

                            Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                            Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : new DateTime()
                        }
                    });
            }

            finally
            {
                Result.Details = new ModbusActionDetails()
                {
                    RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes = GetOutputRX(RX, RX.Length),

                    Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : new DateTime(),
                    Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : new DateTime()
                };

                IsBusy = false;
            }

            return Result;
        }

        public void CycleMode_Start(Func<Task> readRegister_Handler)
        {
            _readRegisterInCycleMode = readRegister_Handler;

            CycleModeTimer.Start();
        }

        public void CycleMode_Stop()
        {
            CycleModeTimer.Stop();
        }

        private async void CycleModeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_readRegisterInCycleMode != null)
                {
                    await _readRegisterInCycleMode.Invoke(); 
                }                
            }

            catch (Exception error)
            {
                CycleMode_Stop();

                Model_ErrorInCycleMode?.Invoke(this, "Ошибка отправки команды в цикличном опросе.\n\n" + error.Message +
                    "\n\nОпрос остановлен.");
            }
        }
    }
}
