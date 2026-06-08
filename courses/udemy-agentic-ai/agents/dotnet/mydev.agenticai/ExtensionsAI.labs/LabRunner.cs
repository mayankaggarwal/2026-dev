using ExtensionsAI.labs.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExtensionsAI.labs
{
    public interface ILabRunner
    {
        Task Run(int labNumber);
    }
    public class LabRunner(IKeyedServiceProvider keyedServiceProvider): ILabRunner
    {
        public async Task Run(int labNumber)
        {
            await keyedServiceProvider.GetRequiredKeyedService<IRunner>(labNumber).Run();
        }
    }

    public static class Registrations
    {
        public static IServiceCollection RegisterLabServices(
            this IServiceCollection services,
            Action<AppConfigurations> configureAppConfigurations)
        {
            // Register lab-specific services here
            ArgumentNullException.ThrowIfNull(configureAppConfigurations, nameof(configureAppConfigurations));
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.Configure<AppConfigurations>(config =>
            {
                configureAppConfigurations.Invoke(config);
                ArgumentNullException.ThrowIfNull(config.OpenAIKey, nameof(config.OpenAIKey));
            });

            return services;
        }

        public static IServiceCollection RegisterLabServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register lab-specific services here
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            string? model = configuration["ModelName"];
            string? key = configuration["OpenAIKey"];
            services.AddSingleton(new AppConfigurations
            {
                ModelName = model,
                OpenAIKey = key
            });
            services.AddSingleton<ILabRunner, LabRunner>();
            services.AddKeyedScoped<IRunner, Day1.BenefitsIdentifier>(1);
            services.AddKeyedScoped<IRunner, Day1.OllamaExample>(2);
            services.AddKeyedScoped<IRunner, Day2.MinimalAssistant>(3);
            services.AddKeyedScoped<IRunner, Day3.MyProfileAssistant>(4);
            services.AddKeyedScoped<IRunner, Day3.MyProfileAssistantWithEvaluation>(5);

            return services;
        }
    }
}


