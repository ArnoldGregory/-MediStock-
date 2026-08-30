# MediStock — Pharmacy Inventory & Sales Management

A full-stack pharmacy management system for tracking products, sales, customers, suppliers and purchase orders — with **built-in AI** for smart reordering and supplier-invoice importing.

MediStock is split into two .NET 10 applications:

| Project | Role | Port |
| --- | --- | --- |
| `MediStock.API` | REST API + business logic + MySQL access (JWT-secured) | `5088` |
| `MediStock.Portal` | ASP.NET Core MVC front-end (BFF proxy to the API) | `5087` |

---

## Features

### Inventory
- Product register with SKU, barcode, category, cost/selling price, DDA flag
- Product categories, stock batches (with expiry tracking), stock adjustments, stock-take sessions
- Batch/FEFO-aware stock, out-of-stock and expiring alerts on the dashboard

### Sales & Customers
- POS screen with sales history
- Retail and wholesale customers with credit limit / payment terms
- **Sales Returns** — record a return against a sale (reason + per-line quantities),
   gets an automatic `RET-…` number, restores stock to the product and batch, tracks
   `returned_qty` against the original line, and refuses quantities over what was sold.
- **Void Sale** — cancel a sale from the POS; the sale is marked `Voided` and stock
  (product + batch) is restored only for the *unreturned* portion, so returns + void
  never double-restock.
- **Notifications** — per-pharmacy inbox (`api/notifications` list / count / dismiss /
  mark-all-read) fed by `add_notification`; voiding a sale raises a `Sale voided` notice.

### Suppliers, Purchasing & Receiving
- Supplier register and **supplier price history**
- Purchase orders (Pending → Partial → Received) with per-line tracking
- **Receive stock** from a PO — adds batches/stock automatically

### Finance & Reports
- Expenses, purchase orders overview
- Sales, stock and financial reports
- **Stock Performance** — movement & margin report: per-product margin (KES & %), 30-day
  units sold, days-of-stock and a Healthy / Slow / Out of Stock flag, with summary cards
  for product count, average margin, slow movers and the value (cost) tied up in slow stock.
- **Excel export** — every report (Sales, Stock, Financial, Stock Performance, Margins,
  Expiring batches) downloads as a formatted `.xlsx` from the server.

### AI & Automation
- **Smart Reorder** — forecasts demand from the last 30 days of sales
  `suggested = (avg daily sales × lead time) + reorder level − current stock`, with a
  `2 × reorder level` fallback when there is no sales history yet; flags Critical/High/Medium
  priorities, estimates order cost and lists batches expiring within 90 days.
- **Reorder → Draft PO** — one click turns the forecast into a Pending purchase order.
- **Invoice Import (OCR)** — upload a supplier invoice as PDF or photo:
  - digital PDFs → text extracted with **PdfPig**
  - scanned PDFs / photos → rendered and read with **Tesseract OCR** (offline, no API keys)
  - parses supplier, invoice number/date, line items (product, qty, unit cost) with
    confidence scores, then a review grid lets you correct names and **type the selling
    price** before importing → creates/matches products, adds batches, increments stock,
    records a Received PO and supplier price history.
- **Drug Interactions** — web page (check multiple products at once, severity labels)
  backed by a 35+ pair knowledge base (Warfarin, Metformin, Lisinopril, Simvastatin, …).

### Platform
- Role-based access (Admin / Pharmacist / Clerk) with a driven menu
- JWT auth with refresh tokens, password hashing (BCrypt), NLog logging
- Audit-trail capture on key actions
- **Setup Checklist** — a readiness page (Settings → Setup Checklist) that points out
  missing suppliers, uncategorised/unpriced products, unbatch-tracked stock and expired
  or expiring batches, with one-click links to fix each item.

---

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Web API, JwtBearer, Newtonsoft.Json
- **Front-end:** ASP.NET Core MVC (Razor), jQuery + DataTables, Bootstrap 3 theme, SweetAlert2
- **Database:** MySQL 8 (stored-procedure driven)
- **Data access:** MySqlConnector (all queries/sp-calls go through `DBHandler`)
- **OCR / PDF:** PdfPig, PDFtoImage, Tesseract (with bundled `eng.traineddata`)
- **Logging:** NLog

---

## Project Structure

