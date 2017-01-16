using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BattPlot
{
    /// <summary>
    /// Holds all the data that will go into the docx
    /// This dataa is populated from the mainwindow class 
    /// etc.
    /// </summary>
    public class DocTextHelper
    {
        //Constuctor here
        public DocTextHelper()
        {
            dicImageName_Descr = new Dictionary<string, string>();
            dirpath = "";
            serialnumber = "";
            descriptionLong = "";
            //No real data so done with this is true
            doneWithThisData = true;
        }
        //add to images and desriptions
        public void AddImageToDict(string fullfilepath, string description)
        {
            if (File.Exists(fullfilepath))
            {
                dicImageName_Descr.Add(fullfilepath, description);
                //Get the  serial number from path
                if(serialnumber == "")serialnumber = Regex.Match(fullfilepath, @"\d{12}").Value;
                //sets up the directory path once
                if (dirpath == "")dirpath = Path.GetDirectoryName(fullfilepath);
                //Added new data not done with this data
                doneWithThisData = false;
            }
        }
        //Get the dictionary with images and desscriptions
        public Dictionary<string, string> GetDictImageName_Descr
        {
            get { return dicImageName_Descr; }
        }
        //Checks if good enough data to create a doc
        public bool helperHealthCheck()
        {
            //if not directory path then false
            if (dirpath == "") return false;
            //if no image to put in report
            if (dicImageName_Descr.Count == 0) return false;
            //The basic are there to do a report, go ahead.
            //done with data there is nothing new to create in docx
            if (doneWithThisData == true) return false;
            return true;
        }        

        //returns the working directory
        public string GetDirPath(){ return dirpath; }
        public string filepath { get; set; }
        public string serialnumber { get; set; }
        public string descriptionLong { get; set; }
        public string temperature { get; set; }
        //stores filename as key text description as item
        private Dictionary<string, string> dicImageName_Descr;
        public bool doneWithThisData { get; set; }
        private string dirpath;
    }
}
