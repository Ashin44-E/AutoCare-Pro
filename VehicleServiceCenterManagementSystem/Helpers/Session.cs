using System;
namespace VehicleServiceCenterManagementSystem.Helpers
{
    public static class Session
    {
        public static int UserId {get;private set;}
        public static string Username {get;private set;}
        public static string Role {get;private set;}
        public static bool IsAdmin => UserId>0 && string.Equals(Role,"Admin",StringComparison.OrdinalIgnoreCase);
        public static void Start(int id,string username,string role) { UserId=id; Username=username; Role=role; }
        public static void Clear() { UserId=0; Username=null; Role=null; }
        public static void Require(bool admin=false)
        { if(UserId==0 || (admin && !IsAdmin)) throw new InvalidOperationException("You do not have permission to use this function."); }
    }
}
