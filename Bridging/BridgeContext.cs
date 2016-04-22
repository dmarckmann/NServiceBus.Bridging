using Bridging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Bridging
{

    public class BridgeContext
    {

        public BridgeContext()
        {
            BridgeConnectionString = () => ConfigurationManager.ConnectionStrings["Bridge"].ConnectionString;
        }
        internal Predicate<Type> BridgedCommandDefinition { get; set; }
        internal Predicate<Type> BridgedEventDefinition { get; set; }

        internal Func<string> BridgeConnectionString { get; private set; }

        public void DefineBridgedCommandsAs(Predicate<Type> action)
        {
            BridgedCommandDefinition = action;
        }

        public void DefineBridgedEventsAs(Predicate<Type> action)
        {
            BridgedEventDefinition = action;
        }

        public void ConnectionStringName(string name)
        {
            BridgeConnectionString = () => ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public void ConnectionString(string connectionString)
        {
            BridgeConnectionString = () => connectionString;
        }

    }
}

