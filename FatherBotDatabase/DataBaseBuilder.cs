using Dapper;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatherBotDatabase
{
    public class DataBaseBuilder
    {
        public bool DBExist;
        public readonly string FilePath;
        public string BackupDataBaseFilepath => $"{FilePath}.{DateTime.Now.ToString("dd-MM-yyyy--HH-mm-ss")}";

        private Dictionary<string, string> ColumnsToAddToProfiles = new Dictionary<string, string>
        {
            {
                "Gots", "ALTER TABLE Profiles ADD COLUMN \"Gots\"\tINTEGER NOT NULL DEFAULT 0"
            },
            {
                "IsIgnored", "ALTER TABLE Profiles ADD COLUMN \"IsIgnored\"\tINTEGER NOT NULL DEFAULT 0"
            },
            {
                "MessagesSent", "ALTER TABLE Profiles ADD COLUMN \"MessagesSent\"\tINTEGER NOT NULL DEFAULT 0"
            }
        };

        private Dictionary<string, string> ColumnsToAddToResponses = new Dictionary<string, string>
        {
            {
                "GiveGot", "ALTER TABLE Responses ADD COLUMN \"GiveGot\"\tINTEGER NOT NULL DEFAULT 1"
            },
            {
                "Response", "ALTER TABLE Responses ADD COLUMN \"Response\"\tTEXT NOT NULL"
            },
            {
                "Enabled", "ALTER TABLE Responses ADD COLUMN \"Enabled\"\tINTEGER NOT NULL DEFAULT 1"
            }
        };

        public DataBaseBuilder(string dbFilePath)
        {
            FilePath = dbFilePath;
            DBExist = File.Exists(FilePath);
        }

        public void CreateNewDataBase()
        {
            SQLiteConnection.CreateFile(FilePath);

            using (IDbConnection cnn = new SQLiteConnection(DataAccess.LoadConnectionString()))
            {
                cnn.Execute("CREATE TABLE \"Profiles\" (" + Environment.NewLine +
                                "\t\"Id\"\tINTEGER NOT NULL UNIQUE," + Environment.NewLine +
                                "\"DiscordId\"\tINTEGER NOT NULL UNIQUE," + Environment.NewLine +
                                "\tPRIMARY KEY(\"Id\" AUTOINCREMENT)" + Environment.NewLine +
                            ")");

                cnn.Execute("CREATE TABLE \"Responses\" (" + Environment.NewLine +
                                "\t\"Id\"\tINTEGER NOT NULL UNIQUE," + Environment.NewLine +
                                "\"Trigger\"\tTEXT NOT NULL UNIQUE," + Environment.NewLine +
                                "\tPRIMARY KEY(\"Id\" AUTOINCREMENT)" + Environment.NewLine +
                             ")");

                UpdateSqlTables();
            }
        }

        public Dictionary<string, ColumnUpdate> UpdateSqlTables()
        {
            File.Copy(FilePath, BackupDataBaseFilepath);

            Dictionary<string, ColumnUpdate> Columns = new Dictionary<string, ColumnUpdate>();

            using (IDbConnection cnn = new SQLiteConnection(DataAccess.LoadConnectionString()))
            {
                foreach (var pair in ColumnsToAddToProfiles)
                {
                    string columnName = pair.Key;
                    string sql = pair.Value;

                    try
                    {
                        var num = cnn.Execute(sql);

                        Columns.Add("Profiles:" + columnName, new ColumnUpdate(null, ColumnUpdateEnum.ColumnAdded));
                    }
                    catch (SQLiteException e)
                    {
                        try
                        {
                            var q = cnn.Query($"SELECT {columnName} From Profiles");
                            Columns.Add("Profiles:" + columnName, new ColumnUpdate(null, ColumnUpdateEnum.ColumnExists));
                        }
                        catch
                        { 
                            Columns.Add("Profiles:" + columnName, new ColumnUpdate(e, ColumnUpdateEnum.ColumnError));
                            throw e;
                        }
                    }
                }

                foreach (var pair in ColumnsToAddToResponses)
                {
                    string columnName = pair.Key;
                    string sql = pair.Value;

                    try
                    {
                        var num = cnn.Execute(sql);

                        Columns.Add("Responses:" + columnName, new ColumnUpdate(null, ColumnUpdateEnum.ColumnAdded));
                    }
                    catch (SQLiteException e)
                    {
                        try
                        {
                            var q = cnn.Query($"SELECT {columnName} From Responses");
                            Columns.Add("Responses:" + columnName, new ColumnUpdate(null, ColumnUpdateEnum.ColumnExists));
                        }
                        catch
                        {
                            Columns.Add("Responses:" + columnName, new ColumnUpdate(e, ColumnUpdateEnum.ColumnError));
                            throw e;
                        }
                    }
                }
            }

            return Columns;
        }


        public struct ColumnUpdate
        {
            public ColumnUpdate(Exception exception, ColumnUpdateEnum columnUpdateEnum)
            {
                Exception = exception;
                ColumnUpdateEnum = columnUpdateEnum;
            }

            public Exception Exception;
            public ColumnUpdateEnum ColumnUpdateEnum;
        }

        public enum ColumnUpdateEnum
        {
            ColumnExists,
            ColumnAdded,
            ColumnError
        }
    }
}
