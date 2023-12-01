using System.Collections.Generic;
using System.Linq;

namespace QuarterMaster.Serialization
{


    public class SqlStoredProcedureCall
    {
        string sproc;
        List<string> args = new List<string>();

        public SqlStoredProcedureCall(string _sproc)
        {
            sproc = _sproc;
        }

        public SqlStoredProcedureCall stringParam(string field, string value)
        {
            args.Add($"{field} = '{value.Replace("'", "''")}'");
            return this;
        }

        public SqlStoredProcedureCall numericParam(string field, object value)
        {
            args.Add($"{field} = {value}");
            return this;
        }

        public SqlStoredProcedureCall dateParam(string field, string value)
        {
            args.Add($"{field} = '{value}'");
            return this;
        }

        public SqlStoredProcedureCall booleanParam(string field, bool value)
        {
            args.Add($"{field} = '{(value ? 1 : 0)}'");
            return this;
        }

        public SqlStoredProcedureCall nullParam(string field)
        {
            args.Add($"{field} = NULL");
            return this;
        }

        public override string ToString()
        {
            return sproc + " " + string.Join(", ", args.ToArray<string>());
        }
    }
}
