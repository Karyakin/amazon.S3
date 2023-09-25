using Amazon.S3;
//https://www.youtube.com/watch?v=2q5jA813ZiI - урок
//https://stackoverflow.com/questions/47917125/how-to-set-aws-credentials-with-net-core - конфигурация
//https://codewithmukesh.com/blog/aws-credentials-for-dotnet-applications/ -настройки
//https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/csharp_s3_code_examples.html - конфигурация
//https://stackoverflow.com/questions/39051477/the-aws-access-key-id-does-not-exist-in-our-records - переменные среды
var builder = WebApplication.CreateBuilder(args);
var aaa = builder.Configuration.GetAWSOptions();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
