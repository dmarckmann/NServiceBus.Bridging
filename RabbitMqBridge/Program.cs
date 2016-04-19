using Newtonsoft.Json;
using NServiceBus;
using Sared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RabbitMqBridge
{

    public class Program
    {
        public static string EndPointName { get; set; }
        public static void Main(string[] args)
        {
            EndPointName = "AzureStorageQueueSender";
            new Program().Run();
        }

        
        
        public void Run()
        {
            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName(EndPointName);
            busConfiguration.Conventions().DefiningCommandsAs(t => t.Name == "Command");
            busConfiguration.Conventions().DefiningEventsAs(t => t.Name == "Event");
            busConfiguration.UseTransport<RabbitMQTransport>();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.UseSerialization<XmlSerializer>();

            busConfiguration.EnableInstallers();

            using (Bus.Create(busConfiguration).Start())
            {
                //ProcessMessagesFromBridge();
                //_timer.Interval = 5000.0;
                //_timer.Start();

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }



        class Handler : IHandleMessages<Event>, IHandleMessages<Command>
        {
            public void Handle(Event message)
            {

                Console.WriteLine("RmqBridge: Event {0}", message.Id);
            }

            public void Handle(Command message)
            {

                Console.WriteLine("RmqBridge: Command {0}", message.Id);
            }
        }
    }
}
