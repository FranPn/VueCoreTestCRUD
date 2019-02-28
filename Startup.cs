using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace food_tracker
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
            services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            /*Cross Origin Resource Sharing (le origini CORS) è uno standard W3C
             * che consente a un server di ridurre i criteri di corrispondenza dell'origine.
             * Con CORS un server può consentire in modo esplicito alcune richieste
             * multiorigine e rifiutarne altre.*/
            //in parole povere impedisce a un sito dannoso di leggere i dati sensibili da un altro sito
            services.AddCors(options =>
              {
                options.AddPolicy("VueCorsPolicy", builder =>
                {
                  builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:8080");
                });
            });

            //Aggiungo l'autenticazione tramite JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                options.Authority = Configuration["Okta:Authority"];
                options.Audience = "api://default";
              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext dbContext)
        {
          if (env.IsDevelopment())
          {
            app.UseDeveloperExceptionPage();
          }

          //Richiamo Cors
          app.UseCors("VueCorsPolicy");

          dbContext.Database.EnsureCreated();

          //richiamo autenticazione
          app.UseAuthentication();


          app.UseMvc();
        }
    }
}
