# AutoCare-Pro review and improvement log

## Inspection findings (2026-11-11)

Reviewed every form's implementation, designer event wiring, application startup, password helper, database helper, SQL scripts, project dependencies, and live SQL Server schema/constraints.

- Customer, vehicle, mechanic, inventory, service, invoice, payment and report queries concatenate input into SQL.
- Live schema has ServiceParts; service creation and invoices reference nonexistent ServiceJobParts. Service creation also references missing cost columns.
- Mileage is displayed but never saved. Updates omit validation. Numeric values and dates are passed as strings.
- Inventory deductions are not conditional on stock; repeated part selections can exceed stock.
- Invoices use a fixed 500 service charge and current part prices instead of recorded charges.
- Payments accept invalid amounts and exclude partially paid invoices.
- Registration permits public Admin selection and alters the schema at runtime. Login ignores roles, keeps a reader open for the entire session, and cannot authenticate existing plaintext accounts.
- No change-password or actual recovery workflow. Back navigation recursively opens Home forms.
- Financial report is unreachable and calculates a meaningless zero profit. Monthly filtering wraps indexed dates in a conversion.
- Connection failures escape transaction handling; database resources are not consistently disposed; global UI errors are swallowed.
- Setup inserts sample data and default plaintext credentials. Missing-field script overwrites existing records.
- Live database: 9 customers, 10 vehicles, 6 mechanics, 8 service jobs, 10 parts, 8 service parts, 5 invoices, 5 payments, 8 users. Existing FK relationships are present. Duplicate groups: 3 vehicle numbers, 2 usernames, 2 invoiced service IDs. No negative inventory detected. Existing data must be preserved and duplicates must not be silently merged or deleted.

## Implementation order

1. Parameterized database helpers, validation, safe additive schema migration.
2. Authentication/session permissions, registration, password change/recovery and navigation.
3. Master-record CRUD, mileage, stock validation.
4. Transactional service parts, recorded charges, invoices and partial payments.
5. Consistent desktop UI, reports and existing exports.
6. Build, isolated database integration tests, form checks and documented limitations.

## Data decisions

Keep ServiceParts and ServiceCost as the existing canonical tables/columns. Preserve historical invoices and payments. Legacy part usage has no recorded unit price; migration can only initialize it from the current inventory price, explicitly marking it as an estimate. The initial preservation plan was superseded by the instruction to resolve duplicates autonomously; see the audited reconciliation below. New duplicate inserts are rejected even when historical duplicates prevent a unique index.

Admin manages mechanics, inventory, account resets, financial reports and deletions. User handles customer/vehicle/service operations, invoices, payments and operational reports. SQL Server permissions remain the deployment security boundary for clients with direct database access.

## Completed improvements

- Retained C#, .NET 8 Windows Forms, SQL Server, ADO.NET, existing forms and export libraries.
- Parameterized queries, disposed database resources, handled transaction connection failures and added understandable errors.
- Added roles, safe registration, password change/recovery, administrator reset, lockout and legacy password compatibility. Removed registration-time schema changes.
- Added validation, persisted mileage, guarded stock edits against stale quantities and protected related records.
- Standardized on ServiceParts and ServiceCost. Added transactional service creation/edit/deletion, stock restoration and conditional deductions, and captured unit prices.
- Calculated invoices from saved charges; protected issued services; added itemized viewing/printing, partial payments and concurrent balance protection.
- Repaired invisible grids and overlapping service controls with reusable responsive layouts and consistent AutoCare-Pro branding.
- Connected financial reports, replaced false profit calculations with billed/received totals, added monthly date filters, repaired CSV escaping and PDF resource disposal.
- Added safe schema upgrades, checks/indexes, duplicate guards, existing-data review and audited reconciliation. Removed sample data/default credentials from fresh setup.

## User-authorized reconciliation

After duplicate records were identified, the user directed autonomous resolution. A full backup was verified, restored into an isolated copy, and upgrade/reconciliation tested there before applying it to the original database.