```
MediStock/
├─ MediStock.API/                 # REST API
│  ├─ Controllers/                #   Auth, Products, Suppliers, Sales, Customers, Stock,
│  │                              #   Finance, Reports, Inventory, AI, Dashboard, ...
│  ├─ Models/                     #   Models + DBHandler (single MySQL access point)
│  ├─ Helpers/                    #   JWT, logger manager, filtering
│  ├─ Services/                   #   InvoiceParsingService (PDF/OCR)
│  └─ tessdata/                   #   Tesseract language data (eng.traineddata)
├─ MediStock.Portal/              # MVC BFF (proxy to API)
│  ├─ Controllers/                #   Page controllers + ApiClient proxy helpers
│  ├─ Services/                   #   ApiClient, AuditService
│  └─ Views/                      #   Razor pages (Dashboard, Products, Suppliers, AI, ...)
└─ database/                      # MySQL schema + stored procedures (apply in order)
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (targets `net10.0`)
- MySQL 8.x instance (schema/procedures below assume MySQL 8)
- Internet access for first `dotnet restore`

---

## Getting Started

### 1. Database

Create a database (e.g. `medistock`) then apply the SQL files **in numeric order**:

```bash
mysql -u <user> -p <medistock> < database/01_tables.sql
mysql -u <user> -p <medistock> < database/02_generic_procedures.sql
mysql -u <user> -p <medistock> < database/03_domain_procedures.sql
# ...04, 05 ...
# ...then all remaining 06–17 fix/migration files in order...
```

> No schema migration tool is used — the numbered files are the migration history.

### 2. Configure the API

Set the connection string and JWT settings in
`MediStock.API/appsettings.json` (ConnectionStrings / Jwt / Logging). Occlusion of
credentials from commits for team use is a good next step.

### 3. Run

```bash
# Terminal 1 — API
cd MediStock.API
dotnet run --urls http://localhost:5088

# Terminal 2 — Portal
cd MediStock.Portal
dotnet run --urls http://localhost:5087
```

Open `http://localhost:5087` and log in (seed accounts are created by the database
seed scripts in `database/05_seed_data.sql` / `database/13_superadmin_platform.sql`).

> **Windows build tip:** stop the running `MediStock.API.exe` / `MediStock.Portal.exe`
> processes before rebuilding — otherwise MSBuild fails with file-lock errors
> (MSB3021/MSB3027). Also start the build output exe with its `bin\Debug\net10.0`
> working directory so `tessdata` is found.

---

## Key API Endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Login, returns JWT (+ optional refresh) |
| `POST` | `/api/auth/resendotp` | Re-send a login OTP |
| `POST` | `/api/auth/resetpassword` | Email-only self-service reset, returns a temp password (`data.temp_password`) |
| `POST` | `/api/auth/changepassword` | Change password after verifying the current one |
| `POST` | `/api/auth/register-pharmacy` | Self-register a new pharmacy with an Admin owner (role 1 platform) |
| `GET` | `/api/menus` | Role-driven menu |
| `GET/POST` | `/api/products`, `/api/suppliers`, `/api/customers`, `/api/sales`, ... | CRUD modules (Riziki pattern) |
| `POST` | `/api/sales/voidsale` | Void a sale and restore stock |
| `GET` | `/api/notifications`, `/api/notifications/count` | Notification inbox + unread count |
| `POST` | `/api/notifications/dismiss`, `/api/notifications/markallread` | Notification handling |
| `GET` | `/api/dashboard/summary` | Dashboard stats |
| `GET` | `/api/dashboard/alerts` | Low-stock / out-of-stock alerts |
| `GET` | `/api/dashboard/expiringitems` | Expiring batches |
| `POST` | `/api/ai/predict-reorder` | Smart Reorder forecast (`lead_days` optional) |
| `POST` | `/api/ai/reorder-po` | Create a Pending PO from forecast lines |
| `POST` | `/api/ai/drug-interactions` | Interaction check for a medication list |
| `POST` | `/api/suppliers/import-invoice` | Multipart upload → parsed invoice JSON |
| `POST` | `/api/suppliers/import-confirm` | Add reviewed lines to stock (PO + batches) |

All endpoints require `Authorization: Bearer <jwt>`.

---

## How the pieces fit

