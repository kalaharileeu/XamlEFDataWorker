using System;
using System.Windows;
using Microsoft.Office.Interop.Word;
using System.Collections.Generic;
using System.IO;

namespace BattPlot
{
    public partial class MainWindow : System.Windows.Window
    {
        //Create document method
        private void createDocument()
        {
            try
            {
                //Create an instance for word app
                Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();
                //Set animation status for word application
                winword.ShowAnimation = false;
                //Set status for word application is to be visible or not.
                winword.Visible = false;
                //Create a missing variable for missing value
                object missing = System.Reflection.Missing.Value;

                //Create a new document
                Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                //Add header into the document
                foreach (Section section in document.Sections)
                {
                    //Get the header range and add the header details.
                    Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
                    headerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = WdColorIndex.wdBlue;
                    headerRange.Font.Size = 10;
                    headerRange.Text = "Accuracy debug plots, raw data: " + doctexthelper.filepath;
                }

                //Footers: Add the footers into the document
                foreach (Section wordSection in document.Sections)
                {
                    //Get the footer range and add the footer details.
                    Range footerRange = wordSection.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    footerRange.Font.ColorIndex = WdColorIndex.wdBlue;
                    footerRange.Font.Size = 10;
                    footerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    footerRange.Text = doctexthelper.serialnumber;
                }

                //adding text to document
                //document.Content.SetRange(0, 0);
                //document.Content.Text = "This is test document " + Environment.NewLine;

                //Add paragraph with Heading 1 style
                Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
                object styleHeading1 = "No Spacing";
                para1.Range.set_Style(ref styleHeading1);
                para1.Range.Text = "Infomation from test data:";
                para1.Range.InsertParagraphAfter();

                //adding text to document
                //document.Content.SetRange(0, 0);
                document.Content.Text = doctexthelper.descriptionLong;

                //Add paragraph with Heading 2 style
                Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
                object styleHeading2 = "No Spacing";
                para2.Range.set_Style(ref styleHeading2);
                ////doctexthelper into action here
                //para2.Range.Text = doctexthelper.descriptionLong;
                //para2.Range.InsertParagraphAfter();

                int count = 0;
                //iterate through doctexthelper to create images and commnets
                foreach(KeyValuePair<string, string> entry in doctexthelper.GetDictImageName_Descr )
                {
                    count += 1;
                    //document.Content.Paragraphs.Add(ref missing);
                    object styleheading2 = "Accuracy plot image: " + count;
                    document.Content.Paragraphs.Add(ref missing).Range.set_Style(ref styleHeading2);
                    document.Content.Paragraphs.Add(ref missing).Range.InlineShapes.AddPicture(entry.Key);
                    document.Content.Paragraphs.Add(ref missing).Range.Text = entry.Value;
                    document.Content.Paragraphs.Add(ref missing).Range.InsertParagraphAfter();
                }

                //Save the document
                string dirstring = doctexthelper.GetDirPath();
                object filename = null;
                //Do some checks on the directory
                if (HelperStatic.DirChecks(dirstring))
                {
                    //create path for the work document to be created
                    filename = Path.Combine(dirstring, "AccuracyPlots" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".docx");
                    //object filename = @"c:\temp\temp1.docx";
                    document.SaveAs2(ref filename);
                    MessageBox.Show("Document created successfully !");
                    //tell the doctexthelper that we are do with what he current has
                    //it does not have to auto saved later!!!!!!!!!!!!!!!!!
                    doctexthelper.doneWithThisData = true;
                }
                else
                {
                    MessageBox.Show("Document NOT created!");
                }
                //object filename = @"c:\temp\temp1.docx";
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

////Create a 5X5 table and insert some dummy record
//Table firstTable = document.Tables.Add(para1.Range, 5, 5, ref missing, ref missing);

//firstTable.Borders.Enable = 1;
//foreach (Row row in firstTable.Rows)
//{
//    foreach (Cell cell in row.Cells)
//    {
//        //Header row
//        if (cell.RowIndex == 1)
//        {
//            cell.Range.Text = "Column " + cell.ColumnIndex.ToString();
//            cell.Range.Font.Bold = 1;
//            //other format properties goes here
//            cell.Range.Font.Name = "verdana";
//            cell.Range.Font.Size = 10;
//            //cell.Range.Font.ColorIndex = WdColorIndex.wdGray25;                            
//            cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
//            //Center alignment for the Header cells
//            cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
//            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
//        }
//        //Data row
//        else
//        {
//            cell.Range.Text = (cell.RowIndex - 2 + cell.ColumnIndex).ToString();
//        }
//    }
//}
