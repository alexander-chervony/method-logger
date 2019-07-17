using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Configuration;

namespace CodeCleaner
{
    public interface ICodeCleanupRepository
    {
        IEnumerable<string> GetLoggedMethods();
    }

    public class CodeCleanupRepository : ICodeCleanupRepository
    {
        private readonly Action<string> _log;

        public CodeCleanupRepository(Action<string> log)
        {
            _log = log;
        }

        public IEnumerable<string> GetLoggedMethods()
        {
            _log("Started CodeCleanupRepository.GetLoggedMethods()");
            string selectClause = string.Format("select method + ISNULL(method_part2, '') from LoggedMethods order by method");
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand(selectClause, conn) { CommandType = CommandType.Text })
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (string)reader[0];
                    }
                }
            }
            _log("Ended CodeCleanupRepository.GetLoggedMethods()");
        }

        private string GetConnectionString()
        {
            var config = new Config(new Logger());
            return config.ConnectionString;
        }
    }
}