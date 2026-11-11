using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class VehicleForm : Form
    {
        public VehicleForm()
        {
            Session.Require(false);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvVehicles, new (string, Control)[] { ("Owner", cmbCustomer), ("Vehicle number", txtVehicleNumber), ("Brand", txtBrand), ("Model", txtModel), ("Year", txtYear), ("Mileage", txtMileage) }, new Control[] { btnAdd, btnUpdate, btnDelete, btnClear, btnClose }, new Control[] { txtSearch, btnSearch, btnViewCustomerVehicles });
            txtVehicleNumber.MaxLength=50;
            txtBrand.MaxLength=50;
            txtModel.MaxLength=50;

            btnDelete.Enabled = Session.IsAdmin;
            txtMileage.Text = "0";
        }

        // Load customers to combo box
        private void LoadCustomers()
        {
            try
            {
                string query = "SELECT CustomerID, CustomerName FROM Customers";
                DataTable dt = DBConnection.ExecuteQuery(query);
                
                cmbCustomer.DisplayMember = "CustomerName";
                cmbCustomer.ValueMember = "CustomerID";
                cmbCustomer.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load vehicle data
        private void LoadVehicles()
        {
            try
            {
                string query = "SELECT v.VehicleID, v.CustomerID, v.VehicleNumber, v.Brand, v.Model, v.Year, v.Mileage, c.CustomerName FROM Vehicles v " +
                               "INNER JOIN Customers c ON v.CustomerID = c.CustomerID";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvVehicles.AutoGenerateColumns = true;
                dgvVehicles.DataSource = null;
                dgvVehicles.DataSource = dt;
                dgvVehicles.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading vehicles: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add vehicle
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!(Ui.Check(cmbCustomer.SelectedValue is int && !string.IsNullOrWhiteSpace(txtVehicleNumber.Text) && txtVehicleNumber.Text.Length <= 50 && !string.IsNullOrWhiteSpace(txtBrand.Text) && txtBrand.Text.Length <= 50 && txtModel.Text.Length <= 50 && int.TryParse(txtYear.Text, out int yearValue) && yearValue >= 1886 && yearValue <= DateTime.Today.Year + 1 && int.TryParse(txtMileage.Text, out int mileageValue) && mileageValue >= 0, "Select an owner and enter vehicle number, brand, valid year and nonnegative mileage."))) return;
            if (cmbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string customerId = cmbCustomer.SelectedValue.ToString();
            string vehicleNumber = txtVehicleNumber.Text.Trim();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            string year = txtYear.Text.Trim();

            // Check if fields are empty
            if (string.IsNullOrWhiteSpace(vehicleNumber) || string.IsNullOrWhiteSpace(brand))
            {
                MessageBox.Show("Please enter vehicle number and brand", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Validate duplicate vehicle number
                string checkQuery = "SELECT COUNT(1) FROM Vehicles WHERE VehicleNumber = @vehicleNumber";
                var checkQueryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@vehicleNumber", vehicleNumber) };
                object exists = DBConnection.ExecuteScalar(checkQuery, checkQueryParameters);
                int count = 0;
                if (exists != null && int.TryParse(exists.ToString(), out count) && count > 0)
                {
                    MessageBox.Show("A vehicle with this number already exists.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Vehicles (CustomerID, VehicleNumber, Brand, Model, Year, Mileage) VALUES (@customerId, @vehicleNumber, @brand, @model, @year, @mileage)";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@customerId", int.Parse(customerId)), DBConnection.P("@vehicleNumber", vehicleNumber), DBConnection.P("@brand", brand), DBConnection.P("@model", model), DBConnection.P("@year", int.Parse(year)), DBConnection.P("@mileage", int.Parse(txtMileage.Text)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Vehicle added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadVehicles();
                }
                else
                {
                    MessageBox.Show("Failed to add vehicle", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update vehicle
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!(Ui.Check(cmbCustomer.SelectedValue is int && !string.IsNullOrWhiteSpace(txtVehicleNumber.Text) && txtVehicleNumber.Text.Length <= 50 && !string.IsNullOrWhiteSpace(txtBrand.Text) && txtBrand.Text.Length <= 50 && txtModel.Text.Length <= 50 && int.TryParse(txtYear.Text, out int yearValue) && yearValue >= 1886 && yearValue <= DateTime.Today.Year + 1 && int.TryParse(txtMileage.Text, out int mileageValue) && mileageValue >= 0, "Select an owner and enter vehicle number, brand, valid year and nonnegative mileage."))) return;
            if (dgvVehicles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a vehicle to update", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string vehicleId = dgvVehicles.SelectedRows[0].Cells["VehicleID"].Value.ToString();
            string customerId = cmbCustomer.SelectedValue.ToString();
            string vehicleNumber = txtVehicleNumber.Text.Trim();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            string year = txtYear.Text.Trim();

            try
            {
                string query = "UPDATE Vehicles SET CustomerID = @customerId, VehicleNumber = @vehicleNumber, Brand = @brand, Model = @model, Year = @year, Mileage = @mileage WHERE VehicleID = @vehicleId";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@customerId", int.Parse(customerId)), DBConnection.P("@vehicleNumber", vehicleNumber), DBConnection.P("@brand", brand), DBConnection.P("@model", model), DBConnection.P("@year", int.Parse(year)), DBConnection.P("@mileage", int.Parse(txtMileage.Text)), DBConnection.P("@vehicleId", int.Parse(vehicleId)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Vehicle updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadVehicles();
                }
                else
                {
                    MessageBox.Show("Failed to update vehicle", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete vehicle
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Only administrators can delete records.")) return;
            if (dgvVehicles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a vehicle to delete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this vehicle?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string vehicleId = dgvVehicles.SelectedRows[0].Cells["VehicleID"].Value.ToString();

                try
                {
                    string query = "DELETE FROM Vehicles WHERE VehicleID = @vehicleId";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@vehicleId", int.Parse(vehicleId)) };
                    int rowsAffected = DBConnection.ExecuteNonQuery(query, queryParameters);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Vehicle deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadVehicles();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete vehicle", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Search vehicle
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT v.VehicleID, v.CustomerID, v.VehicleNumber, v.Brand, v.Model, v.Year, v.Mileage, c.CustomerName FROM Vehicles v INNER JOIN Customers c ON v.CustomerID = c.CustomerID WHERE v.VehicleNumber LIKE @searchText OR v.Brand LIKE @searchText OR c.CustomerName LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvVehicles.DataSource = null;
                dgvVehicles.DataSource = dt;
                dgvVehicles.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // View customer vehicles
        private void btnViewCustomerVehicles_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string customerId = cmbCustomer.SelectedValue.ToString();

            try
            {
                string query = "SELECT v.VehicleID, v.CustomerID, v.VehicleNumber, v.Brand, v.Model, v.Year, v.Mileage, c.CustomerName FROM Vehicles v INNER JOIN Customers c ON v.CustomerID = c.CustomerID WHERE v.CustomerID = @customerId";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@customerId", int.Parse(customerId)) };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvVehicles.DataSource = null;
                dgvVehicles.DataSource = dt;
                dgvVehicles.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear fields
        private void ClearFields()
        {
            txtVehicleNumber.Text = "";
            txtBrand.Text = "";
            txtModel.Text = "";
            txtYear.Text = "";
            txtMileage.Text = "0";
            txtSearch.Text = "";
            if (cmbCustomer.Items.Count > 0)
            {
                cmbCustomer.SelectedIndex = 0;
            }
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadVehicles();
        }

        // DataGridView selection
        private void dgvVehicles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvVehicles.Rows[e.RowIndex];
                cmbCustomer.SelectedValue = row.Cells["CustomerID"].Value;
                txtVehicleNumber.Text = row.Cells["VehicleNumber"].Value.ToString();
                txtBrand.Text = row.Cells["Brand"].Value.ToString();
                txtModel.Text = row.Cells["Model"].Value.ToString();
                txtYear.Text = row.Cells["Year"].Value.ToString();
                txtMileage.Text = row.Cells["Mileage"].Value?.ToString() ?? "0";
            }
        }

        // Form load
        private void VehicleForm_Load(object sender, EventArgs e)
        {
            LoadCustomers();
            LoadVehicles();
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
