using LandmarksAPI.Services;
using LandmarksAPI.Services.UsersDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string databaseName = configurationSection.GetSection("DatabaseName").Value;
			string containerName = configurationSection.GetSection("ContainerName").Value;
			string account = configurationSection.GetSection("Account").Value;
			string key = configurationSection.GetSection("Key").Value;
			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/city");

			return cosmosDbService;
		}

		private static async Task<UsersDbService> InitializeUsersCosmosClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string databaseName = configurationSection.GetSection("DatabaseName").Value;
			string containerName = configurationSection.GetSection("ContainerName").Value;
			string account = configurationSection.GetSection("Account").Value;
			string key = configurationSection.GetSection("Key").Value;
			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			UsersDbService cosmosDbService = new UsersDbService(client, databaseName, containerName);
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/username");

			return cosmosDbService;
		}

		private static async Task<FourSquareService> InitializeSharpSquareClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string clientId = configurationSection.GetSection("ClientId").Value;
			string clientSecret = configurationSection.GetSection("ClientSecret").Value;
			FourSquareService fourSquareService = new FourSquareService(clientId, clientSecret);

			return fourSquareService;
		}

		private static async Task<FlickrService> InitializeFlickrClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string key = configurationSection.GetSection("Key").Value;
			FlickrService flickrService = new FlickrService(key);

			return flickrService;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());

			services.AddSingleton<IUsersDbService>(InitializeUsersCosmosClientInstanceAsync(Configuration.GetSection("UsersDb")).GetAwaiter().GetResult());

			services.AddSingleton<IFourSquareService>(InitializeSharpSquareClientInstanceAsync(Configuration.GetSection("FourSquare")).GetAwaiter().GetResult());

			services.AddSingleton<IFlickrService>(InitializeFlickrClientInstanceAsync(Configuration.GetSection("Flickr")).GetAwaiter().GetResult());
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
