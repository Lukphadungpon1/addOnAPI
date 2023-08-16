

using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class PlAccount
    {
        public int AccountId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public DateTime? Created { get; set; }
        public string? CreateBy { get; set; }
        public string? Name { get; set; }
        public string? Lname { get; set; }
        public string? Position { get; set; }
        public string? Section { get; set; }
        public string? Department { get; set; }
        public string? Site { get; set; }
        public int? RoleId { get; set; }

        public virtual PlRole? Role { get; set; }
    }
}
