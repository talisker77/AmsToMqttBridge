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
        static  List<int> consumption = new List<int>();
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
                    WriteAndEmptyBuffer();
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

            var line = gBuffer.ToArray();

            var size = KaifaHanBeta.GetMessageSize(line, 0, line.Length);
            System.Console.WriteLine("Got message size: {0}", size);

            var packageDateTime = KaifaHanBeta.GetPackageDateTime(line, 0, line.Length);
            System.Console.WriteLine("Package time: {0}", packageDateTime);
            var listId = KaifaHanBeta.GetListID(line, 0, line.Length);
            var consumptionElementStart = listId == HanDebugger.KaifaHanBeta.List1 ? 33 : 70;

            System.Console.WriteLine("Checking consumption...");
            var consume = KaifaHanBeta.GetInt(consumptionElementStart, line, 0, line.Length);
            System.Console.WriteLine("Current consumption: {0}W", consume);
            consumption.Add(consume);
            System.Console.WriteLine("Total consumption: {0}", consumption.Sum());


            var receivedHex = Convert.ToHexString(gBuffer.ToArray());
            System.Console.WriteLine("Received bytes: {0}", receivedHex);
            receivedHex += Environment.NewLine;
            System.IO.File.AppendAllText($"./../../../Samples/Kaifa/kaifa-{DateTime.Today:yyyy-MM-dd}-sample.txt", receivedHex);
            // int j = 0;
            // foreach (var vByte in gBuffer)
            // {
            //     Console.Write(string.Format("{0:X2} ", (int)vByte));

            //     if (++j % 8 == 0)
            //         Console.Write(" ");

            //     if (j % 24 == 0)
            //         Console.WriteLine();
            // }

            Console.WriteLine();
            // Console.WriteLine();

            gBuffer.Clear();
        }
    }
}
