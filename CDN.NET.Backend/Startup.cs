using System;
using System.IO;
using System.Net;
using System.Reflection;
using AutoMapper;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Helpers;
using CDN.NET.Backend.Repositories;
using CDN.NET.Backend.Repositories.Interfaces;
using CDN.NET.Backend.Services;
using CDN.NET.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CDN.NET.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "CDN.NET",
                    Version = "v1",
                    Description = "Contend Delivery Network for secondbooks.ch"
                });
                
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                
            });
            
            ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var consts = new Constants(Configuration);
            services.AddControllers();
            services.AddRouting(op => op.LowercaseUrls = true);
            services.AddDbContext<DataContext>(x =>
            {
                x.UseLazyLoadingProxies();
                x.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });
            
            services.AddAuthentication() //JwtBearerDefaults.AuthenticationScheme
                .AddJwtBearer("Bearer", options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = consts.LazyGetJwtKey(),
                        ValidateIssuer = true,
                        //Usually, this is your application base URL
                        ValidIssuer = Constants.TOKEN_ISSUER,
                        ValidateAudience = false,
                        //Here, we are creating and using JWT within the same application.
                        //In this case, base URL is fine.
                        //If the JWT is created using a web service, then this would be the consumer URL.
                        // WE CANNOT USE THIS 
                        // we use this as an API that you can call via any other machines and processes. 
                        // ValidAudience = "http://localhost:5000/", 
                        RequireExpirationTime =
                            false, // So we can generate permanent tokens for easier API management
                        ValidateLifetime =
                            true, // We still want to validate the frontend tokens tho since those are time bound.
                        LifetimeValidator = LifetimeValidator
                    };
                })
                .AddApiKeySupport(options => { });
            services.AddAuthorization(op =>
            {
                op.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Bearer", "APIKey").Build();
            });

            services.AddCors();
            services.AddAutoMapper(AutoMapperProfiles.ConfigureAutoMapper ,typeof(AuthRepository).Assembly);
            
            services.AddSingleton(consts);
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IUtilsService, UtilsService>();

            services.Configure<UploadSettings>(Configuration.GetSection("UploadSettings"));
            services.Configure<LimitSettings>(Configuration.GetSection("LimitSettings"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        }


        private bool LifetimeValidator(DateTime? notbefore, DateTime? expires, SecurityToken securitytoken, TokenValidationParameters validationparameters)
        {
            return notbefore <= DateTime.UtcNow && expires >= DateTime.UtcNow;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Constants constants,
            IOptions<UploadSettings> uploadSettings, ILogger<Startup> logger)
        {
            // CONFIGURE ANY NEEDED VALUES
            // set root and content path for later use
            constants.SetContentPath(env.ContentRootPath);
            constants.SetRootPath(env.WebRootPath);
            // Check if fileStorage directory exists, otherwise create
            if (!Directory.Exists(Path.Combine(env.ContentRootPath, Constants.STORAGE_FOLDER_NAME)))
                Directory.CreateDirectory(Path.Combine(env.ContentRootPath, Constants.STORAGE_FOLDER_NAME));
            // multiply it by 1'048'576‬ MaxSize to turn megabytes into bytes
            uploadSettings.Value.MaxSize *= 1_048_576;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                // Enable middleware to serve generated swagger as a json endpoint
                app.UseSwagger();
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDN.NET");
                });
            }
            else
            {
                // Global exception handling
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            // app.UseHttpsRedirection();
            

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
