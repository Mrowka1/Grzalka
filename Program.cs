using System;
using System.Device.Gpio;
using EasyModbus;
namespace Grzalka
{
    class Program
    {
        static int pin_F1 = 26;
        static int pin_F2 = 25;
        static int pin_F3 = 24;
        static int pin_Power = 23;
        static int[] PhasesPins = { 26, 25, 24 };

        static GpioController ctrl;
        static ModbusClient modbus;
        static bool Started;

        static void Main(string[] args)
        {
            ctrl = new GpioController(PinNumberingScheme.Board);
            modbus = new ModbusClient(args[1]);
            modbus.ConnectedChanged += Modbus_ConnectedChanged;
            modbus.Parity = System.IO.Ports.Parity.None;
            modbus.StopBits = System.IO.Ports.StopBits.One;
            modbus.UnitIdentifier = 20;
            modbus.ConnectionTimeout = 1000;
            modbus.Connect();


        }

        static void Run()
        {
            if (Started) return;
            Started = true;
            while (true)
            {
                try
                {

                }
                catch
                {

                }
                System.Threading.Thread.Sleep(15000);
            }


        }


        void TurnHeater(bool On, byte phase = 0)
        {
            foreach (int pin in PhasesPins)
            {

            }
        }

        private static void Modbus_ConnectedChanged(object sender)
        {
            if (modbus.Connected) Run();
        }
    }
}
