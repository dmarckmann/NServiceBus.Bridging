using NServiceBus;
using NServiceBus.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Timers;

namespace Bridging
{
    public class BridgeCleaner : IWantToRunWhenBusStartsAndStops
    {
        public BridgeContext Context { get; set; }
        public ReadOnlySettings Settings { get; set; }

        private Timer _timer = new Timer();

        public void Start()
        {
            //Clean();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 60000.0;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Clean();
        }

        public void Clean()
        {
            TimeSpan timeSpan = TimeSpan.FromDays(7);
            if (ConfigurationManager.AppSettings.AllKeys.Contains("TimeToKeepBridgeEntries"))
            {
                timeSpan = TimeSpan.Parse(ConfigurationManager.AppSettings["TimeToKeepBridgeEntries"]);
            }

            var endpointName = Settings.EndpointName();

            var dateBeforeWhichEntriesMayBeDeleted = DateTimeOffset.UtcNow - timeSpan;


            using (SqlConnection conn = new SqlConnection(Context.BridgeConnectionString.Invoke()))
            {
                conn.Open();
                string sql = "DELETE FROM [dbo].[Bridge] WHERE Processed = 1 AND Destination = @EndpointName AND TimeSent < @DateBeforeWhichEntriesMayBeDeleted";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EndpointName", endpointName);
                cmd.Parameters.AddWithValue("@DateBeforeWhichEntriesMayBeDeleted", dateBeforeWhichEntriesMayBeDeleted);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
