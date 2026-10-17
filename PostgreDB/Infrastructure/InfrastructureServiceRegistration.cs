using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Application.Services.Cache;
using Application.Services.Cloudflare;
using Application.Services.Email;
using Application.Services.Jwt;
using Infrastructure.Cache;
using Infrastructure.CloudflareService;
using Infrastructure.Email;
using Infrastructure.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            var r2Config = configuration.GetSection("CloudflareR2");

            var credentials = new BasicAWSCredentials(
                r2Config["AccessKey"],
                r2Config["SecretKey"]
            );

            var s3Config = new AmazonS3Config
            {
                ServiceURL = r2Config["ServiceUrl"],
                ForcePathStyle = true
            };

            services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));
            services.AddScoped<ICloudflareStorageService, CloudflareStorageService>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
            });

            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }
    }
}
