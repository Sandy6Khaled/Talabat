using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.Data.Config;
using Talabat.Repository.Identity;
using Talabat.Repository.Repository;
using Talabat.Repository.Repository.Implmentation;
using Talabat.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Talabat.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services
            // Add services to the container.
            builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            builder.Services.AddScoped(typeof(IPaymentService), typeof(PaymentService));
            builder.Services.AddScoped(typeof(IProductService), typeof(ProductService));
            builder.Services.AddScoped(typeof(IOrderService), typeof(OrderService));
            builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            builder.Services.AddControllers(); // API Services
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<StoreContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddDbContext<AppIdentityDbContext>(optionBuilder =>
            {
                optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });
            builder.Services.AddSingleton<IConnectionMultiplexer>(S =>
            {
                var connection = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connection);
            });
            builder.Services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //builder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfiles()));
            builder.Services.AddAutoMapper(typeof(MappingProfiles));

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                                                         .SelectMany(P => P.Value.Errors)
                                                         .Select(E => E.ErrorMessage)
                                                         .ToArray();
                    var validationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });

            builder.Services.AddScoped(typeof(IAuthService), typeof(AuthService));

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                //options.Password.RequiredUniqueChars = 2;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireLowercase = true;

            }).AddEntityFrameworkStores<AppIdentityDbContext>();

            builder.Services.AddAuthentication(/*JwtBearerDefaults.AuthenticationScheme*/ O =>
            {
                O.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                O.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(O =>
                {
                    // Configure Authentication Handler
                    O.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JWT:ValidAudience"],
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromDays(double.Parse(builder.Configuration["JWT:DurationInDays"]))
                    };
                });

            builder.Services.AddCors(Options =>
            {
                Options.AddPolicy("MyPolicy", O =>
                {
                    O.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["FrontEndUrl"]);//.AllowAnyOrigin();///
                });
            });
            #endregion

            var app = builder.Build();

            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var _dbContext = services.GetRequiredService<StoreContext>();

            var _loggerFactory = services.GetRequiredService<ILoggerFactory>();

            var _identityDbContext = services.GetRequiredService<AppIdentityDbContext>();

            try
            {
                await _dbContext.Database.MigrateAsync(); // Update-Database
                await StoreContextSeed.SeedAsync(_dbContext); // Data Seeding
                await _identityDbContext.Database.MigrateAsync(); // Update-Database
                var _userManger = services.GetRequiredService<UserManager<AppUser>>(); // Explicitly
                await AppIdentityDbContextSeed.SeedUsersAsync(_userManger); // Data Seeding
            }
            catch (Exception ex)
            {
                var logger = _loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An Error has Occured during apply the migration");
            }

            #region Configure Kestrel Middlewares
            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("MyPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}