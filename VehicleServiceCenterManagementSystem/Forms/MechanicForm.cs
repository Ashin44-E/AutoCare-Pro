using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class MechanicForm : Form
    {
        public MechanicForm()
        {
            Session.Require(true);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvMechanics, new (string, Control)[] { ("Name", txtName), ("Phone", txtPhone), ("Skill", txtSkill) }, new Control[] { btnAdd, btnUpdate, btnDelete, btnClear, btnClose }, new Control[] { txtSearch, btnSearch });
            txtName.MaxLength=100;
            txtPhone.MaxLength=20;
            txtSkill.MaxLength=100;

        }

        // Load mechanic data
        private void LoadMechanics()
        {
            try
            {
                string query = "SELECT * FROM Mechanics";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvMechanics.AutoGenerateColumns = true;
                dgvMechanics.DataSource = null;
                dgvMechanics.DataSource = dt;
                dgvMechanics.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading mechanics: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add mechanic
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!(Ui.Contact(txtName.Text, txtPhone.Text))) return;
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string skill = txtSkill.Text.Trim();

            // Check if fields are empty
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Please enter name and phone", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string query = "INSERT INTO Mechanics (MechanicName, Phone, Skill) VALUES (@name, @phone, @skill)";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@name", name), DBConnection.P("@phone", phone), DBConnection.P("@skill", skill) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Mechanic added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadMechanics();
                }
                else
                {
                    MessageBox.Show("Failed to add mechanic", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update mechanic
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!(Ui.Contact(txtName.Text, txtPhone.Text))) return;
            if (dgvMechanics.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a mechanic to update", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string id = dgvMechanics.SelectedRows[0].Cells["MechanicID"].Value.ToString();
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string skill = txtSkill.Text.Trim();

            try
            {
                string query = "UPDATE Mechanics SET MechanicName = @name, Phone = @phone, Skill = @skill WHERE MechanicID = @id";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@name", name), DBConnection.P("@phone", phone), DBConnection.P("@skill", skill), DBConnection.P("@id", int.Parse(id)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Mechanic updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadMechanics();
                }
                else
                {
                    MessageBox.Show("Failed to update mechanic", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete mechanic
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Only administrators can delete records.")) return;
            if (dgvMechanics.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a mechanic to delete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this mechanic?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string id = dgvMechanics.SelectedRows[0].Cells["MechanicID"].Value.ToString();

                try
                {
                    string query = "DELETE FROM Mechanics WHERE MechanicID = @id";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@id", int.Parse(id)) };
                    int rowsAffected = DBConnection.ExecuteNonQuery(query, queryParameters);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Mechanic deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadMechanics();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete mechanic", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Search mechanic
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT * FROM Mechanics WHERE MechanicName LIKE @searchText OR Phone LIKE @searchText OR Skill LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvMechanics.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear fields
        private void ClearFields()
        {
            txtName.Text = "";
            txtPhone.Text = "";
            txtSkill.Text = "";
            txtSearch.Text = "";
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadMechanics();
        }

        // DataGridView selection
        private void dgvMechanics_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMechanics.Rows[e.RowIndex];
                txtName.Text = row.Cells["MechanicName"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtSkill.Text = row.Cells["Skill"].Value.ToString();
            }
        }

        // Form load
        private void MechanicForm_Load(object sender, EventArgs e)
        {
            LoadMechanics();
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
