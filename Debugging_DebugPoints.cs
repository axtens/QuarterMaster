using System.Collections.Generic;

namespace QuarterMaster.Debugging
{
    public static class DebugPoints
    {
        private static readonly List<string> DEBUGPOINTS = new List<string>();

        public static void RequestDebugPoint(string pointName)
        {
            DEBUGPOINTS.Add(pointName.ToUpper());
        }

        public static void ClearDebugPoint(string pointName)
        {
            if (DEBUGPOINTS.Contains(pointName.ToUpper()))
            {
                DEBUGPOINTS.Remove(pointName);
            }
        }

        public static bool DebugPointRequested(string pointName)
        {
            return DEBUGPOINTS.Contains(pointName.ToUpper());
        }

        public static List<string> GetDebugPoints()
        {
            return DEBUGPOINTS;
        }

        public static void Request(string pointName)
        {
            DEBUGPOINTS.Add(pointName.ToUpper());
        }

        public static void Clear(string pointName)
        {
            if (DEBUGPOINTS.Contains(pointName.ToUpper()))
            {
                DEBUGPOINTS.Remove(pointName);
            }
        }

        public static bool Requested(string pointName)
        {
            return DEBUGPOINTS.Contains(pointName.ToUpper());
        }

        public static List<string> GetList()
        {
            return DEBUGPOINTS;
        }
    }
}
