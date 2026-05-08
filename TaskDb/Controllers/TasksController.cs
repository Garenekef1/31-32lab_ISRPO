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

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
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
}
