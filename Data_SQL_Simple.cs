using QuarterMaster.Debugging;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace QuarterMaster.Data
{

    public partial class SQL
    {
        public string SimpleConnectionString { get; set; }

        public ErrorCargo SimpleTextExec(string sql, int timeout = 180)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var result = new ErrorCargo();
            LastError = null;

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

                    try
                    {
                        stopwatch.Start();
                        sqlCommand.ExecuteNonQuery();
                        stopwatch.Stop();
                        this.LastDuration = stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                        NullifyLastError();
                    }
                    catch (SqlException sqlEx)
                    {
                        result.Error = sqlEx;
                        LastError = sqlEx;
                        stopwatch.Stop();
                        stopwatch.Reset();
                    }
                }
                conn.Close();
            }
            return result;
        }

        public ErrorCargo SimpleTextEval(string sql, int timeout = 180)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var result = new ErrorCargo();
            LastError = null;

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

                    try
                    {
                        stopwatch.Start();
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            table.Load(reader);
                            result.Cargo = table;
                        }
                        stopwatch.Stop();
                        this.LastDuration = stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                        NullifyLastError();
                    }
                    catch (SqlException sqlEx)
                    {
                        result.Error = sqlEx;
                        LastError= sqlEx;
                        stopwatch.Stop();
                        stopwatch.Reset();
                    }
                };
                conn.Close();
            }
            return result;
        }
    }
}