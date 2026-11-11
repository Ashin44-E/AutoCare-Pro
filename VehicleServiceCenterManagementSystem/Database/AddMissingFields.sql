-- Compatibility script: safe to rerun. Does not overwrite existing data.
USE VehicleServiceCenterDB;
GO
IF COL_LENGTH('Customers','Email') IS NULL ALTER TABLE Customers ADD Email nvarchar(100) NULL;
IF COL_LENGTH('Vehicles','Mileage') IS NULL ALTER TABLE Vehicles ADD Mileage int NULL;
IF COL_LENGTH('ServiceJobs','ServiceCost') IS NULL ALTER TABLE ServiceJobs ADD ServiceCost decimal(10,2) NULL;
IF COL_LENGTH('ServiceJobs','LaborHours') IS NULL ALTER TABLE ServiceJobs ADD LaborHours decimal(5,2) NULL;
GO
-- Run UpgradeDatabase.sql for the complete application upgrade.
