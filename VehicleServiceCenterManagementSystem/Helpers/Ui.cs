using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net.Mail;
namespace VehicleServiceCenterManagementSystem.Helpers
{
    public static class Ui
    {
        public static string Error(Exception ex)
        {
            if(ex is SqlException sql)
            {
                if(sql.Number==2601 || sql.Number==2627) return "This record already exists. Use a different unique value.";
                if(sql.Number==547) return "This change conflicts with related records or a data rule. Records in use cannot be deleted.";
                if(sql.Number>=50000) return sql.Message;
                if(sql.Number==1205) return "Another user changed these records. Refresh and try again.";
                return "The database operation failed. Check the SQL Server connection and database setup, then try again.";
            }
            return ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "The operation could not be completed. Check the input and try again.";
        }
        public static bool Check(bool valid,string message)
        { if(!valid) MessageBox.Show(message,"AutoCare-Pro",MessageBoxButtons.OK,MessageBoxIcon.Warning); return valid; }
        public static bool Contact(string name,string phone,string email="")
        {
            bool valid=!string.IsNullOrWhiteSpace(name) && name.Trim().Length<=100 && Regex.IsMatch(phone.Trim(),@"^\+?[0-9][0-9 ()-]{6,18}$") && Regex.Replace(phone,@"\D", "").Length>=7 && Regex.Replace(phone,@"\D", "").Length<=15;
            if(!string.IsNullOrWhiteSpace(email)) valid &= email.Length<=100 && MailAddress.TryCreate(email,out var address) && address.Address==email;
            return Check(valid,"Enter a name, a valid phone number (7-20 characters), and a valid email if provided.");
        }
        public static void Apply(Form form)
        { form.Text="AutoCare-Pro - "+form.Text.Replace(" - Vehicle Service Center", ""); form.AutoScroll=true; Style(form); }
        public static TableLayoutPanel Fields(params (string Label, Control Input)[] fields)
        {
            var table = new TableLayoutPanel { Dock=DockStyle.Top, AutoSize=true, ColumnCount=4, Padding=new Padding(8), BackColor=Color.White };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            for(int i=0;i<fields.Length;i++)
            {
                int column=(i%2)*2,row=i/2;
                table.Controls.Add(new Label { Text=fields[i].Label, AutoSize=true, Anchor=AnchorStyles.Left, Margin=new Padding(8,10,8,10) },column,row);
                Control input=fields[i].Input;input.Dock=DockStyle.Fill;input.Margin=new Padding(4,8,12,8);input.MinimumSize=new Size(100,25);
                table.Controls.Add(input,column+1,row);
            }
            return table;
        }
        public static FlowLayoutPanel Actions(params Control[] controls)
        {
            var row=new FlowLayoutPanel { Dock=DockStyle.Top, AutoSize=true, Padding=new Padding(8), BackColor=Color.White, WrapContents=true };
            foreach(Control control in controls)
            {
                control.Dock=DockStyle.None;control.Margin=new Padding(4);if(control is Button)control.Size=new Size(145,36);
                row.Controls.Add(control);
            }
            return row;
        }
        public static void Manage(Form form, DataGridView grid, (string Label, Control Input)[] fields, Control[] actions, Control[] search)
        {
            form.SuspendLayout();form.Controls.Clear();form.Padding=new Padding(12);form.BackColor=Color.FromArgb(239,243,248);
            var layout=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=1, RowCount=4 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            layout.Controls.Add(Fields(fields),0,0);layout.Controls.Add(Actions(actions),0,1);layout.Controls.Add(Actions(search),0,2);
            grid.Dock=DockStyle.Fill;grid.MinimumSize=new Size(300,170);layout.Controls.Add(grid,0,3);form.Controls.Add(layout);
            Fit(form,1100,760);Style(form);form.ResumeLayout(true);
        }
        public static void Fit(Form form,int width,int height)
        {
            var screen=Screen.FromControl(form).WorkingArea;
            form.Size=new Size(Math.Min(width,screen.Width-40),Math.Min(height,screen.Height-60));
            form.FormBorderStyle=FormBorderStyle.Sizable;form.MaximizeBox=true;
            form.MinimumSize=new Size(Math.Min(800,screen.Width-40),Math.Min(600,screen.Height-60));
        }
        private static void Style(Control parent)
        {
            foreach(Control c in parent.Controls)
            {
                if(c is ComboBox combo) combo.DropDownStyle=ComboBoxStyle.DropDownList;
                if(c is DataGridView grid)
                {
                    grid.AllowUserToAddRows=false; grid.ReadOnly=true; grid.MultiSelect=false; grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
                    grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(242,246,250);
                    grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    grid.DataBindingComplete+=(sender,e)=>
                    {
                        foreach(DataGridViewColumn column in grid.Columns)
                        {
                            column.HeaderText=Regex.Replace(column.Name,"([a-z])([A-Z])","$1 $2");
                            if(column.ValueType==typeof(decimal))column.DefaultCellStyle.Format="N2";
                            if(column.ValueType==typeof(DateTime))column.DefaultCellStyle.Format="yyyy-MM-dd";
                        }
                    };
                    grid.EnableHeadersVisualStyles=false; grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(35,57,78); grid.ColumnHeadersDefaultCellStyle.ForeColor=Color.White;
                }
                if(c is Button button && button.Dock==DockStyle.None)
                { button.FlatStyle=FlatStyle.Flat; button.FlatAppearance.BorderSize=0; button.BackColor=button.Text.Contains("Delete")?Color.FromArgb(173,52,62):Color.FromArgb(33,105,157); button.ForeColor=Color.White; }
                if(c is TextBox text) { text.MaxLength=text.Multiline?500:100; if(text.Name=="txtSearch")text.PlaceholderText="Search records"; }
                if(c is Label) c.ForeColor=Color.FromArgb(35,57,78);
                Style(c);
            }
        }
    }
}
