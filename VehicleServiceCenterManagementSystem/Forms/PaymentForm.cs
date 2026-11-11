using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class PaymentForm : Form
    {
        public PaymentForm()
        {
            Session.Require(false);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvPayments, new (string, Control)[] { ("Invoice", cmbInvoice), ("Payment date", dtpPaymentDate), ("Amount", txtAmount), ("Method", cmbPaymentMethod) }, new Control[] { btnSave, btnClear, btnClose }, new Control[] { txtSearch, btnSearch });
        }

        // Load invoices to combo box
        private void LoadInvoices()
        {
            try
            {
                string query = "SELECT i.InvoiceID, i.InvoiceDate, i.TotalAmount, v.VehicleNumber, c.CustomerName, CONCAT(i.InvoiceID, ' - ', v.VehicleNumber, ' - ', c.CustomerName) AS Description FROM Invoices i " +
                               "INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID " +
                               "WHERE i.LegacyDuplicateOf IS NULL AND i.TotalAmount > COALESCE((SELECT SUM(p.Amount) FROM Payments p WHERE p.InvoiceID=i.InvoiceID),0)";
                DataTable dt = DBConnection.ExecuteQuery(query);
                
                cmbInvoice.DisplayMember = "Description";
                cmbInvoice.ValueMember = "InvoiceID";
                cmbInvoice.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoices: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load payments
        private void LoadPayments()
        {
            try
            {
                string query = "SELECT p.*, i.InvoiceDate, v.VehicleNumber, c.CustomerName FROM Payments p " +
                               "INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID " +
                               "INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvPayments.AutoGenerateColumns = true;
                dgvPayments.DataSource = null;
                dgvPayments.DataSource = dt;
                dgvPayments.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payments: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Record payment
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!(cmbInvoice.SelectedValue is int invoice) || !decimal.TryParse(txtAmount.Text, out decimal amount))
            { MessageBox.Show("Select an invoice and enter a valid payment amount."); return; }
            try
            {
                Workshop.Pay(invoice, dtpPaymentDate.Value, amount, cmbPaymentMethod.Text);
                LoadInvoices(); LoadPayments();
                cmbInvoice_SelectedIndexChanged(sender, e);
                MessageBox.Show("Payment recorded.", "AutoCare-Pro");
            }
            catch (Exception ex) { MessageBox.Show(Ui.Error(ex), "AutoCare-Pro"); }
        }

        // Search payment
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT p.*, i.InvoiceDate, v.VehicleNumber, c.CustomerName FROM Payments p INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID INNER JOIN Customers c ON v.CustomerID = c.CustomerID WHERE v.VehicleNumber LIKE @searchText OR c.CustomerName LIKE @searchText OR p.PaymentMethod LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvPayments.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear fields
        private void ClearFields()
        {
            txtAmount.Text = "";
            txtSearch.Text = "";
            cmbPaymentMethod.SelectedIndex = 0;
            if (cmbInvoice.Items.Count > 0)
            {
                cmbInvoice.SelectedIndex = 0;
            }
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadPayments();
        }

        // Invoice selection changed
        private void cmbInvoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtAmount.Clear();
            if (cmbInvoice.SelectedValue is int)
            {
                try
                {
                    string invoiceId = cmbInvoice.SelectedValue.ToString();
                    string query = "SELECT TotalAmount-COALESCE((SELECT SUM(Amount) FROM Payments WHERE InvoiceID=@invoiceId),0) FROM Invoices WHERE InvoiceID = @invoiceId";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@invoiceId", int.Parse(invoiceId)) };
                    object result = DBConnection.ExecuteScalar(query, queryParameters);

                    if (result != null && result != DBNull.Value)
                    {
                        txtAmount.Text = result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Form load
        private void PaymentForm_Load(object sender, EventArgs e)
        {
            LoadInvoices();
            LoadPayments();
            cmbPaymentMethod.SelectedIndex = 0;
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
