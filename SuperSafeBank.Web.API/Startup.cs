using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using SuperSafeBank.Core;
using SuperSafeBank.Domain.Events;
using SuperSafeBank.Domain.Queries.Models;
using SuperSafeBank.Domain.Services;
using SuperSafeBank.Web.API.Commands;
using SuperSafeBank.Web.API.Queries;
using SuperSafeBank.Web.API.Registries;
using SuperSafeBank.Web.API.Workers.EventHandlers;

namespace SuperSafeBank.Web.API
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
            services.AddControllers();

            services.AddSingleton<ICurrencyConverter, FakeCurrencyConverter>();

            services.AddSingleton<IEventDeserializer>(new JsonEventDeserializer(new[]
                {
                    typeof(CustomerCreated).Assembly
                })).AddEventStore(this.Configuration)
                .AddMongoDb(this.Configuration);

            services.AddScoped<ServiceFactory>(ctx => ctx.GetRequiredService);
            services.AddScoped<IMediator, Mediator>();

            services.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(CustomerDetailsHandler))
                    .RegisterHandlers(typeof(IRequestHandler<>))
                    .RegisterHandlers(typeof(IRequestHandler<,>))
                    .RegisterHandlers(typeof(INotificationHandler<>));
            });

            services.Decorate(typeof(INotificationHandler<>), typeof(RetryDecorator<>));

            services.RegisterWorker();
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

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

    }
}
