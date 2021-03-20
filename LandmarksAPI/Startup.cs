using LandmarksAPI.Helpers;
using LandmarksAPI.Services;
using LandmarksAPI.Services.User;
using LandmarksAPI.Services.UsersDb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using LandmarksAPI.Middleware;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using LandmarksAPI.Helpers.AppSettings;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace LandmarksAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		private AppSettings GetAppSettings()
		{
			IConfigurationSection keyVault = Configuration.GetSection("KeyVault");
			string keyVaultUri = keyVault.GetSection("Uri").Value;
			SecretClient secretClient = new SecretClient(vaultUri: new Uri(keyVaultUri), credential: new DefaultAzureCredential());
			KeyVaultSecret appSettings = secretClient.GetSecret("AppSettings");
			return JsonConvert.DeserializeObject<AppSettings>(appSettings.Value);
		}

		private static async Task<LandmarksDbService> InitializeCosmosClientInstanceAsync(CosmosDbSettings settings)
		{
			string databaseName = settings.DatabaseName;
			string containerName = settings.ContainerName;
			string account = settings.Account;
			string key = settings.Key;
			CosmosClient client = new CosmosClient(account, key);
			LandmarksDbService landmarksDbService = new LandmarksDbService(client, databaseName, containerName);
			DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/userid");

			return landmarksDbService;
		}

		private static async Task<UsersDbService> InitializeUsersCosmosClientInstanceAsync(CosmosDbSettings settings)
		{
			string databaseName = settings.DatabaseName;
			string containerName = settings.ContainerName;
			string account = settings.Account;
			string key = settings.Key;
			CosmosClient client = new CosmosClient(account, key);
			UsersDbService usersDbService = new UsersDbService(client, databaseName, containerName);
			DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/username");

			return usersDbService;
		}

		private static FourSquareService InitializeSharpSquareClientInstance(FSquareSettings settings)
		{
			string clientId = settings.ClientId;
			string clientSecret = settings.ClientSecret;
			FourSquareService fourSquareService = new FourSquareService(clientId, clientSecret);

			return fourSquareService;
		}

		private static FlickrService InitializeFlickrClientInstance(FlickrSettings settings)
		{
			string key = settings.Key;
			FlickrService flickrService = new FlickrService(key);

			return flickrService;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			AppSettings settings = GetAppSettings();

			services.AddCors();

			services.AddControllers();

			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			services.AddSingleton<ILandmarksDbService>(InitializeCosmosClientInstanceAsync(settings.LandmarksDb).GetAwaiter().GetResult());

			services.AddSingleton<IUsersDbService>(InitializeUsersCosmosClientInstanceAsync(settings.UsersDb).GetAwaiter().GetResult());

			services.AddSingleton<IFourSquareService>(InitializeSharpSquareClientInstance(settings.FourSquare));

			services.AddSingleton<IFlickrService>(InitializeFlickrClientInstance(settings.Flickr));

			services.AddSingleton(settings.AuthSettings);

			services.AddDistributedRedisCache(options =>
			{
				string redisConnectionString = settings.Redis.ConnectionString;
				options.Configuration = redisConnectionString;
			});

			// configure jwt authentication
			AuthSettings authSettings = settings.AuthSettings;
			byte[] key = Encoding.ASCII.GetBytes(authSettings.Secret);
			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(x =>
			{
				x.Events = new JwtBearerEvents
				{
					OnTokenValidated = context =>
					{
						var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
						var userId = context.Principal.Identity.Name;
						var user = userService.GetById(userId);
						if (user == null)
						{
							// return unauthorized if user no longer exists
							context.Fail("Unauthorized");
						}
						return Task.CompletedTask;
					}
				};
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			// configure DI for application services
			services.AddScoped<IUserService, UserService>();
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

			app.UseCors(x => x
				.SetIsOriginAllowed(origin => true)
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials());

			app.UseMiddleware<JwtMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
