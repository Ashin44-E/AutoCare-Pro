using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;
using VehicleServiceCenterManagementSystem.Helpers;
using VehicleServiceCenterManagementSystem.Forms;
using static VehicleServiceCenterManagementSystem.Database.DBConnection;
internal static class Program
{
 static IEnumerable<Control> Descendants(Control parent)
 { foreach(Control control in parent.Controls) { yield return control; foreach(var child in Descendants(control))yield return child; } }
 static int assertions;
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);assertions++;Console.WriteLine("PASS "+name);}
 static void Reject(Action action,string name){try{action();}catch(Exception){Check(true,name);return;}throw new Exception("Expected rejection: "+name);}
 static int Id(string sql,params System.Data.SqlClient.SqlParameter[] p)=>Convert.ToInt32(ExecuteScalar(sql+"; SELECT CONVERT(int,SCOPE_IDENTITY());",p));
 static decimal Number(string sql)=>Convert.ToDecimal(ExecuteScalar(sql));
 static DataTable Parts(int part,int quantity,decimal price){var t=new DataTable();t.Columns.Add("PartID",typeof(int));t.Columns.Add("Qty",typeof(int));t.Columns.Add("UnitCost",typeof(decimal));t.Rows.Add(part,quantity,price);return t;}
 [STAThread] static int Main()
 {
  try
  {
   var connection=Environment.GetEnvironmentVariable("AUTOCARE_CONNECTION_STRING")??"";
   if(!connection.Contains("Database=AutoCarePro_Test_") && !connection.Contains("Database=AutoCarePro_UpgradeTest_"))throw new Exception("Tests require an isolated AutoCarePro_Test_ database.");
   ApplicationConfiguration.Initialize();
   string suffix=Guid.NewGuid().ToString("N")[..8], user="test_"+suffix, password="Test password 739!";
   int before=(int)ExecuteScalar("SELECT COUNT(*) FROM Users");
   string recovery=Accounts.Register(user,password);Check(recovery.Length==48,"registration issues recovery code");
   Check(Accounts.Login(user,password),"registered account login");if(before==0)Check(Session.IsAdmin,"first account bootstraps Admin");
   Reject(()=>Accounts.Register(user,password),"duplicate username rejected");
   Check(!Accounts.Login(user,"wrong"),"wrong password rejected");Check(Session.UserId==0,"failed login clears session");Accounts.Login(user,password);
   string changed=Accounts.ChangePassword(password,"Changed password 42!");Check(Accounts.Login(user,"Changed password 42!"),"change password login");
   Reject(()=>Accounts.Recover(user,recovery,password),"old recovery code revoked");
   string recovered=Accounts.Recover(user,changed,password);Check(Accounts.Login(user,password),"recovery resets password");
   Reject(()=>Accounts.Recover(user,changed,password),"recovery token single use");
   string explicitAdmin="adm_"+suffix;Accounts.Register(explicitAdmin,password,"Admin");Check(Accounts.Login(explicitAdmin,password)&&Session.IsAdmin,"explicit Admin registration creates Admin");
   using (var mf = new MechanicForm()) Check(mf != null, "Admin can access MechanicForm");
   using (var sf = new SparePartsForm()) Check(sf != null, "Admin can access SparePartsForm");
   string explicitUser="usr_"+suffix;Accounts.Register(explicitUser,password,"User");Check(Accounts.Login(explicitUser,password)&&!Session.IsAdmin,"explicit User registration creates User");
   string other="staff_"+suffix;Accounts.Register(other,password);Check(Accounts.Login(other,password)&&!Session.IsAdmin,"public registration cannot create Admin");
   Reject(()=>new MechanicForm(),"User denied mechanic management");Reject(()=>new SparePartsForm(),"User denied inventory management");Reject(()=>new FinancialReportForm(),"User denied financial reports");Reject(()=>new DataIssuesForm(),"User denied data review");
   Reject(()=>Accounts.Recover(user,"",password,true),"User denied administrator reset");
   for(int n=0;n<5;n++)Accounts.Login(other,"wrong");Check(!Accounts.Login(other,password),"failed login lockout");
   Accounts.Login(user,password);if(!Session.IsAdmin)Session.Start(Session.UserId,user,"Admin");
   Accounts.Recover(other,"",password,true);Check(Accounts.Login(other,password),"administrator resets account lockout");Accounts.Login(user,password);if(!Session.IsAdmin)Session.Start(Session.UserId,user,"Admin");
   byte[] salt=RandomNumberGenerator.GetBytes(16);byte[] legacy=salt.Concat(Rfc2898DeriveBytes.Pbkdf2(password,salt,10000,HashAlgorithmName.SHA256,32)).ToArray();
   Check(PasswordHelper.VerifyPassword(password,Convert.ToBase64String(legacy)),"legacy hash verification");Check(!PasswordHelper.VerifyPassword(password,"malformed"),"malformed hash rejected");
   string plain="plain_"+suffix;ExecuteNonQuery("INSERT INTO Users(Username,Password,UserType) VALUES(@u,@p,'User')",P("@u",plain),P("@p",password));Check(Accounts.Login(plain,password),"legacy plaintext login");Check(Convert.ToString(ExecuteScalar("SELECT Password FROM Users WHERE Username=@u",P("@u",plain)))!.StartsWith("PBKDF2$"),"legacy plaintext upgraded");
   Accounts.Login(user,password);if(!Session.IsAdmin)Session.Start(Session.UserId,user,"Admin");
   string name="O'Brien "+suffix;
   int customer=Id("INSERT INTO Customers(CustomerName,Phone,NIC,Email) VALUES(@n,'0771234567',@nic,'test@example.com')",P("@n",name),P("@nic",suffix));
   Check((string)ExecuteScalar("SELECT CustomerName FROM Customers WHERE CustomerID=@id",P("@id",customer))==name,"apostrophe-safe customer create/read");
   ExecuteNonQuery("UPDATE Customers SET Address=@a WHERE CustomerID=@id",P("@a","Updated address"),P("@id",customer));Check((string)ExecuteScalar("SELECT Address FROM Customers WHERE CustomerID=@id",P("@id",customer))=="Updated address","customer update");
   Check(ExecuteQuery("SELECT * FROM Customers WHERE CustomerName LIKE @q",P("@q","%"+suffix+"%")).Rows.Count==1,"parameterized customer search");
   Check(ExecuteQuery("SELECT * FROM Customers WHERE CustomerName=@q",P("@q","' OR 1=1--")).Rows.Count==0,"SQL injection treated as data");
   int vehicle=Id("INSERT INTO Vehicles(CustomerID,VehicleNumber,Brand,Model,Year,Mileage) VALUES(@c,@n,'Toyota','Test',2020,50000)",P("@c",customer),P("@n",suffix));
   ExecuteNonQuery("UPDATE Vehicles SET Mileage=51000 WHERE VehicleID=@v",P("@v",vehicle));Check(Number($"SELECT Mileage FROM Vehicles WHERE VehicleID={vehicle}")==51000,"vehicle mileage roundtrip");
   Reject(()=>ExecuteNonQuery("INSERT INTO Vehicles(CustomerID,VehicleNumber,Brand) VALUES(@c,@n,'Test')",P("@c",customer),P("@n",suffix)),"duplicate vehicle rejected");
   int mechanic=Id("INSERT INTO Mechanics(MechanicName,Phone,Skill) VALUES(@n,'0711234567','Engine')",P("@n",suffix));
   ExecuteNonQuery("UPDATE Mechanics SET Skill='Electrical' WHERE MechanicID=@m",P("@m",mechanic));Check((string)ExecuteScalar("SELECT Skill FROM Mechanics WHERE MechanicID=@m",P("@m",mechanic))=="Electrical","mechanic CRUD update");
   int part=Id("INSERT INTO SpareParts(PartName,Quantity,Price) VALUES(@n,10,125.50)",P("@n",suffix));
   Reject(()=>ExecuteNonQuery("UPDATE SpareParts SET Quantity=-1 WHERE PartID=@p",P("@p",part)),"negative stock rejected");
   var used=Parts(part,3,125.50m);int job=Workshop.SaveJob(null,vehicle,mechanic,DateTime.Today,"Oil change","Pending",2,500,used);
   Check(Number($"SELECT Quantity FROM SpareParts WHERE PartID={part}")==7,"service deducts stock");
   var tooMany=Parts(part,8,125.50m);tooMany.Rows.Add(part,4,125.50m);
   Reject(()=>Workshop.SaveJob(null,vehicle,mechanic,DateTime.Today,"Too many","Pending",1,50,tooMany),"aggregate stock oversell rejected");Check(Number($"SELECT Quantity FROM SpareParts WHERE PartID={part}")==7,"failed service rolls back stock");
   Reject(()=>Workshop.Invoice(job,DateTime.Today),"pending service cannot invoice");
   used.Rows[0]["Qty"]=2;Workshop.SaveJob(job,vehicle,mechanic,DateTime.Today,"Oil change","Completed",2,500,used);Check(Number($"SELECT Quantity FROM SpareParts WHERE PartID={part}")==8,"service edit restores then deducts stock");
   ExecuteNonQuery("UPDATE SpareParts SET Price=999 WHERE PartID=@p",P("@p",part));Check(Convert.ToDecimal(Workshop.Charges(job).Rows[0]["PartsCost"])==251,"historical prices survive inventory price change");
   Workshop.Invoice(job,DateTime.Today);int invoice=(int)ExecuteScalar("SELECT InvoiceID FROM Invoices WHERE ServiceID=@s",P("@s",job));Check(Number($"SELECT TotalAmount FROM Invoices WHERE InvoiceID={invoice}")==751,"invoice calculation uses saved charges");
   Reject(()=>Workshop.Invoice(job,DateTime.Today),"duplicate invoice rejected");Reject(()=>Workshop.SaveJob(job,vehicle,mechanic,DateTime.Today,"edit","Pending",1,1,used),"invoiced service immutable");Reject(()=>Workshop.DeleteJob(job),"invoiced service deletion denied");
   Reject(()=>Workshop.Pay(invoice,DateTime.Today,0,"Cash"),"zero payment rejected");Reject(()=>Workshop.Pay(invoice,DateTime.Today,-1,"Cash"),"negative payment rejected");Reject(()=>Workshop.Pay(invoice,DateTime.Today,800,"Cash"),"overpayment rejected");Reject(()=>Workshop.Pay(invoice,DateTime.Today.AddDays(-1),1,"Cash"),"payment before invoice rejected");
   Workshop.Pay(invoice,DateTime.Today,200,"Cash");Check(Number($"SELECT TotalAmount-(SELECT SUM(Amount) FROM Payments WHERE InvoiceID={invoice}) FROM Invoices WHERE InvoiceID={invoice}")==551,"partial payment balance");
   int successes=0;Parallel.For(0,2,_=>{try{Workshop.Pay(invoice,DateTime.Today,551,"Card");Interlocked.Increment(ref successes);}catch(InvalidOperationException){}});Check(successes==1,"concurrent final payments cannot overpay");Check(Number($"SELECT SUM(Amount) FROM Payments WHERE InvoiceID={invoice}")==751,"paid balance exact");
   Reject(()=>ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@c",P("@c",customer)),"foreign key protects customer history");
   int disposable=Workshop.SaveJob(null,vehicle,mechanic,DateTime.Today,"Delete test","Pending",0,0,Parts(part,1,999));Workshop.DeleteJob(disposable);Check(Number($"SELECT Quantity FROM SpareParts WHERE PartID={part}")==8,"delete uninvoiced job restores stock");
   // Master CRUD deletion on unreferenced records.
   int emptyCustomer=Id("INSERT INTO Customers(CustomerName,Phone) VALUES('Delete test','0771234567')");Check(ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@id",P("@id",emptyCustomer))==1,"customer delete");
   int emptyVehicle=Id("INSERT INTO Vehicles(CustomerID,VehicleNumber,Brand) VALUES(@c,@n,'Test')",P("@c",customer),P("@n","del"+suffix));Check(ExecuteNonQuery("DELETE FROM Vehicles WHERE VehicleID=@id",P("@id",emptyVehicle))==1,"vehicle delete");
   int emptyMechanic=Id("INSERT INTO Mechanics(MechanicName,Phone) VALUES(@n,'0771111111')",P("@n","del"+suffix));Check(ExecuteNonQuery("DELETE FROM Mechanics WHERE MechanicID=@id",P("@id",emptyMechanic))==1,"mechanic delete");
   int emptyPart=Id("INSERT INTO SpareParts(PartName,Quantity,Price) VALUES(@n,0,0)",P("@n","del"+suffix));Check(ExecuteNonQuery("DELETE FROM SpareParts WHERE PartID=@id",P("@id",emptyPart))==1,"part delete");
   // Instantiate every screen and invoke actual read/report handlers without opening windows.
   var forms=new Form[]{new HomeForm(),new LoginForm(),new RegisterForm(),new DashboardForm(),new CustomerForm(),new VehicleForm(),new MechanicForm(),new SparePartsForm(),new ServiceJobForm(),new InvoiceForm(),new InvoiceDetailsForm(invoice),new PaymentForm(),new ReportForm(),new FinancialReportForm(),new DataIssuesForm(),new PasswordForm("change"),new PasswordForm("recover"),new PasswordForm("admin")};
   Directory.CreateDirectory("Tests/artifacts");
   foreach(var form in forms)
   {
    using(form)
    {
     form.ShowInTaskbar=false;form.Opacity=0;form.Show();Application.DoEvents();
     if(form is ServiceJobForm)
     {
         var quantity=(NumericUpDown)form.GetType().GetField("nudPartQty",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(form)!;
         Check(quantity.Minimum==1 && quantity.Value>=1,"part quantity initializes to a positive value");
     }
     foreach(var grid in Descendants(form).OfType<DataGridView>().Where(g=>g.Visible))
         Check(grid.Parent!.ClientRectangle.IntersectsWith(grid.Bounds),form.GetType().Name+" grid is inside its container");
     if(form is ReportForm)foreach(string handler in new[]{"btnCustomerReport_Click","btnVehicleReport_Click","btnServiceReport_Click","btnSparePartsReport_Click","btnPaymentReport_Click","btnMonthlyReport_Click"})form.GetType().GetMethod(handler,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(form,new object?[]{null,EventArgs.Empty});
     if(form is FinancialReportForm)form.GetType().GetMethod("BtnGenerate_Click",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(form,new object?[]{null,EventArgs.Empty});
     using var bitmap=new System.Drawing.Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,new System.Drawing.Rectangle(0,0,form.Width,form.Height));bitmap.Save("Tests/artifacts/"+form.GetType().Name+".png");
     Check(form.Text.Contains("AutoCare-Pro"),form.GetType().Name+" loads and renders");
    }
   }
   Session.Clear();Reject(()=>new CustomerForm(),"logged-out access denied");
   Console.WriteLine($"SUCCESS: {assertions} assertions passed.");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
 }
}

