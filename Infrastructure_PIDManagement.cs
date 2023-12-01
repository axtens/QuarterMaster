using QuarterMaster.Data;

using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace QuarterMaster.Infrastructure
{
    public static class PidManagement
    {
        public static long RegisterPID(SQL SqlServer, string StartPidSproc, int pidType)
        {
            string utc = Process.GetCurrentProcess().StartTime.ToString("yyyyMMddHHmmss'.'ffffffzzz", CultureInfo.InvariantCulture);
            string[] utcParts = utc.Split(new char[] { '+', '-', ':' });
            int utcMin = int.Parse(utcParts[1], CultureInfo.InvariantCulture) * 60;
            if (utcParts.Length > 2)
            {
                utcMin += int.Parse(utcParts[2], CultureInfo.InvariantCulture);
            }
            string utcFinal = utcParts[0] + (utcMin >= 0 ? "+" : "-") + utcMin.ToString(CultureInfo.InvariantCulture);
            string sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0} '{1}',{2},{3}",
                    StartPidSproc,
                    utcFinal,
                    Process.GetCurrentProcess().Id,
                    pidType
                );

            DataTable pidTable = SqlServer.DTEvalR(sql);
            long PIDFK = (int)pidTable.Rows[0][0];
            return PIDFK;
        }

        public static void DeregisterPID(SQL SqlServer, string FinishPidSproc, long PIDFK)
        {
            string query = string.Format(CultureInfo.InvariantCulture, "EXEC {0} {1}", FinishPidSproc, PIDFK);
            SqlServer.ExecR(query,60,3);
            if (SqlServer.LastError != null)
                throw SqlServer.LastError;
        }
    }
}
