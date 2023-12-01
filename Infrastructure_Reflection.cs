using QuarterMaster.Debugging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace QuarterMaster.Infrastructure
{
    public static class Reflection
    {
        public static Type GetType(object obj)
        {
            return obj.GetType();
        }

        public static string[] ShowMethods(Type type)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var lst = new List<string>();
            foreach (var method in type.GetMethods())
            {
                var parameters = method.GetParameters();
                var parameterDescriptions = string.Join
                    (", ", method.GetParameters()
                                 .Select(x => x.ParameterType + " " + x.Name)
                                 .ToArray());
                lst.Add(String.Format("{0} {1} ({2})",
                                  method.ReturnType,
                                  method.Name,
                                  parameterDescriptions));
            }
            return lst.ToArray<string>();
        }
    }
}
