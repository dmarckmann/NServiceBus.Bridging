using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Pipeline;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            BusConfiguration busConfiguration = new BusConfiguration();
            
            busConfiguration.Conventions().DefiningCommandsAs(t => t.Name == "CommandSendToTransport1" || t.Name == "CommandSendToTransport2");
            busConfiguration.Conventions().DefiningEventsAs(t => t.Name == "EventRaisedByTransport1" || t.Name == "EventRaisedByTransport2");
            busConfiguration.DefineBridgedCommandsAs(t => t.Name == "CommandSendToTransport1");
            busConfiguration.UseTransport<RabbitMQTransport>();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.UseSerialization<XmlSerializer>();

            busConfiguration.EnableInstallers();
          
            using (IBus bus = Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("Press enter to publish an event");
                Console.WriteLine("Press any key to exit.");

                while (true)
                {
                    Console.WriteLine("Press s to Send a command");
                    Console.WriteLine("Press p to publish an event");
                    Console.WriteLine("Press esc to exit.");

                    while (true)
                    {
                        ConsoleKeyInfo key = Console.ReadKey();
                        Console.WriteLine();

                        if (key.Key == ConsoleKey.Escape)
                        {
                            return;
                        }
                        if (key.Key == ConsoleKey.S)
                        {
                            bus.Send(new CommandSendToTransport1() { Id = Guid.NewGuid() });
                            Console.WriteLine("Msg sent ");
                        }

                        if (key.Key == ConsoleKey.P)
                        {
                            bus.Publish(new EventRaisedByTransport2() { Id = Guid.NewGuid() });
                            Console.WriteLine("Msg published");
                        }
                    }
                }
            }

        }
    }

    class Handler : IHandleMessages<EventRaisedByTransport1>, IHandleMessages<CommandSendToTransport2>
    {
        public void Handle(EventRaisedByTransport1 message)
        {

            Console.WriteLine("Transport2 Handler: EventRaisedByTransport1 {0}", message.Id);
        }

        public void Handle(CommandSendToTransport2 message)
        {

            Console.WriteLine("Transport2 Handler: CommandSendToTransport2 {0}", message.Id);
        }
    }
}
