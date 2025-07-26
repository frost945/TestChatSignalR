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

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.MapHub<ChatHub>("/chat");

            await app.RunAsync();
        }
    }
}
