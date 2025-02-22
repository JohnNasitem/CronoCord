//***********************************************************************************
//Program: DatabaseManagement.cs
//Description: Methods to access the database
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Data.SQLite;
using CronoCord.Classes;

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
                UtilityMethods.PrettyConsoleWriteLine($"Problem with connecting to the database: {ex.Message}", UtilityMethods.LogLevel.Critical);
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
                if (table == DatabaseTables.Availabilities || table == DatabaseTables.All)
                {
                    // Drop table
                    using (SQLiteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = "DROP TABLE IF EXISTS availabilities";
                        command.ExecuteNonQuery();
                    }
                    UtilityMethods.PrettyConsoleWriteLine("Dropped availabilities table", UtilityMethods.LogLevel.Info);

                    // Create table
                    using (SQLiteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = @"CREATE TABLE IF NOT EXISTS availabilities(
                                                ID INTEGER PRIMARY KEY AUTOINCREMENT, 
                                                UserID INTEGER NOT NULL,
                                                StartTimeUnix INTEGER NOT NULL,
                                                EndTimeUnix INTEGER NOT NULL,
                                                Recurring TEXT NOT NULL
                                                )";
                        command.ExecuteNonQuery();
                    }
                    UtilityMethods.PrettyConsoleWriteLine("Created availabilities table", UtilityMethods.LogLevel.Info);
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.ResetTable: {ex.Message}", UtilityMethods.LogLevel.Error);
            }

            return false;
        }




        #region Availabilities
        /// <summary>
        /// Add a new entry to the availabilities table
        /// </summary>
        /// <param name="availabilityDetails">availability details</param>
        /// <returns>sucess</returns>
        public static bool CreateAvailability(Availability availabilityDetails)
        {
            try
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO availabilities(UserID, StartTimeUnix, EndTimeUnix, Recurring) " +
                                          "VALUES(@UserID, @StartTimeUnix, @EndTimeUnix, @Recurring)";

                    // Add parameters
                    command.Parameters.AddWithValue("@UserID", availabilityDetails.UserID);
                    command.Parameters.AddWithValue("@StartTimeUnix", availabilityDetails.StartTimeUnix);
                    command.Parameters.AddWithValue("@EndTimeUnix", availabilityDetails.EndTimeUnix);
                    command.Parameters.AddWithValue("@Recurring", Enum.GetName(typeof(Availability.Recurring), availabilityDetails.IsRecurring));

                    // Execute the query
                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.CreateAvailability: {ex.Message}", UtilityMethods.LogLevel.Error);
            }

            return false;
        }



        /// <summary>
        /// Get availabilities for speciied users
        /// </summary>
        /// <param name="userIDs">list of specific user ids to find, if left null it will get all users</param>
        /// <returns>List of Availability</returns>
        public static List<Availability> GetAvailabilites(List<ulong> userIDs = null)
        {
            List<Availability> availabilities = new List<Availability>();

            try
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    // Get everyones availability if userIDs is null
                    // else get specified availabilities
                    if (userIDs == null)
                        command.CommandText = "SELECT * from availabilities";
                    else
                        command.CommandText = $"SELECT * from availabilities where UserID in ({string.Join(",", userIDs)})";

                    // Execute the query
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availabilities.Add(new Availability(
                                userId: ulong.Parse(reader["UserID"].ToString()),
                                startTimeUnix: long.Parse(reader["StartTimeUnix"].ToString()),
                                endTimeUnix: long.Parse(reader["EndTimeUnix"].ToString()),
                                recurring: (Availability.Recurring)Enum.Parse(typeof(Availability.Recurring), reader["Recurring"].ToString())
                            ));
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
                return null;
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.GetAvailabilites: {ex.Message}", UtilityMethods.LogLevel.Error);
                return null;
            }

            return availabilities;
        }



        /// <summary>
        /// Edit an existing entry in the availability table
        /// </summary>
        /// <param name="oldAvailabiltiy">entry to replace</param>
        /// <param name="newAvailability">new details to use</param>
        /// <returns>how many have been editted</returns>
        public static int EditAvailability(Availability oldAvailabiltiy, Availability newAvailability)
        {
            try
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "UPDATE availabilities " +
                                          "SET StartTimeUnix = @NewStartTimeUnix, EndTimeUnix = @NewEndTimeUnix, Recurring = @NewRecurring " +
                                          "WHERE UserID = @UserID AND StartTimeUnix = @StartTimeUnix AND EndTimeUnix = @EndTimeUnix AND Recurring = @Recurring";

                    // Old values
                    command.Parameters.AddWithValue("@UserID", oldAvailabiltiy.UserID);
                    command.Parameters.AddWithValue("@StartTimeUnix", oldAvailabiltiy.StartTimeUnix);
                    command.Parameters.AddWithValue("@EndTimeUnix", oldAvailabiltiy.EndTimeUnix);
                    command.Parameters.AddWithValue("@Recurring", Enum.GetName(typeof(Availability.Recurring), oldAvailabiltiy.IsRecurring));

                    // New values
                    command.Parameters.AddWithValue("@NewStartTimeUnix", newAvailability.StartTimeUnix);
                    command.Parameters.AddWithValue("@NewEndTimeUnix", newAvailability.EndTimeUnix);
                    command.Parameters.AddWithValue("@NewRecurring", Enum.GetName(typeof(Availability.Recurring), newAvailability.IsRecurring));

                    // Execute the query
                    int affectedRows = command.ExecuteNonQuery();

                    return affectedRows;
                }
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.EditAvailability: {ex.Message}", UtilityMethods.LogLevel.Error);
            }

            return -1;
        }



        /// <summary>
        /// Deletes an entry from the availability table
        /// </summary>
        /// <param name="availabilityToDelete">entry to delete</param>
        /// <returns>how many have been deleted</returns>
        public static int DeleteAvailability(Availability availabilityToDelete)
        {
            try
            {
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM availabilities " +
                                          "WHERE UserID = @UserID AND StartTimeUnix = @StartTimeUnix AND EndTimeUnix = @EndTimeUnix AND Recurring = @Recurring;";

                    // Add parameters
                    command.Parameters.AddWithValue("@UserID", availabilityToDelete.UserID);
                    command.Parameters.AddWithValue("@StartTimeUnix", availabilityToDelete.StartTimeUnix);
                    command.Parameters.AddWithValue("@EndTimeUnix", availabilityToDelete.EndTimeUnix);
                    command.Parameters.AddWithValue("@Recurring", Enum.GetName(typeof(Availability.Recurring), availabilityToDelete.IsRecurring));

                    // Execute the query
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected;
                }
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.DeleteAvailability: {ex.Message}", UtilityMethods.LogLevel.Error);
            }

            return -1;
        }




        /// <summary>
        /// Deletes all entries that dont repeat and are before the unixThreshold
        /// </summary>
        /// <param name="unixThreshold">any endtimes before this threshold will be deleted</param>
        /// <returns>how many have been deleted</returns>
        public static int CleanUpAvailabilities(long unixThreshold)
        {
            try
            {
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM availabilities " +
                                          $"WHERE EndTimeUnix < @EndTimeUnix AND Recurring = {Enum.GetName(typeof(Availability.Recurring), Availability.Recurring.N)}";

                    // Add parameters
                    command.Parameters.AddWithValue("@EndTimeUnix", unixThreshold);

                    // Execute the query
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected;
                }
            }
            catch (SQLiteException ex)
            {
                LogSQLiteException(ex);
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in DatabaseManagement.CleanUpAvailabilities: {ex.Message}", UtilityMethods.LogLevel.Error);
            }

            return -1;
        }
        #endregion



        /// <summary>
        /// Log SQLiteExceptions
        /// </summary>
        /// <param name="ex">exception</param>
        private static void LogSQLiteException(SQLiteException ex)
        {
            UtilityMethods.PrettyConsoleWriteLine($"SQLite Exception - Error Code: {(_sqliteResultCodes.ContainsKey(ex.ErrorCode) ? _sqliteResultCodes[ex.ErrorCode] : "N/A")} - Error: {ex.Message}", UtilityMethods.LogLevel.Error);
        }



        /// <summary>
        /// Database tables
        /// </summary>
        public enum DatabaseTables
        {
            All,
            Availabilities
        }
    }
}
