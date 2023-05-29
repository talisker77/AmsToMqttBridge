using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanDebugger
{
    // As it seems Kaifa/Valider put some premature firmware on my AMS,
    // there's need for some dirty hacks to get any information
    public class KaifaHanBeta
    {
        public const byte ListUnknown = 0x00;
        public const byte List1 = 0x27;
        public const byte List2 = 0x79;
        public const byte List3 = 0x9B;

        public static byte GetListID(byte[] package, int start, int length)
        {
            switch (package[start + 2])
            {
                case List1:
                case List2:
                case List3:
                    return package[start + 2];
                default:
                    return 0x00;
            }
        }

        public static double GetPackageTime(byte[] package, int start, int length)
        {
            const int timeStart = 10;
            int year = package[start + timeStart] << 8 |
                package[start + timeStart + 1];

            int month = package[start + timeStart + 2];
            int day = package[start + timeStart + 3];
            int hour = package[start + timeStart + 5];
            int minute = package[start + timeStart + 6];
            int second = package[start + timeStart + 7];


            return new DateTime(year, month, day, hour, minute, second).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime GetPackageDateTime(byte[] package, int start, int length)
        {
            const int timeStart = 19;
            int year = package[start + timeStart] << 8 | package[start + timeStart + 1];

            // System.Console.WriteLine("Year: {0}", year);
            int month = package[start + timeStart + 2];
            // System.Console.WriteLine("Month: {0}", month);
            int day = package[start + timeStart + 3];
            // System.Console.WriteLine("Day: {0}", day);
            int hour = package[start + timeStart + 5];
            // System.Console.WriteLine("Hour: {0}", hour);
            int minute = package[start + timeStart + 6];
            // System.Console.WriteLine("Minute: {0}", minute);
            int second = package[start + timeStart + 7];
            // System.Console.WriteLine("Second: {0}", second);


            return new DateTime(year, month, day, hour, minute, second);
        }

        public static int GetInt(int dataPosition, byte[] buffer, int start, int length)
        {
            int dataStart = dataPosition;
            int value = 0;
            int foundPosition = 0;
            for (int i = start + dataStart; i < start + length; i++)
            {
                if (foundPosition == 0)
                {
                    if (buffer[i] == 0x06)
                        foundPosition = i;
                }
                else
                {
                    value = value << 8 | buffer[i];
                    if (i == foundPosition + 4)
                        return value;
                }
            }
            return 0;
        }

        public static int GetMessageSize(byte[] buffer, int start, int length)
        {
            var size = buffer[1] & 0x0F << 8 | buffer[2];
            return Convert.ToUInt16(size);
        }

        public static ulong GetULong(int start, byte[] buffer)
        {
            var number = BitConverter.ToUInt64(buffer, start);
            return (ulong)number;
        }

        public static ulong ToULong(int start, byte[] buffer)
        {
            //byte[] buffer = /* Your byte array */;
            //int startPosition = /* Start position of the ulong value in the buffer */;

            ulong value = 0;
            for (int i = 0; i < 8; i++)
            {
                value |= (ulong)buffer[start + i] << (8 * i);
            }
            return value;
        }
    }
}
