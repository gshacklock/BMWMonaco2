using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using BWMLib;

namespace BWMConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Tracing.Tracer t = Tracing.Tracer.Instance;
            RightFAXServersSection rightFAXServersSection = ConfigurationManager.GetSection("RightFAXServers") as RightFAXServersSection;
            RightFAXServersCollection rightFAXServersCollection = rightFAXServersSection.RightFAXServers;
            MailBoxSection mailBoxSection = ConfigurationManager.GetSection("MailBoxes") as MailBoxSection;
            MailBoxCollection mailBoxCollection = mailBoxSection.MailBoxes;


            RightFAX rightFAX = new RightFAX();

            // iterate through the servers
            // and then inside, iterate through the mailboxes.

            foreach (RightFAXServersElement rfse in rightFAXServersCollection)
            {
                Console.WriteLine("RightFAXServersElement = " + rfse.ToString());
                rightFAX.Open(rfse.ServerName, rfse.UserName, rfse.Password);
                foreach (MailBoxElement mbe in mailBoxCollection)
                {
                    if (rfse.ServerName == mbe.ServerName)
                    {
                        rightFAX.ProcessMailBox(mbe);
                    }
                }
                rightFAX.Close();
            }
        }
    }
}
