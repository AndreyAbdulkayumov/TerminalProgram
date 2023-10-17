using Core.Clients;
using Core.Models.Modbus.Message;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        private Action? ReadRegisterInCycleMode;

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

        public void WriteRegister(ModbusWriteFunction WriteFunction, MessageData DataForWrite, ModbusMessage Message,
            out byte[] Request, out byte[] Response)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] RX = new byte[30];

            byte[] TX = new byte[0];

            int NumberOfReceivedBytes = 0;

            try
            {
                if (Device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }

                TX = Message.CreateMessage(WriteFunction, DataForWrite);

                Device.Send(TX, TX.Length);

                NumberOfReceivedBytes = Device.Receive(RX);

                ModbusResponse Data = Message.DecodingMessage(WriteFunction, RX);
            }

            catch (ModbusException error)
            {
                throw new ModbusException(error);
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }

            finally
            {
                if (TX.Length != 0)
                {
                    Request = TX;

                    Response = GetOutputRX(RX, NumberOfReceivedBytes);
                }

                else
                {
                    Request = new byte[0];
                    Response = new byte[0];
                }
                
                IsBusy = false;
            }
        }

        private byte[] GetOutputRX(byte[] RX, int Length)
        {
            byte[] OutputArray = new byte[Length];

            Array.Copy(RX, 0, OutputArray, 0, OutputArray.Length);

            return OutputArray;
        }

        public UInt16[] ReadRegister(ModbusReadFunction ReadFunction, MessageData DataForRead, ModbusMessage Message,
            out byte[] Request, out byte[] Response)
        {
            while (IsBusy) ;

            IsBusy = true;

            byte[] RX = new byte[100];

            byte[] TX = new byte[0];

            int NumberOfReceivedBytes = 0;

            UInt16[] Result;

            try
            {
                if (Device == null)
                {
                    throw new Exception("Хост не инициализирован.");
                }

                TX = Message.CreateMessage(ReadFunction, DataForRead);

                Device.Send(TX, TX.Length);

                NumberOfReceivedBytes = Device.Receive(RX);

                ModbusResponse DeviceResponse = Message.DecodingMessage(ReadFunction, RX);
                                
                if (DeviceResponse.Data.Length < 2)
                {
                    Result = new UInt16[1];

                    Result[0] = DeviceResponse.Data[0];
                }

                else
                {
                    Result = new UInt16[DeviceResponse.Data.Length / 2];

                    for (int i = 0; i < Result.Length; i++)
                    {
                        Result[i] = BitConverter.ToUInt16(DeviceResponse.Data, i * 2);
                    }
                }
            }

            catch (ModbusException error)
            {
                throw new ModbusException(error);
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }

            finally
            {
                if (TX.Length != 0)
                {
                    Request = TX;

                    Response = GetOutputRX(RX, NumberOfReceivedBytes);
                }

                else
                {
                    Request = new byte[0];
                    Response = new byte[0];
                }

                IsBusy = false;
            }

            return Result;
        }

        public void CycleMode_Start(Action ReadRegister_Handler)
        {
            ReadRegisterInCycleMode = ReadRegister_Handler;

            CycleModeTimer.Start();
        }

        public void CycleMode_Stop()
        {
            CycleModeTimer.Stop();
        }

        private void CycleModeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ReadRegisterInCycleMode?.Invoke();
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
