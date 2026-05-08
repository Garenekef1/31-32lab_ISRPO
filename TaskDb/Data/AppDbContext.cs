using Microsoft.EntityFrameworkCore;
using TaskDb.Models;

namespace TaskDb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Изучить ASP.NET Core",
                Description = "Разобраться с контроллерами и маршрутами",
                IsCompleted = true,
                CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Priority = "High"
            },
            new TaskItem
            {
                Id = 2,
                Title = "Подключить SQLite через EF Core",
                Description = "Создать DbContext и строку подключения",
                IsCompleted = false,
                CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                Priority = "Normal"
            },
            new TaskItem
            {
                Id = 3,
                Title = "Написать README",
                Description = "Описать выполненную лабораторную работу",
                IsCompleted = false,
                CreatedAt = new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc),
                Priority = "Low"
            }
        );
    }
}
