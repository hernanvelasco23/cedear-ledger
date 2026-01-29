using CedearLedger.Application.Abstractions;
using CedearLedger.Application.DollarRates;
using CedearLedger.Application.Portfolios;
using CedearLedger.Api.Serialization;
using CedearLedger.Infrastructure.BackgroundJobs;
using CedearLedger.Infrastructure.Persistence.SqlServer;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
    options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });
});

//Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

builder.Services.AddDbContext<CedearLedgerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CedearLedger")));
builder.Services.AddMediatR(typeof(CreatePortfolioCommand).Assembly);
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<ICedearPriceRepository, CedearPriceRepository>();
builder.Services.AddScoped<IPortfolioSummaryQueryService, PortfolioSummaryQueryService>();
builder.Services.AddScoped<IPortfolioValuationQueryService, PortfolioValuationQueryService>();
builder.Services.AddScoped<IDollarRateRepository, DollarRateRepository>();
builder.Services.AddScoped<IPortfolioReadService, PortfolioReadService>();
builder.Services.Configure<IolOptions>(
    builder.Configuration.GetSection("IOL"));
builder.Services.Configure<IngestionOptions>(
    builder.Configuration.GetSection("Ingestion"));
builder.Services.AddHttpClient<IolAuthClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IolOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    }
});
builder.Services.AddHttpClient<IolMarketDataProvider>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IolOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    }
});
builder.Services.AddScoped<IMarketDataProvider, IolMarketDataProvider>();
builder.Services.AddHostedService<DollarRatesIngestionJob>();
builder.Services.AddHostedService<CedearPricesIngestionJob>();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
