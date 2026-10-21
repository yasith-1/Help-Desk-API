# 🎧 HelpDesk API — ASP.NET Core Ticket Management Backend (Sinhala)

ඕනෑම company / shop / ආයතනයකට apply කරන්න පුළුවන් **Ticket Management System** එකක backend එක.
Customer කෙනෙක් (හෝ employee කෙනෙක්) ප්‍රශ්නයක් **ticket** එකක් විදිහට raise කරනවා → support **agent**ලා
review කරලා, assign කරලා, විසඳනවා.

Database එක **SQLite** (`helpdesk.db` file) — install කරන්න දෙයක් නෑ, data **persist වෙනවා** (restart කරාම යන්නේ නෑ).

---

## ✅ මොනවද තියෙන්නේ

- **Ticket CRUD** — raise, view, update, assign, status මාරු කරන්න
- **Status life-cycle** — Open → InProgress → OnHold → Resolved → Closed → Reopened
- **Priority + SLA** — Low/Medium/High/Urgent; category එකකට SLA පැය; overdue ඉබේම හඳුනනවා
- **Conversation** — public replies + **internal notes** (agents/admin ට විතරක් පේන)
- **Activity history** — ticket එකකට වුණු හැම වෙනසක්ම log (audit trail)
- **Role-based security** — Customer / Agent / Admin (JWT + ownership checks)
- **Categories** — Admin ට manage කරන්න (default priority + SLA එක්ක)
- **User management** — Admin ට accounts හදන්න, role මාරු කරන්න, activate/deactivate
- **Dashboard stats** — counts by status/priority/category, overdue, unassigned, avg resolution
- **Swagger UI** + JWT, global error handling, BCrypt password hashing

---

## 🚀 Run කරන විදිහ

### අවශ්‍ය දේ
- [.NET 8+ SDK](https://dotnet.microsoft.com/download) (.NET 10 තිබුණත් වැඩ — roll-forward)

### පියවර
```bash
cd ProductApi
dotnet restore
dotnet run
```

Browser එකේ: **http://localhost:5000/swagger** 🎉
(පළවෙනි පාර `helpdesk.db` file එක ඉබේම හැදිලා sample data seed වෙනවා.)

---

## 👥 Roles & අවසර

| Role | කරන්න පුළුවන් |
|------|---------------|
| **Customer** | register/login · ticket raise · **තමන්ගේ** ticket බලන්න/comment · resolved ticket reopen |
| **Agent** | ඔක්කොම tickets · filter/search · assign · status/priority මාරු · public reply + internal note |
| **Admin** | Agent ට පුළුවන් සියල්ල + users manage + categories manage + full dashboard |

### Seed කරපු accounts (test වලට)
| username | password | role |
|----------|----------|------|
| `admin` | `admin123` | Admin |
| `nimal` | `nimal123` | Agent |
| `sunil` | `sunil123` | Agent |
| `kamal` | `kamal123` | Customer |
| `amara` | `amara123` | Customer |

---

## 📋 API Endpoints

| Method | Endpoint | Auth | කරන්නේ |
|--------|----------|------|---------|
| POST | `/api/auth/register` | ❌ | අලුත් Customer + token |
| POST | `/api/auth/login` | ❌ | login + token |
| GET | `/api/tickets` | 🔑 | list (filter/search/sort/page; role-scoped) |
| GET | `/api/tickets/{id}` | 🔑 | එක ticket එකක් (full detail) |
| POST | `/api/tickets` | 🔑 | raise |
| PUT | `/api/tickets/{id}` | 🔑 | update |
| PATCH | `/api/tickets/{id}/status` | 🔑 | status මාරුව |
| PATCH | `/api/tickets/{id}/assign` | 🔑 Agent/Admin | assign |
| POST | `/api/tickets/{id}/comments` | 🔑 | reply / internal note |
| GET | `/api/tickets/{id}/history` | 🔑 | activity log |
| GET/POST/PUT/DELETE | `/api/categories` | 🔑 (write=Admin) | categories |
| GET | `/api/users` · `POST` · `PATCH .../role` · `.../active` | 🔑 Admin | user mgmt |
| GET | `/api/users/agents` | 🔑 Agent/Admin | assignment dropdown |
| GET | `/api/dashboard/stats` | 🔑 | role-aware stats |

### Filter query parameters (`GET /api/tickets`)
`status`, `priority`, `categoryId`, `assignedToId` (`-1` = unassigned), `mine=true`,
`search`, `sort` (`newest`|`oldest`|`priority`|`due`), `page`, `pageSize`.

---

## 📂 ව්‍යුහය (layered — Spring Boot වගේ)

```
ProductApi/
├── Program.cs                 # setup: DI + SQLite + JWT + CORS + middleware
├── appsettings.json           # connection string + Jwt + SLA config
├── Models/                    # entities (DB tables)
│   ├── Ticket.cs  User.cs  Category.cs  TicketComment.cs  TicketHistory.cs  Enums.cs
├── DTOs/                      # request/response + validation
├── Data/
│   ├── AppDbContext.cs        # DbSets + relationships + enum→string
│   └── DbSeeder.cs            # sample users/categories/tickets
├── Services/                  # business logic (permission/scoping/SLA/history)
│   ├── TicketService · AuthService · CategoryService · UserService · DashboardService
├── Controllers/               # REST endpoints (+ ApiControllerBase: current user from JWT)
└── Middleware/
    └── ErrorHandlingMiddleware.cs   # exception → 400/403/404/500 JSON
```

**Request එකක ගමන:** `Client → Middleware → JWT auth → role check → Controller → Service → DbContext → SQLite`

---

## 💡 Production එකකට ගෙනියන්න

1. **Real DB** — `Program.cs` එකේ `UseSqlite` → `UseNpgsql` (PostgreSQL) / `UseSqlServer`; connection string එක `appsettings.json`.
2. **EF Migrations** — දැන් `EnsureCreated()`. Production එකේ `dotnet ef migrations add Init` → `database update`.
3. **JWT key** — secret එකක (environment variable / secrets manager), source control එකේ නෙවෙයි.
4. **HTTPS** + rate limiting + logging.
5. **Extension ideas** — email notifications, file attachments, SLA escalation jobs, real-time (SignalR).

---

*Frontend එක `../product-ui` (Angular) folder එකේ. මේ project එක Claude සමඟ ගොඩනගන ලදී. 🎉*
