using System;
using System.Threading.Tasks;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace AzureAppConfig
{
    class Program
    {
        static IConfigurationRefresher _refresher;
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            //Create  App Configuration in Azure --> Configuration explorer-->Create
            //To read Azure App config , copy the Connection String from Settings-->Access keys-->ConnectionString
            //Save the connection string  which is accessible(for e.g. environment variable in my case)

            var appConfigConnectionString = Environment.GetEnvironmentVariable("AzureAppConfigConnectionString");
            builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(appConfigConnectionString)
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register("DemoApp:Name")
                    .SetCacheExpiration(TimeSpan.FromSeconds(10));
                });
                _refresher = options.GetRefresher();
            });

            //now build the configuration object
            var config = builder.Build();

            //access keys
            var key = "DemoApp:Name";
            var retryCount = 6;
            var start = 0;
            while(start <retryCount)
            {
                Console.WriteLine($"Attempt :{start} Value of key {key} in Azure App Configuration is {config[key]}");
                start+=1;
                await Task.Delay(4000);

                if(start == 4)
                {
                    await _refresher.TryRefreshAsync();
                    Console.WriteLine("Config refreshed");
                }
            }

            var configClient = new ConfigurationClient(appConfigConnectionString);

            var dacConfigKey = $"DemoApp:TestKey";
            var dacAppConfigSetting = new ConfigurationSetting(dacConfigKey, "TestValue");
            await configClient.AddConfigurationSettingAsync(dacAppConfigSetting, default);

            Console.ReadLine();



        }
    }
}
