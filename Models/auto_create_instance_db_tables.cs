using System.Data.SqlClient;
using System.Diagnostics;

namespace POE_MVC_part1.Models
{
    public class auto_create_instance_db_tables
    {
        private string instanceName = "Contract_Monthly_Claim_System";
        private string databaseName = "ContractMonthlyClaimSystem";
        private string connectionStringToInstance => $@"Server=(localdb)\{instanceName};Integrated Security=true;";
        private string connectionStringToDatabase => $@"Server=(localdb)\{instanceName};Database={databaseName};Integrated Security=true;";




        public void InitializeSystem()
        {
            try
            {
                //  Check and create LocalDB instance
                CreateClaimSystemInstance();

                //  Check and create Database
                CreateDatabase();

                // Check and create Tables
                CreateTables();

                Console.WriteLine(" LocalDB instance, database, and tables verified successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error initializing system: {ex.Message}");
            }
        }

        // -----------------------------
        // LocalDB Instance Handling
        // -----------------------------
        private void CreateClaimSystemInstance()
        {
            if (CheckInstanceExists())
            {
                Console.WriteLine($" LocalDB instance '{instanceName}' already exists.");
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sqllocaldb create \"{instanceName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                    Console.WriteLine($" LocalDB instance '{instanceName}' created successfully!");
                else
                    Console.WriteLine($" Error creating instance: {error}");
            }
        }

        private bool CheckInstanceExists()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sqllocaldb info \"{instanceName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error) &&
                    error.Contains($"LocalDB instance \"{instanceName}\" doesn't exist", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return !string.IsNullOrWhiteSpace(output)
                    && !output.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase);
            }
        }


        // -----------------------------
        // Database Handling
        // -----------------------------
        private void CreateDatabase()
        {
            string createDbQuery = $@"
         IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
         BEGIN
             CREATE DATABASE [{databaseName}];
         END";

            using (var connection = new SqlConnection(connectionStringToInstance))
            {
                connection.Open();
                using (var command = new SqlCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine($" Database '{databaseName}' verified or created.");
        }

        // -----------------------------
        //  Table Handling
        // -----------------------------
        private void CreateTables()
        {
            string createUsersTable = @"
         IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
         BEGIN
             CREATE TABLE Users (
                 userID INT PRIMARY KEY IDENTITY(1,1),
                 full_names VARCHAR(100),
                 surname VARCHAR(100),
                 email VARCHAR(100),
                 role VARCHAR(100),
                 gender VARCHAR(100),
                 password VARCHAR(100),
                 date DATE
             );
         END";

            string createClaimsTable = @"
         IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Claims' AND xtype='U')
         BEGIN
             CREATE TABLE Claims (
                 claimID INT PRIMARY KEY IDENTITY(1,1),
                 number_of_sessions INT,
                 number_of_hours INT,
                 amount_of_rate INT,
                 module_name VARCHAR(100),
                 faculty_name VARCHAR(100),
                 supporting_documents VARCHAR(100),
                 claim_status VARCHAR(100),
                 creating_date DATE,
                 lecturerID INT,
                 FOREIGN KEY (lecturerID) REFERENCES Users(userID)
             );
         END";

            using (var connection = new SqlConnection(connectionStringToDatabase))
            {
                connection.Open();

                using (var cmd = new SqlCommand(createUsersTable, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new SqlCommand(createClaimsTable, connection))
                    cmd.ExecuteNonQuery();
            }

            Console.WriteLine(" Tables 'Users' and 'Claims' verified or created.");
        }


    }

}
