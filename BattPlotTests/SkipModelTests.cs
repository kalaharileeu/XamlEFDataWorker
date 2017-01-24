using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CsvAnalyzer;
using System.Diagnostics;

namespace BattPlot.Tests
{
    [TestClass()]
    public class SkipModelTests
    {
        [TestMethod()]
        public void SkipDataProcessingTest()
        {
            //skip data
            CSVinterface csvinterfaceskips = new CSVinterface();
            csvinterfaceskips.InitializeXmlDataModels("Content/XMLFileSkips.xml");

            //Polulate skip test data
            if (csvinterfaceskips.CSVMetaAndColumndata != null)
            {
                Console.WriteLine("The datacolumns capacity is:" + csvinterfaceskips.CSVMetaAndColumndata.Count);
                csvinterfaceskips.LoadCSVdata(@"C:\HW_test_automation_data\S290-72-LL\121121121121\2016y10m16d_12h01m59s_ReactivePwrMap\2016y10m16d_12h01m59s_SN121638052329_S290_72_LL_ReactivePwrMap.csv");

                var column2 = csvinterfaceskips.CSVMetaAndColumndata.Find(x => x.alias == "UO");
                if (column2 != null)
                {
                    Debug.Write($"{column2.Columnvalues.Count} the numer of data points is");
                    Debug.Write($"{csvinterfaceskips.CSVMetaAndColumndata.Count} the count value is");
                    //foreach (var v in column2.Columnvalues)
                    //    Debug.Write($"{v} ");
                }

                SkipModel skipmodel = new SkipModel();
                skipmodel.Setup(csvinterfaceskips.CSVMetaAndColumndata);
                Debug.WriteLine(" ");
                var column = csvinterfaceskips.CSVMetaAndColumndata.Find(x => x.alias == "UO");
                if (column != null)
                {
                    Debug.Write($"{column2.Columnvalues.Count} the numer of data points is");
                    Debug.Write($"{csvinterfaceskips.CSVMetaAndColumndata.Count} the count value is");
                    //foreach (var v in column.Columnvalues)
                    //    Debug.Write($"{v} ");
               }
            }
            else
                Assert.Fail();
        }
    }
}