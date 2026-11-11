using System;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Helpers;
namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class LoginForm:Form
    {
        public LoginForm(){InitializeComponent();Ui.Apply(this);AcceptButton=btnLogin;txtPassword.MaxLength=128;}
        private void btnLogin_Click(object sender,EventArgs e)
        {
            try
            {
                btnLogin.Enabled=false;
                if(!Accounts.Login(txtUsername.Text.Trim(),txtPassword.Text)){MessageBox.Show("Invalid credentials, duplicate username, or account temporarily locked. Contact your administrator if needed.","AutoCare-Pro");return;}
                if (!PasswordHelper.Valid(txtPassword.Text))
                {
                    MessageBox.Show("Your existing password must be replaced with a password of at least 4 characters.", "AutoCare-Pro");
                    using var change = new PasswordForm("change");
                    if (change.ShowDialog(this) != DialogResult.OK) { Session.Clear(); return; }
                }
                txtPassword.Clear(); Hide();
                try { using var dashboard=new DashboardForm();dashboard.ShowDialog(); }
                finally {Session.Clear();Show();}
            }
            catch(Exception ex){MessageBox.Show(Ui.Error(ex),"AutoCare-Pro");}
            finally{btnLogin.Enabled=true;}
        }
        private void btnClear_Click(object sender,EventArgs e){txtUsername.Clear();txtPassword.Clear();txtUsername.Focus();}
        private void linkForgotPassword_LinkClicked(object sender,LinkLabelLinkClickedEventArgs e){using var form=new PasswordForm("recover");form.ShowDialog(this);}
        private void LoginForm_Load(object sender,EventArgs e){txtUsername.Focus();}
        private void btnBack_Click(object sender,EventArgs e){Close();}
    }
}
