using NServiceBus;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageQueueSender
{
    class Program
    {
        static void Main(string[] args)
        {
            BusConfiguration busConfiguration = new BusConfiguration();
            busConfiguration.Conventions().DefiningCommandsAs(t => t.Name.Contains("Command"));
            busConfiguration.Conventions().DefiningEventsAs(t => t.Name.Contains("Event"));
            busConfiguration.Bridge().DefineBridgedCommandsAs(t => t.Name == "CommandSendToTransport2");
            busConfiguration.Bridge().DefineBridgedEventsAs(t => t.Name == "EventRaisedByTransport2");
            busConfiguration.Bridge().ConnectionStringName("Bridge");
            busConfiguration.UseTransport<AzureServiceBusTransport>();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.UseSerialization<XmlSerializer>();
            busConfiguration.ScaleOut().UseSingleBrokerQueue();

            busConfiguration.EnableInstallers();
       
            using (IBus bus = Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("Press s to Send a command");
                Console.WriteLine("Press p to publish an event");
                Console.WriteLine("Press esc to exit.");

                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.Key == ConsoleKey.Escape )
                    {
                        return;
                    }
                    if (key.Key == ConsoleKey.S)
                    {
                        bus.Send(new CommandSendToTransport2() { Id = Guid.NewGuid() });
                        Console.WriteLine("Msg sent ");
                    }

                    if (key.Key == ConsoleKey.P)
                    {
                        bus.Publish(new EventRaisedByTransport1() { Id = Guid.NewGuid() });
                        Console.WriteLine("Msg published");
                    }

                    
                }
            }
        }


        class Handler : IHandleMessages<EventRaisedByTransport2>, IHandleMessages<CommandSendToTransport1>
        {
            public void Handle(EventRaisedByTransport2 message)
            {

                Console.WriteLine("Transport1Handler: EventRaisedByTransport2 {0}", message.Id);
            }

            public void Handle(CommandSendToTransport1 message)
            {

                Console.WriteLine("Transport1 Handler: CommandSendToTransport1 {0}", message.Id);
            }
        }
    }
}
