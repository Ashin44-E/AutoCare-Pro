using System;
using System.Windows.Forms;
using VehicleServiceCenterManagementSystem.Helpers;
namespace VehicleServiceCenterManagementSystem.Forms
{
    public class PasswordForm:Form
    {
        public PasswordForm(string mode)
        {
            if(mode=="change")Session.Require();if(mode=="admin")Session.Require(true);
            Text=mode=="change"?"Change Password":mode=="admin"?"Administrator Password Reset":"Recover Password";
            Width=510;Height=370;StartPosition=FormStartPosition.CenterParent;
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,Padding=new Padding(20),AutoScroll=true};Controls.Add(layout);
            var identity=new TextBox{Width=430,UseSystemPasswordChar=mode=="change"};
            layout.Controls.Add(new Label{Text=mode=="change"?"Current password":"Username",AutoSize=true});layout.Controls.Add(identity);
            var code=new TextBox{Width=430};
            if(mode=="recover"){layout.Controls.Add(new Label{Text="Recovery code saved when registering/changing password",AutoSize=true});layout.Controls.Add(code);}
            var password=new TextBox{Width=430,UseSystemPasswordChar=true};var confirm=new TextBox{Width=430,UseSystemPasswordChar=true};
            layout.Controls.Add(new Label{Text="New password (4-128 characters)",AutoSize=true});layout.Controls.Add(password);
            layout.Controls.Add(new Label{Text="Confirm new password",AutoSize=true});layout.Controls.Add(confirm);
            var save=new Button{Text="Save password",AutoSize=true};layout.Controls.Add(save);AcceptButton=save;
            save.Click+=(s,e)=>
            {
                if(!Ui.Check(password.Text==confirm.Text,"Passwords do not match."))return;
                try{save.Enabled=false;string recovery=mode=="change"?Accounts.ChangePassword(identity.Text,password.Text):Accounts.Recover(identity.Text,code.Text,password.Text,mode=="admin");ShowRecovery(recovery);DialogResult=DialogResult.OK;Close();}
                catch(Exception ex){MessageBox.Show(Ui.Error(ex),"AutoCare-Pro");}finally{save.Enabled=true;}
            };
            Ui.Apply(this);identity.MaxLength=mode=="change"?128:50;password.MaxLength=confirm.MaxLength=128;
        }
        public static void ShowRecovery(string code)
        {
            using var form=new Form{Text="AutoCare-Pro - Save your recovery code",Width=620,Height=200,StartPosition=FormStartPosition.CenterParent};
            form.Controls.Add(new Label{Text="Save this code securely. It replaces any previous recovery code.",Dock=DockStyle.Top,Height=40});
            form.Controls.Add(new TextBox{Text=code,ReadOnly=true,Dock=DockStyle.Top});
            var done=new Button{Text="I have saved the code",Dock=DockStyle.Bottom,Height=40};done.Click+=(s,e)=>form.Close();form.Controls.Add(done);form.ShowDialog();
        }
    }
}
