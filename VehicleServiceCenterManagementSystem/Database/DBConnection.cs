using System;
using System.Data;
using System.Data.SqlClient;
namespace VehicleServiceCenterManagementSystem.Database
{
    public static class DBConnection
    {
        public static SqlConnection GetConnection() => new SqlConnection(Environment.GetEnvironmentVariable("AUTOCARE_CONNECTION_STRING") ?? @"Server=localhost\SQLEXPRESS;Database=VehicleServiceCenterDB;Integrated Security=True;Connect Timeout=10;");
        public static void OpenConnection(SqlConnection c) { if(c.State == ConnectionState.Closed) c.Open(); }
        public static void CloseConnection(SqlConnection c) => c.Close();
        public static SqlParameter P(string name, object value) => new SqlParameter(name, value ?? DBNull.Value);
        public static SqlCommand Command(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] p)
        { var cmd = new SqlCommand(sql,c,t); cmd.Parameters.AddRange(p); return cmd; }
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] p)
        { using var c=GetConnection(); using var cmd=Command(c,null,sql,p); using var a=new SqlDataAdapter(cmd); var dt=new DataTable(); a.Fill(dt); return dt; }
        public static int ExecuteNonQuery(string sql, params SqlParameter[] p)
        { using var c=GetConnection(); c.Open(); using var cmd=Command(c,null,sql,p); return cmd.ExecuteNonQuery(); }
        public static object ExecuteScalar(string sql, params SqlParameter[] p)
        { using var c=GetConnection(); c.Open(); using var cmd=Command(c,null,sql,p); return cmd.ExecuteScalar(); }
        public static bool ExecuteTransaction(Action<SqlConnection,SqlTransaction> action, out string errorMessage)
        {
            try
            {
                using var c=GetConnection(); c.Open(); using var t=c.BeginTransaction(IsolationLevel.Serializable);
                try { action(c,t); t.Commit(); } catch { try { t.Rollback(); } catch { } throw; }
                errorMessage=null; return true;
            }
            catch(Exception ex) { errorMessage=Helpers.Ui.Error(ex); return false; }
        }
    }
}
