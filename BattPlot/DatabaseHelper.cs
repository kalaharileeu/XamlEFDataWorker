using System;
using System.Collections.Generic;
using System.Linq;
using AccuracyDAL.Models;
using AccuracyDAL.Repos;
using System.Windows;
using System.Reflection;

namespace BattPlot
{
    public partial class MainWindow : Window
    {
        //Load up the repo
        //the testrun repo populate function: needs the full .csv link to get
        //the extra data file (.txt)
        //The testpoint repo populate function needs the TESTRUN FOREIGN KEY
        private void loadrepos(string fullcsvfilepath)
        {
            //Load the testrun repo then send the reference to Testpoint repo load to load testpoints
            //The Load_TestRun_repo returns a reference to the Testrun FK.
            Load_Testpoint_repo(Load_TestRun_repo(fullcsvfilepath));
        }
        /// <summary>
        /// Load data from the realpowerdictionary into Testpointrepo
        /// I need a file name for the function, to extract some more data need
        /// </summary>
        private Testrun Load_TestRun_repo(string folderPathForTextFile)
        {
            //TODO: Get a better way to find the number of rows
            if (csvinterface.GetColumnData("SerialNumber").Count == 0)
            {
                //WriteLine("NULL from testrun load");
                return null;
            }
            //Get the properties for the EF.Model
            PropertyInfo[] test_fields = AccuracyDAL.Helper.GetPropertlyArray(typeof(Testrun));
            //PropertyInfo[] test_fields = typeof(Testrun).GetProperties(BindingFlags.Public |
            //    BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
            //Populate the data in the Testrun model, iterate through the property info
            //create a temporay test run to populate
            Testrun tempTestRun = new Testrun();
            foreach (var property in test_fields)
            {
                switch (property.Name)
                {
                    case "HardwareType":
                        //SetValue takes the object reference and the value for the property
                        property.SetValue(tempTestRun, (csvinterface.GetColumnData(property.Name))[0]);//TODO improve:Take the first element, dodgy
                        break;
                    case "SerialNumber":
                        property.SetValue(tempTestRun, (csvinterface.GetColumnData(property.Name))[0]);//TODO improve:Take the first element, dodgy
                        break;
                    case "FirmwareRef":
                        property.SetValue(tempTestRun,
                            HelperStatic.Firmware_version(folderPathForTextFile));
                        break;
                    case "ParameterRef":
                        property.SetValue(tempTestRun,
                            HelperStatic.Parameter_version(folderPathForTextFile));
                        break;
                    case "TestName":
                        property.SetValue(tempTestRun,
                            HelperStatic.TestName(folderPathForTextFile));
                        break;
                    case "testrunID":
                        //Auto populated
                        break;
                    case "Timestamp":
                        //Autopopulated
                        break;
                    case "Testpoints":
                        //not a true field member
                        break;
                    default:
                        //WriteLine("NULL from testrun load: switch {0}", property.Name);
                        return null;
                }
            }
            //********Add tesrun to repo, not needed seems like....*********
            //because it is a foreign key when I add it to foreign key get added
            //when foreign key linked
            //Add the tempTestRun that was created previously to the repo
            //using (var repo_testrun = new TestrunRepo()){ repo_testrun.Add(tempTestRun); }
            return tempTestRun;
        }

        /// <summary>
        /// Load data from the realpowerdictionary into Testpointrepo
        /// </summary>
        private void Load_Testpoint_repo(Testrun testrun_instance)
        {
            //if Testrun is null the do not populate  datapoints
            if (testrun_instance == null) return;
            //Chart checking the data/csv quality vs repo
            using (var repo_tp = new TestpointRepo())
            {
                //WriteLine(repo_tp.ToString());
                //TODO: Get a better way to find the number of rows
                int numberOfRows = 0;
                //Get the properties for the EF.Model
                PropertyInfo[] testpoint_fields = AccuracyDAL.Helper.GetPropertlyArray(typeof(Testpoint));
                // PropertyInfo[] testpoint_fields = typeof(Testpoint).GetProperties(BindingFlags.Public | BindingFlags.Instance 
                // | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);
                //Check if all the point in the database table can be populated and corrolate
                //Look through the tespoint properties and see if they exist in datadictionary
                //WriteLine("Check data against model: ");
                for (int i = 0; i < testpoint_fields.Length; i++)
                {
                    //Check if the model property name is in the dictionary from CSV
                    if (csvinterface.ContainsColumn(testpoint_fields[i].Name) > -1)
                    {
                        //get the number of rows to be used later
                        numberOfRows = csvinterface.GetColumnData(testpoint_fields[i].Name).Count;
                    }
                    else
                    {
                        //If the name contains ID that is ok, do not return, excluded from this
                        if (!(testpoint_fields[i].Name.Contains("ID") || testpoint_fields[i].Name.Contains("Testrun")))
                        {
                            //Return without populating the Repo, columns does not match property
                            return;
                        }
                    }
                    //If all the properties of the model match the column names, all there
                    //then continue
                }
                //REPO: Populate here
                //This will give the lenth of the columns
                for (int k = 0; k < numberOfRows; k++)
                {
                    //New test point created for each row...logical
                    Testpoint tempTP = new Testpoint();
                    //This will iterate through every column
                    Dictionary<string, float> tempDict = new Dictionary<string, float>();

                    foreach (var property in testpoint_fields)
                    {
                        //TODO: This is a bit annoying. Do not want to enter a ID
                        if (!(property.Name.Contains("ID") || property.Name.Contains("Testrun")))
                        {
                            if ((csvinterface.GetColumnData(property.Name))[k] == "")
                                (csvinterface.GetColumnData(property.Name))[k] = "0";
                            //I like this: set the specific property of object tempTP,
                            //Setvalue takes the object reference and the value for the property
                            property.SetValue(tempTP, float.Parse((csvinterface.GetColumnData(property.Name))[k]));
                        }
                    }
                    //Foreign key operation here
                    //This is setting the testrunID foreign key in testpoint, Testpoint.cs model
                    tempTP.Testrun = testrun_instance;
                    //tempTP.Testrun.testrunID = testrun_instance.testrunID;
                    repo_tp.Add(tempTP);
                }
            }
        }
        /// <summary>
        /// Reomove a TestRun by number, the Testpoint data will also be removed
        /// </summary>
        /// <param name="v"></param>
        private void removeTestrunByID(int v)
        {
            using (var repo_testrun = new TestrunRepo())
            {
                Testrun findTR = repo_testrun.GetOne(v);
                if (findTR == null)
                    MessageBox.Show("The value to remove was NULL");
                else
                {
                    //Remove value form repo
                    repo_testrun.Context.Testruns.Remove(findTR);
                    repo_testrun.Context.SaveChanges();
                }
            }
        }
        ///Programming directly aganist the context here not using GetAll Call
        ///from the repo
        ///There is not much difference betwween doing this and coding directly
        ///against the Context, but Repo pattern provides a consistent way to access
        ///and operate on all data across all classes
        ///Populate listBox3 if you have connection to database
        private void populateListBox3()
        {
            try
            {
                var testrunRepo = new TestrunRepo();
                //programming angianst the context
                listBox3.ItemsSource = testrunRepo.Context.Testruns.ToList();
                
            }
            catch (Exception)
            {
                button3.IsEnabled = false;
                MessageBox.Show("No database, you can still look at csv data");
            }
        }
    }
}
