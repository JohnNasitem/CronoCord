using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CronoCord.Modules;
using System.Data.SQLite;

namespace CronoCord.Utilities
{
    public static class DatabaseManagement
    {
        // Connection to db
        private static readonly SQLiteConnection _connection;
        // Sqlite result codes
        private static readonly Dictionary<int, string> _sqliteResultCodes = new Dictionary<int, string>
        {
            { 4, "SQLITE_ABORT" },
            { 23, "SQLITE_AUTH" },
            { 5, "SQLITE_BUSY" },
            { 14, "SQLITE_CANTOPEN" },
            { 19, "SQLITE_CONSTRAINT" },
            { 11, "SQLITE_CORRUPT" },
            { 101, "SQLITE_DONE" },
            { 16, "SQLITE_EMPTY" },
            { 1, "SQLITE_ERROR" },
            { 24, "SQLITE_FORMAT" },
            { 13, "SQLITE_FULL" },
            { 2, "SQLITE_INTERNAL" },
            { 9, "SQLITE_INTERRUPT" },
            { 10, "SQLITE_IOERR" },
            { 6, "SQLITE_LOCKED" },
            { 20, "SQLITE_MISMATCH" },
            { 21, "SQLITE_MISUSE" },
            { 22, "SQLITE_NOLFS" },
            { 7, "SQLITE_NOMEM" },
            { 26, "SQLITE_NOTADB" },
            { 12, "SQLITE_NOTFOUND" },
            { 27, "SQLITE_NOTICE" },
            { 0, "SQLITE_OK" },
            { 3, "SQLITE_PERM" },
            { 15, "SQLITE_PROTOCOL" },
            { 25, "SQLITE_RANGE" },
            { 8, "SQLITE_READONLY" },
            { 100, "SQLITE_ROW" },
            { 17, "SQLITE_SCHEMA" },
            { 18, "SQLITE_TOOBIG" },
            { 28, "SQLITE_WARNING" },
            { 516, "SQLITE_ABORT_ROLLBACK" },
            { 279, "SQLITE_AUTH_USER" },
            { 261, "SQLITE_BUSY_RECOVERY" },
            { 517, "SQLITE_BUSY_SNAPSHOT" },
            { 773, "SQLITE_BUSY_TIMEOUT" },
            { 1038, "SQLITE_CANTOPEN_CONVPATH" },
            { 1294, "SQLITE_CANTOPEN_DIRTYWAL" },
            { 782, "SQLITE_CANTOPEN_FULLPATH" },
            { 526, "SQLITE_CANTOPEN_ISDIR" },
            { 270, "SQLITE_CANTOPEN_NOTEMPDIR" },
            { 1550, "SQLITE_CANTOPEN_SYMLINK" },
            { 275, "SQLITE_CONSTRAINT_CHECK" },
            { 531, "SQLITE_CONSTRAINT_COMMITHOOK" },
            { 3091, "SQLITE_CONSTRAINT_DATATYPE" },
            { 787, "SQLITE_CONSTRAINT_FOREIGNKEY" },
            { 1043, "SQLITE_CONSTRAINT_FUNCTION" },
            { 1299, "SQLITE_CONSTRAINT_NOTNULL" },
            { 2835, "SQLITE_CONSTRAINT_PINNED" },
            { 1555, "SQLITE_CONSTRAINT_PRIMARYKEY" },
            { 2579, "SQLITE_CONSTRAINT_ROWID" },
            { 1811, "SQLITE_CONSTRAINT_TRIGGER" },
            { 2067, "SQLITE_CONSTRAINT_UNIQUE" },
            { 2323, "SQLITE_CONSTRAINT_VTAB" },
            { 779, "SQLITE_CORRUPT_INDEX" },
            { 523, "SQLITE_CORRUPT_SEQUENCE" },
            { 267, "SQLITE_CORRUPT_VTAB" },
            { 257, "SQLITE_ERROR_MISSING_COLLSEQ" },
            { 513, "SQLITE_ERROR_RETRY" },
            { 769, "SQLITE_ERROR_SNAPSHOT" },
            { 3338, "SQLITE_IOERR_ACCESS" },
            { 7178, "SQLITE_IOERR_AUTH" },
            { 7434, "SQLITE_IOERR_BEGIN_ATOMIC" },
            { 2826, "SQLITE_IOERR_BLOCKED" },
            { 3594, "SQLITE_IOERR_CHECKRESERVEDLOCK" },
            { 4106, "SQLITE_IOERR_CLOSE" },
            { 7690, "SQLITE_IOERR_COMMIT_ATOMIC" },
            { 6666, "SQLITE_IOERR_CONVPATH" },
            { 8458, "SQLITE_IOERR_CORRUPTFS" },
            { 8202, "SQLITE_IOERR_DATA" },
            { 2570, "SQLITE_IOERR_DELETE" },
            { 5898, "SQLITE_IOERR_DELETE_NOENT" },
            { 4362, "SQLITE_IOERR_DIR_CLOSE" },
            { 1290, "SQLITE_IOERR_DIR_FSYNC" },
            { 1802, "SQLITE_IOERR_FSTAT" },
            { 1034, "SQLITE_IOERR_FSYNC" },
            { 6410, "SQLITE_IOERR_GETTEMPPATH" },
            { 3850, "SQLITE_IOERR_LOCK" },
            { 6154, "SQLITE_IOERR_MMAP" },
            { 3082, "SQLITE_IOERR_NOMEM" },
            { 2314, "SQLITE_IOERR_RDLOCK" },
            { 266, "SQLITE_IOERR_READ" },
            { 7946, "SQLITE_IOERR_ROLLBACK_ATOMIC" },
            { 5642, "SQLITE_IOERR_SEEK" },
            { 5130, "SQLITE_IOERR_SHMLOCK" },
            { 5386, "SQLITE_IOERR_SHMMAP" },
            { 4618, "SQLITE_IOERR_SHMOPEN" },
            { 4874, "SQLITE_IOERR_SHMSIZE" },
            { 522, "SQLITE_IOERR_SHORT_READ" },
            { 1546, "SQLITE_IOERR_TRUNCATE" },
            { 2058, "SQLITE_IOERR_UNLOCK" },
            { 6922, "SQLITE_IOERR_VNODE" },
            { 778, "SQLITE_IOERR_WRITE" },
            { 262, "SQLITE_LOCKED_SHAREDCACHE" },
            { 518, "SQLITE_LOCKED_VTAB" },
            { 539, "SQLITE_NOTICE_RECOVER_ROLLBACK" },
            { 283, "SQLITE_NOTICE_RECOVER_WAL" },
            { 256, "SQLITE_OK_LOAD_PERMANENTLY" },
            { 1288, "SQLITE_READONLY_CANTINIT" },
            { 520, "SQLITE_READONLY_CANTLOCK" },
            { 1032, "SQLITE_READONLY_DBMOVED" },
            { 1544, "SQLITE_READONLY_DIRECTORY" },
            { 264, "SQLITE_READONLY_RECOVERY" },
            { 776, "SQLITE_READONLY_ROLLBACK" },
            { 284, "SQLITE_WARNING_AUTOINDEX" }
        };



