using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StudyRoomSystem.Server.Controllers.V1;
using StudyRoomSystem.Server.Converters;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using StudyRoomSystem.Server.Hubs;
using StudyRoomSystem.Server.OpenApi;
using StudyRoomSystem.Server.Services;
using Zeng.CoreLibrary.Toolkit.Logging;

// using ApiVersion = Asp.Versioning.ApiVersion;

var builder = WebApplication.CreateBuilder(args);

// 初始化 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.ShortSourceContext()
    .Enrich.FromTraceInfo()
    .WriteTo.Console(outputTemplate: LoggerExtension.ConsoleMessageTemplate)
    .CreateLogger();
builder.Host.UseSerilog();

// 配置 Service
builder.Services.AddHostedService<PgSqlNotificationsService>();

// 配置 OpenApi
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options
            .AddDocumentTransformer<BearerSecuritySchemeTransformer>()
            .AddOperationTransformer<AuthorizeCheckOperationFilter>()
            // .AddSchemaTransformer<ModelDescSchemeTransformer>()
            //
            ;
    }
);
builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options
            .AddDocumentTransformer<BearerSecuritySchemeTransformer>()
            .AddOperationTransformer<AuthorizeCheckOperationFilter>()
            // .AddSchemaTransformer<ModelDescSchemeTransformer>()
            //
            ;
    }
);

// 添加 CORS 服务
builder.Services.AddCors(options =>
    {
        // 允许本地调试前端后端
        options.AddPolicy(
            "AllowFrontend",
            policy =>
            {
                policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }
        );
    }
);

// API文档版本控制
// builder.Services.AddApiVersioning(options =>
//     {
//         options.DefaultApiVersion = new ApiVersion(1, 0);
//         options.AssumeDefaultVersionWhenUnspecified = true;
//         options.ReportApiVersions = true;
//         // options.ApiVersionReader = ApiVersionReader.Combine(
//         //     new UrlSegmentApiVersionReader(),
//         //     new HeaderApiVersionReader("x-api-version"),
//         //     new MediaTypeApiVersionReader("x-api-version")
//         // );
//         // 关键：指定版本号的来源
//     }
// ).AddVersionedApiExplorer(options =>
//     {
//         // 自动添加版本控制
//         options.GroupNameFormat = "'v'VVV";
//         options.SubstituteApiVersionInUrl = true;
//     }
// );
builder
    .Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            // 关键：指定版本号的来源
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version")
            );
        }
    )
    .AddApiExplorer(options =>
        {
            // 自动添加版本控制
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        }
    );

// 添加 DbContext，使用 PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options => options
    // 延迟加载关联属性，但这样Json序列化会有错误数据，还是手动配置关联属性
    // .UseLazyLoadingProxies()
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention()
);

// 添加控制器
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
        {
            // 忽略循环引用
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }
    );
builder.Services.Configure<JsonOptions>(options =>
    {
        // 日期时间转换
        options.JsonSerializerOptions.Converters.Add(new FlexibleDateTimeOffsetConverter());
        // Json枚举字符串转换
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
);

// builder.Services.AddRouting(options =>
// {
//     options.LowercaseQueryStrings = true;
//     options.LowercaseUrls = true;
// });

// 添加 JWT 认证
builder
    .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                    {
                        context.Token = accessToken;
                        Log.Logger.Trace().Debug("Hub Token {Token}", accessToken.ToString());
                    }

                    return Task.CompletedTask;
                }
            };
        }
    );

// 添加认证角色策略
builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        AuthorizationHelper.Policy.User,
        policy => policy.RequireRole(AuthorizationHelper.Role.User, AuthorizationHelper.Role.Admin)
    )
    .AddPolicy(AuthorizationHelper.Policy.Admin, policy => policy.RequireRole(AuthorizationHelper.Role.Admin));

// SignalR
builder
    .Services.AddSignalR()
    .AddJsonProtocol(options =>
        {
            // 日期时间转换
            options.PayloadSerializerOptions.Converters.Add(new FlexibleDateTimeOffsetConverter());
            // Json枚举字符串转换
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // 忽略循环引用
            options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }
    );
// SignalR Hub UserIdProvider
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

await using var app = builder.Build();

// 在 HTTP 请求流水线中加入 Serilog 请求日志
app.UseSerilogRequestLogging();


// 承载网页和静态资源
// app.UseDefaultFiles(
//     new DefaultFilesOptions
//     {
//         FileProvider = new PhysicalFileProvider(
//             Path.Combine(Environment.CurrentDirectory, app.Configuration.GetValue<string>("Web:Root", "web"))
//         ),
//         RequestPath = ""
//     }
// );
// app.UseStaticFiles(
//     new StaticFileOptions
//     {
//         FileProvider = new PhysicalFileProvider(
//             Path.Combine(Environment.CurrentDirectory, app.Configuration.GetValue<string>("Web:Root", "web"))
//         ),
//         RequestPath = ""
//     }
// );
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 开发模式下添加 OpenApi 和 Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => { options.WithClassicLayout().WithTheme(ScalarTheme.DeepSpace); });

    app.UseCors("AllowFrontend");
}

app.UseWebSockets(new WebSocketOptions());

app.MapControllers();
// app.MapUserController();

// SignalR Hub
var hub = app.MapGroup("/hub");
hub.MapHub<DataHub>("/data")
    // .RequireCors(p => p
    //     .WithOrigins("http://localhost:5173") // 前端地址
    //     .AllowAnyHeader()
    //     .AllowAnyMethod()
    //     .AllowCredentials()
    // )
    ;
// .RequireCors(t => t.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader().AllowCredentials());

// warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
// Failed to determine the https port for redirect.
// app.UseHttpsRedirection();

await app.RunAsync();