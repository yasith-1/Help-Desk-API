namespace ProductApi.Models;

// Ticket එකක life-cycle එකේ තත්ත්ව (status). (Spring/Java වල enum එකක්ම තමයි.)
public enum TicketStatus
{
    Open,          // අලුතෙන් raise කරපු, තාම බලලා නෑ
    InProgress,    // agent කෙනෙක් වැඩ කරමින්
    OnHold,        // තාවකාලිකව නවතලා (customer ගෙන් reply එකක් බලාගෙන වගේ)
    Resolved,      // විසඳලා (customer confirm කරන්න ඉතුරුයි)
    Closed,        // සම්පූර්ණයෙන් වහලා
    Reopened       // ආයෙත් විවෘත කරපු (resolve එක හරි නැති නිසා)
}

// ප්‍රමුඛතාව (priority) — SLA (කොච්චර ඉක්මනින් විසඳන්න ඕනද) මේකෙන් තීරණය වෙනවා.
public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

// පරිශීලක භූමිකා (roles). Role-based security මේවා මත.
public static class Roles
{
    public const string Customer = "Customer";  // ticket raise කරන අය
    public const string Agent = "Agent";         // ticket විසඳන support staff
    public const string Admin = "Admin";         // සියල්ල + users/categories manage
}
