using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            Session.Require(false);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvReports, new (string, Control)[] { ("Month", dtpMonth), ("Report", lblReportTitle) }, new Control[] { btnCustomerReport, btnVehicleReport, btnServiceReport, btnSparePartsReport, btnPaymentReport, btnMonthlyReport, btnClose }, new Control[] {  });
        }

        // Customer Report
        private void btnCustomerReport_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT * FROM Customers";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                lblReportTitle.Text = "Customer Report";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Vehicle Report
        private void btnVehicleReport_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT v.*, c.CustomerName FROM Vehicles v " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                lblReportTitle.Text = "Vehicle Report";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Service Report
        private void btnServiceReport_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT sj.*, v.VehicleNumber, m.MechanicName FROM ServiceJobs sj " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID " +
                               "INNER JOIN Mechanics m ON sj.MechanicID = m.MechanicID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                lblReportTitle.Text = "Service Report";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Spare Parts Report
        private void btnSparePartsReport_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT * FROM SpareParts";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                lblReportTitle.Text = "Spare Parts Report";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Payment Report
        private void btnPaymentReport_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT p.*, i.InvoiceDate, v.VehicleNumber FROM Payments p " +
                               "INNER JOIN Invoices i ON p.InvoiceID = i.InvoiceID " +
                               "INNER JOIN ServiceJobs sj ON i.ServiceID = sj.ServiceID " +
                               "INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                lblReportTitle.Text = "Payment Report";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Monthly Report
        private void btnMonthlyReport_Click(object sender, EventArgs e)
        {
            try
            {
                string month = dtpMonth.Value.ToString("yyyy-MM");
                string query = "SELECT sj.*, v.VehicleNumber, m.MechanicName FROM ServiceJobs sj INNER JOIN Vehicles v ON sj.VehicleID = v.VehicleID INNER JOIN Mechanics m ON sj.MechanicID = m.MechanicID WHERE sj.ServiceDate >= @start AND sj.ServiceDate < @end";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@start", new DateTime(dtpMonth.Value.Year, dtpMonth.Value.Month, 1)), DBConnection.P("@end", new DateTime(dtpMonth.Value.Year, dtpMonth.Value.Month, 1).AddMonths(1)) };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvReports.AutoGenerateColumns = true;
                dgvReports.DataSource = null;
                dgvReports.DataSource = dt;
                dgvReports.Refresh();
                var monthStart = new DateTime(dtpMonth.Value.Year, dtpMonth.Value.Month, 1);
                var totals = DBConnection.ExecuteQuery("SELECT (SELECT COALESCE(SUM(TotalAmount),0) FROM Invoices WHERE InvoiceDate>=@start AND InvoiceDate<@end AND LegacyDuplicateOf IS NULL) Billed, (SELECT COALESCE(SUM(Amount),0) FROM Payments WHERE PaymentDate>=@start AND PaymentDate<@end) Received", DBConnection.P("@start", monthStart), DBConnection.P("@end", monthStart.AddMonths(1)));
                lblReportTitle.Text = dtpMonth.Value.ToString("MMMM yyyy") + " | Jobs: " + dt.Rows.Count + " | Billed: " + Convert.ToDecimal(totals.Rows[0]["Billed"]).ToString("0.00") + " | Received: " + Convert.ToDecimal(totals.Rows[0]["Received"]).ToString("0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Form load
        private void ReportForm_Load(object sender, EventArgs e)
        {
            lblReportTitle.Text = "Select a report type";
            dtpMonth.Format = DateTimePickerFormat.Custom;
            dtpMonth.CustomFormat = "MMMM yyyy";
        }
    }
}
