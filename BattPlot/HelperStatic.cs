using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace BattPlot
{
    public class HelperStatic
    {
        static public void SkipModifier()
        {

        }
        /// <summary>
        /// Creates a directory for plots and the reporting
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="newdirname"></param>
        /// <returns></returns>
        static public string CreatePlotSandbox(string filename, string newdirname)
        {
            //if file name does not exist retunr null
            if (!FileChecks(filename))return null;
            string newdirectory = "";
            //Get the path out of the complete file name
            string thepath = Path.GetDirectoryName(filename);
            //Add a directory to the parth
            newdirectory = thepath + "\\" + newdirname;
            //find the serial number in the path
            string resultString = Regex.Match(thepath, @"\d{12}").Value;
            //no need to check it exists
            if (!Directory.Exists(newdirectory))
            {
                //Creates a new directory Accuracy
                Directory.CreateDirectory(newdirectory);
            }
            return newdirectory;
        }
        /// <summary>
        /// Combines to two string to form a path
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        static public string CombineStringtoPaths(string p1, string p2)
        {
            try
            {
                string combination = Path.Combine(p1, p2);
                return combination;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        /// <summary>
        /// Calculation. works out the apparant ac current the the pcu read and feed is ba
        /// </summary>
        /// <param name="pcuimag"></param>
        /// <param name="pcureal"></param>
        /// <returns>list of pcu apparant current</returns>
        public static List<float> Get_pcu_apparantcurrent(List<float> pcuimag, List<float> pcureal)
        {
            List<float> pcu_apparant_i = new List<float>();
            if (pcuimag.Count == pcureal.Count)
            {
                for (int i = 0; i < pcuimag.Count; i++)
                {
                    pcu_apparant_i.Add((float)(Math.Sqrt(
                        Convert.ToDouble(pcuimag[i] * pcuimag[i] + pcureal[i] * pcureal[i]))));
                }
                return pcu_apparant_i;
            }
            //return null; the two list values not equal
            return null;
        }

        /// <summary>
        ///Read the aditional information from the test folder text file
        ///Read inforamation from a file to get inverter information
        /// </summary>
        /// <param name="dirWithInfo">The directory where the info text is stored</param>
        /// <param name="newDirForExtractedInfo"></param>
        /// <returns></returns>
        public static StringBuilder readAdditionalInfo(string fullDataFilePath)
        {
            StringBuilder uutdetail = new StringBuilder("");
            //checks if the fiel is open or in use
            if (!FileChecks(fullDataFilePath))
                return uutdetail.Append("File for additional info does not exist");

            FileInfo FI = new FileInfo(fullDataFilePath);
            if (IsFileLocked(FI))
            {
                uutdetail.Append("The file with general test info is open or ddoes not exist!");
                return uutdetail;
            }

            //Get the serial number to identify the text file
            string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
            //Check if path sting contains valid root
            if (!Path.IsPathRooted(fullDataFilePath))
                return uutdetail.Append("Path string does not contain valid root");
            //Create dir name from full filename
            string theDirPath = Path.GetDirectoryName(fullDataFilePath);
            //if the directory not there return a message
            if (!Directory.Exists(theDirPath))
                return uutdetail.Append("The Directory does not exits");

            //convert to dir info to use getfiles
            DirectoryInfo dirWithTextFile = new DirectoryInfo(theDirPath);
            //Get a file with specific text in name
            FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + serialnumber + "*" + ".txt");
            //if the file is not the then go to end
            if (filesindir.Length != 0)
            {
               // Directory.CreateDirectory(difftooldir);//no need to check it exists
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Serial ")).TakeWhile(line => !line.Contains("v")))
                {
                    uutdetail.Append(lin + " ");
                }
                uutdetail.Append("\r");
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Debugger")).TakeWhile(line => !line.Contains("d>")))
                {
                    uutdetail.Append(lin + " ");
                }
                uutdetail.Append("\r");
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Power Limit")).TakeWhile(line => !line.Contains("Assumed")))
                {
                    uutdetail.Append(lin + " ");
                }
            }
            else
            {
                uutdetail.Append("The .txt info file created by sequencer is not in the results folder. No info to display here" +
                    " or the the folder serial number does not match the internal files serial numbers");
            }
            return uutdetail;
        }

        /// <summary>
        /// Do some directory checks
        /// </summary>
        /// <param name="theFileName">Full directory name</param>
        /// <returns>bool</returns>
        public static bool DirChecks(string theDirPath)
        {
            //Check if path sting contains valid root
            if (!Path.IsPathRooted(theDirPath))
                return false;
            //if the directory not there return a message
            if (!Directory.Exists(theDirPath))
                return false;

            return true;
        }
        /// <summary>
        /// Checks if a file name exists
        /// </summary>
        /// <param name="theFileName">Full file name</param>
        /// <returns>bool</returns>
        public static bool FileChecks(string theFileName)
        {
            if (File.Exists(theFileName))
                return true;
            return false;
        }

        ///check is a file is open by another program
        public static bool IsFileLocked(FileInfo file)
        {
            //The file does not exist
            if (!file.Exists)
                return true;
        
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }
        /// <summary>
        /// Read the firmware version from file
        /// Need a full pathname of a file in the .csv file folder
        /// </summary>
        /// <returns></returns>
        public static string Firmware_version(string fullDataFilePath)
        {
            string firmwareversion = "00.00.00";
            if (!Path.IsPathRooted(fullDataFilePath))
            {
                return firmwareversion;
            }
            //Get the serial number in the file path to identify the text file
            string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
            //Create dir name from full filename
            string theDirPath = Path.GetDirectoryName(fullDataFilePath);
            //if the directory not there return a message
            if (!Directory.Exists(theDirPath))
            {
                return firmwareversion;
            }
            //convert to dir info to use getfiles
            DirectoryInfo dirWithTextFile = new DirectoryInfo(theDirPath);
            //Get a file with specific text in name
            //FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + serialnumber + "*" + ".txt");
            FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + ".txt");
            if (filesindir.Length != 0)
            {
                //Find/read the line with the firmware version
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Debugger")).TakeWhile(line => line.Contains("Debugger")))
                {
                    //Get the serial number in the file path to identify the text file
                    //string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
                    firmwareversion = Regex.Match(lin, @"\d{2}\.\d{2}\.\d{2}").Value;
                }
            }
            return firmwareversion;
        }
        /// <summary>
        /// Need a full pathname of a file in the .csv file folder
        /// Read the Parameter version from file
        /// </summary>
        /// <returns></returns>
        public static string Parameter_version(string fullDataFilePath)
        {
            string parameterversion = "00.00.00";
            if (!Path.IsPathRooted(fullDataFilePath))
            {
                return parameterversion;
            }
            //Get the serial number in the file path to identify the text file
            string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
            //Create dir name from full filename
            string theDirPath = Path.GetDirectoryName(fullDataFilePath);
            //if the directory not there return a message
            if (!Directory.Exists(theDirPath))
            {
                return parameterversion;
            }
            //convert to dir info to use getfiles
            DirectoryInfo dirWithTextFile = new DirectoryInfo(theDirPath);
            //Get a file with specific text in name
            //FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + serialnumber + "*" + ".txt");
            FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + ".txt");
            if (filesindir.Length != 0)
            {
                //Find/read the line with the firmware version
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Parameter")).TakeWhile(line => line.Contains("Parameter")))
                {
                    //Get the parameter number in the file path to identify the text file
                    parameterversion = Regex.Match(lin, @"\d{2}\.\d{2}\.\d{2}").Value;
                    //parameterversion = lin;
                }
            }
            return parameterversion;
        }
        /// <summary>
        /// Read the TestName from file
        /// Need a full pathname of a file in the .csv file folder
        /// </summary>
        /// <returns></returns>
        public static string TestName(string fullDataFilePath)
        {
            string testname = "no name";
            if (!Path.IsPathRooted(fullDataFilePath))
            {
                return testname;
            }
            //Get the serial number in the file path to identify the text file
            string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
            //Create dir name from full filename
            string theDirPath = Path.GetDirectoryName(fullDataFilePath);
            //if the directory not there return a message
            if (!Directory.Exists(theDirPath))
            {
                return testname;
            }
            //convert to dir info to use getfiles
            DirectoryInfo dirWithTextFile = new DirectoryInfo(theDirPath);
            //Get a file with specific text in name
            //FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + serialnumber + "*" + ".txt");
            FileInfo[] filesindir = dirWithTextFile.GetFiles("*" + ".txt");
            if (filesindir.Length != 0)
            {
                //Find/read the line with the firmware version
                foreach (var lin in File.ReadLines(filesindir[0].FullName)
                    .SkipWhile(line => !line.Contains("Test Name:")).TakeWhile(line => line.Contains("Test Name:")))
                {
                    //Get the serial number in the file path to identify the text file
                    //string serialnumber = Regex.Match(fullDataFilePath, @"\d{12}").Value;
                    if (lin.Contains("Test Name"))
                        testname = lin.Remove(0, 11);
                }
            }
            return testname;
        }
    }
}
