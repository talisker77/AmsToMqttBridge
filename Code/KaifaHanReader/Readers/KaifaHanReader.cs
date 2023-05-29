using KaifaHanReader.Models;
using KaifaHanReader.Utils;

namespace KaifaHanReader.Readers
{
    public class KaifaHanReader
    {
        private readonly int position;
        private readonly int dataLength;
        private readonly byte[] buffer;

        public byte[] Buffer => buffer;

        public KaifaHanReader(byte[] buffer)
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

        public ReaderData Anaylyze()
        {
            var data = new ReaderData
            {
                IsValid = IsValid()
            };
            var size = this.GetMessageSize();
            data.MessageSize = size;
            var packageDateTime = this.GetPackageDateTime();
            data.PackageDateTime = packageDateTime;
            var listId = this.GetListID();
            data.ListId = listId;
            var consumptionElementStart = this.GetActivePowerPlussStart(listId);

            var consume = (int)this.GetInt(consumptionElementStart);
            data.ActivePowerPluss = consume;
            if (listId == KaifaConstants.List2 || listId == KaifaConstants.List3)
            {
                var production = (int)this.GetInt(KaifaConstants.ActivePowerMinusStart);
                data.ActivePowerMinus = production;
            }

            if (listId == KaifaConstants.List3)
            {
                var totalConsumption = (decimal)this.GetInt(KaifaConstants.TotalPowerPlussStart);
                data.TotalActivePowerPluss = totalConsumption / 1000;
                var totalReactivePluss = (decimal)this.GetInt(KaifaConstants.TotalReactivePlussStart);
                data.TotalReactivePowerPluss = totalReactivePluss / 1000;
                var totalProduction = (decimal)this.GetInt(KaifaConstants.TotalPowerMinusStart);
                data.TotalActivePowerMinus = totalProduction / 1000;
                var totalReactiveMinus = (decimal)this.GetInt(KaifaConstants.TotalReactiveMinusStart);
                data.TotalReactivePowerMinus = totalReactiveMinus / 1000;
            }
            return data;
        }
    }
}
