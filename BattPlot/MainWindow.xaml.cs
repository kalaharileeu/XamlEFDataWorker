using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvAnalyzer;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Threading;
using System.Windows.Documents;
using AccuracyDAL.Models;

/// <summary>
/// TODO: work on listbox 2 to change the x axis data
/// </summary>

namespace BattPlot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //This only gets loaded once and not reloaded on new csv file load
            //It is just data desciptions and column names

            //XmlManager<DataColumns> columnloader = new XmlManager<DataColumns>();
            ////Initialize the datacolumns class with the wanted columns
            //datacolumns = columnloader.Load("Content/XMLFile1.xml");

            //The csvinterface wrap alot of the dll functionality
            csvinterface = new CSVinterface("Content/XMLFile1.xml");

            //Populate data in listbox
            populateListBox();//Yaxis variable
            polulateListBox1();//Filter variable
            polulateListBox2();//Xaxis variable
            populateListBox3();//These values come from the database
            ////some files to play with should remove/make another plan later
            ///Binding the model to the view in the next two lines
            ///Add a private property of our view model and
            ///initiate this property in the constructor.
            theChargeModel = new ChargeModel();
            //register with delegate events for plotmodel
            //theChargeModel.RegisterWithPlotModelStatus(new ChargeModel.PlotModelStatusHandler(OnPlotModelStatusEvent));
            //Syntax Shortcut below (Method group conversion synatx)
            theChargeModel.RegisterWithPlotModelStatus(OnPlotModelStatusEvent);
            DataContext = theChargeModel;
            //populate text boxes with default values
            populateDefaultTextBoxes();
            //Nothing was selected yet
            selectedPlotItem_Latest = "";
            //Ceate the dispatcherTimer
            dtClockTimer = new DispatcherTimer();
            //The button is disabled until plot added to document
            button.IsEnabled = false;
            doctexthelper = null;
            selected_Testrun_id = -1;
        }
        //desctructor to create a new document before exiting
        //This will call Finalize()
        ~MainWindow()
        {
            //If there are new data to add to doc. do it now
            if(doctexthelper != null)
                if (doctexthelper.helperHealthCheck())
                    createDocument();
        }
         /// <summary>
        /// populating data for the baseline file values
        /// </summary>
        /// <param name="filename"></param>
        private void populateDataTestUnit(string filename)
        {
            if (csvinterface.CSVMetaAndColumndata != null)
            {
                Console.WriteLine("The datacolumns capacity is:" + csvinterface.CSVMetaAndColumndata.Count);
                csvinterface.LoadCSVdata(filename);
            }
        }
        /// <summary>
        /// Populate the list box with the column names.
        /// This list is generated form XML used in GUI listbox
        /// </summary>
        private void populateListBox()
        {
            foreach (Column c in csvinterface.CSVMetaAndColumndata )
            {
                //only add pcu values for plotting but not if is contains "NOFF" && "imag"
                if (c.alias.Contains("pcu") && !(c.alias.Contains("NOFF")) && !(c.alias.Contains("Iacimag")))
                {
                    listBox.Items.Add(new ListBoxItem() { Content = c.alias });
                }
            }
        }
        /// <summary>
        /// Populate ListBox1 with some filter values. There is a 5 filter for now
        /// Should be able to swap these filter, which comes first
        /// The order of how they are added to the listbox is import it is the same order
        /// as they appear in the chargemodel.series
        /// </summary>
        private void polulateListBox1()
        {
            listBox1.Items.Add(new ListBoxItem() { Content = "Filter1: No power out" });
            listBox1.Items.Add(new ListBoxItem() { Content = "Filter2: Bursting" });
            listBox1.Items.Add(new ListBoxItem() { Content = "Filter3: Pwr ratio > 1" });
            listBox1.Items.Add(new ListBoxItem() { Content = "Filter4: Idc > rated" });
            listBox1.Items.Add(new ListBoxItem() { Content = "Filter5: Outside MPPT" });
            listBox1.Items.Add(new ListBoxItem() { Content = "Show all values" });
        }
        //populates the listbox with possible x axis data
        private void polulateListBox2()
        {
            listBox2.Items.Add(new ListBoxItem() { Content = "ACvarpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "ACvapowermeter" }); 
            listBox2.Items.Add(new ListBoxItem() { Content = "Wacpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Vdcpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Idcpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Iacpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Vacpowermeter" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Phaseconfigured" });
            listBox2.Items.Add(new ListBoxItem() { Content = "Temperature" });

        }

        //Add info text to richtextbox and image tags
        private void populateRichText(StringBuilder infotext)
        {
            //add the description to the document helper
            doctexthelper.descriptionLong = infotext.ToString();
            //this is the richtextbox in the gui being updated
            richTextBox.AppendText(infotext.ToString());
            richTextBox.AppendText("\n");
        }
        //Set variable in plotmopel to variables that is currently in textbox to filter/mask against
        //TODO: this can be improved
        private void activateFilterValues()
        {
            //Sets the temperature limits to plotmodel
            int j;
            if(int.TryParse(textBox5.Text, out j))
                theChargeModel.temprPlotLimit = j;
            else
            {
                //Conversion was not succesfull, if above 100 degrees plot all temperatures
                textBox5.Text = "100";
                theChargeModel.temprPlotLimit = 100;
            }
            //Sets the maxDC limits to plotmodel
            if (int.TryParse(textBox2.Text, out j))
                theChargeModel.maxIdcPlotLimit = j;
            else
            {
                //Conversion was not succesfull, if above 100 degrees plot all temperatures
                textBox2.Text = "12";
                theChargeModel.maxIdcPlotLimit = 12;
            }
            //Sets the min MPPT limits to plotmodel
            if (int.TryParse(textBox3.Text, out j))
                theChargeModel.minMPPTPlotLimit = j;
            else
            {
                //Conversion was not succesfull, if above 100 degrees plot all temperatures
                textBox3.Text = "27";
                theChargeModel.minMPPTPlotLimit = 27;
            }
        }
        //populate list box values with defaults
        private void populateDefaultTextBoxes()
        {
            textBox2.Text = "12";//Max rated dc amps
            textBox3.Text = "27";//Minimum Vmpvalue
            textBox5.Text = "100";//set tempperature to all to plot all temperatures
        }
        //give the plotmodel something to plot and a reference to the data
        //or plots from db is the Database checkbox was selected
        private void plotmodeladdscatter()
        {
            //update chargemodel with latest limit values in textboxes
            //Value get read from the filter texboxes and variables gets set
            //in the charge model(plot model)
            activateFilterValues();
            if (checkBox.IsChecked == false)
            {
                //Send the string name of the wanted item and a reference to all the columns data
                theChargeModel.addScatterSeries(selectedPlotItem_Latest, csvinterface.CSVMetaAndColumndata);
            }
            else
            {
                //Database checkbox was checked
                //Y data compare value: Find a Y data value to compare accuracy against the Y axis
                string ycompareValue = csvinterface.CSVMetaAndColumndata.Find(x => x.alias == selectedPlotItem_Latest).accuracystr;
                //Provide Y axis plot values,
                theChargeModel.populatePlotFromDb(selectedPlotItem_Latest, selected_Testrun_id, ycompareValue);
            }
            //Re!-populate the listbox1 with filters to remove
            if (!listBox1.Items.IsEmpty) { listBox1.Items.Clear(); }
            //repopulates all the filter selections in listBox1
            polulateListBox1();
        }
        //use this function to verufy the key strike in the filter textboxes
        private void previewKeyVerify(Key k)
        {
            //if the enter key was pressed and the user selcted a plot item
            //then go abead.
            if ((k == Key.Enter) && (selectedPlotItem_Latest != ""))
            {
                //local function to a add scatter to plot model
                plotmodeladdscatter();
            }
        }
        //sent a pop up message
        private void usermessage(string message)
        {
            MessageBox.Show(message);
        }
        //*******************************************************************************
        //*******************************event handlers here*****************************
        //*******************************************************************************
        //MOUSE upEvent here for list box
        //This is a listbox interupt called when you select a item from the listbox to plot
        //all the filters/masks apply to every item plotted
        private void listBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = (sender as ListBox).SelectedIndex;
            if (i != -1)
            {
                selectedPlotItem_Latest = ((sender as ListBox).Items[i] as
                    ListBoxItem).Content.ToString();
                //add a scatter to the plot model
                plotmodeladdscatter();
            }
        }

        /// <summary>
        /// Plot Filters ListBox, click event
        /// Remove filtered plots and corresponding values from listbox1
        /// </summary>
        /// <param name="sender">listbox</param>
        /// <param name="e"></param>
        private void listBox1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = (sender as ListBox).SelectedIndex;
            if (i != -1)
            {
                //get the name of the selected item
                string v = ((sender as ListBox).Items[i] as ListBoxItem).Content.ToString();
                //if it is equal to the last index then plot all filters back on
                //The last item, refresh complete plot
                if (i == listBox1.Items.Count - 1)
                    plotmodeladdscatter();
                else
                {
                    //remove the item from the charge model
                    theChargeModel.RemoveScatterSeries(i);
                    //Clear out the text of selected
                    (sender as ListBox).Items.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// X-axes variables. The list box hold the x axis variables.
        /// X axis can be changed to theses values. For diagnostics
        /// when the X axis is change replotted the filters must
        /// be readjusted to be the same as the previous plot was
        /// </summary>
        /// <param name="sender">listbox</param>
        /// <param name="e"></param>
        private void listBox2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = (sender as ListBox).SelectedIndex;
            if (i != -1)
            {
                //get the name of the selected x-axis item
                string v = ((sender as ListBox).Items[i] as ListBoxItem).Content.ToString();
                //Change the chargemodel Xaxis data/send the alias name to chargemodel
                theChargeModel.xaxisPlotValues = v;
                //ListBox1(Filter) get current state
                List<string> oldActiveFilters = new List<string>();
                for(int k = 0; k < listBox1.Items.Count; k++)
                {
                    oldActiveFilters.Add((listBox1.Items[k] as ListBoxItem).Content.ToString());
                }
                //replot after Xaxis data change
                plotmodeladdscatter();
                //ListBox1(Filter) get current state
                List<string> newActiveFilters = new List<string>();
                //after replot there will be a new filter state
                //This is after the plot is done with all the filter repopulated
                for (int k = 0; k < listBox1.Items.Count; k++)
                {
                    newActiveFilters.Add((listBox1.Items[k] as ListBoxItem).Content.ToString());
                }
                //Values to remove list
                List<int> plotstoremove = new List<int>();
                for(int j = 0; j < newActiveFilters.Count; j++)
                {
                    if(!oldActiveFilters.Contains(newActiveFilters[j]))
                    {
                        //remove the item from the charge model
                        theChargeModel.RemoveScatterSeries(j);
                        //Clear out the text of selected
                        (listBox1 as ListBox).Items.RemoveAt(j);
                        newActiveFilters.RemoveAt(j);
                        j = -1;
                    }
                }
            }
        }
        /// <summary>
        /// Database acces user interface start here
        /// Database Testrun diplayed in the listBox 3
        /// I select a item I want to be able to plot the values form historical data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Clear the plot when new file uploaded
            theChargeModel.ClearPlot();
            //Clear the richtext box here
            richTextBox.Document.Blocks.Clear();

            //if the checkbox to plot data from database is not checked then check it
            //because the user want to plot database values
            if (checkBox.IsChecked == false)checkBox.IsChecked = true;

            int i = (sender as ListBox).SelectedIndex;
            if (i != -1)
            {
                //if the current item is Testrun type the get the testrun ID
                if((sender as ListBox).Items[i] is Testrun)
                {
                    //testrunId is a column in the Testrun table
                    selected_Testrun_id = ((sender as ListBox).Items[i] as Testrun).testrunID;
                }
            }
        }
        /// <summary>
        /// Open CSV dialog. File dialog operation here to open CSV
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // If there are new data to save in doctexthelper, save it to docx
            if(doctexthelper != null)
                if (doctexthelper.helperHealthCheck())
                    createDocument();
                
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                //create a new doctext helper with each CSV
                doctexthelper = new DocTextHelper();
                //disable the button for docx creation
                button.IsEnabled = false;
                //Clear the plot when new file uploaded
                theChargeModel.ClearPlot();
                //Get file name from Win OpenDialog
                string filename = dlg.FileName;
                doctexthelper.filepath = filename;
                //Clear the richtext box here
                richTextBox.Document.Blocks.Clear();
                //Add additional data to the richtextbox
                populateRichText(HelperStatic.readAdditionalInfo(filename));
                //Creates a sandbox directory
                accuracydir = HelperStatic.CreatePlotSandbox(filename, "Accuracy");
                //add filename text to the textbox
                textBox6.Text = filename;
                //read the data and populate data members, could be sunchronous
                populateDataTestUnit(filename);
            }
        }
        //Add aux. text to the richtextbox
        void richtextbox_populate()
        {
            //Get file name from Win OpenDialog
            string filename = textBox6.Text;
            if (filename != "")
            {
                doctexthelper.filepath = filename;
                //Clear the richtext box here
                richTextBox.Document.Blocks.Clear();
                //Add additional data to the richtextbox
                populateRichText(HelperStatic.readAdditionalInfo(filename));
            }
        }
        //The text preview event handlre uses this
        private static bool IsTextAllowed(string text)
        {
            //this regular expression checks that it is only number, dots and dashes
            //Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            Regex regex = new Regex("[^0-9-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
        //The text preview event handlre uses this
        private static bool IsTextAllowed_nodash(string text)
        {
            //this regular expression checks that it is only number, dots and dashes
            //Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
        //This is the text preview event handler, to verufy text input
        private void textBox2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            //Get the text of the textbox
            if (objTextBox.Text.Length > 6)
                e.Handled = true;
            else if(objTextBox.Text.Length == 0)
                e.Handled = !IsTextAllowed(e.Text);
            else
                e.Handled = !IsTextAllowed_nodash(e.Text);
        }

        private void textBox3_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            //Get the text of the textbox
            if (objTextBox.Text.Length > 6)
                e.Handled = true;
            else if (objTextBox.Text.Length == 0)
                e.Handled = !IsTextAllowed(e.Text);
            else
                e.Handled = !IsTextAllowed_nodash(e.Text);
        }

        private void textBox4_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            //Get the text of the textbox
            if (objTextBox.Text.Length > 6)
                e.Handled = true;
            else if (objTextBox.Text.Length == 0)
                e.Handled = !IsTextAllowed(e.Text);
            else
                e.Handled = !IsTextAllowed_nodash(e.Text);
        }

        private void textBox5_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            //Get the text of the textbox
            if (objTextBox.Text.Length > 6)
                e.Handled = true;
            else if (objTextBox.Text.Length == 0)
                e.Handled = !IsTextAllowed(e.Text);
            else
                e.Handled = !IsTextAllowed_nodash(e.Text);
        }

        //Event called when key pressed in textbox
        private void textBox5_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //call local menemer to verify input
            previewKeyVerify(e.Key);
        }
        //Event called when key pressed in textbox
        private void textBox4_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //call local menemer to verify input
            previewKeyVerify(e.Key);
        }
        //Event called when key pressed in textbox
        private void textBox3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //call local menemer to verify input
            previewKeyVerify(e.Key);
        }
        //Event called when key pressed in textbox
        private void textBox2_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //call local menemer to verify input
            previewKeyVerify(e.Key);
        }

        //Save image to folder, use helper funtions here
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //disable button2 while busy
            button2.IsEnabled = false;
            //creates the directory and
            if (textBox6.Text != "")
            {
                //check if directory exists
                if (HelperStatic.DirChecks(accuracydir))
                {
                    //saveImage pulls in comments if any
                    saveImage(accuracydir);
                }
            }
            else
            {
                usermessage("No file selected yet, see Open File button.");
            }
            //Enable button 2 again.
            button2.IsEnabled = true;
        }

        /// <summary>
        /// Click the button to load current data to database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //disable button3 while busy
            button3.IsEnabled = false;
            if (textBox6.Text != "")
            {
                //Need the full csv file name here
                loadrepos(textBox6.Text);
            }
            //update list box 3 to display the new data
            populateListBox3();
            //Enable button 2 again.
            button3.IsEnabled = true;
        }

        //DocumentX. This button event handler will try and create the document
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //if the seems to be enough correct data, create docx
            if (doctexthelper.helperHealthCheck())
                createDocument();//Create a workd document
            else
                MessageBox.Show("Not enough information to create .docx");
        }

        //when the ClockTimer event runs out
        //this event handler gets called, resets the popup
        private void dtClockTime_Tick(object sender, EventArgs e)
        {
            //stop the timer
            // dtClockTimer.Stop();
            if (PopupFileSaved.IsOpen == true)
            {
                PopupFileSaved.IsOpen = false;
                //PopupFileSaved.InvalidateVisual();
            }

            if (PopupNotSaved.IsOpen == true)
            {
                PopupNotSaved.IsOpen = false;
                //PopupFileSaved.InvalidateVisual();
            }
            //remove the event hamdler
            dtClockTimer.Tick -= dtClockTime_Tick;
        }

        //pop up  the saved message
        public void popupSaved(string message)
        {
            PopupNotSaved.IsOpen = true;
            setupClockTimer(2);
        }
        
        static int noOfImageSaved = 0;
                
        //called when delegate in plotmodel fires
        public static void OnPlotModelStatusEvent(string message)
        {
            noOfImageSaved += 1;
            //messagePopUp = s => popupSaved("saved");
        }
        //**********************************END event handler**************************

        //check and fets the usercommnets in richtextbox1
        private string check_getuserComments(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart,
                rtb.Document.ContentEnd);
            //check if the user edited the text
            if ((textRange.Text.Length == 11) && (textRange.Text == "Comments:\r\n"))
                return null;
            //if the user did edit the text return it
            return textRange.Text;
        }

        /// <summary>
        /// Request charge model to save the image.
        /// Use popups to let the user know image saved or not
        /// The image gets saved async/await
        /// </summary>
        /// <param name="filename"></param>
        private void saveImage(string filename)
        {
            //this calls a async/await method in theChargeModel
            string path =  theChargeModel.save(accuracydir);
            //Check if image file saved and give necesary popup
            //TODO! Does not work synchronous file save still
            //saving while checking if the file exist
            //if (File.Exists(path))
            //{
            //get possible user text and comments
            string temp = check_getuserComments(richTextBox1);
            //If the path is null then image was not saved
            if (path != null)
            {
                //if the user text is not null, add it to doctexthelper helper
                if (temp != null)
                {
                    doctexthelper.AddImageToDict(path, temp);
                }
                    PopupFileSaved.IsOpen = true;
                    button.IsEnabled = true;
            }
            else
            {
                PopupNotSaved.IsOpen = true;
            }
            //this kick off a timer that will show the pop up message
            //for a certain amount of time
            setupClockTimer(3);
        }
        //sets up the clock timer for specific amout of seconds
        private void setupClockTimer(int sec)
        {
            dtClockTimer.Interval = new TimeSpan(0, 0, sec);
            //add a event hadler for Tick event
            dtClockTimer.Tick += dtClockTime_Tick;
            dtClockTimer.Start();
        }

        //Database selection
        int selected_Testrun_id;

        //Csv interface lives in CSVanalyzer.dll
        CSVinterface csvinterface;

        /// Interaction logic for MainWindow.xaml
        private ChargeModel theChargeModel;
        //The latest plot item that was selected from
        //listbox that the user want to look at
        private string selectedPlotItem_Latest;
        private string accuracydir;
        //timer for the popup
        private DispatcherTimer dtClockTimer;
        private DocTextHelper doctexthelper;

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            //Clear the plot when new file uploaded
            theChargeModel.ClearPlot();
            richtextbox_populate();
        }
    }
}

