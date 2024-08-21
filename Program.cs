using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async () =>
{
    var blobClient = new BlobClient(new Uri("https://testapplicationfiles.blob.core.windows.net/appfiles/CoverLetter.pdf"), new DefaultAzureCredential());
    var result = await blobClient.DownloadContentAsync();
    return result.Value.Content;

})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromFormAttribute] FileUploadRequest fileUploadRequest) =>
{

    var blobServiceClient = new BlobServiceClient(new Uri("https://testapplicationfiles.blob.core.windows.net")
    , new DefaultAzureCredential());
    var containerClient = blobServiceClient.GetBlobContainerClient(fileUploadRequest.containerName);
    await using var stream = fileUploadRequest.file.OpenReadStream();
    await containerClient.UploadBlobAsync(fileUploadRequest.file.FileName, stream);


})
.DisableAntiforgery()
.WithName("PostWeatherForecast")
.WithOpenApi();

app.MapDelete("/weatherforecast", async (string fileName) =>
{

    var blobServiceClient = new BlobClient(new Uri("https://testapplicationfiles.blob.core.windows.net/" + fileName)
    , new DefaultAzureCredential());
    await blobServiceClient.DeleteIfExistsAsync();


})
.DisableAntiforgery()
.WithName("DeleteWeatherForecast")
.WithOpenApi();


app.Run();

record FileUploadRequest(IFormFile file, string containerName) { }
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
