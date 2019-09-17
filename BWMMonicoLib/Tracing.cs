using System;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace Tracing
{
    /// <summary>
    /// Summary description for Tracer.
    /// </summary>
    public sealed class Tracer : TraceListener
    {
        static readonly Tracer instance = new Tracer();

        private string _LogDirectory = @"c:\";
        private string _LogFile = "BWMFax.txt";
        private bool _LogEnabled = false;

        public Tracer()
        {

            try
            {
                // Get app settings here
                AppSettingsReader reader = new AppSettingsReader();
                _LogDirectory = reader.GetValue("LogDirectory", typeof(string)).ToString();
                _LogFile = reader.GetValue("LogFile", typeof(string)).ToString();
                _LogEnabled = Convert.ToBoolean(reader.GetValue("LogEnabled", typeof(string)).ToString());
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
            }

            // Add trailing \ if needed
            if (!_LogDirectory.EndsWith(@"\"))
                _LogDirectory += @"\";

            try
            {
                // Create the dir
                if (!Directory.Exists(_LogDirectory))
                    Directory.CreateDirectory(_LogDirectory);
            }
            catch
            {
                // Cant create log directory so use one that will exist
                _LogDirectory = @"c:\";
            }
            try
            {
                // Try and create file
                using (StreamWriter streamWriter = File.AppendText(_LogDirectory + DateTime.Now.Date.ToString("yyyy-MM-dd") + "_" + _LogFile))
                {
                    //Do Nothing.
                }
            }
            catch
            {
                // Cant use specified logfile name try default
                _LogFile = "RFM3Log.txt";
                try
                {
                    // Try and create file
                    using (StreamWriter streamWriter = File.AppendText(_LogDirectory + DateTime.Now.Date.ToString("yyyy-MM-dd") + "_" +_LogFile))
                    {
                        //Do Nothing.
                    }
                }
                catch
                {
                    // Problem with writing to specified directory so use one that will work
                    _LogDirectory = @"c:\";
                }
            }
            // Register the listener
            Trace.Listeners.Add(this);
            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
            Trace.WriteLine("LogDirectory = " + _LogDirectory);
            Trace.WriteLine("LogFile = " + _LogFile);
        }

        public override void Write(string message)
        {
            if (_LogEnabled == false)
            {
                return;
            }

            try
            {
                using (StreamWriter streamWriter = File.AppendText(_LogDirectory + DateTime.Now.Date.ToString("yyyy-MM-dd")  + "_" + _LogFile))
                {
                    streamWriter.Write(DateTime.Now + " - " + message + Environment.NewLine);
                    //Console.Write(DateTime.Now+" - "+message + Environment.NewLine);
                }
            }
            catch
            {
                // Cant write to specified log file so write to default
                using (StreamWriter streamWriter = File.AppendText(@"c:\RFM3Log.txt"))
                {
                    streamWriter.Write(DateTime.Now + " - " + message + Environment.NewLine);
                }
            }
        }

        // 30 June 2010
        // Added this new override so we can use the category to 
        // separate out the exceptions from the normall logging
        public override void WriteLine(string message, string category)
        {
            if (_LogEnabled == false)
            {
                return;
            }

            try
            {
                using (StreamWriter streamWriter = File.AppendText(_LogDirectory + DateTime.Now.Date.ToString("yyyy-MM-dd") + "_" + category + "_" + _LogFile))
                {
                    streamWriter.Write(DateTime.Now + " - " + message + Environment.NewLine);
                }
            }
            catch
            {
                // Cant write to specified log file so write to default
                using (StreamWriter streamWriter = File.AppendText(@"c:\RFM3Log.txt"))
                {
                    streamWriter.WriteLine(DateTime.Now + " - " + message);
                }
            }
        }

        public override void WriteLine(string message)
        {
            if (_LogEnabled == false)
            {
                return;
            }

            try
            {
                using (StreamWriter streamWriter = File.AppendText(_LogDirectory + DateTime.Now.Date.ToString("yyyy-MM-dd") + "_" + _LogFile))
                {
                    streamWriter.Write(DateTime.Now + " - " + message + Environment.NewLine);
                    //Console.Write(DateTime.Now+" - "+message + Environment.NewLine);
                }
            }
            catch
            {
                // Cant write to specified log file so write to default
                using (StreamWriter streamWriter = File.AppendText(@"c:\RFM3Log.txt"))
                {
                    streamWriter.WriteLine(DateTime.Now + " - " + message);
                }
            }
        }


        public static Tracer Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
