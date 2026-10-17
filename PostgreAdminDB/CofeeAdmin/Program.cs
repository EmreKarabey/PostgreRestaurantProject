using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using CofeeAdmin.Mapping;
using CofeeAdmin.ModelBinders;
using CofeeAdmin.Services.Jwt;
using CofeeAdmin.Services.Sync;
using CofeeAdmin.SignalR;
using DataAccessLayer.Abstract;
using DataAccessLayer.Concrete;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection ayarı yapılandırılmalıdır.");
builder.Services.AddDbContext<BaseDBContext>(options => options.UseNpgsql(connectionString));


builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICategoryDal, EFCategoryDal>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IProductDal, EFProductDal>();


builder.Services.AddScoped<IReservationService, ReservationManager>();
builder.Services.AddScoped<IReservationDal, EFReservationDal>();


builder.Services.AddScoped<ICloudflareStorageService,CloudflareStorageService>();

builder.Services.AddScoped<IQrCodeService, QrCodeService>();

builder.Services.AddScoped<IReviewsService, ReviewsManager>();
builder.Services.AddScoped<IReviewsDal, EFReviewsDal>();

builder.Services.AddScoped<IRestaurantTableService, RestaurantTableManager>();
builder.Services.AddScoped<IRestaurantTableDal, EFRestaurantTableDal>();

builder.Services.AddScoped(typeof(IGenericDal<>), typeof(GenericRepository<>));

builder.Services.AddHttpClient("InsecureClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
builder.Services.AddScoped<IClientSyncService, ClientSyncService>();

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<BaseDBContext>()
    .AddDefaultTokenProviders();


// JWT Service & Authentication
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpContextAccessor();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
    throw new InvalidOperationException("Jwt:SecretKey ayarı en az 32 byte olacak şekilde yapılandırılmalıdır.");
var issuer = jwtConfig["Issuer"] ?? "DinaKafe";
var audience = jwtConfig["Audience"] ?? "DinaKafeAdmin";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                context.Response.Redirect("/Login/Index");
            }
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Redirect("/Error/AccessDenied");
            }
            return Task.CompletedTask;
        }
    };
});



builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapping>();
});

var r2Config = builder.Configuration.GetSection("CloudflareR2");
var r2AccessKey = r2Config["AccessKey"]
    ?? throw new InvalidOperationException("CloudflareR2:AccessKey ayarı yapılandırılmalıdır.");
var r2SecretKey = r2Config["SecretKey"]
    ?? throw new InvalidOperationException("CloudflareR2:SecretKey ayarı yapılandırılmalıdır.");
var r2ServiceUrl = r2Config["ServiceUrl"]
    ?? throw new InvalidOperationException("CloudflareR2:ServiceUrl ayarı yapılandırılmalıdır.");

var credentials = new BasicAWSCredentials(
    r2AccessKey,
    r2SecretKey
);

var s3Config = new AmazonS3Config
{
    ServiceURL = r2ServiceUrl,
    ForcePathStyle = true
};

builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<SignalRHub>("/signalrhub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
