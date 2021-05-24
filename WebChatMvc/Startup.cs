using Application;
using Application.Contracts;
using Application.Services;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using WebChatMvc.HostedServices;
using WebChatMvc.Hubs;
using WebChatMvc.Services;

namespace WebChatMvc
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
            services.AddDbContext<WebChatContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("WebChatServer")));
            services.AddIdentity<WebChatUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<WebChatContext>();

            services.AddSingleton<ITicketStore, TicketStore>();

            services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
            .Configure<ITicketStore>((options, store) =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
                options.SessionStore = store;
            });

            services.AddAutoMapper(typeof(AutoMapperProfile));

            var serviceInterface = typeof(IScopedService);
            foreach (var serviceType in serviceInterface.Assembly.GetTypes().Where(type => serviceInterface.IsAssignableFrom(type) && serviceInterface != type))
                services.AddScoped(serviceType);

            var jsonSerializer = new JsonStringEnumConverter();

            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(jsonSerializer);
                });

            services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(jsonSerializer));

            services.AddHostedService<UserStateHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<WebChatHub>("/webChatHub");
            });

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<WebChatContext>();
                context.Database.Migrate();
            }
        }
    }
}
