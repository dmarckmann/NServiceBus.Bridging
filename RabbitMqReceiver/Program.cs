﻿using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Pipeline;
using Sared;
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
            busConfiguration.Conventions().DefiningCommandsAs(t => t.Name == "Command");
            busConfiguration.Conventions().DefiningEventsAs(t => t.Name == "Event");
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
                    ConsoleKeyInfo key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.Key != ConsoleKey.Enter)
                    {
                        return;
                    }
                    bus.Send("AzureStorageQueueSender", new Command() { Id = Guid.NewGuid() });
                    Console.WriteLine("Msg sent / published");
                }
            }

        }
    }

    class Handler : IHandleMessages<Event>, IHandleMessages<Command>
    {
        public void Handle(Event message)
        {

            Console.WriteLine("RmqHandler: Event {0}", message.Id);
        }

        public void Handle(Command message)
        {

            Console.WriteLine("RmqHandler: Command {0}", message.Id);
        }
    }
}
