using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            Session.Require(false);
            InitializeComponent();
            lblTitle.Text = "AutoCare-Pro";
            ApplyRolePermissions();
            var change = new Button { Text = "Change Password", Left = 30, Top = 150, Width = 220, Height = 40 };
            change.Click += (s,e) => { using var form = new PasswordForm("change"); form.ShowDialog(this); };
            panelMain.Controls.Add(change);
            var reset = new Button { Text = "Reset User Password", Left = 30, Top = 200, Width = 220, Height = 40, Visible = Session.IsAdmin };
            reset.Click += (s,e) => { using var form = new PasswordForm("admin"); form.ShowDialog(this); };
            panelMain.Controls.Add(reset);
            var financial = new Button { Text = "Financial Reports", Left = 30, Top = 250, Width = 220, Height = 40, Visible = Session.IsAdmin };
            financial.Click += (s,e) => { using var form = new FinancialReportForm(); form.ShowDialog(this); };
            panelMain.Controls.Add(financial);
            var issues = new Button { Text = "Existing Data Review", Left = 30, Top = 300, Width = 220, Height = 40, Visible = Session.IsAdmin };
            issues.Click += (s,e) => { try { using var form = new DataIssuesForm(); form.ShowDialog(this); } catch (Exception ex) { MessageBox.Show(Ui.Error(ex), "AutoCare-Pro"); } };
            panelMain.Controls.Add(issues);

            var picDashboardBanner = new PictureBox
            {
                Name = "picDashboardBanner",
                Image = GraphicHelper.GetBanner(500, 320),
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 270,
                Top = 140,
                Width = 255,
                Height = 420,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.Transparent
            };
            panelMain.Controls.Add(picDashboardBanner);

            Ui.Apply(this);
        }

        private void ApplyRolePermissions()
        {
            btnMechanic.Enabled = Session.IsAdmin;
            btnSpareParts.Enabled = Session.IsAdmin;
        }

        // Customer Management button
        private void btnCustomer_Click(object sender, EventArgs e)
        {
            try
            {
                using CustomerForm customerForm = new CustomerForm();
                customerForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Vehicle Management button
        private void btnVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                using VehicleForm vehicleForm = new VehicleForm();
                vehicleForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Mechanic Management button
        private void btnMechanic_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Mechanic management is only accessible to administrators.")) return;
            try
            {
                using MechanicForm mechanicForm = new MechanicForm();
                mechanicForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Service Management button
        private void btnService_Click(object sender, EventArgs e)
        {
            try
            {
                using ServiceJobForm serviceForm = new ServiceJobForm();
                serviceForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Spare Parts Management button
        private void btnSpareParts_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Spare parts management is only accessible to administrators.")) return;
            try
            {
                using SparePartsForm sparePartsForm = new SparePartsForm();
                sparePartsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Invoice Management button
        private void btnInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                using InvoiceForm invoiceForm = new InvoiceForm();
                invoiceForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Payment Management button
        private void btnPayment_Click(object sender, EventArgs e)
        {
            try
            {
                using PaymentForm paymentForm = new PaymentForm();
                paymentForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Reports button
        private void btnReports_Click(object sender, EventArgs e)
        {
            try
            {
                using ReportForm reportForm = new ReportForm();
                reportForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro");
            }
        }

        // Logout button
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // Form load
        private void DashboardForm_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = "Welcome, " + Session.Username + " (" + Session.Role + ")";
            ApplyRolePermissions();
        }
    }
}
