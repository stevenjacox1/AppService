using Azure.Data.Tables;
using Azure.Identity;
using AppService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Register Table Storage client
var tableStorageConnectionString = builder.Configuration.GetConnectionString("TableStorageConnection");
if (!string.IsNullOrWhiteSpace(tableStorageConnectionString))
{
    builder.Services.AddSingleton(_ => new TableClient(tableStorageConnectionString, "items"));
}
else
{
    var tableStorageUri = builder.Configuration["TableStorageUri"];
    if (string.IsNullOrWhiteSpace(tableStorageUri))
    {
        throw new InvalidOperationException("TableStorageConnection or TableStorageUri is not configured. Please configure appsettings.Development.json for Azurite or appsettings.Production.json for Azure.");
    }

    try
    {
        builder.Services.AddSingleton(_ =>
            new TableClient(new Uri(tableStorageUri), "items", new DefaultAzureCredential()));
    }
    catch (UriFormatException ex)
    {
        throw new InvalidOperationException(
            $"Invalid TableStorageUri format: '{tableStorageUri}'. " +
            $"For local development, use the Azurite connection string in appsettings.Development.json. " +
            $"For Azure, use 'https://yourstorageaccount.table.core.windows.net'.", ex);
    }
}

// Register application services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure table exists
using (var scope = app.Services.CreateScope())
{
    var tableClient = scope.ServiceProvider.GetRequiredService<TableClient>();
    await tableClient.CreateIfNotExistsAsync();
}

app.Run();
