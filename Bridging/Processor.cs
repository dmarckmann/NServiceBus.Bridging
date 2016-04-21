using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Serialization;
using NServiceBus.Settings;
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
        public IBus Bus { get; set; }

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
            ProcessMessagesFromBridge();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }



        private void ProcessMessagesFromBridge()
        {


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Bridge"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT MessageId, Source, Destination, Intent, Processed, Headers, Body FROM [dbo].[Bridge] WHERE Processed = 0 AND Source = @Source";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Source", Settings.EndpointName());
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string id = reader.GetString(reader.GetOrdinal("MessageId"));
                    string source = reader.GetString(reader.GetOrdinal("Source"));
                    string dest = reader.GetString(reader.GetOrdinal("Destination"));
                    string intent = reader.GetString(reader.GetOrdinal("Intent"));
                    var msgIntent = (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), intent);
                    Dictionary<string, string> headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("Headers")));
                    var messageTypes = headers[Headers.EnclosedMessageTypes].Split(';').Select(i => Type.GetType(i)).ToList();
                    using (Stream body = reader.GetStream(reader.GetOrdinal("Body")))
                    {

                        var msg = Serializer.Deserialize(body, messageTypes).First();
                        //foreach (var pair in headers)
                        //{
                        Bus.SetMessageHeader(msg, Headers.MessageId, id);

                        if (headers.ContainsKey(Headers.ConversationId))
                            Bus.SetMessageHeader(msg, Headers.ConversationId, headers[Headers.ConversationId]);
                        if (headers.ContainsKey(Headers.RelatedTo))
                            Bus.SetMessageHeader(msg, Headers.RelatedTo, headers[Headers.RelatedTo]);
                        if (headers.ContainsKey(Headers.TimeSent))
                            Bus.SetMessageHeader(msg, Headers.TimeSent, headers[Headers.TimeSent]);
                        //}
                        switch (msgIntent)
                        {
                            case MessageIntentEnum.Send:
                                Bus.Send(msg);
                                break;
                            case MessageIntentEnum.Publish:
                                Bus.Publish(msg);
                                break;
                            case MessageIntentEnum.Subscribe:
                                //noop
                                break;
                            case MessageIntentEnum.Unsubscribe:
                                //noop
                                break;
                            case MessageIntentEnum.Reply:
                                //noop
                                break;
                        }
                    }



                    var cmd2 = new SqlCommand("UPDATE [dbo].[Bridge] SET Processed = 1 WHERE MessageId = @MessageId", conn);
                    cmd2.Parameters.AddWithValue("@MessageId", id);
                    cmd2.ExecuteNonQuery();

                }

            }
        }
    }
}
