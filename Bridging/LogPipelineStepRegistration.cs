using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridging
{
    class LogPipelineStepRegistration : INeedInitialization
    {
        public void Customize(BusConfiguration busConfiguration)
        {

            
            // Register the new step in the pipeline
            busConfiguration.Pipeline.Register<BridgeSaverPipelineStep>();

        }
    }
}
