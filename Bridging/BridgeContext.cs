using Bridging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridging
{

    public class BridgeContext
    {
        public Predicate<Type> BridgedCommandDefinition { get; set; }

    }
}

