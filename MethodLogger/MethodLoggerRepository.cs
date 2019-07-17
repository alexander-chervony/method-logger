using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Configuration;

namespace MethodLogger
{
    public class MethodLoggerRepository : IMethodLoggerRepository
    {
        private readonly IConfig _config;

        // todo: don't weave if method length exceeds max value 
        private const int MaxMethodLength = 1800;
        private const int MaxMethodPartLength = 900;

        public MethodLoggerRepository(IConfig config)
        {
            _config = config;
        }

        public void AddMethods(IEnumerable<MethodRow> methods)
        {
            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand("spAddMethods", connection) { CommandType = CommandType.StoredProcedure })
                {
                    var table = new DataTable("methods");
                    table.Columns.Add("method", typeof(string));
                    table.Columns.Add("method_part2", typeof(string));
                    table.Columns.Add("executing_app", typeof(string));
                    table.Columns.Add("pid", typeof(int));
                    table.Columns.Add("machine", typeof(string));
                    table.Columns.Add("inserted_on", typeof(DateTime));
                    foreach (var r in methods)
                    {
                        int methodLength = r.Method.Length;

                        if (methodLength > MaxMethodLength)
                        {
                            // todo: log it
                            continue;
                        }

                        table.Rows.Add(
                            methodLength > MaxMethodPartLength ? r.Method.Substring(0, MaxMethodPartLength) : r.Method,
                            methodLength > MaxMethodPartLength ? r.Method.Substring(MaxMethodPartLength) : null,
                            r.ExecutingApp,
                            r.Pid,
                            r.Machine,
                            r.InsertedOn);
                    }
                    var param = new SqlParameter("@TableParam", table) { SqlDbType = SqlDbType.Structured };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}