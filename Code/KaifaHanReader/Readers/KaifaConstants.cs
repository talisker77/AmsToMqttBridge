using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaifaHanReader.Readers
{
    public static class KaifaConstants
    {
        public const byte ListUnknown = 0x00;
        public const byte List1 = 0x27;
        public const byte List2 = 0x79;
        public const byte List3 = 0x9B;

        public const int ActivePowerPlussList1 = 33;
        public const int ActivePowerPlussList2 = 70;

        public const int ActivePowerMinusStart = 75;
        public const int TotalPowerMinusStart = 139;
        public const int TotalPowerPlussStart = 134;
        public const int TotalReactivePlussStart = 144;
        public const int TotalReactiveMinusStart = 149;

        public static int GetActivePowerPlussStart(this KaifaHanReader reader, byte listId)
        {
            return listId == List1 ? ActivePowerPlussList1 : ActivePowerPlussList2;
        }
    }
}
