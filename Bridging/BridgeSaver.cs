using NServiceBus;

using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Bridging
{
    public class BridgeSaver : IBehavior<IncomingContext>
    {
        public IBus Bus { get; set; }
        public ReadOnlySettings Settings { get; set; }

        public void Invoke(IncomingContext context, Action next)
        {
            Console.WriteLine("HIERO!");
            var msg = context.PhysicalMessage;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Bridge"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO [dbo].[Bridge] (MessageId, Source, Destination, TimeSent, Intent, Processed, Headers, Body) VALUES (@MessageId, @Source, @Destination, @TimeSent, @Intent, 0, @Headers, @Body)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@MessageId", msg.Id);
                    cmd.Parameters.AddWithValue("@Source", msg.Headers[Headers.OriginatingEndpoint]);
                    cmd.Parameters.AddWithValue("@Destination", Settings.EndpointName());
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
    }
}
