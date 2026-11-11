using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class CustomerForm : Form
    {
        public CustomerForm()
        {
            Session.Require(false);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvCustomers, new (string, Control)[] { ("Name", txtName), ("Phone", txtPhone), ("Address", txtAddress), ("NIC", txtNIC), ("Email", txtEmail) }, new Control[] { btnAdd, btnUpdate, btnDelete, btnClear, btnClose }, new Control[] { txtSearch, btnSearch });
            txtName.MaxLength=100;
            txtPhone.MaxLength=20;
            txtAddress.MaxLength=200;
            txtNIC.MaxLength=20;
            txtEmail.MaxLength=100;

            btnDelete.Enabled = Session.IsAdmin;
        }

        // Load customer data
        private void LoadCustomers()
        {
            try
            {
                string query = "SELECT CustomerID, CustomerName, Phone, Address, NIC, Email FROM Customers";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvCustomers.AutoGenerateColumns = true;
                dgvCustomers.DataSource = null;
                dgvCustomers.DataSource = dt;
                dgvCustomers.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add customer
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!(Ui.Contact(txtName.Text, txtPhone.Text, txtEmail.Text) && Ui.Check(txtAddress.Text.Length <= 200 && txtNIC.Text.Length <= 20, "Address or NIC is too long."))) return;
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();
            string nic = txtNIC.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Check if fields are empty
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Please enter name and phone", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string query = "INSERT INTO Customers (CustomerName, Phone, Address, NIC, Email) VALUES (@name, @phone, @address, @nic, @email)";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@name", name), DBConnection.P("@phone", phone), DBConnection.P("@address", address), DBConnection.P("@nic", nic), DBConnection.P("@email", email) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Customer added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadCustomers();
                }
                else
                {
                    MessageBox.Show("Failed to add customer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update customer
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!(Ui.Contact(txtName.Text, txtPhone.Text, txtEmail.Text) && Ui.Check(txtAddress.Text.Length <= 200 && txtNIC.Text.Length <= 20, "Address or NIC is too long."))) return;
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a customer to update", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string id = dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value.ToString();
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();
            string nic = txtNIC.Text.Trim();
            string email = txtEmail.Text.Trim();

            try
            {
                string query = "UPDATE Customers SET CustomerName = @name, Phone = @phone, Address = @address, NIC = @nic, Email = @email WHERE CustomerID = @id";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@name", name), DBConnection.P("@phone", phone), DBConnection.P("@address", address), DBConnection.P("@nic", nic), DBConnection.P("@email", email), DBConnection.P("@id", int.Parse(id)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Customer updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadCustomers();
                }
                else
                {
                    MessageBox.Show("Failed to update customer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete customer
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Only administrators can delete records.")) return;
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a customer to delete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string id = dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value.ToString();

                try
                {
                    string query = "DELETE FROM Customers WHERE CustomerID = @id";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@id", int.Parse(id)) };
                    int rowsAffected = DBConnection.ExecuteNonQuery(query, queryParameters);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadCustomers();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete customer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Search customer
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT CustomerID, CustomerName, Phone, Address, NIC, Email FROM Customers WHERE CustomerName LIKE @searchText OR Phone LIKE @searchText OR NIC LIKE @searchText OR Email LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvCustomers.DataSource = null;
                dgvCustomers.DataSource = dt;
                dgvCustomers.Refresh();
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
            txtAddress.Text = "";
            txtNIC.Text = "";
            txtEmail.Text = "";
            txtSearch.Text = "";
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadCustomers();
        }

        // DataGridView selection
        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];
                txtName.Text = row.Cells["CustomerName"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                txtNIC.Text = row.Cells["NIC"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
            }
        }

        // Form load
        private void CustomerForm_Load(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