- User IDs 3 and 4 retain credentials/roles as admin-legacy-3 and user-legacy-4. Original admin/user names remain on IDs 1 and 2.
- Unused vehicle IDs 7, 8 and 9 exactly matched older vehicles and were archived in ReconciliationAudit before removal.
- Invoices 3 and 4 are marked LegacyDuplicateOf 1 and 2. A filtered unique index protects active invoices. All five invoice amounts and five payment records remain intact. Historical duplicates cannot receive further payments.
- Existing overpayments are displayed as credits. Reports separate legacy duplicate billing and retain all receipts. No refunds or replacement financial records were invented.
- Password-storage migration was tested on the copy, including confirmation that the original administrator password still matched its resulting hash. Six plaintext accounts were identified for migration; the two existing legacy hashes remain compatible and upgrade on login.

## Verification

- Debug application/test builds and Release application build: zero warnings, zero errors.
- Initial integration run: 68 assertions passed on a clean isolated database, including first-account Admin bootstrap.
- Final integration run: 83 assertions passed on the upgraded/reconciled backup copy. Covered authentication/recovery/roles, master CRUD, apostrophe/injection input, mileage, foreign keys, stock rollback/edit/delete, saved prices, invoice totals, partial and concurrent payments.
- All application screens loaded/rendered, all operational report handlers and financial generation invoked. Checked visible grids remain inside containers and service part quantity starts positive. Screenshot inspection exposed and led to repair of the original invisible-grid and overlapping-control defects.
- Schema rerun safety and reconciliation idempotence verified on the backup copy.
- Verified monetary values, payment records and credentials were preserved during reconciliation, while duplicate active usernames, vehicles and invoices were resolved.
- Backup completed with CHECKSUM and RESTORE VERIFYONLY.

## Retained source-data issues and test limits

- Customer IDs 1 and 4 lack names/phones; service IDs 5-8 lack charges. Existing Data Review identifies these for correction from real source information.
- Eight historical part-use rows lacked prices; initialized prices are explicitly marked estimates. Original invoice totals were preserved where historical detail differs.
- Credits on invoices 1 and 2 remain visible. Refund/reconciliation requires underlying payment evidence; no ledger amounts were erased.
- Tests exercised application database/service code, report handlers and form rendering. Physical printer output, interactive export destinations and a complete operator acceptance walkthrough were not verified.
- No Git commit was created. The original application was already untracked in this workspace; changes remain available in place.


## Final live verification

Password migration completed: 8 accounts now contain hashes (6 current PBKDF2, 2 compatible legacy hashes); no plaintext credentials remain. All 5 invoices (33,700.00 total) and 5 payments (34,300.00 total) remain preserved. There are 7 vehicle records, 7 audited reconciliation actions, no duplicate active invoices, and no untrusted foreign keys/check constraints. Release executable is available under `VehicleServiceCenterManagementSystem/bin/Release/net8.0-windows/`.

## Role Registration and Management Access Resolution (2026-11-11)

- **Registration Role Selection**: Restored enabled role dropdown (`cmbUserType`) on `RegisterForm` with `Admin` and `User` choices. `Accounts.Register` now accepts and persists the chosen role into `Users.UserType`.
- **Case-Insensitive Admin Detection**: Updated `Session.IsAdmin` and `Accounts.Login` to normalize roles case-insensitively and handle whitespace or variants (e.g. `admin`, `ADMIN`, `Administrator`), ensuring admin privileges are reliably assigned upon login.
- **Admin Management Access**: Wired `ApplyRolePermissions()` on `DashboardForm` to consistently enable `btnMechanic` and `btnSpareParts` for admin users on load. Wrapped navigation handlers in friendly permission checks.
- **Password Length Range**: Adjusted minimum password validation in `PasswordHelper.Valid` to 4–128 characters, allowing standard passwords without forcing unwanted resets on login.
- **Verification**: Verified existing test suite and added assertions for explicit `Admin` and `User` registrations and admin access to `MechanicForm` and `SparePartsForm`. All 88 assertions passed. Debug and Release builds succeeded with zero warnings and zero errors.
