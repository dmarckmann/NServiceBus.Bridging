using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridging
{
    class MsgSaverPipelineStep : RegisterStep
    {
        public MsgSaverPipelineStep()
            : base("MsgSaverPipelineStep", typeof(MsgSaver), "save msgs to the bridge")
        {
            // Optional: Specify where it needs to be invoked in the pipeline, for example InsertBefore or InsertAfter
            InsertBefore(WellKnownStep.InvokeHandlers);
        }
    }
}
