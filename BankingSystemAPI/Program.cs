
using DataAccessEF.Data;
using DataAccessEF.Repository;
using DataAccessEF.UnitOfWork;
using Domain.CustomPolicy;
using Domain.EmailService;
using Domain.Mapper;
using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace BankingSystemAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);




			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			//	builder.Services.AddSwaggerGen();
			// Caching 
			builder.Services.AddResponseCaching();

			// Register Service For AutoMapper => Convert From Dto Class To Main Class 
			builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo", Version = "v1" });
			});
			builder.Services.AddSwaggerGen(swagger =>
			{
				//This is to generate the Default UI of Swagger Documentation    
				swagger.SwaggerDoc("v2", new OpenApiInfo
				{
					Version = "v1",
					Title = "ASP.NET 7 Web API",
					Description = " ITI Projrcy"
				});

				// To Enable authorization using Swagger (JWT)    
				swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
				});
				swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
					new OpenApiSecurityScheme
					{
					Reference = new OpenApiReference
					{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
					}
					},
					new string[] {}
					}
				});
			});
			// Connection String
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{

				options.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
			});
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBranchRepository, BranchRepository>();


            // Service For Email
            builder.Services.AddScoped<IEmailService, EmailService>();
			builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
			builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
			})
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddRoles<IdentityRole>()
		.AddDefaultTokenProviders();

			// Custom Policy
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("EmployeeOnly", builder =>
				{
					builder.RequireClaim("UserType", "Employee");
				});
			});

			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("EmployeeMaleOnly", builder =>
				{
					builder.AddRequirements(new EmployeeMaleOnlyRequirement());
				});
			});
			builder.Services.AddScoped<IAuthorizationHandler, EmployeeAuthorizationHandler>();
			/*   builder.Services.AddAuthorization(options =>
			   {
				   options.AddPolicy("EmployeeMaleOnly", builder =>
				   {
					   builder.RequireAssertion(context =>
					   {
						   var gender = context.User.FindFirstValue("UserGender");
						   if (gender == "Male")

							   return true;

						   return false;
					   });
				   });
			   });*/

			// [Authoriz] used JWT Token in Chck Authantiaction
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

			}).AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateIssuer = true,
					ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
					ValidateAudience = true,
					ValidAudience = builder.Configuration["JWT:ValiedAudiance"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
				};
			});

			// Configure Rate Limiting
			builder.Services.AddRateLimiter(options =>
			{
				options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(content =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: content.Request.Headers.Host.ToString(),
					factory: Partition => new FixedWindowRateLimiterOptions
					{
						AutoReplenishment = true,
						PermitLimit = 5,
						QueueLimit = 0,
						Window = TimeSpan.FromSeconds(10)
					}
			 ));
				options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			});
			builder.Services.AddMemoryCache();

			// read values for serilog from appsetting 
			var config = new ConfigurationBuilder()
			   .AddJsonFile("appsettings.json")
			   .Build();
			Log.Logger = new LoggerConfiguration()
			   .ReadFrom.Configuration(config)
			   .CreateLogger();
			try
			{
                Log.Information("Application Starting");


                builder.Host.UseSerilog(); // Use Serilog

                var app = builder.Build();

				// Configure the HTTP request pipeline.
				if (app.Environment.IsDevelopment())
				{
					app.UseSwagger();
					app.UseSwaggerUI();
				}
				app.UseRouting();

				// Configure rate limiting middleware
				app.UseRateLimiter();

				app.UseStaticFiles();

				app.UseCors();

				app.UseResponseCaching();
				app.UseHttpsRedirection();

				app.UseAuthentication(); //  Check JWT Token
				app.UseAuthorization();

				app.MapControllers();

				app.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "The application failed to start!");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
