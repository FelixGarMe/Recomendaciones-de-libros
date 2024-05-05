using System;
using System.Threading;
using StackExchange.Redis;
using Npgsql;

namespace Worker
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var redisConn = OpenRedisConnection();
                var redis = redisConn.GetDatabase();

                var redisKeys = redis.Multiplexer.GetServer("my-redis-container", 6379).Keys(pattern: "*");

                foreach (var redisKey in redisKeys)
                {
                    string bookName = redisKey.ToString();
                    string bookList = redis.StringGet(redisKey);
                    Console.WriteLine($"Processing book '{bookName}'");
                    SaveToPostgres(bookName, bookList);
                }

                while (true)
                {
                    Thread.Sleep(100);

                    if (redisConn == null || !redisConn.IsConnected)
                    {
                        Console.WriteLine("Reconnecting Redis");
                        redisConn = OpenRedisConnection();
                        redis = redisConn.GetDatabase();
                    }
                    RedisValue value = redis.ListLeftPop("books");
                    if (!value.IsNull)
                    {
                        string bookName = value.ToString();
                        string bookList = redis.StringGet(bookName);
                        Console.WriteLine($"Processing book '{bookName}'");
                        SaveToPostgres(bookName, bookList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static void SaveToPostgres(string bookName, string bookList)
        {
        var connectionString = "Host=my_postgres_container;Username=postgres;Password=password;Database=recomendaciones_libros";
            Console.WriteLine($"Connecting to PostgreSQL at {connectionString}");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Connected to PostgreSQL");

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO books (book_name, book_list) VALUES (@bookName, @bookList)";
                        cmd.Parameters.AddWithValue("@bookName", bookName);
                        cmd.Parameters.AddWithValue("@bookList", bookList);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Data inserted successfully for book '{bookName}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error connecting to PostgreSQL: {ex.Message}");
                }
            }
        }


        private static ConnectionMultiplexer OpenRedisConnection()
        {
            var options = ConfigurationOptions.Parse("my-redis-container");
            options.ConnectTimeout = 5000;

            while (true)
            {
                try
                {
                    Console.WriteLine("Connecting to Redis");
                    var redisConn = ConnectionMultiplexer.Connect(options);
                    Console.WriteLine("Connected to Redis");
                    return redisConn;
                }
                catch (RedisConnectionException)
                {
                    Console.WriteLine("Waiting for Redis");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
