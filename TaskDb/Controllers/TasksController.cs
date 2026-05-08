using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskDb.Data;
using TaskDb.Models;

namespace TaskDb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext db) : ControllerBase
{
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAll([FromQuery] bool? completed = null)
    {
        var query = _db.Tasks.AsQueryable();

        if (completed.HasValue)
        {
            query = query.Where(task => task.IsCompleted == completed.Value);
        }

        var tasks = await query
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            return NotFound(new { message = $"Задача с id = {id} не найдена" });
        }

        return Ok(task);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<TaskItem>>> Search(
        [FromQuery] string? query = null,
        [FromQuery] string? priority = null,
        [FromQuery] bool? completed = null)
    {
        var tasksQuery = _db.Tasks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            tasksQuery = tasksQuery.Where(task =>
                task.Title.Contains(query) || task.Description.Contains(query));
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            tasksQuery = tasksQuery.Where(task => task.Priority == priority);
        }

        if (completed.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.IsCompleted == completed.Value);
        }

        var tasks = await tasksQuery
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var total = await _db.Tasks.CountAsync();
        var completed = await _db.Tasks.CountAsync(task => task.IsCompleted);
        var pending = await _db.Tasks.CountAsync(task => !task.IsCompleted);
        var createdLastWeek = await _db.Tasks.CountAsync(task => task.CreatedAt >= DateTime.UtcNow.AddDays(-7));
        var byPriority = await _db.Tasks
            .GroupBy(task => task.Priority)
            .Select(group => new
            {
                priority = group.Key,
                count = group.Count()
            })
            .ToListAsync();

        var completionPct = total == 0 ? 0 : Math.Round((double)completed / total * 100, 2);

        return Ok(new
        {
            total,
            completed,
            pending,
            completionPct,
            byPriority,
            createdLastWeek
        });
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 5;
        }

        pageSize = Math.Min(pageSize, 50);

        var totalCount = await _db.Tasks.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var tasks = await _db.Tasks
            .OrderByDescending(task => task.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages,
            items = tasks
        });
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<List<TaskItem>>> GetOverdue()
    {
        var tasks = await _db.Tasks
            .Where(task => task.DueDate != null && task.DueDate < DateTime.UtcNow && !task.IsCompleted)
            .OrderBy(task => task.DueDate)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            DueDate = dto.DueDate
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItem>> Update(int id, UpdateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            return NotFound(new { message = $"Задача с id = {id} не найдена" });
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;

        await _db.SaveChangesAsync();

        return Ok(task);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<TaskItem>> ToggleComplete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            return NotFound(new { message = $"Задача с id = {id} не найдена" });
        }

        task.IsCompleted = !task.IsCompleted;
        await _db.SaveChangesAsync();

        return Ok(task);
    }

    [HttpPatch("complete-all")]
    public async Task<IActionResult> CompleteAll()
    {
        var count = await _db.Tasks
            .Where(task => !task.IsCompleted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(task => task.IsCompleted, true));

        return Ok(new { updated = count });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            return NotFound(new { message = $"Задача с id = {id} не найдена" });
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("completed")]
    public async Task<IActionResult> DeleteCompleted()
    {
        var count = await _db.Tasks
            .Where(task => task.IsCompleted)
            .ExecuteDeleteAsync();

        return Ok(new { deleted = count });
    }
}
