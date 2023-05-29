namespace KaifaHanReader.Readers
{

    // As it seems Kaifa/Valider put some premature firmware on my AMS,
    // there's need for some dirty hacks to get any information

    public static class KaifaDataReaderExtensions
    {
        public static byte GetListID(this KaifaHanReader reader, int start = 0)
        {
            switch (reader.Buffer[start + 2])
            {
                case KaifaConstants.List1:
                case KaifaConstants.List2:
                case KaifaConstants.List3:
                    return reader.Buffer[start + 2];
                default:
                    return 0x00;
            }
        }

        public static double GetPackageTime(this KaifaHanReader reader, int start)
        {
            const int timeStart = 10;
            int year = reader.Buffer[start + timeStart] << 8 |
                reader.Buffer[start + timeStart + 1];

            int month = reader.Buffer[start + timeStart + 2];
            int day = reader.Buffer[start + timeStart + 3];
            int hour = reader.Buffer[start + timeStart + 5];
            int minute = reader.Buffer[start + timeStart + 6];
            int second = reader.Buffer[start + timeStart + 7];


            return new DateTime(year, month, day, hour, minute, second).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime GetPackageDateTime(this KaifaHanReader reader, int start = 0)
        {
            const int timeStart = 19;
            int year = reader.Buffer[start + timeStart] << 8 | reader.Buffer[start + timeStart + 1];
            int month = reader.Buffer[start + timeStart + 2];
            int day = reader.Buffer[start + timeStart + 3];
            int hour = reader.Buffer[start + timeStart + 5];
            int minute = reader.Buffer[start + timeStart + 6];
            int second = reader.Buffer[start + timeStart + 7];

            return new DateTime(year, month, day, hour, minute, second);
        }

        public static int GetInt(this KaifaHanReader reader, int dataPosition)
        {
            int dataStart = dataPosition;
            int value = 0;
            int foundPosition = 0;
            var start = 0;
            var length = reader.Buffer.Length;
            for (int i = start + dataStart; i < start + length; i++)
            {
                if (foundPosition == 0)
                {
                    if (reader.Buffer[i] == 0x06) //start read buffer bytes
                        foundPosition = i;
                }
                else
                {
                    value = value << 8 | reader.Buffer[i];
                    if (i == foundPosition + 4)
                        return value;
                }
            }
            return 0;
        }

        public static int GetMessageSize(this KaifaHanReader reader)
        {
            var size = reader.Buffer[1] & 0x0F << 8 | reader.Buffer[2];
            return Convert.ToUInt16(size);
        }
    }
}

