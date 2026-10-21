using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

// DbContext — Spring වල EntityManager + repositories එකතුවක් වගේ.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Enums → string විදිහට DB එකේ save කරනවා (number වෙනුවට "Open", "High"... — කියවන්න ලේසි)
        b.Entity<Ticket>().Property(t => t.Status).HasConversion<string>();
        b.Entity<Ticket>().Property(t => t.Priority).HasConversion<string>();
        b.Entity<Category>().Property(c => c.DefaultPriority).HasConversion<string>();

        // Indexed string columns — MySQL index වලට max length ඕන (නැත්නම් longtext → index error).
        b.Entity<User>().Property(u => u.Username).HasMaxLength(50);
        b.Entity<Ticket>().Property(t => t.TicketNumber).HasMaxLength(20);
        b.Entity<Role>().Property(r => r.Name).HasMaxLength(60);

        // Unique constraints
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
        b.Entity<Ticket>().HasIndex(t => t.TicketNumber).IsUnique();
        b.Entity<Role>().HasIndex(r => r.Name).IsUnique();

        // Role 1 — * RolePermission (role මැකුණොත් permissions මැකෙනවා)
        b.Entity<RolePermission>()
            .HasOne(p => p.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationships — reporter (CreatedBy) සහ assignee (AssignedTo) දෙකම User එකට.
        // Cascade delete නවතනවා (user delete කරොත් tickets මැකෙන්න එපා).
        b.Entity<Ticket>()
            .HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Ticket>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Ticket>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comments / history — ticket එක මැකුණොත් ඒවත් මැකෙනවා (cascade).
        b.Entity<TicketComment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TicketHistory>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
