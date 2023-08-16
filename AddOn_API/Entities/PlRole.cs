using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class PlRole
    {
        public PlRole()
        {
            PlAccounts = new HashSet<PlAccount>();
        }

        public int RoleId { get; set; }
        public string? Name { get; set; }
        public DateTime? Created { get; set; }
        public string? CreateBy { get; set; }

        public virtual ICollection<PlAccount> PlAccounts { get; set; }
    }
}
