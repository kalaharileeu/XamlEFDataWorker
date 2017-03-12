using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using CsvAnalyzer;
using OxyPlot.Axes;
using System.IO;
using System.Threading.Tasks;
using AccuracyDAL.Repos;
//using System.Drawing;

/// <summary>
/// TODO List: Plot higher definition plot
/// </summary>
namespace BattPlot
{
    public class ChargeModel
    {
        //Constructor
        public ChargeModel()
        {
            //the charge model is bound in the MianWindow.xaml file to ViewPlot Model
            theChargeModel = new PlotModel() { Title = "Debug plot"};
            //theChargeModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            lineseries = new LineSeries();
            scatterseries = new ScatterSeries();
            //some general setup here
            //Default setup for X-Axis data
            xaxisPlotValues = "ACvapowermeter";
            titleBuilderColumnName = "";
            setup();
        }
        /// <summary>
        ///General plot setup over here
        /// </summary>
        private void setup()
        {
            ////Setting up the legend
            //Tip: you must assign a Title ex. Title = "Scatter" to the series, else does appear in legend 
            theChargeModel.LegendTitle = "Legend";
            theChargeModel.LegendOrientation = LegendOrientation.Horizontal;
            theChargeModel.LegendPlacement = LegendPlacement.Outside;
            theChargeModel.LegendBorder = OxyColors.Black;
            theChargeModel.LegendPosition = LegendPosition.BottomCenter;
            //this is another axes
            linearYaxis = new LinearAxis{ Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot, IntervalLength = 50, Title = "PCU value - Powermeter value" };
            linearXaxis = new LinearAxis{ Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot, IntervalLength = 40, Title = "AC VA powermeter", MaximumRange = 350};
            theChargeModel.Axes.Add(linearXaxis);
            theChargeModel.Axes.Add(linearYaxis);
            //make the delgate member null
            listOfHandlers = null;
        }

