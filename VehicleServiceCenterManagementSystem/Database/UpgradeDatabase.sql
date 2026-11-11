SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
-- Run against the intended database using sqlcmd -b. Additive; never deletes or merges records.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.ServiceJobParts','U') IS NOT NULL
    THROW 50001, 'Unexpected ServiceJobParts table: reconcile it with ServiceParts before upgrading.', 1;
ALTER TABLE dbo.Users ALTER COLUMN Password nvarchar(200) NOT NULL;
IF COL_LENGTH('Users','RecoveryHash') IS NULL ALTER TABLE dbo.Users ADD RecoveryHash nvarchar(200) NULL;
IF COL_LENGTH('Users','FailedAttempts') IS NULL ALTER TABLE dbo.Users ADD FailedAttempts int NOT NULL CONSTRAINT DF_Users_Failed DEFAULT 0;
IF COL_LENGTH('Users','LockedUntil') IS NULL ALTER TABLE dbo.Users ADD LockedUntil datetime2 NULL;
IF COL_LENGTH('Invoices','LegacyDuplicateOf') IS NULL ALTER TABLE dbo.Invoices ADD LegacyDuplicateOf int NULL;
IF COL_LENGTH('Customers','Email') IS NULL ALTER TABLE dbo.Customers ADD Email nvarchar(100) NULL;
IF COL_LENGTH('Vehicles','Mileage') IS NULL ALTER TABLE dbo.Vehicles ADD Mileage int NULL;
IF COL_LENGTH('ServiceJobs','ServiceCost') IS NULL ALTER TABLE dbo.ServiceJobs ADD ServiceCost decimal(10,2) NULL;
IF COL_LENGTH('ServiceJobs','LaborHours') IS NULL ALTER TABLE dbo.ServiceJobs ADD LaborHours decimal(5,2) NULL;
IF COL_LENGTH('ServiceParts','UnitCost') IS NULL ALTER TABLE dbo.ServiceParts ADD UnitCost decimal(10,2) NULL;
IF COL_LENGTH('ServiceParts','PriceEstimated') IS NULL ALTER TABLE dbo.ServiceParts ADD PriceEstimated bit NOT NULL CONSTRAINT DF_ServiceParts_Estimated DEFAULT 0;
GO
UPDATE sp SET UnitCost=p.Price, PriceEstimated=1 FROM dbo.ServiceParts sp JOIN dbo.SpareParts p ON p.PartID=sp.PartID WHERE sp.UnitCost IS NULL;
ALTER TABLE dbo.ServiceParts ALTER COLUMN UnitCost decimal(10,2) NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Stock_Valid')
 ALTER TABLE dbo.SpareParts WITH CHECK ADD CONSTRAINT CK_Stock_Valid CHECK(Quantity>=0 AND Price>=0 AND LEN(LTRIM(RTRIM(PartName)))>0);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_ServiceParts_Valid')
 ALTER TABLE dbo.ServiceParts WITH CHECK ADD CONSTRAINT CK_ServiceParts_Valid CHECK(Quantity>0 AND UnitCost>=0);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Payment_Valid')
 ALTER TABLE dbo.Payments WITH CHECK ADD CONSTRAINT CK_Payment_Valid CHECK(Amount>0 AND PaymentMethod IN('Cash','Card','Other'));
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Invoice_Valid')
 ALTER TABLE dbo.Invoices WITH CHECK ADD CONSTRAINT CK_Invoice_Valid CHECK(TotalAmount>=0);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Service_Valid')
 ALTER TABLE dbo.ServiceJobs WITH CHECK ADD CONSTRAINT CK_Service_Valid CHECK((ServiceCost IS NULL OR ServiceCost>=0) AND (LaborHours IS NULL OR LaborHours>=0) AND Status IN('Pending','Repairing','Completed'));
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Vehicle_Valid')
 ALTER TABLE dbo.Vehicles WITH CHECK ADD CONSTRAINT CK_Vehicle_Valid CHECK((Mileage IS NULL OR Mileage>=0) AND (Year IS NULL OR Year BETWEEN 1886 AND 9999));
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ServiceParts_Service') CREATE INDEX IX_ServiceParts_Service ON dbo.ServiceParts(ServiceID);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Payments_Invoice') CREATE INDEX IX_Payments_Invoice ON dbo.Payments(InvoiceID) INCLUDE(Amount);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ServiceJobs_Date') CREATE INDEX IX_ServiceJobs_Date ON dbo.ServiceJobs(ServiceDate);
GO
CREATE OR ALTER TRIGGER dbo.TR_Users_Unique ON dbo.Users AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.UserID=i.UserID
  WHERE (d.UserID IS NULL OR i.Username<>d.Username) AND EXISTS(SELECT 1 FROM dbo.Users u WITH(UPDLOCK,HOLDLOCK) WHERE u.Username=i.Username AND u.UserID<>i.UserID))
 THROW 50002,'Username already exists. Existing duplicate accounts must be reconciled by the database administrator.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_Vehicles_Unique ON dbo.Vehicles AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.VehicleID=i.VehicleID
  WHERE (d.VehicleID IS NULL OR i.VehicleNumber<>d.VehicleNumber) AND EXISTS(SELECT 1 FROM dbo.Vehicles v WITH(UPDLOCK,HOLDLOCK) WHERE v.VehicleNumber=i.VehicleNumber AND v.VehicleID<>i.VehicleID))
 THROW 50003,'Vehicle number already exists.',1;
 -- Preserve historical ownership shown on invoices.
 IF EXISTS(SELECT 1 FROM inserted i JOIN deleted d ON d.VehicleID=i.VehicleID JOIN dbo.ServiceJobs s ON s.VehicleID=i.VehicleID WHERE i.CustomerID<>d.CustomerID)
 THROW 50004,'A vehicle with service history cannot be reassigned to another owner.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_Invoices_Valid ON dbo.Invoices AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.InvoiceID=i.InvoiceID WHERE d.InvoiceID IS NULL AND
 (EXISTS(SELECT 1 FROM dbo.Invoices v WITH(UPDLOCK,HOLDLOCK) WHERE v.ServiceID=i.ServiceID AND v.InvoiceID<>i.InvoiceID)
 OR NOT EXISTS(SELECT 1 FROM dbo.ServiceJobs s WHERE s.ServiceID=i.ServiceID AND s.Status='Completed' AND s.ServiceCost IS NOT NULL
 AND i.InvoiceDate>=s.ServiceDate AND i.TotalAmount=s.ServiceCost+COALESCE((SELECT SUM(CONVERT(decimal(18,2),p.Quantity)*p.UnitCost) FROM dbo.ServiceParts p WHERE p.ServiceID=s.ServiceID),0))))
 THROW 50005,'Invoice requires a completed, uninvoiced service and its recorded charges.',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN deleted d ON i.InvoiceID=d.InvoiceID WHERE i.TotalAmount<>d.TotalAmount OR i.ServiceID<>d.ServiceID OR i.InvoiceDate<>d.InvoiceDate)
 THROW 50006,'Issued invoices cannot be changed.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_Payments_Balance ON dbo.Payments AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted p JOIN dbo.Invoices i WITH(UPDLOCK,HOLDLOCK) ON p.InvoiceID=i.InvoiceID
 WHERE p.PaymentDate<i.InvoiceDate OR p.PaymentDate>CONVERT(date,GETDATE()) OR
 (SELECT SUM(Amount) FROM dbo.Payments WHERE InvoiceID=i.InvoiceID)>i.TotalAmount)
 THROW 50007,'Payment exceeds the invoice balance or has an invalid date.',1;
