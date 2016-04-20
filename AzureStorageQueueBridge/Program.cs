using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Serialization;
using NServiceBus.Serializers.XML;
using NServiceBus.Settings;
using NServiceBus.Unicast.Messages;
using Sared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Timers;

namespace AzureStorageQueueBridge
{
    public class Program
    {
        public static string EndPointName { get; set; }
        public static void Main(string[] args)
        {
            EndPointName = "RabbitMqReceiver";
            new Program().Run();
        }

        


        public void Run()
        {
            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName(EndPointName);
            busConfiguration.Conventions().DefiningCommandsAs(t => t.Name == "Command");
            busConfiguration.Conventions().DefiningEventsAs(t => t.Name == "Event");
            busConfiguration.UseSerialization<XmlSerializer>();
            busConfiguration.UseTransport<AzureServiceBusTransport>();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();
            busConfiguration.EnableInstallers();

            using (Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

    }

    class Handler : IHandleMessages<Event>, IHandleMessages<Command>
    {
        public void Handle(Event message)
        {

            Console.WriteLine("AsbBridge: Event {0}", message.Id);
        }

        public void Handle(Command message)
        {

            Console.WriteLine("AsbBridge: Command {0}", message.Id);
        }
    }
}
