using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace HReader.Core.Storage.Queries
{
    internal static class SqliteExtensions
    {
        public static SqliteCommand CreateCommand(this SqliteConnection @this, string commandText)
        {
            var cmd = @this.CreateCommand();
            cmd.CommandText = commandText;
            return cmd;
        }

        /// <summary>
        /// Sets a parameter with value and returns the command
        /// </summary>
        public static SqliteCommand WithParameter(this SqliteCommand @this, string name, object value)
        {
            @this.Parameters.AddWithValue(name, value);
            return @this;
        }

        public static SqliteParameter CreateParameter(this SqliteCommand @this, string name)
        {
            var param = @this.CreateParameter();
            param.ParameterName = name;
            @this.Parameters.Add(param);
            return param;
        }

        public static async Task<T> ConsumeScalarAsync<T>(this SqliteCommand @this)
        {
            using (@this)
            {
                var result = await @this.ExecuteScalarAsync();
                return (T) result;
            }
        }

        public static async Task<int> ConsumeNonQueryAsync(this SqliteCommand @this)
        {
            using (@this)
            {
                return await @this.ExecuteNonQueryAsync();
            }
        }

        public static async Task<T> ExecuteScalarAsync<T>(this SqliteCommand @this)
        {
            return (T) await @this.ExecuteScalarAsync();
        }
    }
}