END;
GO
IF NOT EXISTS(SELECT Username FROM dbo.Users GROUP BY Username HAVING COUNT(*)>1) AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Users_Username') CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users(Username);
IF NOT EXISTS(SELECT VehicleNumber FROM dbo.Vehicles GROUP BY VehicleNumber HAVING COUNT(*)>1) AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Vehicles_Number') CREATE UNIQUE INDEX UX_Vehicles_Number ON dbo.Vehicles(VehicleNumber);
IF NOT EXISTS(SELECT ServiceID FROM dbo.Invoices GROUP BY ServiceID HAVING COUNT(*)>1) AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UX_Invoices_Service') CREATE UNIQUE INDEX UX_Invoices_Service ON dbo.Invoices(ServiceID);

GO
-- Prevent new duplicate master records while preserving historical duplicates.
CREATE OR ALTER TRIGGER dbo.TR_Customers_Identity ON dbo.Customers AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.CustomerID=i.CustomerID WHERE
 LEN(LTRIM(RTRIM(i.CustomerName)))=0 OR LEN(LTRIM(RTRIM(i.Phone)))=0 OR
 (NULLIF(LTRIM(RTRIM(i.NIC)),'') IS NOT NULL AND (d.CustomerID IS NULL OR ISNULL(i.NIC,'')<>ISNULL(d.NIC,'')) AND EXISTS(SELECT 1 FROM dbo.Customers c WITH(UPDLOCK,HOLDLOCK) WHERE c.NIC=i.NIC AND c.CustomerID<>i.CustomerID)))
 THROW 50008,'Enter a name and phone; this NIC may already belong to another customer.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_Mechanics_Identity ON dbo.Mechanics AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.MechanicID=i.MechanicID WHERE
 LEN(LTRIM(RTRIM(i.MechanicName)))=0 OR LEN(LTRIM(RTRIM(i.Phone)))=0 OR
 ((d.MechanicID IS NULL OR i.MechanicName<>d.MechanicName OR i.Phone<>d.Phone) AND EXISTS(SELECT 1 FROM dbo.Mechanics m WITH(UPDLOCK,HOLDLOCK) WHERE m.MechanicName=i.MechanicName AND m.Phone=i.Phone AND m.MechanicID<>i.MechanicID)))
 THROW 50009,'Enter a name and phone; this mechanic may already exist.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_Parts_Identity ON dbo.SpareParts AFTER INSERT, UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.PartID=i.PartID WHERE
 (d.PartID IS NULL OR i.PartName<>d.PartName) AND EXISTS(SELECT 1 FROM dbo.SpareParts p WITH(UPDLOCK,HOLDLOCK) WHERE p.PartName=i.PartName AND p.PartID<>i.PartID))
 THROW 50010,'Part name already exists. Update the existing stock record.',1;
END;
GO

COMMIT;
PRINT 'Upgrade complete. Existing records preserved.';
GO
