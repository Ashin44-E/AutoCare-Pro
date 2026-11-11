using System;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;
using VehicleServiceCenterManagementSystem.Helpers;
namespace VehicleServiceCenterManagementSystem.Forms
{
    public sealed class DataIssuesForm:Form
    {
        public DataIssuesForm()
        {
            Session.Require(true);Text="Existing data review";Ui.Fit(this,1050,650);
            var tabs=new TabControl{Dock=DockStyle.Fill};Controls.Add(tabs);
            Add(tabs,"Incomplete customers","SELECT CustomerID,CustomerName,Phone,Address,NIC FROM Customers WHERE LEN(LTRIM(RTRIM(CustomerName)))=0 OR LEN(LTRIM(RTRIM(Phone)))=0");
            Add(tabs,"Missing service charges","SELECT ServiceID,VehicleID,ServiceDate,Problem,Status FROM ServiceJobs WHERE ServiceCost IS NULL");
            Add(tabs,"Legacy invoices / credits","SELECT i.InvoiceID,i.ServiceID,i.LegacyDuplicateOf,i.TotalAmount,COALESCE(p.Paid,0) Paid,CASE WHEN COALESCE(p.Paid,0)>i.TotalAmount THEN p.Paid-i.TotalAmount ELSE 0 END Credit FROM Invoices i OUTER APPLY(SELECT SUM(Amount) Paid FROM Payments WHERE InvoiceID=i.InvoiceID) p WHERE i.LegacyDuplicateOf IS NOT NULL OR p.Paid>i.TotalAmount");
            Add(tabs,"Estimated part prices","SELECT s.ServicePartID,s.ServiceID,p.PartName,s.Quantity,s.UnitCost FROM ServiceParts s JOIN SpareParts p ON p.PartID=s.PartID WHERE s.PriceEstimated=1");
            if(Convert.ToInt32(DBConnection.ExecuteScalar("SELECT COUNT(*) FROM sys.tables WHERE name='ReconciliationAudit'"))>0)
                Add(tabs,"Reconciliation history","SELECT EntityName,RecordID,ActionTaken,ChangedAt FROM ReconciliationAudit ORDER BY AuditID");
            var note=new Label{Dock=DockStyle.Bottom,Height=50,Text="Existing records are preserved. Correct customer details and uninvoiced charges in their management screens. Historical credits and estimated prices must be checked against source documents."};Controls.Add(note);Ui.Apply(this);
        }
        private static void Add(TabControl tabs,string title,string query)
        {
            var page=new TabPage(title);var grid=new DataGridView{Dock=DockStyle.Fill,DataSource=DBConnection.ExecuteQuery(query),AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells};page.Controls.Add(grid);tabs.TabPages.Add(page);
        }
    }
}
