using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class TplocationGroup
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Groups { get; set; }
    }
}
