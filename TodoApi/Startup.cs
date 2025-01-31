using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.OpenApi.Models;
using TodoApi.Services;
using Azure.AI.OpenAI;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddApplicationInsightsTelemetry(Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));

            services.AddDbContext<TodoContext>(options =>
            {
                options.UseInMemoryDatabase("TodoDB");
                //options.UseSqlServer(Configuration.GetConnectionString("SqlConnection"));
            });

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title="Todo API",
                    Description = "Todo API operations",
                    Version="1.0"
                });
            });

            services.AddCors(config =>
            {
                config.AddDefaultPolicy(c =>
                {
                    c.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            services.AddControllers();

            services.AddSingleton<SentimentAnalysisService>(provider =>
            {
                var apiKey = Configuration["AzureOpenAI:ApiKey"];
                var endpoint = Configuration["AzureOpenAI:Endpoint"];
                var openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                return new SentimentAnalysisService(openAIClient, apiKey, endpoint);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TodoContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Use when InmemoryDb is used
            DbInitializer.InitializeDatabase(dbContext);

            //Use when SQL Server is used
            //InitializeDatabase(app);

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseSwagger();

            app.UseRouting();           
            
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                config.RoutePrefix = "";
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var database = serviceScope.ServiceProvider.GetService<TodoContext>().Database;
                database.Migrate();
            }
        }
    }
}
