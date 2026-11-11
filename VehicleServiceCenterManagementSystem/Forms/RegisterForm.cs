using System;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Helpers;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            Ui.Apply(this);
            AcceptButton = btnRegister;
            txtPassword.MaxLength = 128;
            txtConfirmPassword.MaxLength = 128;
            InitUserTypes();
        }

        private void InitUserTypes()
        {
            cmbUserType.Items.Clear();
            cmbUserType.Items.Add("Admin");
            cmbUserType.Items.Add("User");
            cmbUserType.SelectedIndex = 0;
            cmbUserType.Enabled = true;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string role = cmbUserType.SelectedItem?.ToString() ?? cmbUserType.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.", "AutoCare-Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.", "AutoCare-Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (!Ui.Check(password == confirmPassword, "Passwords do not match."))
                return;

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please select a user role (Admin or User).", "AutoCare-Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbUserType.Focus();
                return;
            }

            try
            {
                btnRegister.Enabled = false;
                string code = Accounts.Register(username, password, role);
                PasswordForm.ShowRecovery(code);
                MessageBox.Show($"Registration successful as {role}. You can now log in.", "AutoCare-Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Ui.Error(ex), "AutoCare-Pro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            if (cmbUserType.Items.Count > 0)
                cmbUserType.SelectedIndex = 0;
            txtUsername.Focus();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            InitUserTypes();
        }
    }
}
