﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DbTargets
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if ((args.Length < 1) || (args.Length > 2))
            {
                ShowUsage("Incorrect number of arguments.");
                return -1;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                ShowUsage("Server name cannot be empty.");
                return -2;
            }

            var serverName = args[0];
            var regexp = (string)null;

            if (args.Length > 1)
            {
                regexp = args[1];

                if (!IsValidRegex(regexp))
                {
                    ShowUsage("Invalid regular expression.");
                    return -3;
                }
            }

            try
            {
                var databaseNames = QueryDatabaseNames(serverName);
                var filteredDatabaseNames = FilterDatabases(databaseNames, regexp);
                var content = GenerateJsonTargets(serverName, filteredDatabaseNames);

                using (var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true})
                {
                    Console.SetOut(sw);
                    sw.Write(content);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unexpected error.");
                Console.Error.WriteLine(e);
                return -4;
            }
        }

        private static void ShowUsage(string reasonMessage)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"DbTargets {version}");
            Console.WriteLine($"Error : {reasonMessage}");
            Console.WriteLine("Usage : DbTargets <servername> [regexp]");
        }

        private static bool IsValidRegex(string regexp)
        {
            try
            {
                var regex = new Regex(regexp, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static List<string> QueryDatabaseNames(string serverName)
        {
            var titleAttribute = (AssemblyTitleAttribute) Assembly.GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyTitleAttribute));

            var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = "master",
                ConnectTimeout = 5,

                IntegratedSecurity = true,
                WorkstationID = Environment.MachineName,
                ApplicationName = titleAttribute.Title,

                ApplicationIntent = Microsoft.Data.SqlClient.ApplicationIntent.ReadWrite,
                //NetworkLibrary = "dbmssocn",
                Pooling = false,
                Authentication = SqlAuthenticationMethod.NotSpecified
            };

            var databaseNames = new List<string>(500);

            using (var connection = new System.Data.SqlClient.SqlConnection(connectionStringBuilder.ToString()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT name AS DatabaseName
FROM sys.databases WITH(NOLOCK)
WHERE database_id > 4 -- User databases
ORDER BY name
";
                    command.CommandTimeout = 10;

                    using (var reader = command.ExecuteReader())
                    {
                        var fieldCount = reader.FieldCount;

                        if (fieldCount != 1)
                        {
                            throw new InvalidOperationException("Unexpected number of columns.");
                        }

                        var type = reader.GetFieldType(0);

                        if (type != typeof(string))
                        {
                            throw new InvalidOperationException("Invalid column type.");
                        }

                        while (reader.Read())
                        {
                            var itemArray = new object[fieldCount];
                            reader.GetValues(itemArray);
                            var databaseName = itemArray[0];
                            databaseNames.Add((string)databaseName);
                        }

                        reader.Close();
                    }
                }

                connection.Close();
            }

            return databaseNames;
        }
        
        private static List<string> FilterDatabases(List<string> databaseNames, string regexp)
        {
            if (string.IsNullOrWhiteSpace(regexp))
            {
                return databaseNames;
            }

            var regex = new Regex(regexp, RegexOptions.IgnoreCase);
            var filteredDatabases = databaseNames.Where(d => regex.IsMatch(d)).ToList();

            return filteredDatabases;
        }
        
        private static string GenerateJsonTargets(string serverName, List<string> filteredDatabaseNames)
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"{");
            sb.AppendLine("\t\"DatabaseList\": [");

            for (var i = 0; i < filteredDatabaseNames.Count; i++)
            {
                var filteredDatabaseName = filteredDatabaseNames[i];
                sb.Append($"\t\t{{ \"ServerName\": \"{serverName}\", \"DatabaseName\": \"{filteredDatabaseName}\" }}");

                if (i < filteredDatabaseNames.Count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            sb.AppendLine("\t]");
            sb.AppendLine(@"}");

            return sb.ToString();
        }
    }
}
