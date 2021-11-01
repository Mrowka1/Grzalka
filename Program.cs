using System;
using System.Collections.Generic;
using System.Device.Gpio;
using EasyModbus;
namespace Grzalka
{
    class Program
    {

        static int[] PhasesPins = { 26, 25, 24 };

        static int pinPhases1_2 = 26;
        static int pinPhase3 = 25;
        static int pin_CommonPower = 23;

        static byte CurPhase = 0;

        static double MinimalVoltageDiffence = 5.0d;
        static double MinimalVoltage = 230.0;

        static GpioController ctrl;
        static ModbusClient modbus;
        static bool Started;

        static void Main(string[] args)
        {
            ctrl = new GpioController(PinNumberingScheme.Board);

            TurnHeater1Phase(0);

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
                    int[] reg = modbus.ReadHoldingRegisters(0, 20);
                    double pwr = (double)reg[11] / 100.0d;
                    if (pwr >= 1.5)
                    {

                        double VolA = (double)reg[14] / 10.0;
                        double VolB = (double)reg[16] / 10.0;
                        double VolC = (double)reg[18] / 10.0;

                        double[] Voltages = { VolA, VolB, VolC };
                        Array.Sort(Voltages);
                        double MidValue = Voltages[1];
                        double LowValue = Voltages[0];

                        if (VolA > MinimalVoltage & VolA > LowValue + MinimalVoltageDiffence & VolA > MidValue)
                        {
                            TurnHeater1Phase(1);
                        }
                        else if (VolB > MinimalVoltage & VolB > LowValue + MinimalVoltageDiffence & VolB > MidValue)
                        {
                            TurnHeater1Phase(2);
                        }
                        else if (VolC > MinimalVoltage & VolC > LowValue + MinimalVoltageDiffence & VolC > MidValue)
                        {
                            TurnHeater1Phase(3);
                        }
                        else
                        {
                            TurnHeater1Phase(0);
                        }

                        //   
                    }
                    else
                    {
                        TurnHeater1Phase(0);
                    }
                }
                catch
                {
                    TurnHeater1Phase(0);
                }
                System.Threading.Thread.Sleep(15000);
            }


        }


        static void TurnHeater3Phases(bool On, byte phase = 0)
        {
            foreach (int pin in PhasesPins)
            {
                if (!ctrl.IsPinOpen(pin))
                {
                    ctrl.OpenPin(pin, PinMode.Output);
                    ctrl.Write(pin, PinValue.High);
                }
            }
            if (On && phase != 0)
            {
                ctrl.Write(PhasesPins[phase - 1], PinValue.Low);
            }

        }


        static void TurnHeater1Phase(byte phase = 0)
        {

            if (!ctrl.IsPinOpen(pinPhases1_2)) ctrl.OpenPin(pinPhases1_2);
            if (!ctrl.IsPinOpen(pinPhase3)) ctrl.OpenPin(pinPhase3);
            if (!ctrl.IsPinOpen(pin_CommonPower)) ctrl.OpenPin(pin_CommonPower);

            if (phase != 0 && phase == CurPhase) return;
            CurPhase = phase;
            switch (phase)
            {
                case 0:
                    ctrl.Write(pin_CommonPower, PinValue.High);
                    ctrl.Write(pinPhases1_2, PinValue.High);
                    ctrl.Write(pinPhase3, PinValue.High);

                    break;
                case 1:

                    ctrl.Write(pinPhases1_2, PinValue.High);
                    ctrl.Write(pinPhase3, PinValue.High);
                    ctrl.Write(pin_CommonPower, PinValue.Low);
                    break;
                case 2:
                    ctrl.Write(pinPhases1_2, PinValue.Low);
                    ctrl.Write(pinPhase3, PinValue.High);
                    ctrl.Write(pin_CommonPower, PinValue.Low);
                    break;
                case 3:
                    ctrl.Write(pinPhases1_2, PinValue.High);
                    ctrl.Write(pinPhase3, PinValue.Low);
                    ctrl.Write(pin_CommonPower, PinValue.Low);
                    break;
            }
        }

        private static void Modbus_ConnectedChanged(object sender)
        {
            if (modbus.Connected) Run();
        }
    }
}
