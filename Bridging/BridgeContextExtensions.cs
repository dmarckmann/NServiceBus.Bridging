using System;
using System.Linq;
using Bridging;
namespace NServiceBus
{ 
    public static class BridgeContextExtensions
    {
        public static void DefineBridgedCommandsAs(this BusConfiguration busConfiguration, Predicate<Type> action)
        {
            var bridgeContext = new BridgeContext();
            bridgeContext.BridgedCommandDefinition = action;
            busConfiguration.RegisterComponents(c => c.RegisterSingleton(bridgeContext));
        }
    }


}
