using Microsoft.EntityFrameworkCore;
using HelpdeskApi.Data;
using HelpdeskApi.DTOs;
using HelpdeskApi.Models;

namespace HelpdeskApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public List<CategoryResponse> GetAll() =>
        _db.Categories
           .Where(c => !c.IsDeleted)
           .Select(c => new CategoryResponse
           {
               Id = c.Id,
               Name = c.Name,
               Description = c.Description,
               DefaultPriority = c.DefaultPriority.ToString(),
               SlaHours = c.SlaHours,
               TicketCount = c.Tickets.Count
           })
           .OrderBy(c => c.Name)
           .ToList();

    public CategoryResponse Create(CategoryRequest req)
    {
        if (_db.Categories.Any(c => c.Name == req.Name && !c.IsDeleted))
            throw new ArgumentException("A category with this name already exists");

        var c = new Category
        {
            Name = req.Name,
            Description = req.Description,
            DefaultPriority = req.DefaultPriority,
            SlaHours = req.SlaHours
        };
        _db.Categories.Add(c);
        _db.SaveChanges();
        return ToResponse(c, 0);
    }

    public CategoryResponse Update(int id, CategoryRequest req)
    {
        var c = _db.Categories.Find(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        c.Name = req.Name;
        c.Description = req.Description;
        c.DefaultPriority = req.DefaultPriority;
        c.SlaHours = req.SlaHours;
        _db.SaveChanges();
        return ToResponse(c, _db.Tickets.Count(t => t.CategoryId == id));
    }

    // Soft delete — row එක මකන්නේ නෑ, IsDeleted flag එක set කරනවා (tickets තිබුණත් ප්‍රශ්නයක් නෑ;
    // පවතින tickets category නම තවම පෙන්නනවා, අලුත් ticket වලට මේක dropdown එකේ පේන්නේ නෑ).
    public void Delete(int id)
    {
        var c = _db.Categories.Find(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        c.IsDeleted = true;
        c.DeletedAt = DateTime.UtcNow;
        _db.SaveChanges();
    }

    private static CategoryResponse ToResponse(Category c, int count) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        DefaultPriority = c.DefaultPriority.ToString(),
        SlaHours = c.SlaHours,
        TicketCount = count
    };
}
