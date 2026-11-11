using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class HomeForm : Form
    {
        private PictureBox picBanner;

        public HomeForm()
        {
            InitializeComponent();
            lblTitle.Text = "AutoCare-Pro";
            lblWelcome.Text = "Vehicle Service Center Management System";

            this.ClientSize = new Size(680, 560);

            picBanner = new PictureBox
            {
                Name = "picBanner",
                Image = GraphicHelper.GetBanner(600, 310),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(40, 95),
                Size = new Size(600, 310),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(picBanner);

            btnLogin.Size = new Size(260, 50);
            btnLogin.Location = new Point(65, 435);

            btnRegister.Size = new Size(260, 50);
            btnRegister.Location = new Point(355, 435);

            this.Resize += (s, e) => CenterControls();
            Ui.Apply(this);
        }

        private void CenterControls()
        {
            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;
            lblWelcome.Left = (this.ClientSize.Width - lblWelcome.Width) / 2;
            if (picBanner != null)
            {
                picBanner.Left = (this.ClientSize.Width - picBanner.Width) / 2;
            }
            int totalBtnWidth = btnLogin.Width + btnRegister.Width + 30;
            int startX = (this.ClientSize.Width - totalBtnWidth) / 2;
            btnLogin.Left = startX;
            btnRegister.Left = startX + btnLogin.Width + 30;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using LoginForm loginForm = new LoginForm();
            this.Hide();
            loginForm.ShowDialog();
            this.Show();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using RegisterForm registerForm = new RegisterForm();
            this.Hide();
            registerForm.ShowDialog();
            this.Show();
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            CenterControls();
        }
    }
}
