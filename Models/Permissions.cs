namespace ProductApi.Models;

// මුළු system එකේ තියෙන permission keys (fixed catalog). Role එකකට මේවායින් කැමති ඒවා assign කරනවා.
public static class Permissions
{
    public const string TicketsViewAll = "tickets.viewAll";
    public const string TicketsEditAny = "tickets.editAny";
    public const string TicketsAssign = "tickets.assign";
    public const string TicketsChangeStatus = "tickets.changeStatus";
    public const string TicketsChangePriority = "tickets.changePriority";
    public const string TicketsCommentInternal = "tickets.commentInternal";
    public const string CategoriesManage = "categories.manage";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";

    public record Item(string Key, string Label, string Group);

    // UI matrix එකට — key + display label + group.
    public static readonly List<Item> Catalog = new()
    {
        new(TicketsViewAll,        "View all tickets",        "Tickets"),
        new(TicketsEditAny,        "Edit any ticket",         "Tickets"),
        new(TicketsAssign,         "Assign tickets",          "Tickets"),
        new(TicketsChangeStatus,   "Change status",           "Tickets"),
        new(TicketsChangePriority, "Change priority",         "Tickets"),
        new(TicketsCommentInternal,"Internal notes",          "Tickets"),
        new(CategoriesManage,      "Manage categories",       "Administration"),
        new(UsersManage,           "Manage users",            "Administration"),
        new(RolesManage,           "Manage roles & permissions","Administration"),
    };

    public static readonly string[] All = Catalog.Select(c => c.Key).ToArray();

    // Catalog එකේ නැති key එකක්ද කියලා validate කරන්න
    public static bool IsValid(string key) => Catalog.Any(c => c.Key == key);
}
