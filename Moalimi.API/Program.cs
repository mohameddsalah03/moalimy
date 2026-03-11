using Moalimi.Infrastructure.Data;
using Moalimi.Application;
using Moalimi.Infrastructure;

namespace Moalimi.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
                    options.AddPolicy("AllowFrontend", policy =>
                        policy
                            .WithOrigins(
                                "http://localhost:3000",
                                "https://www.hissatak.online",
                                "https://hissatak.online"
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader())
                    );

            var app = builder.Build();
            
            //
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowFrontend");


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
