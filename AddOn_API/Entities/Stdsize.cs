using System;
using System.Collections.Generic;

namespace AddOn_API.Entities
{
    public partial class Stdsize
    {
        public int Id { get; set; }
        public string? Gendar { get; set; }
        public string? Types { get; set; }
        public string? Size { get; set; }
        public string? Value { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
