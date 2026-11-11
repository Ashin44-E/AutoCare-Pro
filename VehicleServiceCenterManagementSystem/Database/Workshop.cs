using System;
using System.Data;
using VehicleServiceCenterManagementSystem.Helpers;
using static VehicleServiceCenterManagementSystem.Database.DBConnection;
namespace VehicleServiceCenterManagementSystem.Database
{
    public static class Workshop
    {
        public static int SaveJob(int? id,int vehicle,int mechanic,DateTime date,string problem,string status,decimal hours,decimal charge,DataTable parts)
        {
            Session.Require();
            if(string.IsNullOrWhiteSpace(problem)||problem.Length>500||hours<0||hours>999.99m||charge<0||charge>99999999.99m||decimal.Round(charge,2)!=charge||decimal.Round(hours,2)!=hours||date.Date>DateTime.Today||!(status=="Pending"||status=="Repairing"||status=="Completed"))throw new ArgumentException("Enter a valid service date, problem, status, labor hours and service charge.");
            int saved=id??0;
            bool ok=ExecuteTransaction((c,t)=>
            {
                if(id.HasValue)
                {
                    using var check=Command(c,t,"SELECT COUNT(*) FROM Invoices WITH(UPDLOCK,HOLDLOCK) WHERE ServiceID=@id",P("@id",id.Value));
                    if((int)check.ExecuteScalar()>0)throw new InvalidOperationException("An invoiced service cannot be changed.");
                    using var restore=Command(c,t,"UPDATE p SET Quantity=p.Quantity+x.Used FROM SpareParts p JOIN (SELECT PartID,SUM(Quantity) Used FROM ServiceParts WHERE ServiceID=@id GROUP BY PartID) x ON x.PartID=p.PartID; DELETE FROM ServiceParts WHERE ServiceID=@id;",P("@id",id.Value));restore.ExecuteNonQuery();
                }
                foreach(DataRow row in parts.Rows)
                {
                    int qty=Convert.ToInt32(row["Qty"]);decimal unit=Convert.ToDecimal(row["UnitCost"]);
                    if(qty<=0||unit<0)throw new ArgumentException("Part quantities must be positive and prices nonnegative.");
                    using var stock=Command(c,t,"UPDATE SpareParts SET Quantity=Quantity-@q WHERE PartID=@p AND Quantity>=@q",P("@q",qty),P("@p",row["PartID"]));
                    if(stock.ExecuteNonQuery()!=1)throw new InvalidOperationException("Insufficient stock. Refresh the parts list and try again.");
                }
                using var save=Command(c,t,id.HasValue?
                    "UPDATE ServiceJobs SET VehicleID=@v,MechanicID=@m,ServiceDate=@d,Problem=@p,Status=@s,LaborHours=@h,ServiceCost=@c WHERE ServiceID=@id; SELECT @@ROWCOUNT;":
                    "INSERT INTO ServiceJobs(VehicleID,MechanicID,ServiceDate,Problem,Status,LaborHours,ServiceCost) VALUES(@v,@m,@d,@p,@s,@h,@c); SELECT CONVERT(int,SCOPE_IDENTITY());",
                    P("@v",vehicle),P("@m",mechanic),P("@d",date.Date),P("@p",problem.Trim()),P("@s",status),P("@h",hours),P("@c",charge),P("@id",id));
                int result=Convert.ToInt32(save.ExecuteScalar());if(id.HasValue&&result!=1)throw new InvalidOperationException("Service no longer exists.");if(!id.HasValue)saved=result;
                foreach(DataRow row in parts.Rows)
                {
                    using var part=Command(c,t,"INSERT INTO ServiceParts(ServiceID,PartID,Quantity,UnitCost,PriceEstimated) VALUES(@s,@p,@q,@u,@e)",P("@s",saved),P("@p",row["PartID"]),P("@q",row["Qty"]),P("@u",row["UnitCost"]),P("@e",parts.Columns.Contains("PriceEstimated")?row["PriceEstimated"]:false));part.ExecuteNonQuery();
                }
            },out var error);
            if(!ok)throw new InvalidOperationException(error);return saved;
        }
        public static void DeleteJob(int id)
        {
            Session.Require(true);
            if(!ExecuteTransaction((c,t)=>
            {
                using var check=Command(c,t,"SELECT COUNT(*) FROM Invoices WITH(UPDLOCK,HOLDLOCK) WHERE ServiceID=@id",P("@id",id));if((int)check.ExecuteScalar()>0)throw new InvalidOperationException("An invoiced service cannot be deleted.");
                using var cmd=Command(c,t,"UPDATE p SET Quantity=p.Quantity+x.Used FROM SpareParts p JOIN (SELECT PartID,SUM(Quantity) Used FROM ServiceParts WHERE ServiceID=@id GROUP BY PartID) x ON x.PartID=p.PartID; DELETE FROM ServiceParts WHERE ServiceID=@id; DELETE FROM ServiceJobs WHERE ServiceID=@id;",P("@id",id));cmd.ExecuteNonQuery();
            },out var error))throw new InvalidOperationException(error);
        }
        public static DataTable Charges(int service) => ExecuteQuery("SELECT s.ServiceCost,COALESCE(SUM(CONVERT(decimal(18,2),p.Quantity)*p.UnitCost),0) PartsCost FROM ServiceJobs s LEFT JOIN ServiceParts p ON s.ServiceID=p.ServiceID WHERE s.ServiceID=@s GROUP BY s.ServiceCost",P("@s",service));
        public static void Invoice(int service,DateTime date)
        {
            Session.Require();if(date.Date>DateTime.Today)throw new ArgumentException("Invoice date cannot be in the future.");
            if(!ExecuteTransaction((c,t)=>
            {
                using var cmd=Command(c,t,"INSERT INTO Invoices(ServiceID,InvoiceDate,TotalAmount) SELECT s.ServiceID,@d,s.ServiceCost+COALESCE((SELECT SUM(CONVERT(decimal(18,2),p.Quantity)*p.UnitCost) FROM ServiceParts p WHERE p.ServiceID=s.ServiceID),0) FROM ServiceJobs s WITH(UPDLOCK,HOLDLOCK) WHERE s.ServiceID=@s AND s.Status='Completed' AND s.ServiceCost IS NOT NULL AND s.ServiceDate<=@d AND NOT EXISTS(SELECT 1 FROM Invoices WITH(UPDLOCK,HOLDLOCK) WHERE ServiceID=@s)",P("@s",service),P("@d",date.Date));
                if(cmd.ExecuteNonQuery()!=1)throw new InvalidOperationException("Select a completed, uninvoiced service with a recorded service charge and a valid invoice date.");
            },out var error))throw new InvalidOperationException(error);
        }
        public static void Pay(int invoice,DateTime date,decimal amount,string method)
        {
            Session.Require();if(amount<=0||amount>99999999.99m||decimal.Round(amount,2)!=amount||date.Date>DateTime.Today||!(method=="Cash"||method=="Card"||method=="Other"))throw new ArgumentException("Enter a positive amount with up to two decimal places, a valid date, and payment method.");
            if(!ExecuteTransaction((c,t)=>
            {
                using var check=Command(c,t,"SELECT TotalAmount-COALESCE((SELECT SUM(Amount) FROM Payments WHERE InvoiceID=@i),0) FROM Invoices WITH(UPDLOCK,HOLDLOCK) WHERE InvoiceID=@i AND InvoiceDate<=@d AND LegacyDuplicateOf IS NULL",P("@i",invoice),P("@d",date.Date));
                object balance=check.ExecuteScalar();if(balance==null||amount>Convert.ToDecimal(balance))throw new InvalidOperationException("Payment exceeds the remaining balance or predates the invoice. Refresh and try again.");
                using var save=Command(c,t,"INSERT INTO Payments(InvoiceID,PaymentDate,Amount,PaymentMethod) VALUES(@i,@d,@a,@m)",P("@i",invoice),P("@d",date.Date),P("@a",amount),P("@m",method));save.ExecuteNonQuery();
            },out var error))throw new InvalidOperationException(error);
        }
    }
}
