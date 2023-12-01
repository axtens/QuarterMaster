using QuarterMaster.Debugging;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace QuarterMaster.Data
{
    public partial class SQL
    {
        private ErrorCargo ExecIterative(string sql, int timeout)
        {
            var result = new ErrorCargo();

            sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandType = CommandType.Text,
                CommandTimeout = timeout,
                CommandText = sql
            };

            while (sqlCommand.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }

            sqlCommand.CommandText = sql;

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
                stopwatch.Stop();
                stopwatch.Reset();
            }
            sqlCommand.Dispose();
            return result;
        }

        public void Exec(string sql, int timeout = 60, int retryCount = 1)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            int calculatedTimeout = 0;
            var result = new ErrorCargo();

            for (var i = 0; i < retryCount; i++)
            {
                calculatedTimeout = timeout * i;
                NullifyLastError();
                result = ExecIterative(sql, calculatedTimeout);
                if (result.Error == null)
                {
                    break;
                }
                // else 
                System.Threading.Thread.Sleep(1000);
            }

            if (result.Error != null) 
            {
                LastError = (Exception)result.Error;
                sqlCommand.Dispose();
                Status = $"{MethodBase.GetCurrentMethod().Name} failed after {retryCount} attempts and a timeout of {calculatedTimeout} seconds";
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                throw new Exception(Status);
            }

        }

        private ErrorCargo ExecIterative(SqlConnection connection, string sql, int timeout)
        {
            var result = new ErrorCargo();

            sqlCommand = new SqlCommand
            {
                Connection = connection,
                CommandType = CommandType.Text,
                CommandTimeout = timeout,
                CommandText = sql
            };

            while (sqlCommand.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }

            sqlCommand.CommandText = sql;

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
                stopwatch.Stop();
                stopwatch.Reset();
            }
            sqlCommand.Dispose();
            return result;
        }

        public void Exec(SqlConnection connection, string sql, int timeout = 60, int retryCount = 1)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            int calculatedTimeout = 0;
            var result = new ErrorCargo();

            for (var i = 0; i < retryCount; i++)
            {
                calculatedTimeout = timeout * i;
                NullifyLastError();
                result = ExecIterative(connection, sql, calculatedTimeout);
                if (result.Error == null)
                {
                    break;
                }
                // else 
                System.Threading.Thread.Sleep(1000);
            }

            if (result.Error != null)
            {
                LastError = (Exception)result.Error;
                sqlCommand.Dispose();
                Status = $"{MethodBase.GetCurrentMethod().Name} failed after {retryCount} attempts and a timeout of {calculatedTimeout} seconds";
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                throw new Exception(Status);
            }
        }

        private ErrorCargo DTEvalIterative(string sql, int timeout)
        {
            var result = new ErrorCargo();

            sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandType = CommandType.Text,
                CommandTimeout = timeout,
                CommandText = sql
            };

            while (sqlCommand.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }

            sqlCommand.CommandText = sql;

            DataTable table = new DataTable();

            try
            {
                stopwatch.Start();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    table.Load(reader);
                    result.Cargo = table;
                }
                sqlCommand.ExecuteNonQuery();
                stopwatch.Stop();
                this.LastDuration = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                NullifyLastError();
            }
            catch (SqlException sqlEx)
            {
                result.Error = sqlEx;
                stopwatch.Stop();
                stopwatch.Reset();
            }
            sqlCommand.Dispose();
            return result;
        }


        public DataTable DTEval(string sql, int timeout = 60, int retryCount = 1)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            int calculatedTimeout = 0;
            var result = new ErrorCargo();

            for (var i = 0; i < retryCount; i++)
            {
                calculatedTimeout = timeout * i;
                NullifyLastError();
                result = DTEvalIterative(sql, calculatedTimeout);
                if (result.Error == null)
                {
                    break;
                }
                // else 
                System.Threading.Thread.Sleep(1000);
            }

            if (result.Error != null)
            {
                LastError = (Exception)result.Error;
                sqlCommand.Dispose();
                Status = $"{MethodBase.GetCurrentMethod().Name} failed after {retryCount} attempts and a timeout of {calculatedTimeout} seconds";
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                throw new Exception(Status);
            }
            return (DataTable)result.Cargo;
        }

        private ErrorCargo DTEvalIterative(SqlConnection connection, string sql, int timeout)
        {
            var result = new ErrorCargo();

            sqlCommand = new SqlCommand
            {
                Connection = connection,
                CommandType = CommandType.Text,
                CommandTimeout = timeout,
                CommandText = sql
            };

            while (sqlCommand.Connection.State == ConnectionState.Connecting)
            {
                Thread.Sleep(1);
            }

            sqlCommand.CommandText = sql;

            DataTable table = new DataTable();

            try
            {
                stopwatch.Start();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    table.Load(reader);
                    result.Cargo = table;
                }
                sqlCommand.ExecuteNonQuery();
                stopwatch.Stop();
                this.LastDuration = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                NullifyLastError();
            }
            catch (SqlException sqlEx)
            {
                result.Error = sqlEx;
                stopwatch.Stop();
                stopwatch.Reset();
            }
            sqlCommand.Dispose();
            return result;
        }


        public DataTable DTEval(SqlConnection connection, string sql, int timeout = 60, int retryCount = 1)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            int calculatedTimeout = 0;
            var result = new ErrorCargo();

            for (var i = 0; i < retryCount; i++)
            {
                calculatedTimeout = timeout * i;
                NullifyLastError();
                result = DTEvalIterative(connection, sql, calculatedTimeout);
                if (result.Error == null)
                {
                    break;
                }
                // else 
                System.Threading.Thread.Sleep(1000);
            }

            if (result.Error != null)
            {
                LastError = (Exception)result.Error;
                sqlCommand.Dispose();
                Status = $"{MethodBase.GetCurrentMethod().Name} failed after {retryCount} attempts and a timeout of {calculatedTimeout} seconds";
                asana.CreateNewTaskInWorkspaceProjectSection("ComplaintsError", DomainProcess, Status);
                throw new Exception(Status);
            }
            return (DataTable)result.Cargo;
        }

    }
}