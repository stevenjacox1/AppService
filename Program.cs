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
var tableStorageUri = builder.Configuration["TableStorageUri"];
if (string.IsNullOrEmpty(tableStorageUri))
{
    throw new InvalidOperationException("TableStorageUri is not configured");
}

builder.Services.AddSingleton(x => 
    new TableClient(new Uri(tableStorageUri), "items", 
        new DefaultAzureCredential()));

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
