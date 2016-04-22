using System;
using System.Linq;
using Bridging;
namespace NServiceBus
{ 
    public static class BridgeContextExtensions
    {


        private static BridgeContext _context;
        public static BridgeContext Bridge(this BusConfiguration busConfiguration)
        {
                if (_context == null)
                {
                    _context = new BridgeContext();
                busConfiguration.RegisterComponents(c => c.RegisterSingleton(_context));
            }

            return _context;
        }
    }


}
