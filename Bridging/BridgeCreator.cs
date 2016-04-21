using NServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridging
{
    public class BridgeCreator : IWantToRunWhenBusStartsAndStops
    {
        public void Start()
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Bridge"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT OBJECT_ID('Bridge')";
                SqlCommand cmd = new SqlCommand(sql, conn);
                var result = cmd.ExecuteScalar();
                if (result == DBNull.Value)
                {
                    string sql2 = @"CREATE TABLE [dbo].[Bridge](
                                        [MessageId][nvarchar](255) NOT NULL,
                                        [Source] [nvarchar](255) NOT NULL,
                                        [Destination] [nvarchar](255) NOT NULL,
                                        [TimeSent] [datetime] NOT NULL,
                                        [Intent] [nvarchar](10) NOT NULL,
                                        [Processed] [bit] NOT NULL,
                                        [Headers] [nvarchar](max) NOT NULL,
                                        [Body] [varbinary](max) NOT NULL,
                                        PRIMARY KEY CLUSTERED
                                            (
                                                [MessageId] ASC
                                            )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                                        ) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
                    SqlCommand cmd2 = new SqlCommand(sql2, conn);
                  
                    cmd2.ExecuteNonQuery();
                }
            }
        }

        public void Stop()
        {
        }
    }
}
