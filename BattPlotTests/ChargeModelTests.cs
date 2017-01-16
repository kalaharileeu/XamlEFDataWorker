using BattPlot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using CsvAnalyzer;
using System.Collections.Generic;

namespace BattPlot.Tests
{
    [TestClass()]
    public class ChargeModelTests
    {
        [TestMethod()]
        public void saveTest()
        {
            //arrange
            ChargeModel CM = new ChargeModel();
            string path = null;
            //Act
            path = CM.save(@"c:\temp");
            //path = CM.save("kjsdkfj");
            //if(!File.Exists(@"c:\temp\test.txt"))
            if (path == null)
            {
                Debug.WriteLine("The path was set to null, because of rubbish directory given");
                return;
            }
            ////Clear the file again for the next test
            //File.Delete(@"c:\temp\test.txt");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!File.Exists(@"c:\temp\test.txt"))
            {
                if (sw.ElapsedMilliseconds > 1000)
                {
                    Debug.WriteLine(sw.ElapsedMilliseconds);
                    Assert.Fail("The file was never saved");
                    return;
                }
            }
            Debug.WriteLine("milli seconds: " + sw.ElapsedMilliseconds);
            Debug.WriteLine("ticks:  " + sw.ElapsedTicks);
            //create image file name similar that will happen in code to be test
            string imagefilename = Path.Combine(@"c:\temp\", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".png");
            sw.Restart();
            while (!File.Exists(imagefilename))
            {
                if (sw.ElapsedMilliseconds > 1000)
                {
                    Debug.WriteLine(sw.ElapsedMilliseconds);
                    Assert.Fail("The image was newver saved");
                    return;
                }
            }
            Debug.WriteLine("milli seconds: " + sw.ElapsedMilliseconds);
            Debug.WriteLine("ticks:  " + sw.ElapsedTicks);
            //Restart to ceate delay to wait for .txt released 
            sw.Restart();
            while (sw.ElapsedMilliseconds < 500)
            {
                //delay so the txt file can be release from proces
            }
            Debug.WriteLine("milli seconds: " + sw.ElapsedMilliseconds);
            Debug.WriteLine("ticks:  " + sw.ElapsedTicks);

            //Clean up and delete created files
            File.Delete(@"c:\temp\test.txt");
            File.Delete(imagefilename);
        }

        [TestMethod()]
        public void addToLineSeriesTest()
        {
            //arrange
            ChargeModel CM = new ChargeModel();
            //string dataY = "g";
            //List<string> valulist = new List<string>() {"9.0", "8.0f", "7.0", "6.0" };
            //Column col = new Column();
            //IBaselist baselist = new ValuelistI(valulist, "test column", col);
            //List<IBaselist> baselistList = new List<IBaselist>();
            List<float> xdata = new List<float> { 9999.2f, 1.22222f };
            List<float> ydata = new List<float> { 100.6f, 1.4f, 1.6f, 1.4f };
            //List<float> ydata = new List<float> { 1.6f };
            try
            {
                CM.addToLineSeries(xdata, ydata);
            }
            catch (SystemException)
            {
                Assert.Fail();
            }

            CM.save(@"c:\temp");
        }

        [TestMethod()]
        public void addScatterSeriesTest()
        {
            //arrange
            ChargeModel CM = new ChargeModel();
            string dataY = "testcolumn";
            List<string> valulist = new List<string>() { "9.0", "8.0", "7.0", "6.0" };
            //List<string> valulist = new List<string>() { "900.0" };
            Column col = new Column();
            col.accuracystr = "testvalue";
            List<Column> listofcolumn = new List<Column>() { col };
            //IBaselist baselist = new ValuelistI(valulist, "testcolumn", col);
            //List<IBaselist> baselistList = new List<IBaselist>();
           // baselistList.Add(baselist);
            try
            {
                CM.addScatterSeries(dataY, listofcolumn);
            }
            catch (SystemException)
            {
                Assert.Fail();
            }
            CM.save(@"c:\temp");
        }

        [TestMethod()]
        public void RemoveScatterSeriesTest()
        {
            //arrange
            ChargeModel CM = new ChargeModel();

            try
            {
                CM.RemoveScatterSeries(-3);
            }
            catch (SystemException)
            {
                Assert.Fail();
            }
        }
    }
}