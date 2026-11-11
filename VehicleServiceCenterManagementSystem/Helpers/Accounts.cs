using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using VehicleServiceCenterManagementSystem.Database;
using static VehicleServiceCenterManagementSystem.Database.DBConnection;
namespace VehicleServiceCenterManagementSystem.Helpers
{
    public static class Accounts
    {
        public static bool Login(string username,string password)
        {
            Session.Clear();
            bool authenticated=false; int id=0; string role=null;
            bool ok=ExecuteTransaction((c,t)=>
            {
                using var cmd=Command(c,t,"SELECT UserID,Password,UserType,LockedUntil FROM Users WITH(UPDLOCK,HOLDLOCK) WHERE Username=@u",P("@u",username.Trim()));
                var dt=new DataTable(); using(var reader=cmd.ExecuteReader())dt.Load(reader);
                if(dt.Rows.Count!=1) return; // Never choose an arbitrary duplicate account.
                var r=dt.Rows[0]; id=(int)r["UserID"];
                string rawRole=r["UserType"]?.ToString()?.Trim()??"";
                bool isAdmin=string.Equals(rawRole,"Admin",StringComparison.OrdinalIgnoreCase)||string.Equals(rawRole,"Administrator",StringComparison.OrdinalIgnoreCase);
                bool isUser=string.Equals(rawRole,"User",StringComparison.OrdinalIgnoreCase);
                if(r["LockedUntil"]!=DBNull.Value && (DateTime)r["LockedUntil"]>DateTime.UtcNow) return;
                string stored=r["Password"].ToString();
                bool match=PasswordHelper.VerifyPassword(password,stored);
                // One-time compatibility for existing plaintext accounts; successful login upgrades immediately.
                if(!PasswordHelper.IsHash(stored)) match=CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(password)),SHA256.HashData(Encoding.UTF8.GetBytes(stored)));
                if(!match || (!isAdmin && !isUser))
                {
                    using var fail=Command(c,t,"UPDATE Users SET FailedAttempts=FailedAttempts+1,LockedUntil=CASE WHEN FailedAttempts>=4 THEN DATEADD(minute,15,SYSUTCDATETIME()) ELSE NULL END WHERE UserID=@id",P("@id",id)); fail.ExecuteNonQuery(); return;
                }
                role=isAdmin?"Admin":"User";
                using var success=Command(c,t,"UPDATE Users SET Password=@p,FailedAttempts=0,LockedUntil=NULL WHERE UserID=@id",P("@p",stored.StartsWith("PBKDF2$600000$")?stored:PasswordHelper.HashPassword(password)),P("@id",id)); success.ExecuteNonQuery(); authenticated=true;
            },out var error);
            if(!ok) throw new InvalidOperationException(error);
            if(authenticated) Session.Start(id,username.Trim(),role);
            return authenticated;
        }
        public static string Register(string username,string password,string role=null)
        {
            if(string.IsNullOrWhiteSpace(username) || username.Trim().Length<3 || username.Trim().Length>50 || !PasswordHelper.Valid(password)) throw new ArgumentException("Use a username of 3-50 characters and a password of 4-128 characters.");
            string recovery=PasswordHelper.RecoveryCode();
            bool ok=ExecuteTransaction((c,t)=>
            {
                string targetRole;
                if(string.IsNullOrWhiteSpace(role))
                {
                    using var count=Command(c,t,"SELECT COUNT(*) FROM Users WITH(UPDLOCK,HOLDLOCK)"); bool first=(int)count.ExecuteScalar()==0;
                    targetRole=first?"Admin":"User";
                }
                else if(string.Equals(role.Trim(),"Admin",StringComparison.OrdinalIgnoreCase)||string.Equals(role.Trim(),"Administrator",StringComparison.OrdinalIgnoreCase))
                {
                    targetRole="Admin";
                }
                else if(string.Equals(role.Trim(),"User",StringComparison.OrdinalIgnoreCase))
                {
                    targetRole="User";
                }
                else
                {
                    throw new ArgumentException("Select a valid user role (Admin or User).");
                }
                using var cmd=Command(c,t,"INSERT INTO Users(Username,Password,UserType,RecoveryHash) VALUES(@u,@p,@r,@h)",P("@u",username.Trim()),P("@p",PasswordHelper.HashPassword(password)),P("@r",targetRole),P("@h",PasswordHelper.HashPassword(recovery))); cmd.ExecuteNonQuery();
            },out var error);
            if(!ok) throw new InvalidOperationException(error); return recovery;
        }
        public static string ChangePassword(string current,string password)
        {
            Session.Require(); if(!PasswordHelper.Valid(password)) throw new ArgumentException("Use a password of 4-128 characters.");
            string recovery=PasswordHelper.RecoveryCode();
            bool ok=ExecuteTransaction((c,t)=>
            {
                using var read=Command(c,t,"SELECT Password FROM Users WITH(UPDLOCK,HOLDLOCK) WHERE UserID=@id",P("@id",Session.UserId));
                if(!PasswordHelper.VerifyPassword(current,Convert.ToString(read.ExecuteScalar()))) throw new InvalidOperationException("Current password is incorrect.");
                using var write=Command(c,t,"UPDATE Users SET Password=@p,RecoveryHash=@h WHERE UserID=@id",P("@p",PasswordHelper.HashPassword(password)),P("@h",PasswordHelper.HashPassword(recovery)),P("@id",Session.UserId));write.ExecuteNonQuery();
            },out var error);
            if(!ok) throw new InvalidOperationException(error);return recovery;
        }
        public static string Recover(string username,string code,string password,bool admin=false)
        {
            if(admin) Session.Require(true);
            if(!PasswordHelper.Valid(password))throw new ArgumentException("Use a password of 4-128 characters.");
            string recovery=PasswordHelper.RecoveryCode(); bool changed=false;
            bool ok=ExecuteTransaction((c,t)=>
            {
                using var read=Command(c,t,"SELECT UserID,RecoveryHash,LockedUntil FROM Users WITH(UPDLOCK,HOLDLOCK) WHERE Username=@u",P("@u",username.Trim()));
                var dt=new DataTable();using(var reader=read.ExecuteReader())dt.Load(reader);
                if(dt.Rows.Count!=1)return;
                var row=dt.Rows[0];
                if(!admin && row["LockedUntil"]!=DBNull.Value && (DateTime)row["LockedUntil"]>DateTime.UtcNow)return;
                if(!admin && !PasswordHelper.VerifyPassword(code.Trim(),row["RecoveryHash"].ToString()))
                {
                    using var fail=Command(c,t,"UPDATE Users SET FailedAttempts=FailedAttempts+1,LockedUntil=CASE WHEN FailedAttempts>=4 THEN DATEADD(minute,15,SYSUTCDATETIME()) ELSE NULL END WHERE UserID=@id",P("@id",row["UserID"]));fail.ExecuteNonQuery();return;
                }
                using var write=Command(c,t,"UPDATE Users SET Password=@p,RecoveryHash=@h,FailedAttempts=0,LockedUntil=NULL WHERE UserID=@id",P("@p",PasswordHelper.HashPassword(password)),P("@h",PasswordHelper.HashPassword(recovery)),P("@id",row["UserID"]));write.ExecuteNonQuery();changed=true;
            },out var error);
            if(!ok)throw new InvalidOperationException(error);
            if(!changed)throw new InvalidOperationException("Account or recovery code is invalid, ambiguous, or temporarily locked. Contact your administrator if needed.");
            return recovery;
        }
    }
}
