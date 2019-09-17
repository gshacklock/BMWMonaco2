using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using System.Text;
using System.Configuration;
using System.IO;
using BWMMonacoLib;

namespace BWMMonacoService
{
    public partial class BWMMonacoService : ServiceBase
    {
        Timer timer = new Timer();
        Monitor _monitor;
        Tracing.Tracer t = Tracing.Tracer.Instance;
        bool _startUpSleepDone = false;

        public BWMMonacoService()
        {
            InitializeComponent();
        }

        public BWMMonacoService(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Trace.WriteLine("Service Starting");

            try
            {
                AppSettingsReader reader = null;
                int sleepPeriod = 30000;
                try
                {
                    reader = new AppSettingsReader();
                    sleepPeriod = 1000 * Convert.ToInt32(reader.GetValue("Sleep", typeof(string)));
                }
                catch
                {
                    Trace.Write("Couldn't load service Sleep value from application configuration file using 30 seconds");
                }

                Trace.WriteLine("Starting Processing...");
                timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                timer.Interval = sleepPeriod;
                timer.Start();
            }
            catch (Exception ex)
            {
                // something bad happened so log it.
                Trace.WriteLine("Exception occurred in OnStart: " + ex, "exceptions");
            }
        }

        private void StartupSleep()
        {
                // before we start, wait for a period of time as 
                // specified by the startupsleep.
                int startUpSleep = Convert.ToInt32(ConfigurationManager.AppSettings["StartUpSleep"]);
                if (startUpSleep == 0)
                {
                    startUpSleep = 1;
                }
                Trace.WriteLine("Entering Startup Sleep Period for " + startUpSleep + " seconds");
                for (int sleep = 1; sleep < startUpSleep; sleep++)
                {
                    // loop for 1 second until the startupSleep is complete.
                    System.Threading.Thread.Sleep(1000);
                }
                Trace.WriteLine("Completed Startup Sleep Period");
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            Trace.WriteLine("Service Stopping");
        }

        protected void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            timer.Stop();
            try
            {
                // perform the startup sleep if it has not already been done.
                if (_startUpSleepDone == false)
                {
                    // ok, not done so sleep.
                    StartupSleep();
                    _startUpSleepDone = true;
                    // Start the monitoring Timer
                    Trace.WriteLine("Starting Service Monitor");
                    StartMonitor();
                }
                Process();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, "exceptions");
                System.Threading.Thread.Sleep(30000);
            }
            finally
            {
                timer.Start();
            }
        }

        private void Process()
        {
            if (_monitor.Processing == false)
            {
                // we are not currently processing.
                return;
            }
            RightFAXServersSection rightFAXServersSection = ConfigurationManager.GetSection("RightFAXServers") as RightFAXServersSection;
            RightFAXServersCollection rightFAXServersCollection = rightFAXServersSection.RightFAXServers;
            MailBoxSection mailBoxSection = ConfigurationManager.GetSection("MailBoxes") as MailBoxSection;
            MailBoxCollection mailBoxCollection = mailBoxSection.MailBoxes;

            RightFAX rightFAX = new RightFAX();

            // iterate through the servers
            // and then inside, iterate through the mailboxes.

            foreach (RightFAXServersElement rfse in rightFAXServersCollection)
            {
                Trace.WriteLine("RightFAXServersElement = " + rfse.ToString());
                rightFAX.Open(rfse.ServerName, rfse.UserName, rfse.Password);
                foreach (MailBoxElement mbe in mailBoxCollection)
                {
                    if (_monitor.Processing == false)
                    {
                        // we are not currently processing.
                        rightFAX.Close();
                        return;
                    }
                    if (rfse.ServerName == mbe.ServerName)
                    {
                        rightFAX.ProcessMailBox(mbe);
                    }
                }
                rightFAX.Close();
            }
        }

        private void StartMonitor()
        {
            // start the monitor timer.
            // this is used to determine if we should be processing or not
            string _monitorStatusFileName = ConfigurationManager.AppSettings["MonitorStatusFileName"];
            string _serverName = ConfigurationManager.AppSettings["ServiceName"];
            int _serviceStartUpDelay = Convert.ToInt32(ConfigurationManager.AppSettings["ServiceStartupDelay"]);
            int _switchOverTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["SwitchOverTimeout"]);
            int _monitorStatusUpdateInterval = Convert.ToInt32(ConfigurationManager.AppSettings["MonitorStatusUpdateInterval"]);

            // kick off the monitor timer.
            Trace.WriteLine("Starting Monitor");
            _monitor = new Monitor(_monitorStatusFileName, _serverName, _switchOverTimeout, _monitorStatusUpdateInterval);
            _monitor.Start();
        }
    }
}
