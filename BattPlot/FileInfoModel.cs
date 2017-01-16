using System;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using TestFileMiner;
using System.Collections.Generic;

namespace BattPlot
{
    class FileInfoModel
    {
        public FileInfoModel()
        {
            theFileInfoModel = new PlotModel() { Title = "Saved Test Results" };
            //theDischargeModel.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 0.2, "sin(x)"));
            fileminer = new FileMiner();
            Testpaths = new List<Testpath>();
            setup();
        }
        //setup general parameters
        private void setup()
        {
            ////Setting up the legend
            //Tip: you must assign a Title ex. Title = "Scatter" to the series, else does appear in legend 
            theFileInfoModel.LegendTitle = "Legend";
            theFileInfoModel.LegendOrientation = LegendOrientation.Horizontal;
            theFileInfoModel.LegendPlacement = LegendPlacement.Outside;
            theFileInfoModel.LegendBorder = OxyColors.Black;
            theFileInfoModel.LegendPosition = LegendPosition.BottomCenter;
            //this is another axes
            linearYaxis = new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 50, Title = "Firmware version" };
            linearXaxis = new LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot, IntervalLength = 40, Title = "Procload", MaximumRange = 350 };
            theFileInfoModel.Axes.Add(linearXaxis);
            theFileInfoModel.Axes.Add(linearYaxis);
            //make the delgate member null
            // listOfHandlers = null;
        }

        private void Load()
        {
            string filename = "N:\\automation\\HW_test_automation_data";
            //string filename = "C:\\testvsc";


            if (fileminer.FindAllFilesInFolder(filename))
            {
                fileminer.ReadTextFilesForInfo();
                foreach (var serialnumber in fileminer.Testdictionary)
                {
                    foreach (var TestCase in serialnumber.Value)
                    {
                        var v = Testpaths.Find(x => x.Serialnumbers.Contains(TestCase.Serialnumber));
                    }
                }
            }
        }

        FileMiner fileminer;
        public string Title { get; private set; }
        public PlotModel theFileInfoModel { get; private set; }
        private LinearAxis linearYaxis;
        private LinearAxis linearXaxis;
        private List<Testpath> Testpaths; 
     }

    public class Testpath
    {
        public List<string> Serialnumbers { get; set; }
        public string Invetertype { get; set; }
        public int Procload { get; set; }
        public int Parameter { get; set; }
    }
}
