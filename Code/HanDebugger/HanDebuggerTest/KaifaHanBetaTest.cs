using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using HanDebugger;
using HanDebugger.Extensions;

namespace HanDebuggerTest
{
    [TestClass]
    // [DeploymentItem(@"ESP 20170918 Raw.txt")]
    // [DeploymentItem("Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll")]
    public class KaifaHanBetaTest
    {
        [TestMethod]
        public void TestGetPackageID()
        {
            Dictionary<byte, int> packageCount = new Dictionary<byte, int>();
            //var packages = File.ReadAllLines("ESP 20170918 Raw.txt");
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, "Samples", "kaifa-2023-05-17-sample.txt");
            var packages = File.ReadAllLines(path);
            Console.WriteLine("Path: {0}", path);
            var lines = packages.Select(line => line.Trim().Split(' ').Select(v => (byte)int.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray()).ToArray();
            foreach (var line in lines)
            {
                byte list = KaifaHanBeta.GetListID(line, 0, line.Length);
                if (packageCount.ContainsKey(list))
                    packageCount[list]++;
                else
                    packageCount.Add(list, 1);
            }
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(KaifaHanBeta.GetPackageTime(lines[0], 0, lines[0].Length));
            var finishTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(KaifaHanBeta.GetPackageTime(lines[packages.Length - 1], 0, packages[packages.Length - 1].Length));
            var durationInSeconds = finishTime.Subtract(startTime).TotalSeconds;

            Assert.IsTrue(durationInSeconds > 60 * 60 * 4, $"There should be more than 4 hours of recording. Was: {durationInSeconds / 60 * 60 * 4}");

            double list3PerSecond = (1.0 / 3600.0);
            double list2PerSecond = (1.0 / 10.0) - list3PerSecond;
            double list1PerSecond = (1.0 / 2.0) - list2PerSecond;

            Assert.AreEqual(false, packageCount.ContainsKey(KaifaHanBeta.ListUnknown), "There should be no unknown packages");
            Assert.AreEqual(durationInSeconds * list1PerSecond, packageCount[KaifaHanBeta.List1], 1 + 0.01 * packageCount[KaifaHanBeta.List1], "There should be one 'List1' every 2.5s");
            Assert.AreEqual(durationInSeconds * list2PerSecond, packageCount[KaifaHanBeta.List2], 1 + 0.01 * packageCount[KaifaHanBeta.List2], "There should be one 'List2' every 10s");
            Assert.AreEqual(durationInSeconds * list3PerSecond, packageCount[KaifaHanBeta.List3], 1 + 0.01 * packageCount[KaifaHanBeta.List3], "There should be one 'List3' every 1h");

            double targetList1To2Ratio = 4.0;
            double actualList1To2Ratio = (double)packageCount[KaifaHanBeta.List1] / (double)packageCount[KaifaHanBeta.List2];
            Assert.AreEqual(targetList1To2Ratio, actualList1To2Ratio, 0.01, "There should be a ratio of List1:List2 of 4");
        }

        [TestMethod()]

        public void TestGetConsumption()
        {
            List<int> consumption = new List<int>();
            var packages = File.ReadAllLines("ESP 20170918 Raw.txt");
            var lines = packages.Select(line => line.Trim().Split(' ').Select(v => (byte)int.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray()).ToArray();
            foreach (var line in lines)
            {
                if (KaifaHanBeta.GetListID(line, 0, line.Length) == KaifaHanBeta.List1)
                {
                    consumption.Add(KaifaHanBeta.GetInt(0, line, 0, line.Length));
                }
            }
            Assert.AreEqual(1500.0, consumption.Average(), 500.0, "Consumption should be between 1000 and 2000 watts");
        }

        [TestMethod]
        public void TestKaifaReadings()
        {
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, "Samples", "kaifa-2023-05-27-sample.txt");
            //var packages = File.ReadAllLines(path);
            var lines = File.ReadAllLines(path);
            System.Console.WriteLine("Read {0} lines", lines.Length);

            foreach (var line in lines)
            {
                var parts = line.SplitInParts(2);
                var package = parts.Select(v => (byte)int.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray();
                var reader = new HanDebugger.Reader(package);
                System.Console.WriteLine("Trying to get consuption for package: {0}", line);

                reader.Anaylyze();
                var packageDateTime = HanDebugger.KaifaHanBeta.GetPackageDateTime(package, 0, package.Length);
                var listId = KaifaHanBeta.GetListID(package, 0, package.Length);
                if (listId == KaifaHanBeta.List1)
                {
                    var consume = KaifaHanBeta.GetInt(33, package, 0, package.Length);
                    System.Console.WriteLine("Got consumption of {0:N4} in list 1", (decimal)consume / 1000);
                }
                if (listId == KaifaHanBeta.List2 || listId == KaifaHanBeta.List3)
                {
                    var consume = KaifaHanBeta.GetInt(70, package, 0, package.Length);
                    System.Console.WriteLine(
                        "Got consumption of {0} in list 2(x79/{3}) or 3(x9B/{2}) ({1})"
                        , consume
                        , listId
                        , KaifaHanBeta.List3
                        , KaifaHanBeta.List2
                    );
                }
                //if (listId == KaifaHanBeta.List1)
                //{
                //    var production = KaifaHanBeta.GetInt(38, package, 0, package.Length);
                //    System.Console.WriteLine("Got production of {0} in list 1 ({1}) ", production, listId);
                //}
                if (listId == KaifaHanBeta.List2 || listId == KaifaHanBeta.List3)
                {
                    var production = KaifaHanBeta.GetInt(75, package, 0, package.Length);
                    System.Console.WriteLine("Got production of {0} in list 2(79) or 3(9B) ({1})", production, listId);
                    Console.WriteLine("Production: {0} in list 2(79) or 3(9B) ({1})", (decimal)KaifaHanBeta.GetInt(75, package, 0, package.Length), listId);
                    Console.WriteLine("Active power A-: {0:#.###}", (decimal)KaifaHanBeta.GetInt(75, package, 0, package.Length) / 1000);
                    Console.WriteLine("Reactive power A+: {0:#.###}", (decimal)KaifaHanBeta.GetInt(80, package, 0, package.Length) / 1000);
                    Console.WriteLine("Reactive power A-: {0:#.###}", (decimal)KaifaHanBeta.GetInt(85, package, 0, package.Length) / 1000);
                }
                if (listId == KaifaHanBeta.List3)
                {
                    var consume = KaifaHanBeta.GetInt(134, package, 0, package.Length);
                    System.Console.WriteLine("Got consumption of {0:N4} list 3(9B/{1})", (decimal)consume / 1000, KaifaHanBeta.List3);
                    var production = KaifaHanBeta.GetInt(139, package, 0, package.Length);
                    System.Console.WriteLine("Total production of {0:N4} in list 3 ({1})", (decimal)production / 1000, listId);
                    Console.WriteLine("Got production of {0:#.###} in list 3 ({1})", (decimal)KaifaHanBeta.GetInt(139, package, 0, package.Length) / 1000, listId);
                    Console.WriteLine("Cummulative hourly Active power A- value: {0:N4}", (decimal)(KaifaHanBeta.GetInt(139, package, 0, package.Length) / 1000));
                    Console.WriteLine("Cummulative hourly reactive power A+ value: {0:N4}", (decimal)KaifaHanBeta.GetInt(144, package, 0, package.Length) / 1000);
                    Console.WriteLine("Cummulative hourly reactive power A- value: {0:#.###}", (decimal)KaifaHanBeta.GetInt(149, package, 0, package.Length) / 1000);
                }

            }
        }
    }
}
