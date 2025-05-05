using Azure.Identity;
using MBatch.Azure.Extensions.TestApi.DI;
using MBatch.TestApi;
using Microsoft.Extensions.Azure;

namespace MBatch.Azure.Extensions.TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var batchConfiguration = builder.Configuration.GetSection("Batch").Get<BatchConfiguration>();

            // Add services to the container.

            builder.Services.AddSingleton(batchConfiguration);

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.UseCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    TenantId = batchConfiguration.TenantId
                }));
            });

            builder.Services.AddMBatch(batchConfiguration.BatchAccountUrl, batchConfiguration.BatchAccountName, batchConfiguration.BatchAccountKey, batchConfiguration.SubscriptionId, batchConfiguration.ResourceGroup);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.CustomSchemaIds(s => s.FullName.Replace("+", "."));
            });

            builder.WebHost.ConfigureKestrel(options =>
                options.Limits.MaxRequestBodySize = 50 * 1024 * 1024
            );

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.Run();
        }
    }
}
