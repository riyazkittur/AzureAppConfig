using System;
using System.Threading.Tasks;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

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
                })
                .UseFeatureFlags();
                _refresher = options.GetRefresher();
            });

            //now build the configuration object
            var config = builder.Build();
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config).AddFeatureManagement();

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

            var dacConfigKey = $"DemoApp:TestKey{Guid.NewGuid()}";
            var dacAppConfigSetting = new ConfigurationSetting(dacConfigKey, "TestValue");
            await configClient.AddConfigurationSettingAsync(dacAppConfigSetting, default);


            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                IFeatureManager featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

                if (await featureManager.IsEnabledAsync("Beta"))
                {
                    Console.WriteLine("Welcome to the beta!");
                }
            }
            Console.WriteLine("Press any KEY to stop");

            Console.ReadLine();



        }
    }
}
