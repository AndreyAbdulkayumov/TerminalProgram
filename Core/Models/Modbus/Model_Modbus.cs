using Core.Clients;
using Core.Models.Modbus.Message;
using Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public UInt16[]? ReadedData;

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

        private IConnection? Device;

        private readonly System.Timers.Timer CycleModeTimer;
        private const double IntervalDefault = 100;

        private Func<Task>? ReadRegisterInCycleMode;

        public Model_Modbus(ConnectedHost Host)
        {
            Host.DeviceIsConnect += Host_DeviceIsConnect;
            Host.DeviceIsDisconnected += Host_DeviceIsDisconnected;

            CycleModeTimer = new System.Timers.Timer(IntervalDefault);
            CycleModeTimer.Elapsed += CycleModeTimer_Elapsed;
        }

        private void Host_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice != null && e.ConnectedDevice.IsConnected)
            {
                Device = e.ConnectedDevice;
            }
        }

        private void Host_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            Device = null;
        }

        public async Task<ModbusOperationResult> WriteRegister(ModbusWriteFunction WriteFunction, MessageData DataForWrite, ModbusMessage Message)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] TX = Array.Empty<byte>();
            byte[] RX = Array.Empty<byte>();

            ModbusOperationResult Result = new ModbusOperationResult();

            ModbusOperationInfo? TX_Info = null, RX_Info = null;

            try
            {
                if (Device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }

                TX = Message.CreateMessage(WriteFunction, DataForWrite);

                TX_Info = await Device.Send(TX, TX.Length);

                RX_Info = await Device.Receive();

                if (RX_Info.ResponseBytes != null && RX_Info.ResponseBytes.Length > 0)
                {
                    RX = RX_Info.ResponseBytes;

                    ModbusResponse Data = Message.DecodingMessage(WriteFunction, RX);
                }

                else
                {
                    throw new Exception("Хост не ответил.\n\n" +
                        "Таймаут записи: " + Device.WriteTimeout + " мс." + "\n" +
                        "Таймаут чтения: " + Device.ReadTimeout + " мс.");
                }
            }

            catch (ModbusException error)
            {
                throw new ModbusException(
                    ErrorObject: error,
                    RequestBytes: TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes: GetOutputRX(RX, RX.Length),
                    Response_ExecutionTime: TX_Info != null ? TX_Info.ExecutionTime : null,
                    Request_ExecutionTime: RX_Info != null ? RX_Info.ExecutionTime : null
                    );
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

                            Request_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : null,
                            Response_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : null
                        }
                    });
            }

            finally
            {
                Result.Details = new ModbusActionDetails()
                {
                    RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes = GetOutputRX(RX, RX.Length),

                    Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : null,
                    Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : null
                };
            }

            IsBusy = false;

            return Result;
        }

        private byte[] GetOutputRX(byte[] RX, int Length)
        {
            byte[] OutputArray = new byte[Length];

            Array.Copy(RX, 0, OutputArray, 0, OutputArray.Length);

            return OutputArray;
        }

        public async Task<ModbusOperationResult> ReadRegister(ModbusReadFunction ReadFunction, MessageData DataForRead, ModbusMessage Message)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] TX = Array.Empty<byte>();
            byte[] RX = Array.Empty<byte>();

            ModbusOperationResult Result = new ModbusOperationResult();

            ModbusOperationInfo? TX_Info = null, RX_Info = null;

            try
            {
                if (Device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }
                
                TX = Message.CreateMessage(ReadFunction, DataForRead);

                TX_Info = await Device.Send(TX, TX.Length);

                RX_Info = await Device.Receive();

                if (RX_Info.ResponseBytes != null && RX_Info.ResponseBytes.Length > 0)
                {
                    RX = RX_Info.ResponseBytes;

                    ModbusResponse DeviceResponse = Message.DecodingMessage(ReadFunction, RX);

                    if (DeviceResponse.Data.Length < 2)
                    {
                        Result.ReadedData = new UInt16[1];

                        Result.ReadedData[0] = DeviceResponse.Data[0];
                    }

                    else
                    {
                        Result.ReadedData = new UInt16[DeviceResponse.Data.Length / 2];

                        for (int i = 0; i < Result.ReadedData.Length; i++)
                        {
                            Result.ReadedData[i] = BitConverter.ToUInt16(DeviceResponse.Data, i * 2);
                        }
                    }
                }
                
                else
                {
                    throw new Exception("Хост не ответил.\n\n" +
                        "Таймаут записи: " + Device.WriteTimeout + " мс." + "\n" +
                        "Таймаут чтения: " + Device.ReadTimeout + " мс.");
                }
            }

            catch (ModbusException error)
            {
                throw new ModbusException(
                    ErrorObject:   error,
                    RequestBytes:  TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes: GetOutputRX(RX, RX.Length),
                    Response_ExecutionTime: TX_Info != null ? TX_Info.ExecutionTime : null,
                    Request_ExecutionTime: RX_Info != null ? RX_Info.ExecutionTime : null
                    );
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

                            Request_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : null,
                            Response_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : null
                        }
                    });
            }

            finally
            {
                Result.Details = new ModbusActionDetails()
                {
                    RequestBytes = TX.Length > 0 ? TX : Array.Empty<byte>(),
                    ResponseBytes = GetOutputRX(RX, RX.Length),

                    Request_ExecutionTime = TX_Info != null ? TX_Info.ExecutionTime : null,
                    Response_ExecutionTime = RX_Info != null ? RX_Info.ExecutionTime : null
                };

                IsBusy = false;
            }

            return Result;
        }

        public void CycleMode_Start(Func<Task> ReadRegister_Handler)
        {
            ReadRegisterInCycleMode = ReadRegister_Handler;

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
                if (ReadRegisterInCycleMode != null)
                {
                    await ReadRegisterInCycleMode.Invoke();
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
