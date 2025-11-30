using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Data;
using TestChatSignalR.Hubs;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Repositories;
using TestChatSignalR.Services;
using WebApiDraft.Middleware;


WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Logging.ClearProviders(); // убираем все логгеры ASP.NET Core

builder.Services.AddDbContext<ChatDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IChatsRepository, ChatsRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IChatsService, ChatsService>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

//для тестирования работы с БД
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
Console.WriteLine("DB Connection: " + db.Database.CanConnect());

db.Database.EnsureDeleted();
db.Database.EnsureCreated();
}

//Определение текущей среды
string environmentName = app.Environment.EnvironmentName;
Console.WriteLine($"Environment: {environmentName}");

//Автоматическое применение миграций
using (var scope = app.Services.CreateScope())
{
ChatDbContext db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
db.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
// app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestChatSignalR v1"));
}
app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapHub<ChatHub>("/chat");

app.MapGet("/ping", () => "API is running"); //проверка работы API

app.MapFallbackToFile("login.html");

await app.RunAsync();