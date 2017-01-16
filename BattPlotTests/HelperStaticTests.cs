using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics;

namespace BattPlot.Tests
{
    [TestClass()]
    public class HelperStaticTests
    {
        [TestMethod()]
        public void readAdditionalInfoTest()
        {
            //string thepath = "N:\\Temp\\S290-72-LL\\01.05.81\\2016y08m23d_20h18m24s_ReactivePwrMap-40\\2016y08m23d_20h18m24s_SN121629026789_S290_72_LL_ReactivePwrMap.csv";
            string thepath = "c:\\testvsc\\121121121121\\test_lol - Copy.csv";
            //testvsc\121121121121\test_lol - Copy.csv
            //string thepath = "";
            StringBuilder infotext = new StringBuilder();
            infotext = HelperStatic.readAdditionalInfo(thepath);
            Debug.WriteLine(infotext);
            if(infotext.Length == 0)
                Assert.Fail();
        }
    }
}