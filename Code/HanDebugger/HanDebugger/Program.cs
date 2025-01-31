﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Runtime;

namespace HanDebugger
{
    class Program
    {
        static List<byte> gBuffer = new List<byte>();
        // static List<int> consumption = new List<int>();
        static bool collectData = false; //save data
        static void Main(string[] args)
        {
            SerialPort vPort = new SerialPort("/dev/ttyUSB0", 2400, Parity.Even, 8, StopBits.One);
            vPort.DataReceived += VPort_DataReceived;
            vPort.Open();

            while (true)
                Thread.Sleep(100);
        }

        private static void VPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var vPort = sender as SerialPort;
            byte[] vBuffer = new byte[1024];
            int vBytesRead = vPort.Read(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i < vBytesRead; i++)
            {
                gBuffer.Add(vBuffer[i]);

                // If we're catching a '7E' and it's not the beginning, it must be the end
                if (gBuffer.Count > 1 && vBuffer[i] == 0x7e)
                {
                    WriteAndEmptyBuffer();
                }
            }
        }

        private static void WriteAndEmptyBuffer()
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Received {gBuffer.Count} (0x{gBuffer.Count:X2}) bytes]");
            var hanReader = new HanDebugger.Reader(gBuffer.ToArray());

            if (hanReader.IsValid())
            {
                System.Console.WriteLine("Received bytes is valid");
            }
            else
            {
                System.Console.WriteLine("Received bytes not valid");
                return;
            }
            hanReader.Anaylyze();
            if (collectData)
            {
                var receivedHex = Convert.ToHexString(gBuffer.ToArray());
                System.Console.WriteLine("Received bytes: {0}", receivedHex);
                receivedHex += Environment.NewLine;
                System.IO.File.AppendAllText($"./../../../Samples/Kaifa/kaifa-{DateTime.Today:yyyy-MM-dd}-sample.txt", receivedHex);
                Console.WriteLine();
            }
            gBuffer.Clear();
        }
    }
}
