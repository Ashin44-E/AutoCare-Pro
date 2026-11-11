using VehicleServiceCenterManagementSystem.Helpers;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Database;
using ClosedXML.Excel;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace VehicleServiceCenterManagementSystem.Forms
{
    public class FinancialReportForm : Form
    {
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnGenerate;
        private Button btnExportExcel;
        private Button btnExportCsv;
        private DataGridView dgvReport;

        private DataTable reportTable;

        public FinancialReportForm()
        {
            Text = "Financial Reports";
            Width = 900;
            Height = 600;
            Session.Require(true);
            InitializeComponents();
            Ui.Apply(this);
        }

        private void InitializeComponents()
        {
            dtpFrom = new DateTimePicker { Left = 10, Top = 10, Width = 150 };
            dtpTo = new DateTimePicker { Left = 170, Top = 10, Width = 150 };
            btnGenerate = new Button { Left = 340, Top = 10, Width = 100, Text = "Generate" };
            btnExportExcel = new Button { Left = 450, Top = 10, Width = 120, Text = "Export Excel" };
            btnExportCsv = new Button { Left = 580, Top = 10, Width = 120, Text = "Export CSV" };
            var btnExportPdf = new Button { Left = 710, Top = 10, Width = 120, Text = "Export PDF" };
            dgvReport = new DataGridView { Left = 10, Top = 50, Width = 860, Height = 500, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            btnGenerate.Click += BtnGenerate_Click;
            btnExportExcel.Click += BtnExportExcel_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
            btnExportPdf.Click += BtnExportPdf_Click;

            Controls.Add(dtpFrom);
            Controls.Add(dtpTo);
            Controls.Add(btnGenerate);
            Controls.Add(btnExportExcel);
            Controls.Add(btnExportCsv);
            Controls.Add(btnExportPdf);
            Controls.Add(dgvReport);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string from = dtpFrom.Value.ToString("yyyy-MM-dd");
                string to = dtpTo.Value.ToString("yyyy-MM-dd");

                if (dtpFrom.Value.Date > dtpTo.Value.Date) throw new ArgumentException("Start date must not be after end date.");
                string query = "SELECT d.ReportDate, COALESCE(i.Billed,0) AS Billed, COALESCE(p.Received,0) AS Received, COALESCE(i.LegacyDuplicateBilled,0) AS LegacyDuplicateBilled FROM (SELECT InvoiceDate ReportDate FROM Invoices UNION SELECT PaymentDate FROM Payments) d LEFT JOIN (SELECT InvoiceDate, SUM(CASE WHEN LegacyDuplicateOf IS NULL THEN TotalAmount ELSE 0 END) Billed, SUM(CASE WHEN LegacyDuplicateOf IS NOT NULL THEN TotalAmount ELSE 0 END) LegacyDuplicateBilled FROM Invoices GROUP BY InvoiceDate) i ON i.InvoiceDate=d.ReportDate LEFT JOIN (SELECT PaymentDate, SUM(Amount) Received FROM Payments GROUP BY PaymentDate) p ON p.PaymentDate=d.ReportDate WHERE d.ReportDate>=@from AND d.ReportDate<@to ORDER BY d.ReportDate";
                reportTable = DBConnection.ExecuteQuery(query, DBConnection.P("@from", dtpFrom.Value.Date), DBConnection.P("@to", dtpTo.Value.Date.AddDays(1)));
                dgvReport.DataSource = reportTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            if (reportTable == null)
            {
                MessageBox.Show("Generate a report first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "FinancialReport.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var wb = new XLWorkbook())
                        {
                            wb.Worksheets.Add(reportTable, "Report");
                            wb.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Exported to Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            if (reportTable == null)
            {
                MessageBox.Show("Generate a report first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV File|*.csv", FileName = "FinancialReport.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new StreamWriter(sfd.FileName))
                        {
                            // header
                            for (int i = 0; i < reportTable.Columns.Count; i++)
                            {
                                if (i > 0) sw.Write(",");
                                sw.Write(reportTable.Columns[i].ColumnName);
                            }
                            sw.WriteLine();

                            foreach (DataRow row in reportTable.Rows)
                            {
                                for (int i = 0; i < reportTable.Columns.Count; i++)
                                {
                                    if (i > 0) sw.Write(",");
                                    sw.Write("\"" + Convert.ToString(row[i], System.Globalization.CultureInfo.InvariantCulture).Replace("\"", "\"\"") + "\"");
                                }
                                sw.WriteLine();
                            }
                        }
                        MessageBox.Show("Exported to CSV.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            if (reportTable == null)
            {
                MessageBox.Show("Generate a report first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF File|*.pdf", FileName = "FinancialReport.pdf" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var doc = new PdfDocument())
                        {
                            XFont font = new XFont("Verdana", 8, XFontStyle.Regular);
                            double margin = 40;
                            double yStart = margin;
                            double lineHeight;

                            PdfPage page = doc.AddPage();
                            var gfx = XGraphics.FromPdfPage(page);
                            lineHeight = font.GetHeight() + 6;

                            // draw header
                            for (int c = 0; c < reportTable.Columns.Count; c++)
                            {
                                gfx.DrawString(reportTable.Columns[c].ColumnName, font, XBrushes.Black, margin + c * 125, yStart);
                            }
                            yStart += lineHeight;

                            foreach (DataRow row in reportTable.Rows)
                            {
                                for (int c = 0; c < reportTable.Columns.Count; c++)
                                {
                                    string text = row[c]?.ToString() ?? string.Empty;
                                    gfx.DrawString(text, font, XBrushes.Black, margin + c * 125, yStart);
                                }
                                yStart += lineHeight;
                                if (yStart > page.Height - margin)
                                {
                                    gfx.Dispose();
                                    page = doc.AddPage();
                                    gfx = XGraphics.FromPdfPage(page);
                                    yStart = margin;
                                }
                            }

                            gfx.Dispose();
                            doc.Save(sfd.FileName);
                        }

                        MessageBox.Show("Exported to PDF.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting PDF: " + Ui.Error(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