- **BFF pattern** — the Portal never touches MySQL; every page action proxies through
  `ApiClient` to the API, which wraps responses as
  `{ success, message, action, data }`.
- **Stored procedures** — `get_records`, `get_records_by_id`, `delete_records`, and
  domain procs (`add_product`, `create_sale`, `receive_stock`, `add_purchase_order`, ...)
  hold the data logic; `DBHandler` is the only place that calls them.
- **Smart Reorder** uses the `sales_demand` / `low_stock_products` / `expiring_batches`
  branches of `get_records`.
- **Invoice Import** flow: `import-invoice` (parse) → Portal review grid (edit
  names/prices/expiry/skip) → `import-confirm` (write products, batches, PO, price history).

---

## Automated Tests

`MediStock.Tests` is an **integration test suite** (xUnit + WebApplicationFactory) that
boots the real API against a throwaway `medistock_test` database and exercises whole
flows end-to-end:

- **Provisioning** — DROP/CREATEs `medistock_test`, replays every `/database/*.sql`
  migration in order (with `DELIMITER` handling), then seeds a super-admin
  (`role_id = 1`) and admin (`role_id = 2`) pharmacy with password `Test@1234`.
  Because fresh installs rebuild from the same files, the suite **catches schema and
  procedure drift** that only the live database had accumulated by hand.
- **Coverage** — auth (login/OTP/reset/change/register-pharmacy), suppliers, products,
  categories, batches, stock adjustments, the full sale → return → void cycle, all six
  reports + `.xlsx` export, notifications, customers, expenses, admin stats, DDA,
  setup checklist (9 checks) and super-admin endpoints.
- **CI** — `.github/workflows/ci.yml` runs build + tests on every push/PR, using the
  `MEDISTOCK_TEST_SERVER` / `MEDISTOCK_TEST_USER` / `MEDISTOCK_TEST_PASSWORD`
  environment variables (or a `MEDISTOCK_TEST_PASSWORD` repo secret) instead of the
  local defaults.

```bash
dotnet test MediStock.Tests/MediStock.Tests.csproj
```

---

## Roadmap / Known Follow-ups

- Barcode scanning for receiving & sales
- Wholesale credit handling / printed receipts
- Stock expiry warnings in more surfaces (currently inside Smart Reorder)

> ### What was recently built (and shipped)
> - **Sales Returns** (API + page + menu) — atomic return SP restores stock, tracks
>   `returned_qty`, refuses over-returns.
> - **Excel report exports** — server-side `.xlsx` download for all reports.
> - **Setup Checklist** page (Settings → Setup Checklist) — readiness checks with fix links.
> - **Stock Performance report** (API + page + menu) — margin, 30-day movement, days-of-stock.
> - **Drug Interactions page** — knowledge base expanded to 35+ pairs with severity + recommendation.
> - **Reorder → Draft PO** — Smart Reorder now creates a real Pending purchase order.
> - **Import Supplier Invoice** shortcut from the Products page.
> - Dashboard summary bug fixed (stored-proc parameter names).
> - **Category auto-matching** on invoice import — new products inherit a category from their name
>   (e.g. “Ampiclox…” → Antibiotics) when one exists.
> - Audit `session_id` filled (per-request identifier) instead of `"TODO"`.
> - Removed dead `GetUnapprovedRecords` / `ApproveRecord` API surface.
> - DB connection string can be overridden with the `MEDISTOCK_DBCONN` environment variable.
> - **Void Sale** (POS) + notification raised on void.
> - **Notifications** module (inbox, unread count, dismiss, mark-all-read).
> - **Auth gaps closed** — self-service reset-password (email lookup + temp password shown
>   on the reset page until email sending lands), change-password, resend OTP,
>   register-pharmacy (self-registration flow).
> - **Integration test suite** (25 tests) + GitHub Actions CI — proves every endpoint and
>   repays the whole migration history against a fresh `medistock_test` database.
> - Fixed latent bugs found by the suite: `RemoteIpAddress` null-guard in audit capture
>   (login 500 under some hosts), `AddBatch` returning id 0 from `LAST_INSERT_ID()` on a
>   stray connection, `void_sale` double-restocking after partial returns, and the broken
>   `mig_add_menu_icon` helper that blocked fresh installs.

---

## License

Proprietary — for the MediStock project team. (Add your license here.)