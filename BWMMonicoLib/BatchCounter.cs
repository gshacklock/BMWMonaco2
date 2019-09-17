using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace BWMMonacoLib
{
    class BatchCounter
    {
        #region Constructors
        public BatchCounter()
        {

        }
        #endregion

        #region Public Methods
        public int GetCounter(string date, string folderName)
        {
            int batchCounter = 1;
            try
            {
                string key = folderName + "-" + date;
                // get the batchcounter filename
                string batchCounterFile = ConfigurationManager.AppSettings["BatchCounterFile"];
                string batchKey = "";

                if (File.Exists(batchCounterFile))
                {
                    // ok so it exists, so read the file.
                    TextReader tr = new StreamReader(batchCounterFile);
                    batchKey = tr.ReadLine();
                    if (batchKey.ToUpper().Trim() == key.ToUpper().Trim())
                    {
                        batchCounter = Convert.ToInt32(tr.ReadLine());
                        batchCounter++;
                    }
                    else
                    {
                        batchKey = key;
                        batchCounter = 1;
                    }
                    tr.Close();
                }
                else
                {
                    batchKey = key;
                    batchCounter = 1;
                }
                // write out the batch counter file for next time.
                TextWriter tw = new StreamWriter(batchCounterFile);
                tw.WriteLine(batchKey);
                tw.WriteLine(batchCounter);
                tw.Close();

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, "exceptions");
            }                

            return batchCounter;
        }
        #endregion
    }
}