        public string save(string dirname)
        {
            SavePlot(dirname);
            return latestimagepath;
        }
        /// <summary>
        /// Saves the plot asynchronously, some drives are slow and there might. be delays in saving files. 
        /// Todo:delegate here to check that file is there and let the user know the file is realy saved
        /// </summary>
        /// <param name="dirname"></param>
         public async void SavePlot(string dirname)
        {
            //var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 600, Height = 400, Background = OxyColors.White };
            //pngExporter.Export(theChargeModel,);
            ////var img = System.Drawing.Image.FromStream(myStream);
            //stream.Save(System.IO.Path.GetTempPath() + "\\myImage.Jpeg", ImageFormat.Jpeg);
            latestimagepath = null;
            //if directory does not check out the return
            if (!HelperStatic.DirChecks(dirname)){ return; }

            //Have to have this test.txt to create stream to file
           using (var stream = File.Create(dirname + "\\test.txt"))
           {
                var pngExporter = new OxyPlot.Wpf.PngExporter() {
                    Width = 1200, Height = 800, Background = OxyColors.White };
                pngExporter.Export(theChargeModel, stream);

                var img = System.Drawing.Image.FromStream(stream);
                //This forms a file name approved string
                string imagefilename = titleBuilderColumnName + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".png";
                latestimagepath = Path.Combine(dirname, imagefilename);
                //await for this task to funnish
                //await Task.Run(() => img.Save(latestimagepath, System.Drawing.Imaging.ImageFormat.Png));
                await Task.Run(() => SaveImage(latestimagepath, img));
            }
        }
        //This function can be called asynchornously
        private void SaveImage(string latestfilepath, System.Drawing.Image img)
        {
            //await for this task to funnish
            img.Save(latestfilepath, System.Drawing.Imaging.ImageFormat.Png);
            //delegate invoke here, use delgate to call back
            listOfHandlers?.Invoke("Image saved");
        }
        /// <summary>
        /// Adds a general two list-series plot
        /// </summary>
        /// <param name="valuex">List of float</param>
        /// <param name="valuey">List of float</param>
        public void addToLineSeries(List<float> valuex, List<float> valuey)
        {
            //list x longer than will y, return
            if (valuex.Count > valuey.Count)return;
            //loop through values
            for (int i = 0; i < valuex.Count; i++)
                lineseries.Points.Add(new DataPoint(valuex[i], valuey[i]));
            theChargeModel.Series.Add(lineseries);
        }
        /// <summary>
        /// Add skip  data to the plot
        /// </summary>
        /// <param name="vColumns">reference var to data from CSV</param>
        /// <param name="skipColumns">reference var to skip data</param>
        public void AddSkipsLines(string dataY, List<Column> vColumns, List<Column> skipColumns)
        {
            if (dataY == "") return;
            //Y axis data: dataY and YColumn is the wanted Y axis data/plot
            var Ycolumn = vColumns.Find(y => y.alias == dataY);
            //no variable to comapre against for the accuracy varlues
            if (Ycolumn.accuracystr == "")return;
            //Copy constructor not not to have reference that is modified
            List<float> Ycolumnfloats = new List<float>(Ycolumn.GetFloats);
            //X axis data: xaxisPlotValue is selected by the user
            var Xcolumnfloats = vColumns.Find(x => x.alias == xaxisPlotValues).GetFloats;
            //Y axis compare data: compare values to set the amplitude of the skip plotted line
            var Ycompare = vColumns.Find(ycomp => ycomp.alias == Ycolumn.accuracystr);
            List<float> Ycomparefloats;
            if (Ycolumn.accuracystr == "Phaseconfigured")
            {
                //Have to create this in order not to modify the original list pointed to
                //copy constructor the dataYcompare list
                Ycomparefloats = new List<float>(Ycompare.GetFloats);
                //Modify the list values from degrees to power factor 
                modifyPhaseconfigured(Ycomparefloats);
            }
            else
            {
                //get the Y compare data, copy constructor NOT used used
                Ycomparefloats = new List<float>(Ycompare.GetFloats);
            }

            //Modify value if needed
            //Special treatment for pf reporting accuracy, some conversions needed for pcu pf
            if (Ycolumn.alias == "Powerfactorpcu")
            {
                //Get the data for "Powerfactorpcu" 
                //dataYList = new List<float>(vColumn.GetFloats);
                modifyPowerfactorpcu(Ycolumnfloats);//Send a reference to be modified
            }
            //Iac Apparant mods
            if (dataY == "IacApparantpcu_calc")
            {
                //iacApparantpcu_calc is the real current measured by pcu,
                // iacimagpcu is the imaginary part of the ac current from pcu, then calculate the apparant current
                List<float> iacimagpcuData = (vColumns.Find(iimag => iimag.alias == "Iacimagpcu")).GetFloats;
                Ycolumnfloats = HelperStatic.Get_pcu_apparantcurrent(iacimagpcuData, Ycolumn.GetFloats);
            }

            //For every skip column
            for (int k = 0; k < skipColumns.Count; k++)
            {
                //Add a line series for xaxis value vs skip
                var templine = new LineSeries() { Title = $"{skipColumns[k].alias} skip", MarkerSize = 2 };
                //Look through the skip columns
                for (int j = 0; j < skipColumns[k].GetFloats.Count; j++)
                {
                    //Work for all the skips in the list, 
                    //if there is one add it, with xaxis value
                    if (skipColumns[k].GetFloats[j] == 1f)
                    {
                        var Yamplitude = Ycolumnfloats[j] - Ycomparefloats[j];
                        //Get the last inserted scatter series
                        templine.Points.Add(new DataPoint(Xcolumnfloats[j], 0.0f));
                        templine.Points.Add(new DataPoint(Xcolumnfloats[j], Yamplitude));
                        templine.Points.Add(new DataPoint(Xcolumnfloats[j], 0.0f));
                    }
                }
                theChargeModel.Series.Add(templine);
            }
            //Refresh the plot
            theChargeModel.InvalidatePlot(true);
        }
        /// <summary>
        /// The is effectively just for accuracy plots, prefetch acpower to plot against
        /// MainWindow.xaml calls this to add a plot to PlotView Model
        /// The valuelists contains reference to all data, name to plot
        /// </summary>
        /// <param name="dataY">String alias name for csv data, Y data name</param>
        /// <param name="valuelists">List of IBaselist</param>
        public void addScatterSeries(string dataY, List<Column> vList)
        {
            if (vList == null) return;
            //List<float> dataXList = new List<float>();
            //Title update.
            plotTitleBuilder("CSV.", dataY);
            //titleBuilderColumnName = valuelist.GetName();
            List<float> dataYList = new List<float>();
            //this is most likely power meter data
            List<float> dataYcompare = new List<float>();
            //xaxisPlotValue is selected by the user, LINQ or lambda
            var temp_column = vList.Find(x => x.alias == xaxisPlotValues);
            List<float> dataXList = temp_column.GetFloats;
            //Scale value if needed
            for (int k = 0; k < dataXList.Count; k++)
                dataXList[k] /= temp_column.scale;
            //Iterate through the list of Column to further work the data
            foreach (var vColumn in vList)
            {
                //get the value list with the requested name for y axis
                //if there is a accuracy column related to this column(see xml) get that column too
                if (vColumn.alias == dataY)
                {
                    //Some special treatment for cfg_phase here, unique data, needs attention
                    //Special treatment for pf reporting accuracy, some conversions needed for pcu pf
                    if (dataY == "Powerfactorpcu")
                    {
                        //Get the data for "Powerfactorpcu" 
                        dataYList = new List<float>(vColumn.GetFloats);
                        modifyPowerfactorpcu(dataYList);//Send a reference to be modified
                    }
                    else if (dataY == "IacApparantpcu_calc")
                    {
                        //iacApparantpcu_calc is the real current measured by pcu
                        //iacimagpcu is the imaginary part of the ac current from pcu, then calculate the apparant current
                        List<float> iacimagpcuData = (vList.Find(x => x.alias == "Iacimagpcu")).GetFloats;
                        dataYList = HelperStatic.Get_pcu_apparantcurrent(iacimagpcuData, vColumn.GetFloats);
                    }
                    else
                    {
                        //get the Y data, most likely pcu data, this ydata column name comes from GUI
                        dataYList = vColumn.GetFloats;
                    }
                    //Check if the valuelist has a accuracy value to compare too
                    //if there is text non empty text it will have
                    //accuracystr is the alias name of the column that contains the reference data, to get accuracy data
                    if (vColumn.accuracystr != "")
                    {
                        //Getaccuracystr gets the string "accuracystr" property, it is most likely
                        //a pm object "alias" - property
                        //to for accuracy, find the right set of data to compare accuracy too
                        //The data in the datainterface used for accuracy calculations
                        //Find the data interface with the same name as the valuelist accuracy dependency
                        var compareColumn = vList.Find(x => x.alias == vColumn.accuracystr);
                        //Some special treatment for cfg_phase, fin it and deal with it
                        if (vColumn.accuracystr == "Phaseconfigured")
                        {
                            //Have to create this in order not to modify the original list pointed to
                            //copy construct the dataYcompare list
                            dataYcompare = new List<float>(compareColumn.GetFloats);
                            //Modify the list values from degrees to power factor 
                            modifyPhaseconfigured(dataYcompare);
                        }
                        else
                        {
                            //get the list of data
                            if (dataYcompare != null)dataYcompare = compareColumn.GetFloats;
                        }
                    }
                }
            }
            //populate the plot values in this datamodel
            populateACpowerVSyvalue(dataXList, dataYList, dataYcompare, vList);
            //Invalidate to refresh graph
            //theChargeModel.InvalidatePlot(true);
        }

