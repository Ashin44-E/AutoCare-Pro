using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;
using VehicleServiceCenterManagementSystem.Helpers;
using static VehicleServiceCenterManagementSystem.Database.DBConnection;
namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class ServiceJobForm:Form
    {
        private readonly DataTable partsUsed=new DataTable();
        private readonly DataGridView usedGrid=new DataGridView();
        private readonly TextBox serviceCharge=new TextBox();
        private int? selectedId;
        public ServiceJobForm()
        {
            Session.Require();InitializeComponent();
            ((System.ComponentModel.ISupportInitialize)nudPartQty).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvParts).EndInit();
            partsUsed.Columns.Add("PartID",typeof(int));partsUsed.Columns.Add("PartName",typeof(string));partsUsed.Columns.Add("Qty",typeof(int));partsUsed.Columns.Add("UnitCost",typeof(decimal));partsUsed.Columns.Add("PriceEstimated",typeof(bool));
            partsUsed.Columns.Add("TotalCost",typeof(decimal),"Qty * UnitCost");
            Controls.Clear();Padding=new Padding(12);BackColor=Color.FromArgb(239,243,248);
            var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};Controls.Add(root);
            root.RowStyles.Add(new RowStyle(SizeType.Absolute,350));root.RowStyles.Add(new RowStyle(SizeType.AutoSize));root.RowStyles.Add(new RowStyle(SizeType.AutoSize));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var top=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2};top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));root.Controls.Add(top,0,0);
            serviceCharge.Text="0";txtProblem.Multiline=true;
            var fields=Ui.Fields(("Vehicle",cmbVehicle),("Mechanic",cmbMechanic),("Service date",dtpServiceDate),("Status",cmbStatus),("Problem",txtProblem),("Labor hours",txtLaborHours),("Service charge",serviceCharge),("Total charge",lblTotalCost));
            var left=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};left.Controls.Add(fields);top.Controls.Add(left,0,0);
            var note=new Label{Text="Labor hours suggest 50 per hour. Edit the service charge as needed.",Dock=DockStyle.Bottom,Height=45};left.Controls.Add(note);
            var tabs=new TabControl{Dock=DockStyle.Fill};var available=new TabPage("Available parts");var selected=new TabPage("Job parts");tabs.TabPages.Add(available);tabs.TabPages.Add(selected);top.Controls.Add(tabs,1,0);
            dgvParts.Dock=DockStyle.Fill;available.Controls.Add(dgvParts);var addRow=Ui.Actions(nudPartQty,btnAddPart);addRow.Dock=DockStyle.Bottom;available.Controls.Add(addRow);nudPartQty.Minimum=1;nudPartQty.Value=1;
            usedGrid.Dock=DockStyle.Fill;usedGrid.DataSource=partsUsed;selected.Controls.Add(usedGrid);
            var remove=new Button{Text="Remove selected part",Dock=DockStyle.Bottom,Height=36};remove.Click+=(s,e)=>{if(usedGrid.CurrentRow?.DataBoundItem is DataRowView row){row.Row.Delete();partsUsed.AcceptChanges();Total();}};selected.Controls.Add(remove);
            var delete=new Button{Text="Delete service",Enabled=Session.IsAdmin};delete.Click+=(s,e)=>
            {if(selectedId.HasValue&&MessageBox.Show("Delete this uninvoiced service and return its parts to stock?","AutoCare-Pro",MessageBoxButtons.YesNo)==DialogResult.Yes)Run(()=>{Workshop.DeleteJob(selectedId.Value);ClearFields();LoadServiceJobs();LoadParts();});};
            root.Controls.Add(Ui.Actions(btnAdd,btnUpdate,delete,btnClear,btnClose),0,1);root.Controls.Add(Ui.Actions(txtSearch,btnSearch),0,2);dgvServiceJobs.Dock=DockStyle.Fill;root.Controls.Add(dgvServiceJobs,0,3);Ui.Fit(this,1250,850);
            txtLaborHours.TextChanged+=(s,e)=>{if(decimal.TryParse(txtLaborHours.Text,out var h)&&h>=0&&h<=999.99m)serviceCharge.Text=(h*50m).ToString("0.00");};
            serviceCharge.TextChanged+=(s,e)=>Total();Ui.Apply(this);
            txtProblem.MaxLength=500;

        }
        private void Run(Action action){try{action();}catch(Exception ex){MessageBox.Show(Ui.Error(ex),"AutoCare-Pro");}}
        private void LoadParts(){dgvParts.DataSource=ExecuteQuery("SELECT PartID,PartName,Quantity,Price AS UnitPrice FROM SpareParts ORDER BY PartName");}
        private void LoadServiceJobs(){dgvServiceJobs.DataSource=ExecuteQuery("SELECT s.*,v.VehicleNumber,c.CustomerName,m.MechanicName FROM ServiceJobs s JOIN Vehicles v ON v.VehicleID=s.VehicleID JOIN Customers c ON c.CustomerID=v.CustomerID JOIN Mechanics m ON m.MechanicID=s.MechanicID ORDER BY s.ServiceID DESC");dgvServiceJobs.ClearSelection();}
        private void Save(bool update)
        {
            if(!Ui.Check(!update||selectedId.HasValue,"Select a service to update."))return;
            if(!Ui.Check(cmbVehicle.SelectedValue is int&&cmbMechanic.SelectedValue is int&&decimal.TryParse(txtLaborHours.Text,out _)&&decimal.TryParse(serviceCharge.Text,out _),"Select a vehicle and mechanic, and enter numeric labor hours and service charge."))return;
            Run(()=>{Workshop.SaveJob(update?selectedId:null,(int)cmbVehicle.SelectedValue,(int)cmbMechanic.SelectedValue,dtpServiceDate.Value,txtProblem.Text,cmbStatus.Text,decimal.Parse(txtLaborHours.Text),decimal.Parse(serviceCharge.Text),partsUsed);ClearFields();LoadServiceJobs();LoadParts();MessageBox.Show("Service saved.","AutoCare-Pro");});
        }
        private void btnAdd_Click(object sender,EventArgs e){if(!Ui.Check(!selectedId.HasValue,"Clear the selected job before creating a new service."))return;Save(false);}
        private void btnUpdate_Click(object sender,EventArgs e)=>Save(true);
        private void btnSearch_Click(object sender,EventArgs e)=>Run(()=>{dgvServiceJobs.DataSource=ExecuteQuery("SELECT s.*,v.VehicleNumber,m.MechanicName FROM ServiceJobs s JOIN Vehicles v ON v.VehicleID=s.VehicleID JOIN Mechanics m ON m.MechanicID=s.MechanicID WHERE v.VehicleNumber LIKE @q OR m.MechanicName LIKE @q OR s.Status LIKE @q",P("@q","%"+txtSearch.Text.Trim()+"%"));});
        private void ClearFields(){selectedId=null;partsUsed.Clear();txtProblem.Clear();txtSearch.Clear();txtLaborHours.Text="0";serviceCharge.Text="0";cmbStatus.SelectedIndex=0;dtpServiceDate.Value=DateTime.Today;nudPartQty.Value=1;Total();}
        private void btnClear_Click(object sender,EventArgs e)=>Run(()=>{ClearFields();LoadServiceJobs();LoadParts();});
        private void dgvServiceJobs_CellClick(object sender,DataGridViewCellEventArgs e)
        {
            if(e.RowIndex<0)return;
            Run(()=>
            {
                var row=dgvServiceJobs.Rows[e.RowIndex];selectedId=Convert.ToInt32(row.Cells["ServiceID"].Value);
                cmbVehicle.SelectedValue=row.Cells["VehicleID"].Value;cmbMechanic.SelectedValue=row.Cells["MechanicID"].Value;dtpServiceDate.Value=Convert.ToDateTime(row.Cells["ServiceDate"].Value);txtProblem.Text=Convert.ToString(row.Cells["Problem"].Value);cmbStatus.Text=Convert.ToString(row.Cells["Status"].Value);
                txtLaborHours.Text=row.Cells["LaborHours"].Value==DBNull.Value?"0":Convert.ToString(row.Cells["LaborHours"].Value);serviceCharge.Text=Convert.ToString(row.Cells["ServiceCost"].Value);
                partsUsed.Clear();var table=ExecuteQuery("SELECT p.PartID,p.PartName,s.Quantity Qty,s.UnitCost,s.PriceEstimated FROM ServiceParts s JOIN SpareParts p ON p.PartID=s.PartID WHERE s.ServiceID=@s",P("@s",selectedId));
                foreach(DataRow r in table.Rows)partsUsed.Rows.Add(r["PartID"],r["PartName"],r["Qty"],r["UnitCost"],r["PriceEstimated"]);Total();
            });
        }
        private void ServiceJobForm_Load(object sender,EventArgs e)=>Run(()=>
        {
            cmbVehicle.DisplayMember="Description";cmbVehicle.ValueMember="VehicleID";cmbVehicle.DataSource=ExecuteQuery("SELECT v.VehicleID,CONCAT(v.VehicleNumber,' - ',c.CustomerName) Description FROM Vehicles v JOIN Customers c ON c.CustomerID=v.CustomerID");
            cmbMechanic.DisplayMember="MechanicName";cmbMechanic.ValueMember="MechanicID";cmbMechanic.DataSource=ExecuteQuery("SELECT MechanicID,MechanicName FROM Mechanics");LoadParts();LoadServiceJobs();ClearFields();
        });
        private void btnAddPart_Click(object sender,EventArgs e)
        {
            if(dgvParts.CurrentRow==null)return;
            Run(()=>{var row=dgvParts.CurrentRow;partsUsed.Rows.Add(row.Cells["PartID"].Value,row.Cells["PartName"].Value,(int)nudPartQty.Value,row.Cells["UnitPrice"].Value,false);Total();});
        }
        private void Total(){decimal.TryParse(serviceCharge.Text,out var total);foreach(DataRow row in partsUsed.Rows)total+=Convert.ToDecimal(row["TotalCost"]);lblTotalCost.Text=total.ToString("0.00");}
        private void btnClose_Click(object sender,EventArgs e)=>Close();
    }
}
