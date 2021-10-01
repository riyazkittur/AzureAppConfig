using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace AzureAppConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            //Create  App Configuration in Azure --> Configuration explorer-->Create
            //To read Azure App config , copy the Connection String from Settings-->Access keys-->ConnectionString
            //Save the connection string  which is accessible(for e.g. environment variable in my case)
            builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AzureAppConfigConnectionString"));

            //now build the configuration object
            var config = builder.Build();

            //access keys
            var key = "DempApp:Name";
            Console.WriteLine($"Value of key {key} in Azure App Configuration is {config[key]}");

            Console.ReadLine();



        }
    }
}
