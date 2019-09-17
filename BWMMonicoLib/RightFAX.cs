using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;
using System.Xml.Serialization;
using System.Diagnostics;
using RFCOMAPILib;
using C1.C1Pdf;

namespace BWMMonacoLib
{
    public class RightFAX
    {
        FaxServer _fs;
        string _workingFolder;
        string _successCodes;
        string _username;

        public RightFAX()
        {
            _workingFolder = ConfigurationManager.AppSettings["WorkingFolder"];
            if (_workingFolder == String.Empty)
            {
                _workingFolder = @"c:\";
            }
            if (_workingFolder.Substring(_workingFolder.Length - 1, 1) != @"\")
            {
                _workingFolder += @"\";
            }

            _successCodes = ConfigurationManager.AppSettings["SuccessCodes"].ToUpper();

        }

        public void Open(string serverName, string userName, string password)
        {
            // just incase it is already open, close it first and
            // destroy the _fs object.
            Close();

            _fs = new FaxServer();
            _fs.ServerName = serverName;
            _fs.AuthorizationUserID = userName;
            _fs.AuthorizationUserPassword = password;
            _fs.UseNTAuthentication = BoolType.False;
            _fs.Protocol = CommunicationProtocolType.cpTCPIP;
            _fs.OpenServer();
        }

        public void Close()
        {
            if (_fs != null)
            {
                _fs.CloseServer();
                _fs = null;
            }
        }

        public void ProcessMailBox(MailBoxElement mbe)
        {
            try
            {
                // Iterate through the faxes for this user.
                User user = _fs.Users[mbe.UserName.ToUpper()];
                Trace.WriteLine("Processing User: " + mbe.UserName);

                string processedTag = ConfigurationManager.AppSettings["ProcessedTag"].ToUpper().Trim();
                if (processedTag == null)
                {
                    processedTag = "PROCESSED";
                }

                string validProcessedTags = ConfigurationManager.AppSettings["ValidProcessedTags"].ToUpper().Trim();

                if (validProcessedTags == "")
                {
                    // for some reason the ValidProcessedTags field has not been set,
                    // so set it to the same as the processedTag, so we can still mark them as processed.
                    validProcessedTags = processedTag;
                }
                // make sure we include "PROCESSED" in the validProcessedTags.
                if (!validProcessedTags.Contains("PROCESSED"))
                {
                    validProcessedTags += ",PROCESSED";
                }

                foreach (Fax fax in user.Faxes)
                {

                    
                    try
                    {
                        if (true == _successCodes.Contains(Convert.ToInt32(fax.FaxStatus).ToString() + @","))
                        {
                            // we are only interested if it has not already been processed.
                            // We must make sure it has not been processed by either
                            // of the services - so we use the validProcessedTags.
                            if ((validProcessedTags.Contains(fax.BillingCode2.ToUpper()) == false) || (fax.BillingCode2.Trim() == ""))
                            {
                                string batchId = String.Empty;
                                string fileName = GetFileName(mbe.UNCShare, out batchId);
                                Trace.WriteLine("PDF File = " + fileName);
                                // 18-10-2010 - changed username to ID from the User Object.
                                _username = user.ID.ToString();
                                bool result = ExportData(fileName, fax);
                                if (result == true)
                                {
                                    // only set the billing code to the processed tag
                                    // if the data has been exported correctly
                                    fax.BillingCode2 = processedTag;
                                    fax.Save(BoolType.False);

                                    try
                                    {
                                        // tidy up temp tiff files.
                                        string[] files = System.IO.Directory.GetFiles(_workingFolder, "~*.tif");
                                        foreach (string file in files)
                                        {
                                            System.IO.File.Delete(file);
                                        }
                                    }
                                    catch { } // file in use, so ignore and delete it later.
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(@"An unhandled exception occurred for Fax Unique ID: " + fax.UniqueID.ToString() + Environment.NewLine +
                                        "with exception message: " + ex.Message + Environment.NewLine + @"Continuing with the next fax for this user: " + user.UserName +
                                        Environment.NewLine + Environment.NewLine, "exceptions");
                    }
                }
            }
            catch (ArgumentException argEx)
            {
                Trace.WriteLine(@"An ArgumentException occured:  " + argEx.Message + Environment.NewLine + @"Parameter Name: " + argEx.ParamName + " with value " + argEx.Data, "exceptions");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(@"An unhandled exception occurred " + Environment.NewLine +
                                "with exception message: " + ex.Message, "exceptions");
            }
        }

        private string GetFileName(string UNCShare, out string batchId)
        {
            string date = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string fileName = String.Empty;
            do
            {
                batchId = GetBatchId(date, UNCShare);
                fileName = UNCShare + @"\Fax" + batchId + date + @".pdf";
            } while (File.Exists(fileName) == true);
            //Directory.CreateDirectory(folderName);
            return fileName;
        }

        private string GetBatchId(string date, string folderName)
        {
            BatchCounter batchCounter = new BatchCounter();
            int counter = batchCounter.GetCounter(date, folderName);
            string batchId = counter.ToString().PadLeft(5, '0');

            return batchId;
        }

        private bool ExportData(string fileName, Fax fax)
        {
            bool result = CreatePDF(fax, fileName);

            // 18/10/2010 - requested that the XML File is no longer required.
            //if (true == result)
            //{
            //    result = CreateXMLFile(fax, folderName, "BatchData.xml", batchId);
            //}
            return result;
        }

        private void AddMetaText(C1PdfDocument doc, Fax fax)
        {
            string datetimecreated = "";
            string faxaninumber = "";
            string FaxDIDNum = "";
            string ownerid = _username;
            string pagecount = "";
            string remoteid = "";
            string status = "";
            string uniqueidentifier = "";
            string Team_id = _username;

            //datetimecreated = fax.FaxRecordDateTime.ToUniversalTime().ToString();
            // changed to local time as requested on 13/1/2011.
            // ToLocalTime not working properly when IsDayLight = True, it's adding 2 hours !!!
            //Console.WriteLine("ToLocalTime: " + datetimecreated.ToString());
            //Console.WriteLine("ToUniversalTime: " +  fax.FaxRecordDateTime.ToUniversalTime().ToString());
            //Console.WriteLine("Composite DateTime: " + faxDateTime);
            //Console.WriteLine("Is Daylight: " + isDaylight.ToString());

            //datetimecreated = fax.FaxRecordDateTime.ToLocalTime().ToString();

            string day = fax.FaxRecordDateTime.Day.ToString().PadLeft(2, '0');
            string month = fax.FaxRecordDateTime.Month.ToString().PadLeft(2, '0');
            string year = fax.FaxRecordDateTime.Year.ToString().PadLeft(4, '0');
            string hour = fax.FaxRecordDateTime.Hour.ToString().PadLeft(2, '0');
            string minute = fax.FaxRecordDateTime.Minute.ToString().PadLeft(2, '0');
            string second = fax.FaxRecordDateTime.Second.ToString().PadLeft(2, '0');
            datetimecreated = day + '/' + month + '/' + year + " " + hour + ":" + minute + ":" + second;
            //bool isDaylight = fax.FaxRecordDateTime.IsDaylightSavingTime();

            faxaninumber = ANI(fax);
            FaxDIDNum = fax.FromFaxNumber.ToString();
            ownerid = _username;
            pagecount = fax.TotalPages.ToString();
            remoteid = fax.RemoteID.ToString();
            if (remoteid.Trim() == "")
            {
                try
                {
                    remoteid = ConfigurationManager.AppSettings["emptyRemoteId"];
                }
                catch
                {
                    // probably not in the config file so default to none.
                    Trace.WriteLine("Could not read config key 'emptyRemotId', defalting to 'none'", "exceptions");
                    remoteid = "none";
                }
            }
            status = Convert.ToInt32(fax.FaxStatus).ToString();
            uniqueidentifier = fax.UniqueID.ToString();
            Team_id = _username;

            // get the coordinates and fontsize from the config file.
            AddTextToPage(datetimecreated, "datetimecreated", doc);
            AddTextToPage(faxaninumber, "faxaninumber", doc);
            AddTextToPage(FaxDIDNum, "FaxDIDNum", doc);
            AddTextToPage(ownerid, "ownerid", doc);
            AddTextToPage(pagecount, "pagecount", doc);
            AddTextToPage(remoteid, "remoteid", doc);
            AddTextToPage(status, "status", doc);
            AddTextToPage(uniqueidentifier, "uniqueidentifier", doc);
            AddTextToPage(Team_id, "Team_id", doc);

        }

        private void AddTextToPage(string text, string key, C1PdfDocument doc)
        {
            int x = 0;
            int y = 0;
            int fontsize = 0;
            string description = "";

            GetTextAttributes(key, out x, out y, out fontsize, out description);

            text = description + text;
            // Add the text.
            PointF startPoint = new PointF(x, y);
            Font fnt;
            string fontname = "Ariel";
            fontname = ConfigurationManager.AppSettings["fontname"];
            if (fontname == null)
            {
                fontname = "Ariel";
            }
            fnt = new Font(fontname, fontsize);
            using (fnt)
            {
                doc.DrawString(text, fnt, Brushes.Blue, startPoint);
            }
        }

        private void GetTextAttributes(string key, out int x, out int y, out int fontsize, out string description)
        {
            string value = ConfigurationManager.AppSettings[key];
            string[] values = value.Split('|');
            if (values.Length != 4)
            {
                // if there are not 3 values then return zeros.
                x = 0;
                y = 0;
                fontsize = 0;
                description = "";
            }
            x = Convert.ToInt32(values[0]);
            y = Convert.ToInt32(values[1]);
            fontsize = Convert.ToInt32(values[2]);
            description = values[3];
        }

        private bool CreatePDF(Fax fax, string fileName)
        {
            bool result = false;
            try
            {
                C1PdfDocument c1pdf = new C1PdfDocument();
                // create pdf document
                c1pdf.Clear();
                c1pdf.DocumentInfo.Title = "Fax: " + fax.FaxFilename;

                // calculate document name
                if (true == File.Exists(fileName))
                {
                    // pdf file already exists - so don't recreate it.
                    Trace.WriteLine("PDF File already exists: " + fileName);
                    result = true;
                }
                else
                {
                    bool newPage = false;
                    RectangleF rcPage = RectangleF.Empty;
                    RectangleF rc1 = RectangleF.Empty;
                    bool addMetaText = true;
                    foreach (Attachment attachment in fax.Attachments)
                    {
                        if (newPage)
                        {
                            c1pdf.NewPage();
                            rc1.Y = rcPage.Y;
                        }

                        rcPage = c1pdf.PageRectangle;
                        //rcPage.Inflate(-72, -72);

                        rc1 = rcPage;
                        rc1.Inflate(-10, 0);
                        rc1 = RenderMultiPageImage(ref c1pdf, rcPage, rc1, attachment.FileName, fax, addMetaText);
                        addMetaText = false;
                        Trace.WriteLine("Rendered page: " + attachment.FileName + " to pdf: " + fileName);
                        newPage = true;
                    }

                    
                    c1pdf.Save(fileName);
                    Trace.WriteLine("Saved (" + fileName + ") successfully.");

                    result = true;
                }
            }
            catch (IOException fileEx)
            {
                Trace.WriteLine("IOException occurred in method CreatePDF with: " + fileEx.Message, "exceptions");
                result = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("General Exception occurred in method CreatePDF with: " + ex.Message, "exceptions");
                result = false;
            }
            return result;
        }

        private RectangleF RenderMultiPageImage(ref C1PdfDocument c1pdf, RectangleF rcPage, RectangleF rc, string fileName, Fax fax, bool addMetaText)
        {
            //Image img = Image.FromFile(fileName);
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName));
            Image img = Image.FromStream(ms);

            FrameDimension oDimension = new FrameDimension(img.FrameDimensionsList[0]);
            int FrameCount = img.GetFrameCount(oDimension);

            for (int i = 0; i < FrameCount; i++)
            {
                // calculate image height
                // based on image size and page size
                rc.Height = Math.Min(img.Height / 96f * 72, rcPage.Height);

                // skip page if necessary
                if (rc.Bottom > rcPage.Bottom)
                {
                    c1pdf.NewPage();
                    rc.Y = rcPage.Y;
                }

                // draw solid background (mainly to see transparency)
                rc.Inflate(+2, +2);
                c1pdf.FillRectangle(Brushes.White, rc);
                rc.Inflate(-2, -2);

                // draw image (keep aspect ratio)
                img.SelectActiveFrame(oDimension, i);
                string fn = System.IO.Path.GetFileName(fileName);
                fn = _workingFolder + fn.Substring(0, fn.Length - 4) + "-" + i + ".tif";
                img.Save(fn);
                //Image img1 = Image.FromFile(fn);
                MemoryStream ms1 = new MemoryStream(File.ReadAllBytes(fn));
                Image img1 = Image.FromStream(ms1);

                c1pdf.DrawImage(img1, rc, ContentAlignment.MiddleCenter, ImageSizeModeEnum.Scale);
                // update rectangle
                rc.Y = rc.Bottom + 20;

                if ((i == 0) && (addMetaText == true))
                {
                    AddMetaText(c1pdf, fax);
                }
            }
            return rc;
        }

        private bool CreateXMLFile(Fax fax, string folderName, string fileName, string batchId)
        {
            bool result = false;
            try
            {
                BatchContainer myFax = new BatchContainer();
                FaxBatch myBatch = new FaxBatch();
                Document[] myDocuments;
                myDocuments = new Document[1];
                myDocuments[0] = new Document();
                myDocuments[0].id = 1;
                myDocuments[0].pageCount = fax.TotalPages;

                myBatch.id = Convert.ToInt32(batchId);
                myBatch.timezone = "GMT";
                string fromFaxNumber = ANI(fax);
                if (String.Empty == fromFaxNumber.Trim())
                {
                    fromFaxNumber = "Not Available";
                }
                myBatch.sourceCli = fromFaxNumber;
                string toFaxNumber = fax.OwnerID;
                if (String.Empty == toFaxNumber.Trim())
                {
                    toFaxNumber = "Not Available";
                }
                myBatch.dateTime = fax.FaxRecordDateTime;
                myBatch.destinationCli = toFaxNumber;
                myBatch.documents = myDocuments;
                myFax.batch = myBatch;

                // Serialization 
                XmlSerializer sFax = new XmlSerializer(typeof(BatchContainer));
                TextWriter wFax = new StreamWriter(folderName + @"\" + fileName);
                sFax.Serialize(wFax, myFax);
                wFax.Close();
                result = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("General Exception occurred in method CreateXMLFile with: " + ex.Message, "exceptions");
                result = false;
            }

            return result;
        }

        string ANI(Fax fax)
        {
            // Find the ANI Number in the Fax History Sudo XML
            // Cannot use XML Classes as the content can containg & < > etc
            // code from Gary to extract ANI Number.
            string result = "Withheld";
            foreach (FaxHistory fh in fax.Histories)
            {
                string xmlhist = fh.XML;
                string cmlhis2 = fh.ToString();

                // Find <ANI> in String
                int ANIPos = xmlhist.IndexOf("<ANI>");
                if (ANIPos > -1)
                {
                    // ANI Exists, so extract
                    int ANIEnd = xmlhist.IndexOf(@"</ANI>");
                    if (ANIEnd > -1)
                    {
                        string ANINum = xmlhist.Substring(ANIPos + 5, (ANIEnd - ANIPos - 5));
                        result = ANINum;
                    }
                }
            }
            return result;
        }
    }
}
