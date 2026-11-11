-- Vehicle Service Center Management System Database Setup Script
-- Run this script in SQL Server Management Studio to create the database and tables

-- Create Database
IF DB_ID(N'VehicleServiceCenterDB') IS NULL CREATE DATABASE VehicleServiceCenterDB;
GO

USE VehicleServiceCenterDB;
GO

-- Create Users Table
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(200) NOT NULL,
    UserType NVARCHAR(20) NOT NULL
);
GO

-- Create Customers Table
IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Address NVARCHAR(200),
    NIC NVARCHAR(20)
);
GO

-- Create Vehicles Table
IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
CREATE TABLE Vehicles (
    VehicleID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT NOT NULL,
    VehicleNumber NVARCHAR(50) NOT NULL,
    Brand NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50),
    Year INT,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);
GO

-- Create Mechanics Table
IF OBJECT_ID(N'dbo.Mechanics', N'U') IS NULL
CREATE TABLE Mechanics (
    MechanicID INT PRIMARY KEY IDENTITY(1,1),
    MechanicName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Skill NVARCHAR(100)
);
GO

-- Create ServiceJobs Table
IF OBJECT_ID(N'dbo.ServiceJobs', N'U') IS NULL
CREATE TABLE ServiceJobs (
    ServiceID INT PRIMARY KEY IDENTITY(1,1),
    VehicleID INT NOT NULL,
    MechanicID INT NOT NULL,
    ServiceDate DATE NOT NULL,
    Problem NVARCHAR(500) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID),
    FOREIGN KEY (MechanicID) REFERENCES Mechanics(MechanicID)
);
GO

-- Create SpareParts Table
IF OBJECT_ID(N'dbo.SpareParts', N'U') IS NULL
CREATE TABLE SpareParts (
    PartID INT PRIMARY KEY IDENTITY(1,1),
    PartName NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);
GO

-- Create ServiceParts Table
IF OBJECT_ID(N'dbo.ServiceParts', N'U') IS NULL
CREATE TABLE ServiceParts (
    ServicePartID INT PRIMARY KEY IDENTITY(1,1),
    ServiceID INT NOT NULL,
    PartID INT NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (ServiceID) REFERENCES ServiceJobs(ServiceID),
    FOREIGN KEY (PartID) REFERENCES SpareParts(PartID)
);
GO

-- Create Invoices Table
IF OBJECT_ID(N'dbo.Invoices', N'U') IS NULL
CREATE TABLE Invoices (
    InvoiceID INT PRIMARY KEY IDENTITY(1,1),
    ServiceID INT NOT NULL,
    InvoiceDate DATE NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (ServiceID) REFERENCES ServiceJobs(ServiceID)
);
GO

-- Create Payments Table
IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
CREATE TABLE Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    InvoiceID INT NOT NULL,
    PaymentDate DATE NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID)
);
GO


-- Next run UpgradeDatabase.sql in the same database. No sample records or default credentials are installed.
