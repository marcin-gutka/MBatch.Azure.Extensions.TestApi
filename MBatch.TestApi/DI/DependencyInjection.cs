using Azure.ResourceManager;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Extensions.Azure;

namespace MBatch.Azure.Extensions.TestApi.DI
{
    public static class DependencyInjection
    {
        public static void AddMBatch(this IServiceCollection services, string batchAccountUrl, string batchAccountName, string batchAccountKey, string subscriptionId, string resourceGroup)
        {
            ConfigureBatchClient(services, batchAccountUrl, batchAccountName, batchAccountKey);
            ConfigureAzureClients(services, subscriptionId);

            services.AddSingleton(sp =>
            {
                var armClient = sp.GetRequiredService<ArmClient>();

                return armClient.GetBatchAccountResourceAsync(subscriptionId, resourceGroup, batchAccountName).Result;
            });
        }

        private static void ConfigureBatchClient(IServiceCollection services, string batchAccountUrl, string batchAccountName, string batchAccountKey)
        {
            services.AddSingleton(sp => BatchClient.Open(new BatchSharedKeyCredentials(batchAccountUrl, batchAccountName, batchAccountKey)));
        }

        private static void ConfigureAzureClients(IServiceCollection services, string subscriptionId)
        {
            services.AddAzureClients((x) =>
            {
                x.AddArmClient(subscriptionId);
            });
        }
    }
}
