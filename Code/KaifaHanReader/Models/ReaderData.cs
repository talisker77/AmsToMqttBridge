using KaifaHanReader.Readers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaifaHanReader.Models
{
    public class ReaderData
    {
        public int MessageSize { get; set; }
        public bool IsValid { get; set; }

        public byte ListId { get; set; }

        public DateTime PackageDateTime { get; set; }

        public int ActivePowerPluss { get; set; }
        public int? ActivePowerMinus { get; set; }

        public decimal? TotalActivePowerPluss { get; set; }
        public decimal? TotalReactivePowerPluss { get; set; }
        public decimal? TotalActivePowerMinus { get; set; }
        public decimal? TotalReactivePowerMinus { get; set; }

        public void ToConsole()
        {
            Console.WriteLine("Got message size: {0}", MessageSize);

            Console.WriteLine("Package time: {0}", PackageDateTime);
            Console.WriteLine("Current consumption: {0} Watt", ActivePowerPluss);
            if (ListId == KaifaConstants.List2 || ListId == KaifaConstants.List3)
            {
                Console.WriteLine("Current production: {0} Watt", ActivePowerMinus);
            }

            if (ListId == KaifaConstants.List3)
            {
                Console.WriteLine("Total consumption (A+): {0:#.###} kW/h", TotalActivePowerPluss);
                Console.WriteLine("Total reactive (A+): {0:#.###} kW/h", TotalReactivePowerPluss);
                Console.WriteLine("Total production (A-): {0:#.###} kW/h", TotalActivePowerMinus);
                Console.WriteLine("Total reactive (A-): {0:#.###} kW/h", TotalReactivePowerMinus);
            }
        }
    }
}
