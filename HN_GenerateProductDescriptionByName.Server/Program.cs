using Microsoft.EntityFrameworkCore;


namespace HN_GenerateProductDescriptionByName.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContextFactory<CustomDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("CustomDbContext")
                                  ?? throw new InvalidOperationException("Connection string 'CustomDbContext' not found.")));


            var app = builder.Build();

            var factory = app.Services.GetRequiredService<IDbContextFactory<CustomDbContext>>();
            using (var context = factory.CreateDbContext())
            {
                //Applying migrations to run programm
                context.Database.Migrate();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
