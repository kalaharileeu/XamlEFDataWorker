using CsvAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattPlot
{
    public class SkipModel
    {
        public SkipModel(){ }
        /// <summary>
        /// Modify the skips to only contain "1" or "0"
        /// </summary>
        /// <param name="skipList"></param>
        public void Setup(List<Column> skipList)
        {
            if (skipList == null) return;
            bool keeplist;

            for (int i = 0; i < skipList.Count; i++)
            {
                //if a list is all zero then do not keep it
                keeplist = false;
                //Starting j at 1 and not 0 as usual
                for (int j = skipList[i].Columnvalues.Count - 1; j > 0; j--)
                {
                    if (skipList[i].Columnvalues[j - 1] != skipList[i].Columnvalues[j])
                    {
                        skipList[i].Columnvalues[j] = "1.0";
                        keeplist = true;
                    }
                    else
                        skipList[i].Columnvalues[j] = "0.0";
                }
                if (skipList[i].Columnvalues[0] != "0.0")
                {
                    skipList[i].Columnvalues[0] = "1.0";
                    keeplist = true;
                }
                //if the list was all 0.0, then remove it
                //The list count now changes
                if (keeplist == false)
                {
                    skipList.RemoveAt(i);
                    i--;//Adjust i
                }
            }
        }
    }
}
