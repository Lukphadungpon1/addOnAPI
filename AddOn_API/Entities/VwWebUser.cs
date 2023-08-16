using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class VwWebUser
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
        public string? EmpPassword { get; set; }
        public string? EmpEmail { get; set; }
        public string? EmpStatus { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? AuthDate { get; set; }
        public string? Site { get; set; }
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
