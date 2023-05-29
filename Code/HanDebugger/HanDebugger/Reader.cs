using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanDebugger
{
    public class Reader
    {
        private int position;
        private int dataLength;
        private byte[] buffer;

        static List<long> consumption = new List<long>();

        public Reader(byte[] buffer)
        {
            this.buffer = buffer;
            position = 0;
            dataLength = ((buffer[1] & 0x0F) << 8) | buffer[2];
        }

        public bool IsValid()
        {
            return IsValidStart() &&
                IsValidLength() &&
                IsValidHeaderChecksum() &&
                IsValidChecksum();
        }

        private bool IsValidChecksum()
        {
            ushort checkSum = GetChecksum(dataLength - 2);
            return checkSum == Crc16.ComputeChecksum(buffer, 1, dataLength - 2);
        }

        private bool IsValidHeaderChecksum()
        {
            int headerLength = GetHeaderLength();
            ushort checkSum = GetChecksum(headerLength);
            return checkSum == Crc16.ComputeChecksum(buffer, 1, headerLength);
        }

        private ushort GetChecksum(int checksumPosition)
        {
            return (ushort)(buffer[checksumPosition + 2] << 8 |
                buffer[checksumPosition + 1]);
        }

        private int GetHeaderLength()
        {
            var pos = position + 3; // Dest address
            while ((buffer[pos] & 0x01) == 0x00)
                pos++;
            pos++; // source address
            while ((buffer[pos] & 0x01) == 0x00)
                pos++;
            pos++; // control field
            return pos;
        }

        private bool IsValidLength()
        {
            return buffer.Length >= dataLength + 2;
        }

        public bool IsValidStart()
        {
            return (buffer[0] == 0x7E);
        }

        public void Anaylyze()
        {
            var line = buffer;
            var size = KaifaHanBeta.GetMessageSize(line, 0, line.Length);
            System.Console.WriteLine("Got message size: {0}", size);

            var packageDateTime = KaifaHanBeta.GetPackageDateTime(line, 0, line.Length);
            System.Console.WriteLine("Package time: {0}", packageDateTime);
            var listId = KaifaHanBeta.GetListID(line, 0, line.Length);
            var consumptionElementStart = listId == HanDebugger.KaifaHanBeta.List1 ? 33 : 70;

            System.Console.WriteLine("Checking consumption...");
            var consume =(long) KaifaHanBeta.GetInt(consumptionElementStart, line, 0, line.Length);
            System.Console.WriteLine("Current consumption: {0} Watt", consume);
            consumption.Add(consume);
            //System.Console.WriteLine("Average consumption: {0:0.###} kW", consumption.Average());

            var productionElementStart = 75;

            if (listId == HanDebugger.KaifaHanBeta.List2 || listId == HanDebugger.KaifaHanBeta.List3)
            {
                var produce = KaifaHanBeta.GetInt(productionElementStart, line, 0, line.Length);
                System.Console.WriteLine("Current production: {0} Watt", produce);
                var production = KaifaHanBeta.GetInt(productionElementStart, line, 0, line.Length);
                System.Console.WriteLine("Current production (ulong): {0} Watt", production);
            }

            if (listId == HanDebugger.KaifaHanBeta.List3)
            {
                var cumulativeProduction = 139;
                var start = 134;
                var currentHourlyConsumption = KaifaHanBeta.GetULong(start, line);
                System.Console.WriteLine("Current annual consumption is: {0:0.###} kW/h", currentHourlyConsumption / 10000);
                var annualConsumption = KaifaHanBeta.GetULong(start, line);
                System.Console.WriteLine("Current annual consumption is: {0:0.###} kW/h", annualConsumption);
                var currentHourlyProduction = KaifaHanBeta.GetULong(cumulativeProduction, line);
                System.Console.WriteLine("Current annual production is: {0:0.###} kW/h", currentHourlyProduction / 10000);
                var annualProduction = KaifaHanBeta.GetULong(cumulativeProduction, line);
                System.Console.WriteLine("Current annual production is: {0:0.###} kW/h", annualProduction);
            }
        }
    }
}