        //Modify the power factor pcu value ex.  -45deg to 0.707
        private void modifyPowerfactorpcu(List<float> listtomodify)
        {
            //convert from degrees to power factor for ex. -45deg to 0.707
            for (int k = 0; k < listtomodify.Count; k++)
                listtomodify[k] = (Convert.ToSingle(Convert.ToDouble(listtomodify[k]) / Math.Pow(2, 22)));
        }

        //Modify the Phaseconfigured pcu value ex.  -45deg to 0.707
        private void modifyPhaseconfigured(List<float> listtomodify)
        {
            //convert from degrees to power factor for ex. -45deg to 0.707
            for (int k = 0; k < listtomodify.Count; k++)
                listtomodify[k] = Convert.ToSingle(Math.Cos(Convert.ToDouble(listtomodify[k]) * Math.PI / 180));
        }
        //builds the title of the plot
        private void plotTitleBuilder(string data_origin, string columnNameYata)
        {
            linearXaxis.Title = xaxisPlotValues;
            string tempstr;
            //it the tempreature is above 99 it will plot all temperatures
            if (temprPlotLimit > 99) tempstr = "All";
            else tempstr = temprPlotLimit.ToString();
            //gets the alias of the Y data and make it part of the title
            titleBuilderColumnName = columnNameYata;
            theChargeModel.Title = "Accuracy plot: " + titleBuilderColumnName + " Tempr: " + tempstr + " °C" + " from " + data_origin;//alt 0176 = °
            Title = theChargeModel.Title;
        }

