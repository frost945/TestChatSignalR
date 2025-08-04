using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Domain;
using TestChatSignalR.Hubs;

namespace TestChatSignalR
{
    public class Program
    {
        public static async Task Main()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //хранилище сообщений
            builder.Services.AddScoped<ChatRepository>();

            builder.Services.AddSignalR();

            WebApplication app = builder.Build();

            string environmentName = app.Environment.EnvironmentName;
            Console.WriteLine($"Environment: {environmentName}");

            //Автоматическое применение миграций
            using (var scope = app.Services.CreateScope())
            {
                ChatDbContext db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                db.Database.Migrate();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.MapHub<ChatHub>("/chat");

            app.MapGet("/ping", () => "API is running"); //проверка работы API

            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }
    }
}