using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;
using VehicleServiceCenterManagementSystem.Helpers;
using static VehicleServiceCenterManagementSystem.Database.DBConnection;
namespace VehicleServiceCenterManagementSystem.Forms
{
    public sealed class InvoiceDetailsForm:Form
    {
        public InvoiceDetailsForm(int invoice)
        {
            Session.Require();Text="Invoice "+invoice;Width=800;Height=650;StartPosition=FormStartPosition.CenterParent;
            var header=ExecuteQuery("SELECT i.*,s.ServiceDate,s.Problem,s.ServiceCost,v.VehicleNumber,v.Brand,v.Model,c.CustomerName,c.Phone,c.Address FROM Invoices i JOIN ServiceJobs s ON s.ServiceID=i.ServiceID JOIN Vehicles v ON v.VehicleID=s.VehicleID JOIN Customers c ON c.CustomerID=v.CustomerID WHERE i.InvoiceID=@i",P("@i",invoice));
            if(header.Rows.Count!=1)throw new InvalidOperationException("Invoice no longer exists.");
            var r=header.Rows[0];var text=new StringBuilder();text.AppendLine("AutoCare-Pro");text.AppendLine("INVOICE #"+invoice+"   Date: "+Convert.ToDateTime(r["InvoiceDate"]).ToString("yyyy-MM-dd"));
            if(r["LegacyDuplicateOf"]!=DBNull.Value)text.AppendLine("HISTORICAL DUPLICATE OF INVOICE #"+r["LegacyDuplicateOf"]+" - retained for audit; do not collect again.");
            text.AppendLine("Customer: "+r["CustomerName"]+"   Phone: "+r["Phone"]);text.AppendLine("Address: "+r["Address"]);text.AppendLine("Vehicle: "+r["VehicleNumber"]+" "+r["Brand"]+" "+r["Model"]);text.AppendLine("Service: "+r["ServiceID"]+" / "+Convert.ToDateTime(r["ServiceDate"]).ToString("yyyy-MM-dd"));text.AppendLine("Work: "+r["Problem"]);text.AppendLine();
            var parts=ExecuteQuery("SELECT p.PartName,s.Quantity,s.UnitCost,s.PriceEstimated FROM ServiceParts s JOIN SpareParts p ON p.PartID=s.PartID WHERE s.ServiceID=@s",P("@s",r["ServiceID"]));decimal partTotal=0;
            foreach(DataRow part in parts.Rows){decimal total=Convert.ToInt32(part["Quantity"])*Convert.ToDecimal(part["UnitCost"]);partTotal+=total;text.AppendLine($"{part["PartName"]}: {part["Quantity"]} x {part["UnitCost"]:0.00} = {total:0.00}"+((bool)part["PriceEstimated"]?" (legacy price estimate)":""));}
            text.AppendLine("Service charge: "+(r["ServiceCost"]==DBNull.Value?"Not recorded":Convert.ToDecimal(r["ServiceCost"]).ToString("0.00")));text.AppendLine("Parts total: "+partTotal.ToString("0.00"));
            decimal totalAmount=Convert.ToDecimal(r["TotalAmount"]);text.AppendLine("ISSUED INVOICE TOTAL: "+totalAmount.ToString("0.00"));
            if(r["ServiceCost"]!=DBNull.Value&&Convert.ToDecimal(r["ServiceCost"])+partTotal!=totalAmount)text.AppendLine("Historical charge details differ from the issued total; the original invoice amount has been preserved.");
            var payments=ExecuteQuery("SELECT PaymentDate,Amount,PaymentMethod FROM Payments WHERE InvoiceID=@i ORDER BY PaymentDate,PaymentID",P("@i",invoice));decimal paid=0;
            foreach(DataRow payment in payments.Rows){paid+=Convert.ToDecimal(payment["Amount"]);text.AppendLine($"Payment {Convert.ToDateTime(payment["PaymentDate"]):yyyy-MM-dd}: {payment["Amount"]:0.00} ({payment["PaymentMethod"]})");}
            text.AppendLine("Paid: "+paid.ToString("0.00"));text.AppendLine("Balance due: "+Math.Max(0,totalAmount-paid).ToString("0.00"));if(paid>totalAmount)text.AppendLine("Recorded overpayment / credit: "+(paid-totalAmount).ToString("0.00"));
            var content=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,Text=text.ToString(),Font=new Font("Consolas",10)};Controls.Add(content);
            var print=new Button{Text="Print invoice",Dock=DockStyle.Bottom,Height=40};Controls.Add(print);
            print.Click+=(s,e)=>
            {
                try
                {
                    using var document=new PrintDocument();string remaining=content.Text;document.BeginPrint+=(a,b)=>remaining=content.Text;
                    document.PrintPage+=(a,b)=>{b.Graphics.MeasureString(remaining,content.Font,b.MarginBounds.Size,StringFormat.GenericTypographic,out int chars,out int lines);if(chars==0)throw new InvalidOperationException("Printer margins leave no room for the invoice.");b.Graphics.DrawString(remaining[..chars],content.Font,Brushes.Black,b.MarginBounds,StringFormat.GenericTypographic);remaining=remaining[chars..];b.HasMorePages=remaining.Length>0;};
                    using var dialog=new PrintDialog{Document=document,UseEXDialog=true};if(dialog.ShowDialog(this)==DialogResult.OK)document.Print();
                }
                catch(Exception ex){MessageBox.Show(Ui.Error(ex),"AutoCare-Pro");}
            };
            Ui.Apply(this);content.MaxLength=0;
        }
    }
}