        static DatabaseManagement()
        {
            try
            {
                _connection = new SQLiteConnection(@"Data Source=mydatabase.db");
                _connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem with connecting to the database: {ex.Message}");
            }
        }



        /// <summary>
        /// Reset database tables
        /// </summary>
        /// <param name="table">Table to reset</param>
        public static bool ResetTable(DatabaseTables table)
        {
            try
            {
                if (table == DatabaseTables.Events || table == DatabaseTables.All)
                {
                    // Drop table
                    using (SQLiteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = "DROP TABLE IF EXISTS events";
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Dropped events table");

                    // Create table
                    using (SQLiteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = @"CREATE TABLE IF NOT EXISTS events(
                                                ID INTEGER PRIMARY KEY AUTOINCREMENT, 
                                                CreatorID INTEGER,
                                                Name TEXT NOT NULL,
                                                Description TEXT NOT NULL,
                                                StartTimeUnix INTEGER NOT NULL,
                                                EndTimeUnix INTEGER NOT NULL,
                                                Status TEXT NOT NULL,
                                                ChannelID INTEGER NOT NULL,
                                                AlreadyRemindedOwner INTEGER NOT NULL,
                                                AlreadyRemindedParticipants INTEGER NOT NULL,
                                                AlreadyAnnounced INTEGER NOT NULL
                                                )";
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Created events table");
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"SQLite Exception - Error Code: {(_sqliteResultCodes.ContainsKey(ex.ErrorCode) ? _sqliteResultCodes[ex.ErrorCode] : "N/A")} - Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem occured in DatabaseManagement.ResetTable: {ex.Message}");
            }

            return false;
        }





        #region Events
        /// <summary>
        /// Create a new entry in the events table
        /// </summary>
        /// <param name="event">Event ot add to events table</param>
        public static bool CreateEvent(Classes.Event eventToAdd)
        {
            try
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO events (CreatorID, Name, Description, StartTimeUnix, EndTimeUnix, Status, ChannelID, AlreadyRemindedOwner, AlreadyRemindedParticipants, AlreadyAnnounced) " +
                                          "VALUES (@CreatorID, @Name, @Description, @StartTimeUnix, @EndTimeUnix, @Status, @ChannelID, @AlreadyRemindedOwner, @AlreadyRemindedParticipants, @AlreadyAnnounced)";

                    // Add parameters
                    command.Parameters.AddWithValue("@CreatorID", eventToAdd.CreatorID);
                    command.Parameters.AddWithValue("@Name", eventToAdd.Name);
                    command.Parameters.AddWithValue("@Description", eventToAdd.Description);
                    command.Parameters.AddWithValue("@StartTimeUnix", eventToAdd.StartTimeUnix);
                    command.Parameters.AddWithValue("@EndTimeUnix", eventToAdd.EndTimeUnix);
                    command.Parameters.AddWithValue("@Status", eventToAdd.Status);
                    command.Parameters.AddWithValue("@ChannelID", eventToAdd.ChannelID);
                    command.Parameters.AddWithValue("@AlreadyRemindedOwner", eventToAdd.AlreadyRemindedOwner);
                    command.Parameters.AddWithValue("@AlreadyRemindedParticipants", eventToAdd.AlreadyRemindedParticipants);
                    command.Parameters.AddWithValue("@AlreadyAnnounced", eventToAdd.AlreadyAnnounced);

                    // Execute the query
                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"SQLite Exception - Error Code: {(_sqliteResultCodes.ContainsKey(ex.ErrorCode) ? _sqliteResultCodes[ex.ErrorCode] : "N/A")} - Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem occured in DatabaseManagement.CreateEvent: {ex.Message}");
            }

