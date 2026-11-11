-- Read-only reconciliation report. Do not merge invoices/payments without owner review.
SELECT Username,COUNT(*) AS Records FROM Users GROUP BY Username HAVING COUNT(*)>1;
SELECT VehicleNumber,COUNT(*) AS Records FROM Vehicles GROUP BY VehicleNumber HAVING COUNT(*)>1;
SELECT ServiceID,COUNT(*) AS Records FROM Invoices GROUP BY ServiceID HAVING COUNT(*)>1;
SELECT ServicePartID,ServiceID,PartID,UnitCost FROM ServiceParts WHERE PriceEstimated=1;
SELECT ServiceID FROM ServiceJobs WHERE ServiceCost IS NULL;
SELECT i.InvoiceID,i.TotalAmount,SUM(p.Amount) AS Paid FROM Invoices i JOIN Payments p ON p.InvoiceID=i.InvoiceID GROUP BY i.InvoiceID,i.TotalAmount HAVING SUM(p.Amount)>i.TotalAmount;
SELECT CustomerID,CustomerName,Phone FROM Customers WHERE LEN(LTRIM(RTRIM(CustomerName)))=0 OR LEN(LTRIM(RTRIM(Phone)))=0;
SELECT InvoiceID,ServiceID,LegacyDuplicateOf,TotalAmount FROM Invoices WHERE LegacyDuplicateOf IS NOT NULL;
