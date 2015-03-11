using System;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Transactions;
using Hosting.Contracts;

namespace Client {
    class Program {
        static void Main(string[] args) {
            var client = CreateWorkflowClient();

            // WRONG WAY
            Execute(() => {
                using (var context = new TransactionScope()) {
                    using (var connection = CreateSqlConnection()) {
                        // Start workflow
                        string instanceId = client.Initiate();

                        // Opening the connection after workflow start
                        // leads to a race condition that may cause errors
                        connection.Open();

                        SaveInstanceId(connection, instanceId);
                    }

                    context.Complete();
                }
            }, 10, "OPTION-1");

            // RIGHT WAY
            Execute(() => {
                using (var context = new TransactionScope()) {
                    using (var connection = CreateSqlConnection()) {
                        // Opening the connection before workflow start
                        // eliminates the race condition
                        connection.Open();

                        // Start workflow
                        string instanceId = client.Initiate();

                        SaveInstanceId(connection, instanceId);
                    }

                    context.Complete();
                }
            }, 10, "OPTION-2");
        }

        static void Execute(Action action, int count, string sample) {
            int failures = 0;

            Console.WriteLine(sample);

            for (int i = 0; i < count; i++) {
                try {
                    action();
                } catch (Exception exception) {
                    failures++;

                    Console.WriteLine(exception.Message);
                }
            }

            if (failures > 0) {
                Console.WriteLine("Failed {0} attempts in {1}.", failures, count);
            } else {
                Console.WriteLine("All attempts passed.");
            }

            Console.WriteLine(new string('=', 80));
            Console.WriteLine();
        }

        static SqlConnection CreateSqlConnection() {
            return new SqlConnection(
                ConfigurationManager.ConnectionStrings["Master"].ConnectionString);
        }

        static ITestService CreateWorkflowClient() {
            var binding = new WSHttpBinding {
                TransactionFlow = true,
                ReliableSession = new OptionalReliableSession { Enabled = true },
                Security = new WSHttpSecurity { Mode = SecurityMode.None },
            };

            var factory = new ChannelFactory<ITestService>(binding);

            string address = ConfigurationManager.AppSettings["WorkflowServiceAddress"];

            return factory.CreateChannel(new EndpointAddress(address));
        }

        static void SaveInstanceId(SqlConnection connection, string id) {
            var command = connection.CreateCommand();

            command.CommandText = "SELECT @id";
            command.Parameters.Add(new SqlParameter("@id", id));

            command.ExecuteNonQuery();
        }
    }
}
