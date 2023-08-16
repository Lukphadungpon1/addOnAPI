namespace AddOn_API.DTOs.Account
{
    public class AccountResponse
    {
         public int RowId { get; set; }
        public string? EmpCode { get; set; }
        public string? EmpName { get; set; }
        public string? EmpLname { get; set; }
        public string? EmpNameTh { get; set; }
        public string? EmpSex { get; set; }
        public string? EmpPosition { get; set; }
        public string? EmpSection { get; set; }
        public string? EmpDepartment { get; set; }
        public string? EmpUsername { get; set; }
        public string? EmpEmail { get; set; }
         public string? Site { get; set; }
         public string? RoleName {get; set;}
        
    }
}