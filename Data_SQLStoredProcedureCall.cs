using QuarterMaster.Debugging;
using QuarterMaster.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace QuarterMaster.Data
{
    public class SQLStoredProcedureCall
    {
        internal static ApplicationLogging AL;
        internal string _sproc { set; get; }
        internal List<string> _param;

        public SQLStoredProcedureCall(string sproc)
        {
            _sproc = sproc;
            _param = new List<string>();
        }

        public SQLStoredProcedureCall StringParam(string sym, string val)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _param.Add($"{sym}='{val.Replace("'", "''")}'");
            return this;
        }

        public SQLStoredProcedureCall NumericParam(string sym, Object val)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _param.Add($"{sym}={val}");
            return this;
        }

        public SQLStoredProcedureCall DateParam(string sym, string val)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _param.Add($"{sym}='{val}'");
            return this;
        }

        public SQLStoredProcedureCall BooleanParam(string sym, bool val)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _param.Add($"{sym}={(val ? 1 : 0)}");
            return this;
        }

        public override string ToString() => _sproc + " " + string.Join(", ", _param.ToArray());

        public void RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
        }
    }
}
