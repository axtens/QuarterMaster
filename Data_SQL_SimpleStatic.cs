using QuarterMaster.Debugging;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace QuarterMaster.Data
{
    public static class SimpleSQL
    {
        public static string SimpleConnectionString { get; set; }
        public static long LastDuration { get; set; }
        
        internal static Stopwatch stopwatch = new Stopwatch();

        public static ErrorCargo SimpleTextExec(string sql, int timeout = 180)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var result = new ErrorCargo();

            using (var conn = new SqlConnection(SimpleConnectionString))
            {
                conn.Open();

                using (var sqlCommand = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    CommandText = sql
                })
                {
                    while (sqlCommand.Connection.State == ConnectionState.Connecting)
                    {
                        Thread.Sleep(1);
                    }

                    LastDuration = 0;

                    try
                    {
                        stopwatch.Start();
                        sqlCommand.ExecuteNonQuery();
                        stopwatch.Stop();
                        LastDuration = stopwatch.ElapsedMilliseconds;
                    }
                    catch (SqlException sqlEx)
                    {
                        result.Error = sqlEx;
                        stopwatch.Stop();
                    }
                    stopwatch.Reset();
                }
                conn.Close();
            }
            return result;
        }

        public static ErrorCargo SimpleTextEval(string sql, int timeout = 180)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var result = new ErrorCargo();

            using (var conn = new SqlConnection(SimpleConnectionString))
            {
                conn.Open();

                using (var sqlCommand = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    CommandText = sql
                })
                {

                    while (sqlCommand.Connection.State == ConnectionState.Connecting)
                    {
                        Thread.Sleep(1);
                    }

                    DataTable table = new DataTable();

                    LastDuration = 0;

                    try
                    {
                        stopwatch.Start();
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            table.Load(reader);
                            result.Cargo = table;
                        }
                        stopwatch.Stop();
                        LastDuration = stopwatch.ElapsedMilliseconds;
                    }
                    catch (SqlException sqlEx)
                    {
                        result.Error = sqlEx;
                        stopwatch.Stop();
                    }
                    stopwatch.Reset();
                };
                conn.Close();
            }
            return result;
        }
    }
}