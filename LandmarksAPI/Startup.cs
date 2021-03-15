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

namespace LandmarksAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		private static async Task<LandmarksDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string databaseName = configurationSection.GetSection("DatabaseName").Value;
			string containerName = configurationSection.GetSection("ContainerName").Value;
			string account = configurationSection.GetSection("Account").Value;
			string key = configurationSection.GetSection("Key").Value;
			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			LandmarksDbService landmarksDbService = new LandmarksDbService(client, databaseName, containerName);
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/userid");

			return landmarksDbService;
		}

		private static async Task<UsersDbService> InitializeUsersCosmosClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string databaseName = configurationSection.GetSection("DatabaseName").Value;
			string containerName = configurationSection.GetSection("ContainerName").Value;
			string account = configurationSection.GetSection("Account").Value;
			string key = configurationSection.GetSection("Key").Value;
			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			UsersDbService usersDbService = new UsersDbService(client, databaseName, containerName);
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/username");

			return usersDbService;
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
			services.AddCors();

			services.AddControllers();

			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			services.AddSingleton<ILandmarksDbService>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("LandmarksDb")).GetAwaiter().GetResult());

			services.AddSingleton<IUsersDbService>(InitializeUsersCosmosClientInstanceAsync(Configuration.GetSection("UsersDb")).GetAwaiter().GetResult());

			services.AddSingleton<IFourSquareService>(InitializeSharpSquareClientInstanceAsync(Configuration.GetSection("FourSquare")).GetAwaiter().GetResult());

			services.AddSingleton<IFlickrService>(InitializeFlickrClientInstanceAsync(Configuration.GetSection("Flickr")).GetAwaiter().GetResult());

			var appSettingsSection = Configuration.GetSection("AppSettings");
			services.Configure<AppSettings>(appSettingsSection);

			services.AddDistributedRedisCache(options =>
			{
				IConfigurationSection redisSetting = Configuration.GetSection("Redis");
				string redisConnectionString = redisSetting.GetSection("ConnectionString").Value;
				options.Configuration = redisConnectionString;
			});

			// configure jwt authentication
			var appSettings = appSettingsSection.Get<AppSettings>();
			var key = Encoding.ASCII.GetBytes(appSettings.Secret);
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
