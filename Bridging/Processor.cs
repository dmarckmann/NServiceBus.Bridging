using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.ObjectBuilder;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Serialization;
using NServiceBus.Settings;
using NServiceBus.Unicast;
using NServiceBus.Unicast.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Bridging
{

    public class BridgeProcessor : IWantToRunWhenBusStartsAndStops
    {
        public Schedule Scheduler { get; set; }
        public ReadOnlySettings Settings { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public PipelineExecutor PipelineExecutor { get; set; }
        public IBuilder Builder{ get; set; }

        public IBus Bus { get; set; }
        public BridgeContext Context { get; set; }

        public MessageEndpointMappingCollection MessageEndpointMappings { get; set; }


        private Timer _timer = new Timer();
        public void Start()
        {
            //ProcessMessagesFromBridge();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 10000.0;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            ProcessMessagesFromBridge();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }



        private void ProcessMessagesFromBridge()
        {


            using (SqlConnection conn = new SqlConnection(Context.BridgeConnectionString.Invoke()))
            {
                conn.Open();
                string sql = "SELECT MessageId, Source, Destination, Intent, Processed, Headers, Body FROM [dbo].[Bridge] WHERE Processed = 0 AND (Destination = @Destination OR Destination IS NULL)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Destination", Settings.EndpointName());
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader.GetString(reader.GetOrdinal("MessageId"));
                    string source = reader.GetString(reader.GetOrdinal("Source"));
                    string dest = reader.IsDBNull(reader.GetOrdinal("Destination")) ? null : reader.GetString(reader.GetOrdinal("Destination"));
                    string intent = reader.GetString(reader.GetOrdinal("Intent"));
                    var msgIntent = (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), intent);
                    Dictionary<string, string> headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("Headers")));
                    var messageTypes = headers[Headers.EnclosedMessageTypes].Split(';').Select(i => Type.GetType(i)).ToList();
                    if (ShouldHandleMessage(dest, messageTypes))
                    {

                        var bodyStream = reader.GetStream(reader.GetOrdinal("Body"));
                        byte[] body = new byte[bodyStream.Length];
                        body = GetByteArray(bodyStream);


                        //Let's treat this message as a normal incoming message.
                        var transportMessage = new TransportMessage(id, headers);
                        transportMessage.Body = body;
                        transportMessage.MessageIntent = msgIntent;
                        var incomingContext = new IncomingContext(null, transportMessage);
                        incomingContext.Set<IBuilder>(Builder);
                        PipelineExecutor.InvokePipeline(PipelineExecutor.Incoming.Select(i => i.BehaviorType), incomingContext);

                        var cmd2 = new SqlCommand("UPDATE [dbo].[Bridge] SET Processed = 1 WHERE MessageId = @MessageId", conn);
                        cmd2.Parameters.AddWithValue("@MessageId", id);
                        cmd2.ExecuteNonQuery();

                    }
                }

            }
        }

        private bool ShouldHandleMessage(string destination, List<Type> messageTypes)
        {
            if (destination == Settings.EndpointName())
                return true;

            var q = from t in messageTypes
                    where Context.BridgedEventDefinition(t)
                    select t;
            return q.Any();
        }

        private static byte[] GetByteArray(Stream bodyStream)
        {
            byte[] body;
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = bodyStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                body = ms.ToArray();
            }

            return body;
        }
    }
}
