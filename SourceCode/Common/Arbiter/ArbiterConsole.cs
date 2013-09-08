using System;
using System.Collections.Generic;
using System.Text;
using CommonSupport;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Arbiter
{
    static class ArbiterConsole
    {
        public static void Main()
        {
            #region Speed Test
            
            Arbiter a = new Arbiter("Arbiter");

            ManualTransportClient d1 = new ManualTransportClient("d1");
            a.AddClient(d1);

            ManualTransportClient d2 = new ManualTransportClient("d1");
            a.AddClient(d2);

            DateTime start = DateTime.Now;
            
            // The current speed is around 3.200 messages / sec; the workload is on the serialization.
            for (int i = 0; i < 100; i++)
            {
                OperationMessage om = new OperationMessage();
                d1.SendAddressed(d2.SubscriptionClientID, om);
            }

            Console.WriteLine(DateTime.Now - start);

            #endregion

            Console.ReadKey();
        }
    }
}
