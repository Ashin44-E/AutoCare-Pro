# AutoCare-Pro

C# .NET 8 Windows Forms vehicle service center management, using SQL Server and ADO.NET. The existing project, forms and export libraries are retained.

## Run

```powershell
dotnet build VehicleServiceCenterManagementSystem/VehicleServiceCenterManagementSystem.csproj -c Release
& ./VehicleServiceCenterManagementSystem/bin/Release/net8.0-windows/VehicleServiceCenterManagementSystem.exe
```

The default connection uses Windows authentication against `localhost\SQLEXPRESS`, database `VehicleServiceCenterDB`. Override it without editing source:

```powershell
$env:AUTOCARE_CONNECTION_STRING='Server=YOUR_SERVER;Database=VehicleServiceCenterDB;Integrated Security=True;Connect Timeout=10;'
```

Use a SQL Server login/Windows principal with the necessary database permissions. The desktop role checks do not replace SQL Server access controls. The application does not alter its schema during registration or login.

## Database setup

The existing local database was backed up, upgraded and reconciled on 2026-11-11. No setup action is needed for that database.

For a new database:

```powershell
sqlcmd -S localhost\SQLEXPRESS -E -C -b -i VehicleServiceCenterManagementSystem/Database/SetupDatabase.sql
sqlcmd -S localhost\SQLEXPRESS -E -C -b -d VehicleServiceCenterDB -i VehicleServiceCenterManagementSystem/Database/UpgradeDatabase.sql
```

For another existing installation, take a backup first, inspect `Database/DataQuality.sql` and run `UpgradeDatabase.sql` against the intended database, then `Database/UpgradePasswords.ps1` to secure legacy plaintext credentials. `ReconcileDuplicates.sql` preserves account access with unique aliases, archives only identical unused vehicle duplicates, and labels historical duplicate invoices without changing amounts or payment links. It writes a SQL audit trail and is safe to rerun. Do not apply it to another business database without inspecting its duplicate records first.

No demo records or default passwords are installed in a fresh database. The first registered account becomes Admin under a database transaction; later registrations are User accounts.

## Accounts and permissions

- Admin: all operational screens, mechanic/inventory management, financial reports, deletions, password resets and existing-data review.
- User: customer and vehicle management, service jobs, invoices, payments and operational reports. Cannot delete records or open administrator screens.
- Passwords use versioned, salted PBKDF2-SHA256 hashes. Existing hashes remain compatible; successful plaintext logins immediately upgrade their storage. Existing short passwords must be changed before opening the dashboard.
- Five failed authentication/recovery attempts temporarily lock the account for 15 minutes. Successful administrator reset clears the lock.
- Registration and password changes display a recovery code. Save it securely: recovery rotates it, so an old code cannot be reused. Users without a code can ask an authenticated administrator to reset their account.
- Existing `admin` and `user` login names retain the oldest accounts. Their duplicate accounts were renamed `admin-legacy-3` and `user-legacy-4`, retaining their original credentials and roles.

## Workflows

1. Add customers and their vehicles. Mileage is saved; vehicle numbers are unique. Vehicles with service history cannot be reassigned to another owner, preserving invoice ownership.
2. Admin adds mechanics and inventory. Names, contact details, quantities and prices are validated. Referenced master records cannot be deleted. Stock edits detect changes made since the record was loaded.
3. Create a service with a vehicle, mechanic, date, description, status and charges. Add parts from Available parts; inspect/remove them in Job parts. Labor hours suggest the original application's rate of 50 per hour, and the service charge is editable. Parts are charged separately.
4. Saving a service atomically records part prices and deducts stock. Editing an uninvoiced job restores its previous stock before applying the revised quantities. Deleting an uninvoiced job restores stock. Insufficient stock rolls back the whole operation.
5. Complete the service, then create its invoice. The issued amount comes from recorded service and part charges. Issued jobs/invoices cannot be edited or deleted through the application. View / Print shows customer, vehicle, job, line items, payments and balance; printing uses installed Windows printers, including Print to PDF where available.
6. Record full or partial payments. The application locks the invoice while checking its balance, preventing simultaneous overpayments. Dates cannot precede the invoice or be in the future. Historical duplicate invoices cannot receive new payments.
7. Monthly reports use actual service dates and show billing and receipts separately. Financial reports use invoice/payment dates and support the existing Excel, CSV and PDF exports. Historical duplicate billing is separately identified rather than added to active billing; all recorded receipts are retained. The application does not invent a profit figure from sales charges.

## Preserved historical issues

Open **Dashboard ? Existing Data Review** as Admin. Existing blank customer details, missing service charges, estimated legacy part prices, duplicate invoices and overpayments are visible there. These are source-data issues, not values the application can reliably invent.

- Customer IDs 1 and 4 have missing names/phones.
- Service IDs 5–8 have no recorded service charge.
- Eight historical part-use rows had no unit price. The migration initialized these from current inventory prices and marked them as estimates; issued invoice amounts were not recalculated.
- Invoices 3 and 4 are marked historical duplicates of 1 and 2. All five invoices and five payment records remain intact.
- Invoice 1 has 1,100.00 recorded excess payment and invoice 2 has 15,500.00. These are displayed as credits/anomalies; no payment was erased or refund fabricated.
- Unused duplicate vehicle IDs 7, 8 and 9 were archived to `ReconciliationAudit` before removal; their canonical records are 2, 3 and 4.

Verified pre-upgrade backup:
`D:\SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\AutoCarePro_before_upgrade_20261111_01.bak`

The database administrator can restore that backup if needed. `ReconciliationAudit` also retains original vehicle/invoice data and account rename history.

## Verification

See `REVIEW.md` for findings, changes and test results. Run the integration harness against a newly created isolated database:

```powershell
./Tests/RunTests.ps1
```

The script creates a uniquely named `AutoCarePro_Test_...` database and leaves it for inspection. Tests never use the business database. They cover authentication, recovery, roles, CRUD, SQL-injection input, relationships, stock rollback, invoice calculations, partial/concurrent payments, reports, form loading and rendering. Screenshots are written to `Tests/artifacts`.

Error logs for unexpected application failures are written to `%LOCALAPPDATA%\AutoCare-Pro\errors.log`. Physical printer output and interactive export dialogs require a workstation check with the intended printer and destination.

