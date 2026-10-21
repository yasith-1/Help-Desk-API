using ProductApi.Models;

namespace ProductApi.Data;

// App එක මුලින්ම run වෙද්දී sample data දානවා (DB එක හිස් නම් විතරක්).
// Test/demo වලට ලේසි වෙන්න — users, categories, tickets එක්ක conversation + history.
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        SeedRoles(db);   // roles — වෙනම guard (පරණ DB එකටත් roles එකතු වෙන්න)

        if (db.Users.Any()) return;   // දැනටමත් data තියෙනවා නම් ආයෙත් දාන්නේ නෑ

        var now = DateTime.UtcNow;

        // ── Users (හැම role එකකින්ම) ───────────────────────────────────────────────
        string Hash(string p) => BCrypt.Net.BCrypt.HashPassword(p);

        var admin = new User { FullName = "Admin User", Email = "admin@helpdesk.com", Username = "admin", PasswordHash = Hash("admin123"), Role = Roles.Admin };
        var nimal = new User { FullName = "Nimal Perera", Email = "nimal@helpdesk.com", Username = "nimal", PasswordHash = Hash("nimal123"), Role = Roles.Agent };
        var sunil = new User { FullName = "Sunil Silva", Email = "sunil@helpdesk.com", Username = "sunil", PasswordHash = Hash("sunil123"), Role = Roles.Agent };
        var kamal = new User { FullName = "Kamal Fernando", Email = "kamal@gmail.com", Username = "kamal", PasswordHash = Hash("kamal123"), Role = Roles.Customer };
        var amara = new User { FullName = "Amara Jayasuriya", Email = "amara@gmail.com", Username = "amara", PasswordHash = Hash("amara123"), Role = Roles.Customer };

        db.Users.AddRange(admin, nimal, sunil, kamal, amara);
        db.SaveChanges();

        // ── Categories ──────────────────────────────────────────────────────────────
        var catIt = new Category { Name = "IT & Hardware", Description = "Computers, printers, network", DefaultPriority = TicketPriority.High, SlaHours = 8 };
        var catSoft = new Category { Name = "Software & Accounts", Description = "Apps, logins, access", DefaultPriority = TicketPriority.Medium, SlaHours = 24 };
        var catBilling = new Category { Name = "Billing & Payments", Description = "Invoices, refunds, charges", DefaultPriority = TicketPriority.Medium, SlaHours = 24 };
        var catFacility = new Category { Name = "Facilities", Description = "Office, building, utilities", DefaultPriority = TicketPriority.Low, SlaHours = 72 };
        var catGeneral = new Category { Name = "General Inquiry", Description = "Anything else", DefaultPriority = TicketPriority.Low, SlaHours = 48 };

        db.Categories.AddRange(catIt, catSoft, catBilling, catFacility, catGeneral);
        db.SaveChanges();

        // ── Sample tickets ───────────────────────────────────────────────────────────
        var seq = 0;
        Ticket NewTicket(string title, string desc, Category cat, TicketPriority pri, TicketStatus status,
                         User reporter, User? assignee, int createdHoursAgo)
        {
            seq++;
            var created = now.AddHours(-createdHoursAgo);
            var t = new Ticket
            {
                TicketNumber = $"TKT-{seq:D5}",
                Title = title,
                Description = desc,
                Category = cat,
                Priority = pri,
                Status = status,
                CreatedBy = reporter,
                AssignedTo = assignee,
                CreatedAt = created,
                UpdatedAt = created,
                DueAt = created.AddHours(cat.SlaHours),
                ResolvedAt = status is TicketStatus.Resolved or TicketStatus.Closed ? created.AddHours(cat.SlaHours / 2.0) : null,
                ClosedAt = status == TicketStatus.Closed ? created.AddHours(cat.SlaHours) : null
            };
            t.History.Add(new TicketHistory { Actor = reporter, Action = "Ticket created", CreatedAt = created });
            if (assignee != null)
                t.History.Add(new TicketHistory { Actor = admin, Action = $"Assigned to {assignee.FullName}", CreatedAt = created.AddMinutes(30) });
            if (status != TicketStatus.Open)
                t.History.Add(new TicketHistory { Actor = assignee ?? admin, Action = $"Status: Open → {status}", CreatedAt = created.AddHours(1) });
            return t;
        }

        var tickets = new List<Ticket>
        {
            NewTicket("Laptop won't turn on", "My work laptop is completely dead since this morning. Tried charging — no light.", catIt, TicketPriority.Urgent, TicketStatus.InProgress, kamal, nimal, 3),
            NewTicket("Cannot login to email", "Getting 'invalid password' even after reset. Need access urgently for a client mail.", catSoft, TicketPriority.High, TicketStatus.Open, amara, null, 1),
            NewTicket("Wrong amount on invoice #4521", "Charged twice for the same subscription this month. Please refund the duplicate.", catBilling, TicketPriority.Medium, TicketStatus.InProgress, kamal, sunil, 20),
            NewTicket("Printer on 2nd floor jammed", "Paper keeps jamming on the main printer. Whole team affected.", catIt, TicketPriority.Medium, TicketStatus.Resolved, amara, nimal, 30),
            NewTicket("Request new monitor", "Could I get a second monitor for my desk? Current one is quite small.", catFacility, TicketPriority.Low, TicketStatus.Open, kamal, null, 50),
            NewTicket("VPN keeps disconnecting", "The office VPN drops every few minutes when working from home.", catIt, TicketPriority.High, TicketStatus.OnHold, amara, sunil, 40),
            NewTicket("How do I export my data?", "Is there a way to download all my reports as Excel?", catGeneral, TicketPriority.Low, TicketStatus.Closed, kamal, nimal, 120),
            NewTicket("Software license expired", "The design tool says my license expired. Need renewal to continue work.", catSoft, TicketPriority.High, TicketStatus.InProgress, amara, nimal, 6),
            NewTicket("AC not working in meeting room", "The air conditioner in Room B is not cooling at all.", catFacility, TicketPriority.Medium, TicketStatus.Open, kamal, null, 10),
            NewTicket("Refund not received", "I was promised a refund 2 weeks ago but nothing in my account yet.", catBilling, TicketPriority.High, TicketStatus.Reopened, amara, sunil, 300),
            NewTicket("New employee account setup", "Please create accounts and access for our new hire starting Monday.", catSoft, TicketPriority.Medium, TicketStatus.Resolved, kamal, nimal, 70),
            NewTicket("Keyboard keys not responding", "Several keys on my keyboard stopped working. Hard to type.", catIt, TicketPriority.Low, TicketStatus.Closed, amara, sunil, 200),
        };

        db.Tickets.AddRange(tickets);
        db.SaveChanges();

        // ── Sample conversation on a couple of tickets ────────────────────────────────
        var t1 = tickets[0];
        db.TicketComments.AddRange(
            new TicketComment { Ticket = t1, Author = nimal, Body = "Hi Kamal, sorry to hear that. Can you tell me if the charger light turns on when plugged in?", IsInternal = false, CreatedAt = t1.CreatedAt.AddHours(1) },
            new TicketComment { Ticket = t1, Author = kamal, Body = "No light at all on the charger or the laptop.", IsInternal = false, CreatedAt = t1.CreatedAt.AddHours(1.5) },
            new TicketComment { Ticket = t1, Author = nimal, Body = "Likely a power adapter failure. Ordering a replacement unit.", IsInternal = true, CreatedAt = t1.CreatedAt.AddHours(2) }
        );

        var t3 = tickets[2];
        db.TicketComments.AddRange(
            new TicketComment { Ticket = t3, Author = sunil, Body = "Thanks for reporting. I can see the duplicate charge — processing the refund now.", IsInternal = false, CreatedAt = t3.CreatedAt.AddHours(2) },
            new TicketComment { Ticket = t3, Author = sunil, Body = "Refund reference submitted to finance, ETA 3-5 business days.", IsInternal = true, CreatedAt = t3.CreatedAt.AddHours(3) }
        );

        db.SaveChanges();
    }

    // System roles 3ක් + ඒවායේ default permissions (DB එකේ roles නැත්නම් විතරක්).
    private static void SeedRoles(AppDbContext db)
    {
        if (db.Roles.Any()) return;

        Role MakeRole(string name, string desc, IEnumerable<string> perms) => new()
        {
            Name = name,
            Description = desc,
            IsSystem = true,
            Permissions = perms.Select(p => new RolePermission { Permission = p }).ToList()
        };

        var customer = MakeRole(Roles.Customer, "Raises and tracks their own tickets",
            Array.Empty<string>());

        var agent = MakeRole(Roles.Agent, "Support staff who work and resolve tickets", new[]
        {
            Permissions.TicketsViewAll, Permissions.TicketsEditAny, Permissions.TicketsAssign,
            Permissions.TicketsChangeStatus, Permissions.TicketsChangePriority, Permissions.TicketsCommentInternal
        });

        var admin = MakeRole(Roles.Admin, "Full access to everything", Permissions.All);

        db.Roles.AddRange(customer, agent, admin);
        db.SaveChanges();
    }
}
