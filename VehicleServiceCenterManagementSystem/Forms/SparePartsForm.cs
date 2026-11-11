using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public partial class SparePartsForm : Form
    {
        public SparePartsForm()
        {
            Session.Require(true);
            InitializeComponent();
            Ui.Apply(this);
            Ui.Manage(this, dgvSpareParts, new (string, Control)[] { ("Part name", txtPartName), ("Stock quantity", txtQuantity), ("Unit price", txtPrice) }, new Control[] { btnAdd, btnUpdate, btnUpdateStock, btnDelete, btnClear, btnClose }, new Control[] { txtSearch, btnSearch });
        }

        // Load spare parts data
        private void LoadSpareParts()
        {
            try
            {
                string query = "SELECT * FROM SpareParts";
                DataTable dt = DBConnection.ExecuteQuery(query);
                dgvSpareParts.AutoGenerateColumns = true;
                dgvSpareParts.DataSource = null;
                dgvSpareParts.DataSource = dt;
                dgvSpareParts.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading spare parts: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add spare part
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!(Ui.Check(!string.IsNullOrWhiteSpace(txtPartName.Text) && int.TryParse(txtQuantity.Text, out int quantityValue) && quantityValue >= 0 && decimal.TryParse(txtPrice.Text, out decimal priceValue) && priceValue >= 0 && priceValue <= 99999999.99m && decimal.Round(priceValue, 2) == priceValue, "Enter a part name, nonnegative whole stock quantity, and a price with up to two decimal places."))) return;
            string partName = txtPartName.Text.Trim();
            string quantity = txtQuantity.Text.Trim();
            string price = txtPrice.Text.Trim();

            // Check if fields are empty
            if (string.IsNullOrWhiteSpace(partName) || string.IsNullOrWhiteSpace(quantity) || string.IsNullOrWhiteSpace(price))
            {
                MessageBox.Show("Please fill all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string query = "INSERT INTO SpareParts (PartName, Quantity, Price) VALUES (@partName, @quantity, @price)";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@partName", partName), DBConnection.P("@quantity", int.Parse(quantity)), DBConnection.P("@price", decimal.Parse(price)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Spare part added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadSpareParts();
                }
                else
                {
                    MessageBox.Show("Failed to add spare part", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update spare part
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!(Ui.Check(!string.IsNullOrWhiteSpace(txtPartName.Text) && int.TryParse(txtQuantity.Text, out int quantityValue) && quantityValue >= 0 && decimal.TryParse(txtPrice.Text, out decimal priceValue) && priceValue >= 0 && priceValue <= 99999999.99m && decimal.Round(priceValue, 2) == priceValue, "Enter a part name, nonnegative whole stock quantity, and a price with up to two decimal places."))) return;
            if (dgvSpareParts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a spare part to update", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string partId = dgvSpareParts.SelectedRows[0].Cells["PartID"].Value.ToString();
            string partName = txtPartName.Text.Trim();
            string quantity = txtQuantity.Text.Trim();
            string price = txtPrice.Text.Trim();

            try
            {
                string query = "UPDATE SpareParts SET PartName = @partName, Quantity = @quantity, Price = @price WHERE PartID = @partId AND Quantity = @originalQuantity";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@partName", partName), DBConnection.P("@quantity", int.Parse(quantity)), DBConnection.P("@price", decimal.Parse(price)), DBConnection.P("@partId", int.Parse(partId)), DBConnection.P("@originalQuantity", Convert.ToInt32(dgvSpareParts.SelectedRows[0].Cells["Quantity"].Value)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Spare part updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadSpareParts();
                }
                else
                {
                    MessageBox.Show("Stock changed or record no longer exists. Refresh before updating", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update stock
        private void btnUpdateStock_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(int.TryParse(txtQuantity.Text, out int stock) && stock >= 0, "Enter a nonnegative whole stock quantity.")) return;
            if (dgvSpareParts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a spare part to update stock", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string partId = dgvSpareParts.SelectedRows[0].Cells["PartID"].Value.ToString();
            string quantity = txtQuantity.Text.Trim();

            try
            {
                string query = "UPDATE SpareParts SET Quantity = @quantity WHERE PartID = @partId AND Quantity = @originalQuantity";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@quantity", int.Parse(quantity)), DBConnection.P("@partId", int.Parse(partId)), DBConnection.P("@originalQuantity", Convert.ToInt32(dgvSpareParts.SelectedRows[0].Cells["Quantity"].Value)) };
                int result = DBConnection.ExecuteNonQuery(query, queryParameters);

                if (result > 0)
                {
                    MessageBox.Show("Stock updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadSpareParts();
                }
                else
                {
                    MessageBox.Show("Stock changed or record no longer exists. Refresh before updating", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete spare part
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Ui.Check(Session.IsAdmin, "Only administrators can delete records.")) return;
            if (dgvSpareParts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a spare part to delete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this spare part?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string partId = dgvSpareParts.SelectedRows[0].Cells["PartID"].Value.ToString();

                try
                {
                    string query = "DELETE FROM SpareParts WHERE PartID = @partId AND Quantity = @originalQuantity";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@partId", int.Parse(partId)), DBConnection.P("@originalQuantity", Convert.ToInt32(dgvSpareParts.SelectedRows[0].Cells["Quantity"].Value)) };
                    int rowsAffected = DBConnection.ExecuteNonQuery(query, queryParameters);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Spare part deleted successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadSpareParts();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete spare part", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Search spare part
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string query = "SELECT * FROM SpareParts WHERE PartName LIKE @searchText";
                var queryParameters = new System.Data.SqlClient.SqlParameter[] { DBConnection.P("@searchText", "%" + searchText + "%") };
                DataTable dt = DBConnection.ExecuteQuery(query, queryParameters);
                dgvSpareParts.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear fields
        private void ClearFields()
        {
            txtPartName.Text = "";
            txtQuantity.Text = "";
            txtPrice.Text = "";
            txtSearch.Text = "";
        }

        // Clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadSpareParts();
        }

        // DataGridView selection
        private void dgvSpareParts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSpareParts.Rows[e.RowIndex];
                txtPartName.Text = row.Cells["PartName"].Value.ToString();
                txtQuantity.Text = row.Cells["Quantity"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
            }
        }

        // Form load
        private void SparePartsForm_Load(object sender, EventArgs e)
        {
            LoadSpareParts();
        }

        // Close button
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
