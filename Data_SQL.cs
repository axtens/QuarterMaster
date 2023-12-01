using QuarterMaster.Communications;
using QuarterMaster.Debugging;
using QuarterMaster.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace QuarterMaster.Data
{
    public partial class SQL
    {
        internal static ApplicationLogging AL;

        internal static Asana asana = new Asana(@"C:\web\AsanaSettings.INI")
            .Workspace("FortescueTechnologies")
            .Project("Complaints");

        internal static string ProcessName = Process.GetCurrentProcess().ProcessName;
        internal static string UserDomain = Environment.GetEnvironmentVariable("USERDOMAIN");
        internal static string DomainProcess = "[" + UserDomain.Split('-')[1] + "] " + ProcessName;

        public static SqlConnection sqlConnection;
        public static SqlCommand sqlCommand;
        public Exception LastError;
        public string ServerName { get; private set; }
        public string DatabaseName { get; private set; }
        private const int MAX_RETRY_COUNT = 7;
        private const int SQL_DEADLOCK_ERROR_CODE = 1205;
        private const int SQL_TIMEOUT_ERROR_CODE = -2;
        public static string Status { get; set; }
        readonly private static Stopwatch stopwatch = new Stopwatch();
        public long LastDuration { get; private set; }

        public SQL()
        {
            this.LastDuration = 0;
            stopwatch.Reset();
        }

        public void ClearStatus()
        {
            Status = string.Empty;
        }

        public void NullifyLastError()
        {
            LastError = null;
        }

        public SQL(string connectionString)
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            try
            {
                this.Connect(connectionString);
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public void Connect(string connectionString)
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                while (sqlConnection.State == ConnectionState.Connecting)
                {
                    Thread.Sleep(1);
                }
                this.ServerName = sqlConnection.DataSource;
                this.DatabaseName = sqlConnection.Database;
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public string Eval(string sql, int timeout = 60)
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            StringBuilder evalResult = new StringBuilder();
            DataTable table = new DataTable();
            sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                //sqlCommand = sqlConnection.CreateCommand();
                CommandText = sql,
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };
            try
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    table.Load(reader);
                }

                foreach (var col in table.Columns)
                {
                    evalResult.Append(col).Append(",");
                }

                evalResult.Replace(",", System.Environment.NewLine, evalResult.Length - 1, 1);

                foreach (DataRow dr in table.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        var typ = column.GetType();
                        evalResult.Append("\"").Append(column.ToString()).Append("\",");
                    }

                    evalResult.Replace(",", System.Environment.NewLine, evalResult.Length - 1, 1);
                }

                table.Dispose();
                sqlCommand.Dispose();
                NullifyLastError();
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
                stopwatch.Stop();
                this.LastDuration = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                return null;
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            return evalResult.ToString();
        }

        public string Eval(SqlConnection connection, string sql, int timeout = 60)
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            SqlCommand command = new SqlCommand
            {
                Connection = connection,
                CommandText = sql,
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };
            //SqlCommand command = connection.CreateCommand();

            StringBuilder evalResult = new StringBuilder();
            DataTable table = new DataTable();

            while (command.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }
            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }

                foreach (var col in table.Columns)
                {
                    evalResult.Append(col).Append(",");
                }

                evalResult.Replace(",", System.Environment.NewLine, evalResult.Length - 1, 1);

                foreach (DataRow dr in table.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        var typ = column.GetType();
                        evalResult.Append("\"").Append(column.ToString()).Append("\",");
                    }

                    evalResult.Replace(",", System.Environment.NewLine, evalResult.Length - 1, 1);
                }
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
            }
            command.Dispose();
            table.Dispose();
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            return evalResult.ToString();
        }


        public DataTable DTEvalR(string sql, int timeout = 60, int retryCount = 1, bool recursing = false)
        {
            if (!recursing)
            {
                stopwatch.Start();
            }

            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            try
            {
                DataTable table = new DataTable();
                sqlCommand = new SqlCommand
                {
                    Connection = sqlConnection,
                    //sqlCommand = sqlConnection.CreateCommand();
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    CommandText = sql
                };
                while (sqlCommand.Connection.State == ConnectionState.Connecting)
                {
                    Thread.Sleep(1);
                }
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    table.Load(reader);
                }
                if (!recursing)
                {
                    stopwatch.Stop();
                    this.LastDuration = stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                }
                sqlCommand.Dispose();
                return table;
            }
            catch (SqlException sqlEx)
            {
                LastError = (Exception)sqlEx;
                sqlCommand.Dispose();
                if (retryCount == MAX_RETRY_COUNT) //5, 7, Whatever
                {
                    Status = MethodBase.GetCurrentMethod().Name + " failed after " + retryCount + " attempts and a timeout of " + timeout + " seconds";
                    asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                    throw;
                }

                switch (sqlEx.Number)
                {
                    case SQL_DEADLOCK_ERROR_CODE: //1205
                                                  //log.Warn("DoSomeSql was deadlocked, will try again.");
                        break;
                    case SQL_TIMEOUT_ERROR_CODE: //-2
                                                 //log.Warn("DoSomeSql was timedout, will try again.");
                        break;
                    default:
                        //log.WarnFormat(buf.ToString(), sqlEx);
                        break;
                }

                System.Threading.Thread.Sleep(1000); //Can also use Math.Rand for a random interval of time
                return DTEvalR(sql, timeout * 2, ++retryCount, true);
            }
        }

        public DataTable DTEvalR(SqlConnection connection, string sql, int timeout = 60, int retryCount = 1, bool recursing = false)
        {
            if (!recursing)
            {
                stopwatch.Start();
            }

            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            try
            {
                DataTable table = new DataTable();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = timeout;
                    command.CommandText = sql;
                    while (command.Connection.State == ConnectionState.Connecting)
                    {
                        Thread.Sleep(1);
                    }
                    using SqlDataReader reader = command.ExecuteReader();
                    table.Load(reader);
                }
                if (!recursing)
                {
                    stopwatch.Stop();
                    this.LastDuration = stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                }

                return table;
            }
            catch (SqlException sqlEx)
            {
                LastError = (Exception)sqlEx;

                if (retryCount == MAX_RETRY_COUNT) //5, 7, Whatever
                {
                    Status = MethodBase.GetCurrentMethod().Name + " failed after " + retryCount + " attempts and a timeout of " + timeout + " seconds";
                    asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                    throw;
                }

                switch (sqlEx.Number)
                {
                    case SQL_DEADLOCK_ERROR_CODE: //1205
                                                  //log.Warn("DoSomeSql was deadlocked, will try again.");
                        break;
                    case SQL_TIMEOUT_ERROR_CODE: //-2
                                                 //log.Warn("DoSomeSql was timedout, will try again.");
                        break;
                    default:
                        //log.WarnFormat(buf.ToString(), sqlEx);
                        break;
                }

                System.Threading.Thread.Sleep(1000); //Can also use Math.Rand for a random interval of time
                return DTEvalR(sql, timeout * 2, ++retryCount, true);
            }
        }

        public void ExecR(string sql, int timeout = 60, int retryCount = 1, bool recursing = false)
        {
            if (!recursing)
            {
                stopwatch.Start();
            }

            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            try
            {
                sqlCommand = new SqlCommand
                {
                    Connection = sqlConnection,
                    //sqlCommand = sqlConnection.CreateCommand();
                    CommandType = CommandType.Text,
                    CommandTimeout = timeout,
                    CommandText = sql
                };
                while (sqlCommand.Connection.State == ConnectionState.Connecting)
                {
                    Thread.Sleep(1);
                }

                sqlCommand.CommandText = sql;

                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                NullifyLastError();
                stopwatch.Stop();
                this.LastDuration = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            catch (SqlException sqlEx)
            {
                LastError = (Exception)sqlEx;
                sqlCommand.Dispose();
                if (retryCount == MAX_RETRY_COUNT) //5, 7, Whatever
                {
                    Status = MethodBase.GetCurrentMethod().Name + " failed after " + retryCount + " attempts and a timeout of " + timeout + " seconds";
                    asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                    throw;
                }

                switch (sqlEx.Number)
                {
                    case SQL_DEADLOCK_ERROR_CODE: //1205
                                                  //log.Warn("DoSomeSql was deadlocked, will try again.");
                        break;
                    case SQL_TIMEOUT_ERROR_CODE: //-2
                                                 //log.Warn("DoSomeSql was timedout, will try again.");
                        break;
                    default:
                        //log.WarnFormat(buf.ToString(), sqlEx);
                        break;
                }

                System.Threading.Thread.Sleep(1000); //Can also use Math.Rand for a random interval of time
                ExecR(sql, timeout * 2, ++retryCount, true);
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public void ExecR(SqlConnection connection, string sql, int timeout = 60, int retryCount = 1, bool recursing = false)
        {
            if (!recursing)
            {
                stopwatch.Start();
            }

            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            SqlCommand command = new SqlCommand
            {
                Connection = connection,
                //SqlCommand command = connection.CreateCommand();
                CommandType = CommandType.Text,
                CommandTimeout = timeout,
                CommandText = sql
            };
            while (command.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }

            try
            {
                command.ExecuteNonQuery();
                command.Dispose();
                stopwatch.Stop();
                this.LastDuration = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            catch (SqlException sqlEx)
            {
                LastError = (Exception)sqlEx;
                command.Dispose();
                if (retryCount == MAX_RETRY_COUNT) //5, 7, Whatever
                {
                    Status = MethodBase.GetCurrentMethod().Name + " failed after " + retryCount + " attempts and a timeout of " + timeout + " seconds";
                    asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                    throw;
                }

                switch (sqlEx.Number)
                {
                    case SQL_DEADLOCK_ERROR_CODE: //1205
                                                  //log.Warn("DoSomeSql was deadlocked, will try again.");
                        break;
                    case SQL_TIMEOUT_ERROR_CODE: //-2
                                                 //log.Warn("DoSomeSql was timedout, will try again.");
                        break;
                    default:
                        //log.WarnFormat(buf.ToString(), sqlEx);
                        break;
                }

                System.Threading.Thread.Sleep(1000); //Can also use Math.Rand for a random interval of time
                ExecR(sql, timeout * 2, ++retryCount, true);
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public void Close()
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            try
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public void Close(SqlConnection connection)
        {
            stopwatch.Start();
            NullifyLastError();
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            try
            {
                connection.Close();
                connection.Dispose();
            }
            catch (Exception E)
            {
                LastError = E;
                Status = E.Message;
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
        }

        public bool SqlExec(SQL sqlServer, string sql, int timeout = 60, int retryCount = 1)
        {
            stopwatch.Start();
            AL?.Module(MethodBase.GetCurrentMethod().Name);
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            bool result;
            try
            {
                sqlServer.ExecR(sql, timeout, retryCount);
                result = true;
            }
            catch (Exception exc)
            {
                AL?.Fail("SqlExec exception", exc.Message, exc.Source, exc.StackTrace);
                Status = exc.Message;
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                result = false;
            }
            stopwatch.Stop();
            this.LastDuration = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            AL?.Module();
            return result;
        }

        public static void UpdateDictionaryFromDataTableDataRow(ref Dictionary<string, object> theDictionary, DataTable dataTable, DataRow dataRow)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            int columnCount = dataTable.Columns.Count;
            for (var i = 0; i < columnCount; i++)
            {
                theDictionary[dataTable.Columns[i].ColumnName] = dataRow[i];
            }
        }

        public static void UpdateDictionaryFromDataRow(ref Dictionary<string, object> theDictionary, DataRow dataRow)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var dataTable = dataRow.Table;
            int columnCount = dataTable.Columns.Count;
            for (var i = 0; i < columnCount; i++)
            {
                theDictionary[dataTable.Columns[i].ColumnName] = dataRow[i];
            }
        }

        public static Dictionary<string, object> ConvertDataTableDataRowToDictionary(DataTable dataTable, DataRow dataRow)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var result = new Dictionary<string, object>();
            int columnCount = dataTable.Columns.Count;
            for (var i = 0; i < columnCount; i++)
            {
                result[dataTable.Columns[i].ColumnName] = dataRow[i];
            }
            return result;
        }

        /// <summary>
        /// Creates a Dictionary<string,object> from the values in the dataRow
        /// </summary>
        /// <param name="dataRow"></param>
        /// <returns>Dictionary<string,object></returns>
        public static Dictionary<string, object> ConvertDataRowToDictionary(DataRow dataRow)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var result = new Dictionary<string, object>();
            var dataTable = dataRow.Table;
            int columnCount = dataTable.Columns.Count;
            for (var i = 0; i < columnCount; i++)
            {
                result[dataTable.Columns[i].ColumnName] = dataRow[i];
            }
            return result;
        }

        /// <summary>
        /// Associates local T with global telemetry T object
        /// </summary>
        /// <param name="ptr"></param>
        public void RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
        }
    }
}