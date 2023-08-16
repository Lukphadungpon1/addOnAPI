using AddOn_API.DTOs.StoreProceduce;
using Microsoft.EntityFrameworkCore;

namespace AddOn_API.Data
{
    public class DbStoreProceduce : DbContext
    {
    
        public DbStoreProceduce()
        {
          
        }

        public DbStoreProceduce(DbContextOptions<DbStoreProceduce> options)
            : base(options)
        {
        }


        public virtual DbSet<spGenerateLot> spGenerateLot { get; set; } = null;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
          
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=10.192.10.44;user id=sa; password=@R0FuTH2019; Database=KTH_MASTER;Persist Security Info=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<spGenerateLot>(entity =>
            {
                entity.HasKey(e => new { e.RowId });
            });



        }

        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}