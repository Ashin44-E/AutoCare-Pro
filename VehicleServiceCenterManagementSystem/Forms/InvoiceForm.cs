using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class InvoiceForm : Form
    {
        public InvoiceForm()
        {
            Session.Require(false);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvInvoices, new (string, Control)[] { ("Service", cmbService), ("Invoice date", dtpInvoiceDate), ("Service charge", txtServiceCost), ("Parts charge", txtPartsCost), ("Invoice total", txtTotalAmount) }, new Control[] { btnCreate, btnClear, btnClose }, new Control[] { txtSearch, btnSearch });
            var details = new Button { Text = "View / Print", Left = btnClose.Right + 15, Top = btnClose.Top, Width = 130, Height = btnClose.Height };
            btnClose.Parent.Controls.Add(details);
            details.BackColor=btnClose.BackColor;details.ForeColor=btnClose.ForeColor;details.FlatStyle=FlatStyle.Flat;details.FlatAppearance.BorderSize=0;
            details.Click += (sender, e) =>
            {
                if (dgvInvoices.CurrentRow == null) return;
                try { using var form = new InvoiceDetailsForm(Convert.ToInt32(dgvInvoices.CurrentRow.Cells["InvoiceID"].Value)); form.ShowDialog(this); }
                catch (Exception ex) { MessageBox.Show(Ui.Error(ex), "AutoCare-Pro"); }
            };
        }

        // Load service jobs to combo box
        private void LoadServiceJobs()
        {
            try
            {
                string query = "SELECT sj.ServiceID, sj.ServiceDate, v.VehicleNumber, c.CustomerName, CONCAT(sj.ServiceID, ' - ', v.VehicleNumber, ' - ', c.CustomerName) AS Description FROM ServiceJobs sj " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID " +
                               "WHERE sj.Status = 'Completed' AND sj.ServiceID NOT IN (SELECT ServiceID FROM Invoices)";
                DataTable dt = DBConnection.ExecuteQuery(query);
                
                cmbService.DisplayMember = "Description";
                cmbService.ValueMember = "ServiceID";
                cmbService.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading service jobs: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load invoices
        private void LoadInvoices()
        {
            try
            {
                string query = "SELECT i.*, COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) AS Paid, i.TotalAmount-COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) AS Balance, CASE WHEN i.TotalAmount < COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) THEN COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0)-i.TotalAmount ELSE 0 END AS Credit, sj.ServiceDate, v.VehicleNumber, c.CustomerName FROM Invoices i " +
                               "INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvInvoices.AutoGenerateColumns = true;
                dgvInvoices.DataSource = null;
                dgvInvoices.DataSource = dt;
                dgvInvoices.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoices: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Calculate total
        private void CalculateTotal()
        {
            txtServiceCost.Clear(); txtPartsCost.Clear(); txtTotalAmount.Clear();
            if (!(cmbService.SelectedValue is int service)) return;
            try
            {
                var charges = Workshop.Charges(service);
                if (charges.Rows.Count == 0 || charges.Rows[0]["ServiceCost"] == DBNull.Value) return;
                decimal serviceCost = Convert.ToDecimal(charges.Rows[0]["ServiceCost"]);
                decimal partsCost = Convert.ToDecimal(charges.Rows[0]["PartsCost"]);
                txtServiceCost.Text = serviceCost.ToString("0.00");
                txtPartsCost.Text = partsCost.ToString("0.00");
                txtTotalAmount.Text = (serviceCost + partsCost).ToString("0.00");
            }
            catch (Exception ex) { MessageBox.Show(Ui.Error(ex), "AutoCare-Pro"); }
        }

        // Create invoice
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!(cmbService.SelectedValue is int service)) { MessageBox.Show("Select a completed service."); return; }
            try
            {
                Workshop.Invoice(service, dtpInvoiceDate.Value);
                LoadServiceJobs(); LoadInvoices(); CalculateTotal();
                MessageBox.Show("Invoice created.", "AutoCare-Pro");
            }
            catch (Exception ex) { MessageBox.Show(Ui.Error(ex), "AutoCare-Pro"); }
        }

        // Search invoice
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT i.*, COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) AS Paid, i.TotalAmount-COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) AS Balance, CASE WHEN i.TotalAmount < COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0) THEN COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0)-i.TotalAmount ELSE 0 END AS Credit, sj.ServiceDate, v.VehicleNumber, c.CustomerName FROM Invoices i INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID INNER JOIN Customers c ON v.CustomerID = c.CustomerID WHERE v.VehicleNumber LIKE @searchText OR c.CustomerName LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvInvoices.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear fields
        private void ClearFields()
        {
            txtServiceCost.Text = "";
            txtPartsCost.Text = "";
            txtTotalAmount.Text = "";
            txtSearch.Text = "";
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadInvoices();
            CalculateTotal();
        }

        // Service selection changed
        private void cmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        // Form load
        private void InvoiceForm_Load(object sender, EventArgs e)
        {
            LoadServiceJobs();
            LoadInvoices();
            CalculateTotal();
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
