SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
-- Reconcile duplicates without losing accounts, service history, invoices or payments.
-- Apply UpgradeDatabase.sql first; take a backup before applying this script.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.ReconciliationAudit','U') IS NULL
 CREATE TABLE dbo.ReconciliationAudit(AuditID int IDENTITY PRIMARY KEY,EntityName nvarchar(50) NOT NULL,RecordID int NOT NULL,OriginalData nvarchar(max) NOT NULL,ActionTaken nvarchar(500) NOT NULL,ChangedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
-- Preserve all accounts: oldest account retains the username; later duplicates get stable aliases.
DECLARE @users TABLE(UserID int,OldName nvarchar(50),NewName nvarchar(50));
INSERT @users SELECT UserID,Username,LEFT(Username,30)+'-legacy-'+CONVERT(varchar(12),UserID)
 FROM(SELECT *,ROW_NUMBER() OVER(PARTITION BY Username ORDER BY UserID) rn FROM dbo.Users WITH(UPDLOCK,HOLDLOCK)) u WHERE rn>1;
IF EXISTS(SELECT 1 FROM @users d JOIN dbo.Users u ON u.Username=d.NewName) THROW 50020,'Legacy account alias collision: no changes applied.',1;
INSERT dbo.ReconciliationAudit(EntityName,RecordID,OriginalData,ActionTaken)
 SELECT 'Users',UserID,(SELECT OldName AS Username FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),'Renamed duplicate login to '+NewName FROM @users;
UPDATE u SET Username=d.NewName FROM dbo.Users u JOIN @users d ON d.UserID=u.UserID;
-- Archive only redundant vehicles with identical identity/owner and no service references.
DECLARE @vehicles TABLE(VehicleID int,CanonicalID int);
INSERT @vehicles SELECT v.VehicleID,(SELECT MIN(c.VehicleID) FROM dbo.Vehicles c WHERE c.VehicleNumber=v.VehicleNumber)
 FROM dbo.Vehicles v WITH(UPDLOCK,HOLDLOCK)
 WHERE EXISTS(SELECT 1 FROM dbo.Vehicles c WHERE c.VehicleID<v.VehicleID AND c.VehicleNumber=v.VehicleNumber AND c.CustomerID=v.CustomerID AND c.Brand=v.Brand AND ISNULL(c.Model,'')=ISNULL(v.Model,'') AND ISNULL(c.Year,0)=ISNULL(v.Year,0) AND (v.Mileage IS NULL OR c.Mileage=v.Mileage))
 AND NOT EXISTS(SELECT 1 FROM dbo.ServiceJobs s WHERE s.VehicleID=v.VehicleID);
INSERT dbo.ReconciliationAudit(EntityName,RecordID,OriginalData,ActionTaken)
 SELECT 'Vehicles',v.VehicleID,(SELECT v.* FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),'Archived unused duplicate of vehicle '+CONVERT(varchar(12),d.CanonicalID)
 FROM dbo.Vehicles v JOIN @vehicles d ON d.VehicleID=v.VehicleID;
DELETE v FROM dbo.Vehicles v JOIN @vehicles d ON d.VehicleID=v.VehicleID;
-- Financial amounts and payment links remain untouched. Mark historical duplicate bills.
DECLARE @invoices TABLE(InvoiceID int,CanonicalID int);
INSERT @invoices SELECT InvoiceID,CanonicalID FROM(SELECT InvoiceID,LegacyDuplicateOf,MIN(InvoiceID) OVER(PARTITION BY ServiceID) CanonicalID FROM dbo.Invoices WITH(UPDLOCK,HOLDLOCK)) i WHERE InvoiceID<>CanonicalID AND LegacyDuplicateOf IS NULL;
INSERT dbo.ReconciliationAudit(EntityName,RecordID,OriginalData,ActionTaken)
 SELECT 'Invoices',i.InvoiceID,(SELECT i.* FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),'Marked historical duplicate of invoice '+CONVERT(varchar(12),d.CanonicalID)+'; amounts and payments retained'
 FROM dbo.Invoices i JOIN @invoices d ON d.InvoiceID=i.InvoiceID;
UPDATE i SET LegacyDuplicateOf=d.CanonicalID FROM dbo.Invoices i JOIN @invoices d ON d.InvoiceID=i.InvoiceID;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Users_Username') CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users(Username);
IF NOT EXISTS(SELECT VehicleNumber FROM dbo.Vehicles GROUP BY VehicleNumber HAVING COUNT(*)>1) AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Vehicles_Number') CREATE UNIQUE INDEX UX_Vehicles_Number ON dbo.Vehicles(VehicleNumber);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Invoices_ActiveService') CREATE UNIQUE INDEX UX_Invoices_ActiveService ON dbo.Invoices(ServiceID) WHERE LegacyDuplicateOf IS NULL;
COMMIT;
SELECT EntityName,RecordID,ActionTaken FROM dbo.ReconciliationAudit ORDER BY AuditID;
GO
