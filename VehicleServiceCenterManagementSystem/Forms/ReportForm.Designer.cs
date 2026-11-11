namespace VehicleServiceCenterManagementSystem.Forms
{
    partial class ReportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblReportTitle = new System.Windows.Forms.Label();
            this.btnCustomerReport = new System.Windows.Forms.Button();
            this.btnVehicleReport = new System.Windows.Forms.Button();
            this.btnServiceReport = new System.Windows.Forms.Button();
            this.btnSparePartsReport = new System.Windows.Forms.Button();
            this.btnPaymentReport = new System.Windows.Forms.Button();
            this.btnMonthlyReport = new System.Windows.Forms.Button();
            this.dtpMonth = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.dgvReports = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReports)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblReportTitle
            // 
            this.lblReportTitle.AutoSize = true;
            this.lblReportTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReportTitle.ForeColor = System.Drawing.Color.White;
            this.lblReportTitle.Location = new System.Drawing.Point(20, 20);
            this.lblReportTitle.Name = "lblReportTitle";
            this.lblReportTitle.Size = new System.Drawing.Size(171, 24);
            this.lblReportTitle.TabIndex = 0;
            this.lblReportTitle.Text = "Select a report type";
            // 
            // btnCustomerReport
            // 
            this.btnCustomerReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCustomerReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCustomerReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCustomerReport.ForeColor = System.Drawing.Color.White;
            this.btnCustomerReport.Location = new System.Drawing.Point(20, 60);
            this.btnCustomerReport.Name = "btnCustomerReport";
            this.btnCustomerReport.Size = new System.Drawing.Size(150, 40);
            this.btnCustomerReport.TabIndex = 1;
            this.btnCustomerReport.Text = "Customer Report";
            this.btnCustomerReport.UseVisualStyleBackColor = false;
            this.btnCustomerReport.Click += new System.EventHandler(this.btnCustomerReport_Click);
            // 
            // btnVehicleReport
            // 
            this.btnVehicleReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnVehicleReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVehicleReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVehicleReport.ForeColor = System.Drawing.Color.White;
            this.btnVehicleReport.Location = new System.Drawing.Point(180, 60);
            this.btnVehicleReport.Name = "btnVehicleReport";
            this.btnVehicleReport.Size = new System.Drawing.Size(150, 40);
            this.btnVehicleReport.TabIndex = 2;
            this.btnVehicleReport.Text = "Vehicle Report";
            this.btnVehicleReport.UseVisualStyleBackColor = false;
            this.btnVehicleReport.Click += new System.EventHandler(this.btnVehicleReport_Click);
            // 
            // btnServiceReport
            // 
            this.btnServiceReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnServiceReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnServiceReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnServiceReport.ForeColor = System.Drawing.Color.White;
            this.btnServiceReport.Location = new System.Drawing.Point(340, 60);
            this.btnServiceReport.Name = "btnServiceReport";
            this.btnServiceReport.Size = new System.Drawing.Size(150, 40);
            this.btnServiceReport.TabIndex = 3;
            this.btnServiceReport.Text = "Service Report";
            this.btnServiceReport.UseVisualStyleBackColor = false;
            this.btnServiceReport.Click += new System.EventHandler(this.btnServiceReport_Click);
            // 
            // btnSparePartsReport
            // 
            this.btnSparePartsReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnSparePartsReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSparePartsReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSparePartsReport.ForeColor = System.Drawing.Color.White;
            this.btnSparePartsReport.Location = new System.Drawing.Point(20, 110);
            this.btnSparePartsReport.Name = "btnSparePartsReport";
            this.btnSparePartsReport.Size = new System.Drawing.Size(150, 40);
            this.btnSparePartsReport.TabIndex = 4;
            this.btnSparePartsReport.Text = "Spare Parts Report";
            this.btnSparePartsReport.UseVisualStyleBackColor = false;
            this.btnSparePartsReport.Click += new System.EventHandler(this.btnSparePartsReport_Click);
            // 
            // btnPaymentReport
            // 
            this.btnPaymentReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnPaymentReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPaymentReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPaymentReport.ForeColor = System.Drawing.Color.White;
            this.btnPaymentReport.Location = new System.Drawing.Point(180, 110);
            this.btnPaymentReport.Name = "btnPaymentReport";
            this.btnPaymentReport.Size = new System.Drawing.Size(150, 40);
            this.btnPaymentReport.TabIndex = 5;
            this.btnPaymentReport.Text = "Payment Report";
            this.btnPaymentReport.UseVisualStyleBackColor = false;
            this.btnPaymentReport.Click += new System.EventHandler(this.btnPaymentReport_Click);
            // 
            // btnMonthlyReport
            // 
            this.btnMonthlyReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnMonthlyReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMonthlyReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonthlyReport.ForeColor = System.Drawing.Color.White;
            this.btnMonthlyReport.Location = new System.Drawing.Point(340, 110);
            this.btnMonthlyReport.Name = "btnMonthlyReport";
            this.btnMonthlyReport.Size = new System.Drawing.Size(150, 40);
            this.btnMonthlyReport.TabIndex = 6;
            this.btnMonthlyReport.Text = "Monthly Report";
            this.btnMonthlyReport.UseVisualStyleBackColor = false;
            this.btnMonthlyReport.Click += new System.EventHandler(this.btnMonthlyReport_Click);
            // 
            // dtpMonth
            // 
            this.dtpMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpMonth.Location = new System.Drawing.Point(500, 115);
            this.dtpMonth.Name = "dtpMonth";
            this.dtpMonth.Size = new System.Drawing.Size(120, 23);
            this.dtpMonth.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(500, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Month";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(630, 110);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 40);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // dgvReports
            // 
            this.dgvReports.AllowUserToAddRows = false;
            this.dgvReports.AllowUserToDeleteRows = false;
            this.dgvReports.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReports.BackgroundColor = System.Drawing.Color.White;
            this.dgvReports.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReports.Location = new System.Drawing.Point(20, 20);
            this.dgvReports.Name = "dgvReports";
            this.dgvReports.ReadOnly = true;
            this.dgvReports.RowHeadersVisible = false;
            this.dgvReports.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvReports.Size = new System.Drawing.Size(750, 300);
            this.dgvReports.TabIndex = 10;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.dtpMonth);
            this.panel1.Controls.Add(this.btnMonthlyReport);
            this.panel1.Controls.Add(this.btnPaymentReport);
            this.panel1.Controls.Add(this.btnSparePartsReport);
            this.panel1.Controls.Add(this.btnServiceReport);
            this.panel1.Controls.Add(this.btnVehicleReport);
            this.panel1.Controls.Add(this.btnCustomerReport);
            this.panel1.Location = new System.Drawing.Point(20, 60);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(750, 170);
            this.panel1.TabIndex = 11;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.dgvReports);
            this.panel2.Location = new System.Drawing.Point(20, 240);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(780, 340);
            this.panel2.TabIndex = 12;
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(820, 600);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblReportTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ReportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reports";
            this.Load += new System.EventHandler(this.ReportForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReports)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblReportTitle;
        private System.Windows.Forms.Button btnCustomerReport;
        private System.Windows.Forms.Button btnVehicleReport;
        private System.Windows.Forms.Button btnServiceReport;
        private System.Windows.Forms.Button btnSparePartsReport;
        private System.Windows.Forms.Button btnPaymentReport;
        private System.Windows.Forms.Button btnMonthlyReport;
        private System.Windows.Forms.DateTimePicker dtpMonth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView dgvReports;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}
