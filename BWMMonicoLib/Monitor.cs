using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.Diagnostics;

namespace BWMMonacoLib
{
    public class Monitor
    {
        #region Private Variables
        string _monitorStatusFileName;
        string _serverName;
        int _switchOverTimeout;
        int _monitorStatusUpdateInterval;
        DateTime _startTime;
        Timer _monitorTimer;
        #endregion

        #region Properties
        bool _processing = false;

        public bool Processing
        {
            get { return _processing; }
        }
        #endregion

        #region Constructors
        public Monitor(string monitorStatusFileName,
                            string serverName,
                            int switchOverTimeout,
                            int monitorStatusUpdateInterval)
        {
            _monitorStatusFileName = monitorStatusFileName;
            _serverName = serverName;
            _switchOverTimeout = switchOverTimeout;
            _monitorStatusUpdateInterval = monitorStatusUpdateInterval;
            _startTime = DateTime.MinValue;
            _monitorTimer = new Timer();
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _monitorTimer.Elapsed += new ElapsedEventHandler(MonitorWork);
            _monitorTimer.Interval = _monitorStatusUpdateInterval;
            _monitorTimer.Start();
        }

        private void MonitorWork(object source, ElapsedEventArgs e)
        {
            _monitorTimer.Stop();

            // Check to see if we are currently updating this file
            string server = "";
            string time = "";
            if (File.Exists(_monitorStatusFileName))
            {
                // file exists - so check to see if we are processing it?
                try
                {
                    TextReader tr = new StreamReader(_monitorStatusFileName);
                    server = tr.ReadLine();
                    time = tr.ReadLine();
                    tr.Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, "exceptions");
                }
                if (server == _serverName)
                {
                    try
                    {
                        // yes we are processing at the moment
                        // update the file with the current time stamp.
                        TextWriter tw = new StreamWriter(_monitorStatusFileName);
                        tw.WriteLine(_serverName);
                        tw.WriteLine(DateTime.Now.ToUniversalTime().ToString());
                        tw.Close();
                        _processing = true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex, "exceptions");
                    }
                }
                else
                {
                    // no we are not processing so check to see if 
                    // the other service has timed out
                    DateTime remoteServiceTimeStamp = Convert.ToDateTime(time);
                    _processing = false;
                    if (DateTime.Now.ToUniversalTime() > remoteServiceTimeStamp.AddSeconds(_switchOverTimeout))
                    {
                        Trace.WriteLine("Switched Over Timeout has occurred to: " + _serverName);
                        // also update monitorstatus file to show we are now starting to process
                        try
                        {
                            TextWriter tw = new StreamWriter(_monitorStatusFileName);
                            tw.WriteLine(_serverName);
                            tw.WriteLine(DateTime.Now.ToUniversalTime().ToString());
                            tw.Close();
                            _processing = false;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex, "exceptions");
                        }
                    }
                }
            }
            else
            {
                // _monitorStatusFileName does not exist, so create it.
                try
                {
                    TextWriter tw = new StreamWriter(_monitorStatusFileName);
                    tw.WriteLine(_serverName);
                    tw.WriteLine(DateTime.Now.ToUniversalTime().ToString());
                    tw.Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, "exceptions");
                }
            }

            _monitorTimer.Start();
        }

        public void Stop()
        {
            _monitorTimer.Stop();
        }
        #endregion
    }
}