        //plot acpowermeter againsome other value from csv. file
        private void populateACpowerVSyvalue(List<float> valueX, List<float> valueY, List<float>valueYcompare, List<Column> vList)
        {
            ClearPlot();
            //lambda expressions here
            //Get all the columns interfaces that has to serve a s limits
            //creating the noff series
            //FILTER 1: Temperature
            //List<float> temprData = (datalistInterfaces.Find(x => x.GetName() == "Temperature")).GetFloats();
            List<float> temprData = (vList.Find(x => x.alias == "Temperature")).GetFloats;
            //FILTER 2: Did not do power
            List<float> acwPmData = (vList.Find(x => x.alias == "Wacpowermeter")).GetFloats;
            ScatterSeries acwPmSeries = new ScatterSeries { Title = "No output Wac", MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.IndianRed };
            //FILTER 3 : Is it bursting
            List<float> noffPcuData = (vList.Find(x => x.alias == "NOFFpcu")).GetFloats;
            ScatterSeries noffSeries = new ScatterSeries {Title = "Bursting", MarkerType = MarkerType.Square, MarkerSize = 5, MarkerFill = OxyColors.Orange };
            //creating the powerratio series
            //FILTER 4 : Power ratio larger than 1. Output over input
            List<float> powerRatioCnfData = (vList.Find(x => x.alias == "Powerratioconfigured")).GetFloats;
            ScatterSeries powerRatioSeries = new ScatterSeries { Title = "Pwr ratio > 1", MarkerType = MarkerType.Diamond, MarkerSize = 5, MarkerFill = OxyColors.Red };
            //creating the idclLlimitSeries
            //FILTER 5 : idc current limiting high dc current
            List<float> idcPmData = (vList.Find(x => x.alias == "Idcpowermeter")).GetFloats;
            ScatterSeries idcPmSeries = new ScatterSeries { Title = "Idc > rated", MarkerType = MarkerType.Diamond, MarkerSize = 5, MarkerFill = OxyColors.Olive};
            //Dcv configured series
            //FILTER 6: dcV smaller than MPPT min range
            List<float> dcvCnfData = (vList.Find(x => x.alias == "Vdcconfigured")).GetFloats;
            ScatterSeries dcvCnfSeries = new ScatterSeries { Title = "Outside MPPT", MarkerType = MarkerType.Triangle, MarkerSize = 4, MarkerFill = OxyColors.CornflowerBlue };

            //Real values the ones I actually want
            ScatterSeries acwFilteredSeries = new ScatterSeries { Title = "Nominal op. range", MarkerType = MarkerType.Circle, MarkerSize = 4, MarkerFill = OxyColors.Black };

            //If there are the same amount of values in each list then go.
            if (valueYcompare.Count == valueY.Count)
            {
                //Apply the filters
                for (int k = 0; k < valueY.Count; k++)
                {
                    //if the spedific temperatur is not wanted, if larger than 99 plot all temperatures
                    if (temprData[k] == temprPlotLimit || temprPlotLimit > 99)
                    {
                        if (acwPmData[k] <= 3.0)//if smaller than 4W AC power
                            acwPmSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (noffPcuData[k] > 0)//Bursting
                            noffSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (powerRatioCnfData[k] > 1.0)//Power ratio > 1
                            powerRatioSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (idcPmData[k] > maxIdcPlotLimit)//DC current larger than ERD limit
                            idcPmSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (dcvCnfData[k] < minMPPTPlotLimit)//Dc configered smaller than 27Vmp
                            dcvCnfSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else
                            acwFilteredSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                    }
                }
            }
            //Add all the series to the plots
            //The order off this is important to the filter remove function
            theChargeModel.Series.Add(acwPmSeries);
            theChargeModel.Series.Add(noffSeries);
            theChargeModel.Series.Add(powerRatioSeries);
            theChargeModel.Series.Add(idcPmSeries);
            theChargeModel.Series.Add(dcvCnfSeries);
            //This is all the values that was not filter 
            theChargeModel.Series.Add(acwFilteredSeries);

            //acwFilteredSeries.MouseDown += acw_MouseDown;
            //Refresh the plot
            theChargeModel.InvalidatePlot(true);
        }
        /*****************************************Database plot config******************************************/
        /// <summary>
        /// Database! Populate data from the database. 
        /// </summary>
        /// <param name="columnAliasToPlot">Column Alias name</param>
        public void populatePlotFromDb(string columnAliasToPlotY, int SelectedDBTesrun, string columnAliasYcompare)
        {
            //if not db value selected, do not try and extract data or plot
            //or is the is no legitimate value to compare 
            if ((SelectedDBTesrun == -1) || (columnAliasYcompare == "")) return;
            ClearPlot();
            var repoTP = new TestpointRepo();
            //Database: Get values from repo here
            List<float> valueY = repoTP.GetColumnData(columnAliasToPlotY, SelectedDBTesrun);
            List<float> valueYcompare = repoTP.GetColumnData(columnAliasYcompare, SelectedDBTesrun);
            //xaxis vale to plot comes from local variable and listbox2
            List<float> valueX = repoTP.GetColumnData(xaxisPlotValues, SelectedDBTesrun);
            //Special condition: if the Y data is "Powerfactorpcu", then it needs some calculations
            if (columnAliasToPlotY == "Powerfactorpcu")modifyPowerfactorpcu(valueY);//Send a reference to ne modified
            //Special condition where data needs tunning
            if (columnAliasYcompare == "Phaseconfigured")modifyPhaseconfigured(valueYcompare);
            if (columnAliasToPlotY == "IacApparantpcu_calc")
            {
                //iacApparantpcu_calc real current measured by pcu
                //iacimagpcu is the imaginary part of the ac current from pcu, then calculate the apparant current
                List<float> iacimagpcuData = repoTP.GetColumnData("Iacimagpcu", SelectedDBTesrun);
                valueY = HelperStatic.Get_pcu_apparantcurrent(iacimagpcuData, valueY);
            }

            //update the Title builde here
            plotTitleBuilder("database.", columnAliasToPlotY);
            //I the values were not in database then return
            if ((valueY == null) || (valueYcompare == null) || (valueX == null))return;

            //lambda expressions here
            //Get all the columns interfaces that has to serve a s limits
            //creating the noff series
            //FILTER 1: Temperature
            List<float> temprData = repoTP.GetColumnData("Temperature", SelectedDBTesrun);
            //ScatterSeries acwPmSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.IndianRed };
            //FILTER 2: Did not do power
            List<float> acwPmData = repoTP.GetColumnData("Wacpowermeter", SelectedDBTesrun);
            ScatterSeries acwPmSeries = new ScatterSeries { Title = "No output Wac", MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.IndianRed };
            //FILTER 3 : Is it bursting
            List<float> noffPcuData = repoTP.GetColumnData("NOFFpcu", SelectedDBTesrun);
            ScatterSeries noffSeries = new ScatterSeries { Title = "Bursting", MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.Orange };
            //creating the powerratio series
            //FILTER 4 : Power ratio larger than 1. Output over input
            List<float> powerRatioCnfData = repoTP.GetColumnData("Powerratioconfigured", SelectedDBTesrun);
            ScatterSeries powerRatioSeries = new ScatterSeries { Title = "Pwr ratio > 1", MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.Red };
            //creating the idclLlimitSeries
            //FILTER 5 : idc current limiting high dc current
            List<float> idcPmData = repoTP.GetColumnData("Idcpowermeter", SelectedDBTesrun);
            ScatterSeries idcPmSeries = new ScatterSeries { Title = "Idc > rated", MarkerType = MarkerType.Circle, MarkerSize = 5, MarkerFill = OxyColors.Olive };
            //Dcv configured series
            //FILTER 6: dcV smaller than MPPT min range
            List<float> dcvCnfData = repoTP.GetColumnData("Vdcconfigured", SelectedDBTesrun);
            ScatterSeries dcvCnfSeries = new ScatterSeries { Title = "Outside MPPT", MarkerType = MarkerType.Circle, MarkerSize = 4, MarkerFill = OxyColors.CornflowerBlue };

            //Real values the ones I actually want
            ScatterSeries acwFilteredSeries = new ScatterSeries { Title = "Nominal op. range", MarkerType = MarkerType.Circle, MarkerSize = 4, MarkerFill = OxyColors.Black };
            //If there are the same amount of values in each list the go.
            if (valueYcompare.Count == valueY.Count)
            {
                //Apply filters
                for (int k = 0; k < valueY.Count; k++)
                {
                    //if the spedific temperatur is not wanted, if larger than 99 plot all temperatures
                    if (temprData[k] == temprPlotLimit || temprPlotLimit > 99)
                    {
                        if (acwPmData[k] <= 3.0)//if smaller than 4W AC power
                            acwPmSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (noffPcuData[k] > 0)//Bursting
                            noffSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (powerRatioCnfData[k] > 1.0)//Power ratio > 1
                            powerRatioSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (idcPmData[k] > maxIdcPlotLimit)//DC current larger than ERD limit
                            idcPmSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else if (dcvCnfData[k] < minMPPTPlotLimit)//Dc configered smaller than 27Vmp
                            dcvCnfSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                        else
                            acwFilteredSeries.Points.Add(new ScatterPoint(valueX[k], valueY[k] - valueYcompare[k]));
                    }
                }
            }
            //Add all the series to the plots
            //The order off this is important to the filter remove function
            theChargeModel.Series.Add(acwPmSeries);
            theChargeModel.Series.Add(noffSeries);
            theChargeModel.Series.Add(powerRatioSeries);
            theChargeModel.Series.Add(idcPmSeries);
            theChargeModel.Series.Add(dcvCnfSeries);
            //This is all the values that was not filter 
            theChargeModel.Series.Add(acwFilteredSeries);

            //acwFilteredSeries.MouseDown += acw_MouseDown;

            //Refresh the plot
            theChargeModel.InvalidatePlot(true);
        }
        //Clears all the values from the plot
        internal void ClearPlot()
        {
            theChargeModel.Series.Clear();
            //Refresh the plot
            theChargeModel.InvalidatePlot(true);
        }
        //Removes the scatter series that matches int parameter
        //int value comes from listbox selection
        public void RemoveScatterSeries(int seriestoremove)
        {
            //check is series to remove is in range of what is currently in the list
            if ((theChargeModel.Series.Count - 1) >= seriestoremove && (seriestoremove >= 0))
            {
                theChargeModel.Series.RemoveAt(seriestoremove);
                //Refresh the plot
                theChargeModel.InvalidatePlot(true);
            }
        }
        /*********************************Start Delegate declaration here******************************/
        //Define: Delegate defined that will call function that takes a single string parameter
        public delegate void PlotModelStatusHandler(string msgForThisClassUser);
        //Define: Local Member variable for above delgate
        private PlotModelStatusHandler listOfHandlers;
        //Registration function for delegate, not multicast enable to multicast = vs. +=
        public void RegisterWithPlotModelStatus(PlotModelStatusHandler methodcall)
        {
            listOfHandlers = methodcall;
        }
        /*****************************************END delegate******************************************/
        public string Title { get; private set; }
      // public string imageTag { get; set; }
        public PlotModel theChargeModel { get; private set; }
        public int temprPlotLimit { get; set; }
        public int maxIdcPlotLimit { get; set; }
        public int minMPPTPlotLimit { get; set; }
        public int maxACWPlotLimit { get; set; }
        //db functionality here
        //This is the selcted testrun that comes from listBox3 Mainwindow
        //public int SelectedDBTesrun { get; set; } = -1;
        //The X data for upcoming plots, set from gui interface
        public string xaxisPlotValues { get; set; }
        //used to build the ttile of the plot
        //private string titleBuilderTempr;
        private string titleBuilderColumnName;
        private string latestimagepath;//this is where latest image saved
        private LinearAxis linearYaxis;
        private LinearAxis linearXaxis;
        private LineSeries lineseries;
        private ScatterSeries scatterseries;
    }
}
/*
#pull in reported and measured power factor data
reported_pf = df.pcu_qP_PF/2**22
measured_pf = df.pm_ac_lambda
commanded_pf = np.cos(df.cfg_phase * np.pi / 180)
reporting_error_pf = reported_pf - measured_pf
control_error_pf = measured_pf - commanded_pf
*/
// theChargeModel.LegendArea.
//Format Date time axes to display date format
//var dateAxis = new DateTimeAxis()
//{ MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80,
//StringFormat = "dd/MM/yy HH:mm:ss", Title = "Date Time" };
//theChargeModel.Axes.Add(dateAxis);

////lineplot against time
/////Populate line series. Function that adds values to a lineseries
//private void populateTimeVSValue(LineSeries ls, List<float> timeX, List<float> valueY)
//{
//    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

//    for (int i = 0; i < timeX.Count; i++)
//    {

//        epoch.AddSeconds(timeX[i]);
//        ls.Points.Add(new DataPoint
//            (DateTimeAxis.ToDouble(epoch.AddSeconds(timeX[i])), valueY[i]));
//    }
//}
//Scatterplot against time
//Pupolate scatterplot series. Function that adds values to a lineseries
//Time needs to be a double, overload valuelist to do this 
//private void populateTimeVSValuescatter(ScatterSeries sc, List<float> timeX, List<float> valueY)
//{
//    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

//    for (int i = 0; i < timeX.Count; i++)
//    {
//        epoch.AddSeconds(timeX[i]);
//        sc.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(epoch.AddSeconds(timeX[i])), valueY[i]));
//    }
//}
