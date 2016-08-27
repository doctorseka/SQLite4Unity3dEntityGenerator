using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SQLite.Linq;

namespace SqliteEntityGenerator.Classes
{
    public class DatabaseAnalyzer : IDisposable
    {
        string _dbpath = string.Empty;

        SQLiteConnection conn = null;

        public DatabaseAnalyzer(string path)
        {
            _dbpath = path;           
        }

        public bool Load()
        {
            try
            {
                conn = new SQLiteConnection(string.Format("Data Source={0}; Version=3;", _dbpath));
                conn.Open();

                if (conn.State != ConnectionState.Open)
                {
                    Console.WriteLine("Connection closed");
                    return false;
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            catch (Exception qex)
            {
                Console.WriteLine(qex.ToString());
                return false;
            }

        }

        public void Dispose()
        {
            if (conn == null)
                return;
            
            conn.Close();
            conn.Dispose();
        }

        public List<string> GetAllTables()
        {
            if (conn == null)
                return null;
            
            SQLiteCommand cmd = new SQLiteCommand("SELECT [name] FROM [sqlite_master] WHERE [type]='table' and [name] <> 'sqlite_sequence';", conn);

            try
            {
                SQLiteDataReader reader = cmd.ExecuteReader();
                List<string> line = new List<string>();
                while (reader.Read())
                {
                    line.Add((string)reader[0]);
                    Console.WriteLine(line);
                }
                reader.Close();

                return line;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<ColumnInfo> BuildTableEntity(string tableName)
        {
            if (conn == null)
                return null ;
           
            SQLiteCommand cmd = new SQLiteCommand(string.Format("PRAGMA table_info('{0}') ", tableName), conn);

            try
            {               
                SQLiteDataReader reader = cmd.ExecuteReader();

                List<ColumnInfo> columns = new List<ColumnInfo>();

                while (reader.Read())
                {
                    columns.Add(new ColumnInfo() {
                        cid = reader.GetInt32(0),
                        name = reader.GetString(1),
                        type = reader.GetString(2),
                        notnull = reader.GetInt32(3),
                        dflt_value = reader[4],
                        pk = reader.GetInt32(5),
                    });

                }

                reader.Close();

                return columns;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public string BuildClassCode(string tableName)
        {
            List<ColumnInfo> columns = BuildTableEntity(tableName);

            ColumnInfo pk_column = columns.FirstOrDefault(e => e.pk == 1);

            List<string> fields = columns
                .OrderByDescending(e => e.pk)
                .Select(ci => (ci.pk == 1 ? PK_FLAGS : string.Empty) + string.Format(FIELD_TEMPLATE, SqliteToSharp[ci.type], ci.name))
                .ToList();

            return string.Format(TEMPLATE, tableName, string.Join(string.Empty, fields));

        }

        readonly Dictionary<string, string> SqliteToSharp = new Dictionary<string, string>() {
            { "INTEGER", "int" },
            { "TEXT", "string"},
            { "REAL", "double"}
        };

        readonly string FIELD_TEMPLATE = @"    public {0} {1} {{ get; set; }}" + Environment.NewLine;

        readonly string PK_FLAGS = @"    [PrimaryKey, AutoIncrement]" + Environment.NewLine;

        readonly string TEMPLATE = @"using SQLite4Unity3d;

public class {0}  {{
{1}
}}";

    }
}
