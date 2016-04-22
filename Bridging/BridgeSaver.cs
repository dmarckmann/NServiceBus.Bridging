using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Settings;
using NServiceBus.Unicast;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Bridging
{
    public class BridgeSaver : IBehavior<OutgoingContext>
    {
        public BridgeContext Context { get; set; }

        public IBus Bus { get; set; }
        public ReadOnlySettings Settings { get; set; }
        public MessageEndpointMappingCollection MessageEndpointMappings { get; set; }
        public void Invoke(OutgoingContext context, Action next)
        {

            var msg = context.OutgoingMessage;
            if (ShouldBridgeMessage(msg))
            {
                string destination = null;
                if (context.DeliveryOptions is SendOptions)
                {
                    destination = (context.DeliveryOptions as SendOptions).Destination.Queue;
                }
                
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Bridge"].ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        string sql = "INSERT INTO [dbo].[Bridge] (MessageId, Source, Destination, TimeSent, Intent, Processed, Headers, Body) VALUES (@MessageId, @Source, @Destination, @TimeSent, @Intent, 0, @Headers, @Body)";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@MessageId", msg.Id);
                        cmd.Parameters.AddWithValue("@Source", Settings.EndpointName());
                        if (string.IsNullOrEmpty(destination))
                            cmd.Parameters.AddWithValue("@Destination", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@Destination", destination);
                        cmd.Parameters.AddWithValue("@TimeSent", DateTimeExtensions.ToUtcDateTime(msg.Headers[Headers.TimeSent]));
                        cmd.Parameters.AddWithValue("@Intent", msg.MessageIntent.ToString());
                        cmd.Parameters.AddWithValue("@Headers", Newtonsoft.Json.JsonConvert.SerializeObject(msg.Headers));
                        cmd.Parameters.AddWithValue("@Body", msg.Body);

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("SOMETHING WENT TERRIBLY WRONG....");
                        Console.WriteLine(ex);
                    }
                }
            }
            else
            {
                next();
            }

        }

        private string GetDestination(TransportMessage msg)
        {
            var messageTypes = msg.Headers[Headers.EnclosedMessageTypes].Split(';').Select(i => Type.GetType(i)).ToList();
            var q = from t in messageTypes
                    from m in MessageEndpointMappings.Cast<MessageEndpointMapping>()
                    where m.TypeFullName == t.FullName
                    select m.Endpoint;
            return q.FirstOrDefault();
        }

        private bool ShouldBridgeMessage(TransportMessage msg)
        {
            if (msg.Headers.ContainsKey("Bridging.IsBridgedMessage"))
                return false;
            var messageTypes = msg.Headers[Headers.EnclosedMessageTypes].Split(';').Select(i => Type.GetType(i)).ToList();
            var q = from m in messageTypes
                    where Context.BridgedCommandDefinition(m)
                    select m;

            var v = msg.MessageIntent == MessageIntentEnum.Publish || q.Any();
            return v;
        }
    }




}
