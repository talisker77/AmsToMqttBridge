using System.Collections.Generic;

namespace HanDebugger
{
    public class Reader
    {
        private readonly int position;
        private readonly int dataLength;
        private readonly byte[] buffer;

        public byte[] Buffer => buffer;
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
            var consume = (long)KaifaHanBeta.GetInt(consumptionElementStart, line, 0, line.Length);
            System.Console.WriteLine("Current consumption: {0} Watt", consume);
            var productionElementStart = 75;
            if (listId == HanDebugger.KaifaHanBeta.List2 || listId == HanDebugger.KaifaHanBeta.List3)
            {
                var production = KaifaHanBeta.GetInt(productionElementStart, line, 0, line.Length);
                System.Console.WriteLine("Current production: {0} Watt", production);
            }

            if (listId == HanDebugger.KaifaHanBeta.List3)
            {
                var cumulativeProductionStart = 139;
                var cumulativeConsumptionStart = 134;
                var startReactivePluss = 144;
                var startReactiveMinus = 149;
                var totalConsumption = KaifaHanBeta.GetInt(cumulativeConsumptionStart, line, 0, line.Length);
                System.Console.WriteLine("Total consumption (A+): {0:#.###} kW/h", (decimal)totalConsumption / 1000);
                var totalReactivePluss = KaifaHanBeta.GetInt(startReactivePluss, line, 0, line.Length);
                System.Console.WriteLine("Total reactive (A+): {0:#.###} kW/h", (decimal)totalReactivePluss / 1000);
                var totalProduction = KaifaHanBeta.GetInt(cumulativeProductionStart, line, 0, line.Length);
                System.Console.WriteLine("Total production (A-): {0:#.###} kW/h", (decimal)totalProduction / 1000);
                var totalReactiveMinus = KaifaHanBeta.GetInt(startReactiveMinus, line, 0, line.Length);
                System.Console.WriteLine("Total reactive (A-): {0:#.###} kW/h", (decimal)totalReactiveMinus / 1000);
            }
        }
    }
}