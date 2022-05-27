using HanDebugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace HanDebuggerTest
{
    [TestClass]
    public class DlmsReaderTest
    {
        [TestMethod]
        public void TestDlmsReader()
        {
            var text = File.ReadAllLines("SampleData.txt");//.Replace(@"\r\n", " ").Replace("  ", " ");
            int packages = 0;
            foreach (var line in text)
            {
                byte[] bytes = line.Trim().Split(' ').Select(v => (byte)int.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray();
                var reader = new DlmsReader();

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (reader.Read(bytes[i]))
                    {
                        packages++;
                        //byte[] data = reader.GetRawData();
                    }
                }
            }
            Assert.IsTrue(packages == 559, $"There should be 559 packages. Was: {packages}");

        }


        [TestMethod]
        public void TestDlmsReaderWithError()
        {
            var text = File.ReadAllLines("SampleData.txt");//.Replace("\r\n", " ").Replace("  ", " ");
            int packages = 0;
            foreach (var line in text)
            {
                byte[] bytes = line.Trim().Split(' ').Select(v => (byte)int.Parse(v, System.Globalization.NumberStyles.HexNumber)).ToArray();
                bytes = bytes.Skip(10).ToArray(); //creating error 
                var reader = new DlmsReader();

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (reader.Read(bytes[i]))
                    {
                        packages++;
                        //byte[] data = reader.GetRawData();
                    }
                }
            }
            Assert.IsTrue(packages == 0, $"There should be 0 correct packages. Was: {packages}");

        }
    }
}