            return false;
        }



        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns>List of events</returns>
        public static List<Classes.Event> GetEvents()
        {
            List<Classes.Event> events = new List<Classes.Event>();

            try
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT * from events";

                    // Execute the query
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(new Classes.Event(
                                creatorId: ulong.Parse(reader["CreatorID"].ToString()),
                                name: reader["Name"].ToString(),
                                description: reader["Description"].ToString(),
                                startTimeUnix: long.Parse(reader["StartTimeUnix"].ToString()),
                                endTimeUnix: long.Parse(reader["EndTimeUnix"].ToString()),
                                status: (Classes.Event.EventsStatuses)Enum.Parse(typeof(Classes.Event.EventsStatuses), reader["Status"].ToString()),
                                channelId: ulong.Parse(reader["ChannelID"].ToString()),
                                alreadyRemindedOwner: Convert.ToBoolean(reader["AlreadyRemindedOwner"]),
                                alreadyRemindedParticipants: Convert.ToBoolean(reader["AlreadyRemindedParticipants"]),
                                alreadyAnnounced: Convert.ToBoolean(reader["AlreadyAnnounced"])
                            ));
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"SQLite Exception - Error Code: {(_sqliteResultCodes.ContainsKey(ex.ErrorCode) ? _sqliteResultCodes[ex.ErrorCode] : "N/A")} - Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problem occured in DatabaseManagement.GetEvents: {ex.Message}");
                return null;
            }

            return events;
        }
        #endregion



        /// <summary>
        /// Database tables
        /// </summary>
        public enum DatabaseTables
        {
            All,
            Events
        }
    }
}
