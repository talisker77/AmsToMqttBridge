using System;
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
            var consumption = new List<int>();
            var line = gBuffer.ToArray();

            var size = KaifaHanBeta.GetMessageSize(line, 0, line.Length);
            System.Console.WriteLine("Got message size: {0}", size);
            if (KaifaHanBeta.GetListID(line, 0, line.Length) == KaifaHanBeta.List3)
            {
                var packageDateTime = KaifaHanBeta.GetPackageDateTime(line, 0, line.Length);
                System.Console.WriteLine("Package time: {0}", packageDateTime);
            }
            if (KaifaHanBeta.GetListID(line, 0, line.Length) == KaifaHanBeta.List1)
            {
                System.Console.WriteLine("Checking consumption...");
                var consume = KaifaHanBeta.GetInt(0, line, 0, line.Length);
                System.Console.WriteLine("Got comsume counter: {0}", consume);
                consumption.Add(consume);
                System.Console.WriteLine("Consumption: {0}", consumption.Sum());
            }

            var receivedHex = Convert.ToHexString(gBuffer.ToArray());
            System.Console.WriteLine("Received bytes: {0}", receivedHex);
            receivedHex += Environment.NewLine;
            System.IO.File.AppendAllText($"~/projects/ams-dotnet/Samples/Kaifa/kaifa-{DateTime.Today:s}-sample.txt", receivedHex);
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
