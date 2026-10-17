using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationServiceRegistration
    {

        public static IServiceCollection AddApplicationServiceRegistration(this IServiceCollection services, params Assembly[] additionalAssemblies)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() }.Concat(additionalAssemblies).ToArray();
            services.AddAutoMapper(n => n.AddMaps(assemblies));

            services.AddMediatR(n => n.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
