using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using AddOn_API.Entities;

namespace AddOn_API.Data
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AllocateCal> AllocateCals { get; set; } = null!;
        public virtual DbSet<AllocateCalSize> AllocateCalSizes { get; set; } = null!;
        public virtual DbSet<AllocateCalSizeTemp> AllocateCalSizeTemps { get; set; } = null!;
        public virtual DbSet<AllocateLot> AllocateLots { get; set; } = null!;
        public virtual DbSet<AllocateLotSize> AllocateLotSizes { get; set; } = null!;
        public virtual DbSet<AllocateMc> AllocateMcs { get; set; } = null!;
        public virtual DbSet<BomOfMaterialD> BomOfMaterialDs { get; set; } = null!;
        public virtual DbSet<BomOfMaterialH> BomOfMaterialHs { get; set; } = null!;
        public virtual DbSet<BuyMonthMaster> BuyMonthMasters { get; set; } = null!;
        public virtual DbSet<BuyYearMaster> BuyYearMasters { get; set; } = null!;
        public virtual DbSet<IssueMaterialD> IssueMaterialDs { get; set; } = null!;
        public virtual DbSet<IssueMaterialH> IssueMaterialHs { get; set; } = null!;
        public virtual DbSet<IssueMaterialLog> IssueMaterialLogs { get; set; } = null!;
        public virtual DbSet<IssueMaterialTranBarcode> IssueMaterialTranBarcodes { get; set; } = null!;
        public virtual DbSet<ProductionOrderD> ProductionOrderDs { get; set; } = null!;
        public virtual DbSet<ProductionOrderH> ProductionOrderHs { get; set; } = null!;
        public virtual DbSet<QueueConvertToSap> QueueConvertToSaps { get; set; } = null!;
        public virtual DbSet<ReqIssueMaterialD> ReqIssueMaterialDs { get; set; } = null!;
        public virtual DbSet<ReqIssueMaterialH> ReqIssueMaterialHs { get; set; } = null!;
        public virtual DbSet<ReqIssueMaterialLog> ReqIssueMaterialLogs { get; set; } = null!;
        public virtual DbSet<SaleOrderD> SaleOrderDs { get; set; } = null!;
        public virtual DbSet<SaleOrderH> SaleOrderHs { get; set; } = null!;
        public virtual DbSet<SaleType> SaleTypes { get; set; } = null!;
        public virtual DbSet<SizeMaster> SizeMasters { get; set; } = null!;
        public virtual DbSet<Stdsize> Stdsizes { get; set; } = null!;
        public virtual DbSet<TplocationGroup> TplocationGroups { get; set; } = null!;
        public virtual DbSet<TpstyleWithLocation> TpstyleWithLocations { get; set; } = null!;
        public virtual DbSet<VwGenerateMcgroupSize> VwGenerateMcgroupSizes { get; set; } = null!;
        public virtual DbSet<VwWebTpapproval> VwWebTpapprovals { get; set; } = null!;
        public virtual DbSet<VwWebUser> VwWebUsers { get; set; } = null!;

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
            modelBuilder.Entity<AllocateCal>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK_AO_ALLOCATE");

                entity.ToTable("ALLOCATE_Cal", "ao");

                entity.Property(e => e.Buy).HasMaxLength(6);

                entity.Property(e => e.Canceled)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreateBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Freeze)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ItemName).HasMaxLength(100);

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.Lot).HasMaxLength(15);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.PurOrder)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.S035).HasColumnName("S_035");

                entity.Property(e => e.S040).HasColumnName("S_040");

                entity.Property(e => e.S045).HasColumnName("S_045");

                entity.Property(e => e.S050).HasColumnName("S_050");

                entity.Property(e => e.S055).HasColumnName("S_055");

                entity.Property(e => e.S060).HasColumnName("S_060");

                entity.Property(e => e.S065).HasColumnName("S_065");

                entity.Property(e => e.S070).HasColumnName("S_070");

                entity.Property(e => e.S075).HasColumnName("S_075");

                entity.Property(e => e.S080).HasColumnName("S_080");

                entity.Property(e => e.S085).HasColumnName("S_085");

                entity.Property(e => e.S090).HasColumnName("S_090");

                entity.Property(e => e.S095).HasColumnName("S_095");

                entity.Property(e => e.S100).HasColumnName("S_100");

                entity.Property(e => e.S105).HasColumnName("S_105");

                entity.Property(e => e.S110).HasColumnName("S_110");

                entity.Property(e => e.S115).HasColumnName("S_115");

                entity.Property(e => e.S120).HasColumnName("S_120");

                entity.Property(e => e.S130).HasColumnName("S_130");

                entity.Property(e => e.S140).HasColumnName("S_140");

                entity.Property(e => e.S150).HasColumnName("S_150");

                entity.Property(e => e.S160).HasColumnName("S_160");

                entity.Property(e => e.S170).HasColumnName("S_170");

                entity.Property(e => e.SaleDocDate).HasColumnType("datetime");

                entity.Property(e => e.SalesOrder)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Transfered)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.UpdateBy).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Width).HasMaxLength(10);
            });

            modelBuilder.Entity<AllocateCalSize>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK_AO_ALLOCATE_CalSize1");

                entity.ToTable("ALLOCATE_CalSize", "ao");

                entity.Property(e => e.Buy).HasMaxLength(6);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(100);

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.Lot).HasMaxLength(15);

                entity.Property(e => e.PurOrder)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SalesOrder)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SizeNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Types)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Width).HasMaxLength(10);
            });

            modelBuilder.Entity<AllocateCalSizeTemp>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK_AO_ALLOCATE_CalSize");

                entity.ToTable("ALLOCATE_CalSizeTemp", "ao");

                entity.Property(e => e.Buy).HasMaxLength(6);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(100);

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.PurOrder)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.RowNo).HasColumnName("rowNo");

                entity.Property(e => e.SalesOrder)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToCode).HasMaxLength(10);

                entity.Property(e => e.SizeNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeSale)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Types).HasMaxLength(10);

                entity.Property(e => e.Width).HasMaxLength(10);
            });

            modelBuilder.Entity<AllocateLot>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.SaleOrderId, e.Lot })
                    .HasName("PK_AllocateLot_1");

                entity.ToTable("AllocateLot", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.Buy).HasMaxLength(6);

                entity.Property(e => e.CreateBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.GenerateMc).HasColumnName("GenerateMC");

                entity.Property(e => e.GenerateMcby)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("GenerateMCBy");

                entity.Property(e => e.GeneratePd).HasColumnName("GeneratePD");

                entity.Property(e => e.GeneratePdby)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("GeneratePDBy");

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.PurOrder).HasMaxLength(30);

                entity.Property(e => e.S035).HasColumnName("S_035");

                entity.Property(e => e.S040).HasColumnName("S_040");

                entity.Property(e => e.S050).HasColumnName("S_050");

                entity.Property(e => e.S055).HasColumnName("S_055");

                entity.Property(e => e.S060).HasColumnName("S_060");

                entity.Property(e => e.S065).HasColumnName("S_065");

                entity.Property(e => e.S070).HasColumnName("S_070");

                entity.Property(e => e.S075).HasColumnName("S_075");

                entity.Property(e => e.S080).HasColumnName("S_080");

                entity.Property(e => e.S085).HasColumnName("S_085");

                entity.Property(e => e.S090).HasColumnName("S_090");

                entity.Property(e => e.S095).HasColumnName("S_095");

                entity.Property(e => e.S100).HasColumnName("S_100");

                entity.Property(e => e.S105).HasColumnName("S_105");

                entity.Property(e => e.S110).HasColumnName("S_110");

                entity.Property(e => e.S115).HasColumnName("S_115");

                entity.Property(e => e.S120).HasColumnName("S_120");

                entity.Property(e => e.S130).HasColumnName("S_130");

                entity.Property(e => e.S140).HasColumnName("S_140");

                entity.Property(e => e.S150).HasColumnName("S_150");

                entity.Property(e => e.S160).HasColumnName("S_160");

                entity.Property(e => e.S170).HasColumnName("S_170");

                entity.Property(e => e.SaleDocDate).HasColumnType("datetime");

                entity.Property(e => e.SaleStartDate).HasColumnType("datetime");

                entity.Property(e => e.ShipToCode).HasMaxLength(10);

                entity.Property(e => e.ShipToName).HasMaxLength(500);

                entity.Property(e => e.SoNumber).HasMaxLength(20);

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.StatusIssueMat).HasMaxLength(10);

                entity.Property(e => e.StatusPlanning).HasMaxLength(10);

                entity.Property(e => e.StatusProduction).HasMaxLength(10);

                entity.Property(e => e.StatusReceiveFg)
                    .HasMaxLength(10)
                    .HasColumnName("StatusReceiveFG");

                entity.Property(e => e.StatusReceiveMat).HasMaxLength(10);

                entity.Property(e => e.UpdateBy).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UploadFile)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Width).HasMaxLength(10);

                entity.HasOne(d => d.SaleOrder)
                    .WithMany(p => p.AllocateLots)
                    .HasForeignKey(d => d.SaleOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AllocateLot_SaleOrderH");
            });

            modelBuilder.Entity<AllocateLotSize>(entity =>
            {
                entity.HasKey(e => e.RowId);

                entity.ToTable("AllocateLotSize", "ao");

                entity.Property(e => e.BomVersion).HasMaxLength(10);

                entity.Property(e => e.CreateBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.SizeNo).HasMaxLength(10);

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdateBy).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.HasOne(d => d.BomOfMaterialH)
                    .WithMany(p => p.AllocateLotSizes)
                    .HasForeignKey(d => new { d.ItemCode, d.BomVersion })
                    .HasConstraintName("FK_AllocateLotSize_BomOfMaterialH");

                entity.HasOne(d => d.AllocateLot)
                    .WithMany(p => p.AllocateLotSizes)
                    .HasForeignKey(d => new { d.AllocateLotId, d.SaleOrderId, d.Lot })
                    .HasConstraintName("FK_AllocateLotSize_AllocateLot");
            });

            modelBuilder.Entity<AllocateMc>(entity =>
            {
                entity.HasKey(e => new { e.PlantCode, e.TypeCode, e.BarcodeId, e.BasketSeq })
                    .HasName("PK_AllocateMC_1");

                entity.ToTable("AllocateMC", "ao");

                entity.Property(e => e.PlantCode).HasMaxLength(10);

                entity.Property(e => e.TypeCode).HasMaxLength(10);

                entity.Property(e => e.BarcodeId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BarcodeQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.Buy).HasMaxLength(6);

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Colors)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.Ponumber)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("PONumber");

                entity.Property(e => e.ShipToCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToDesc)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SizeNo).HasMaxLength(10);

                entity.Property(e => e.StatusMc)
                    .HasMaxLength(10)
                    .HasColumnName("StatusMC");

                entity.Property(e => e.UpdateBy).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Width)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.AllocateLot)
                    .WithMany(p => p.AllocateMcs)
                    .HasForeignKey(d => new { d.AllocateLotid, d.SaleOrderid, d.Lot })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AllocateMC_AllocateLot");
            });

            modelBuilder.Entity<BomOfMaterialD>(entity =>
            {
                entity.HasKey(e => new { e.ItemCodeH, e.Version, e.LineNum });

                entity.ToTable("BomOfMaterialD", "ao");

                entity.Property(e => e.ItemCodeH).HasMaxLength(50);

                entity.Property(e => e.Version).HasMaxLength(10);

                entity.Property(e => e.Comment).HasMaxLength(500);

                entity.Property(e => e.Comment2).HasMaxLength(500);

                entity.Property(e => e.Department).HasMaxLength(50);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.UomName).HasMaxLength(50);

                entity.Property(e => e.Warehouse)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.BomOfMaterialH)
                    .WithMany(p => p.BomOfMaterialDs)
                    .HasForeignKey(d => new { d.ItemCodeH, d.Version })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BomOfMaterialD_BomOfMaterialH");
            });

            modelBuilder.Entity<BomOfMaterialH>(entity =>
            {
                entity.HasKey(e => new { e.ItemCode, e.Version });

                entity.ToTable("BomOfMaterialH", "ao");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.Version).HasMaxLength(10);

                entity.Property(e => e.CenvertSapdate)
                    .HasColumnType("datetime")
                    .HasColumnName("CenvertSAPDate");

                entity.Property(e => e.ConvertSap).HasColumnName("ConvertSAP");

                entity.Property(e => e.CreateBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DefaultBom)
                    .HasMaxLength(10)
                    .HasColumnName("DefaultBOM");

                entity.Property(e => e.DefaultBomDate).HasColumnType("datetime");

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.ProCode).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.ToWh)
                    .HasMaxLength(50)
                    .HasColumnName("ToWH");

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UpdateBy).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<BuyMonthMaster>(entity =>
            {
                entity.ToTable("BuyMonthMaster", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<BuyYearMaster>(entity =>
            {
                entity.ToTable("BuyYearMaster", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<IssueMaterialD>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.IssueHid });

                entity.ToTable("IssueMaterialD", "ao");

                entity.Property(e => e.IssueHid).HasColumnName("IssueHId");

                entity.Property(e => e.BaseQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.Buy)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.ConfirmQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.IssueMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IssueQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.Pddid).HasColumnName("PDDId");

                entity.Property(e => e.Pdhid).HasColumnName("PDHId");

                entity.Property(e => e.PickQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.PlandQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.ReqDid).HasColumnName("ReqDId");

                entity.Property(e => e.ReqHid).HasColumnName("ReqHId");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UomName).HasMaxLength(50);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Warehouse)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.IssueH)
                    .WithMany(p => p.IssueMaterialDs)
                    .HasForeignKey(d => d.IssueHid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IssueMaterialD_IssueMaterialH");

                entity.HasOne(d => d.ReqH)
                    .WithMany(p => p.IssueMaterialDs)
                    .HasForeignKey(d => d.ReqHid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IssueMaterialD_ReqIssueMaterialH");
            });

            modelBuilder.Entity<IssueMaterialH>(entity =>
            {
                entity.ToTable("IssueMaterialH", "ao");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConvertSap).HasColumnName("ConvertSAP");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IssueBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IssueDate).HasColumnType("datetime");

                entity.Property(e => e.IssueNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PickingBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PickingDate).HasColumnType("datetime");

                entity.Property(e => e.PrintDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UploadFile)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<IssueMaterialLog>(entity =>
            {
                entity.ToTable("IssueMaterialLog", "ao");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Action)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ClientName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.IssueHid).HasColumnName("IssueHId");

                entity.Property(e => e.LogDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Users)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.IssueH)
                    .WithMany(p => p.IssueMaterialLogs)
                    .HasForeignKey(d => d.IssueHid)
                    .HasConstraintName("FK_IssueMaterialLog_IssueMaterialH");
            });

            modelBuilder.Entity<IssueMaterialTranBarcode>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.IssueDid, e.IssueHid, e.BarcodeId });

                entity.ToTable("IssueMaterialTranBarcode");

                entity.Property(e => e.IssueDid).HasColumnName("IssueDId");

                entity.Property(e => e.IssueHid).HasColumnName("IssueHId");

                entity.Property(e => e.BarcodeId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IssueQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.Qty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.ScanBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ScanDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Issue)
                    .WithMany(p => p.IssueMaterialTranBarcodes)
                    .HasForeignKey(d => new { d.IssueDid, d.IssueHid })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IssueMaterialTranBarcode_IssueMaterialD");
            });

            modelBuilder.Entity<ProductionOrderD>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Pdhid, e.AllocateLotSizeId, e.LineNum })
                    .HasName("PK_ProductionOrderD_1");

                entity.ToTable("ProductionOrderD", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Pdhid).HasColumnName("PDHID");

                entity.Property(e => e.BaseQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.BomItemCode).HasMaxLength(50);

                entity.Property(e => e.BomVersion).HasMaxLength(10);

                entity.Property(e => e.Department).HasMaxLength(50);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.IssueMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.ItemType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PlandQty).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.UomName).HasMaxLength(50);

                entity.Property(e => e.Warehouse)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.ProductionOrderH)
                    .WithMany(p => p.ProductionOrderDs)
                    .HasForeignKey(d => new { d.Pdhid, d.AllocateLotSizeId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductionOrderD_ProductionOrderH");
            });

            modelBuilder.Entity<ProductionOrderH>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.AllocateLotSizeId });

                entity.ToTable("ProductionOrderH", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ConvertSap).HasColumnName("ConvertSAP");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.Priority).HasMaxLength(50);

                entity.Property(e => e.Project).HasMaxLength(50);

                entity.Property(e => e.Remark).HasMaxLength(254);

                entity.Property(e => e.SodocEntry).HasColumnName("SODocEntry");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.Property(e => e.UomCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Warehouse).HasMaxLength(50);
            });

            modelBuilder.Entity<QueueConvertToSap>(entity =>
            {
                entity.ToTable("QueueConvertToSAP", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TypeDc)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("TypeDC");
            });

            modelBuilder.Entity<ReqIssueMaterialD>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ReqHid });

                entity.ToTable("ReqIssueMaterialD", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ReqHid).HasColumnName("ReqHId");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pddid).HasColumnName("PDDId");

                entity.Property(e => e.Pdhid).HasColumnName("PDHId");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.HasOne(d => d.ReqH)
                    .WithMany(p => p.ReqIssueMaterialDs)
                    .HasForeignKey(d => d.ReqHid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReqIssueMaterialD_ReqIssueMaterialH");
            });

            modelBuilder.Entity<ReqIssueMaterialH>(entity =>
            {
                entity.ToTable("ReqIssueMaterialH", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.Remark)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ReqDept)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReqNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequestBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequestDate).HasColumnType("datetime");

                entity.Property(e => e.RequireDate).HasColumnType("datetime");

                entity.Property(e => e.Site)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ReqIssueMaterialLog>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ReqHid });

                entity.ToTable("ReqIssueMaterialLog", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ReqHid).HasColumnName("ReqHId");

                entity.Property(e => e.Action)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ClientName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasMaxLength(5000)
                    .IsUnicode(false);

                entity.Property(e => e.LogDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Users)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ReqH)
                    .WithMany(p => p.ReqIssueMaterialLogs)
                    .HasForeignKey(d => d.ReqHid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReqIssueMaterialLog_ReqIssueMaterialH");
            });

            modelBuilder.Entity<SaleOrderD>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Sohid, e.LineNum })
                    .HasName("PK_SaleOrderD_1");

                entity.ToTable("SaleOrderD", "ao");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Sohid).HasColumnName("SOHId");

                entity.Property(e => e.BillOfMaterial).HasMaxLength(50);

                entity.Property(e => e.Buy)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Colors)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Dscription)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemNo)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LineStatus)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.PoNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("decimal(16, 6)");

                entity.Property(e => e.ShipToCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToDesc)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SizeNo).HasColumnType("decimal(10, 1)");

                entity.Property(e => e.Style)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.UomCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Updateby)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Width)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Soh)
                    .WithMany(p => p.SaleOrderDs)
                    .HasForeignKey(d => d.Sohid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleOrderD_SaleOrderH");
            });

            modelBuilder.Entity<SaleOrderH>(entity =>
            {
                entity.ToTable("SaleOrderH", "ao");

                entity.Property(e => e.Buy)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.CardCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CardName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ConvertSap).HasColumnName("ConvertSAP");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Currency).HasMaxLength(10);

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DocStatus)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasComment("D = Draft, A = Active,C = Close,CN = Cancel");

                entity.Property(e => e.GenerateLotBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.SaleTypes)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SoNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UploadFile)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SaleType>(entity =>
            {
                entity.ToTable("SaleType", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SizeMaster>(entity =>
            {
                entity.HasKey(e => e.SizeCode);

                entity.ToTable("SizeMaster", "ao");

                entity.Property(e => e.SizeCode).HasMaxLength(10);

                entity.Property(e => e.Canceled).HasMaxLength(1);

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Object)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.QtyMc).HasColumnName("QtyMC");

                entity.Property(e => e.QtyPerMc).HasColumnName("QtyPerMC");

                entity.Property(e => e.RowNumber).HasColumnName("rowNumber");

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Stdsize>(entity =>
            {
                entity.ToTable("Stdsize", "ao");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Gendar)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Size)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Types)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TplocationGroup>(entity =>
            {
                entity.ToTable("TPLocationGroup", "ao");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Groups)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<TpstyleWithLocation>(entity =>
            {
                entity.ToTable("TPStyleWithLocation", "ao");

                entity.Property(e => e.ArticleCode).HasMaxLength(50);

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.GroupItem)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<VwGenerateMcgroupSize>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_GenerateMCGroupSize");

                entity.Property(e => e.Buy)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Colors)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Dscription)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.ItemCodePd).HasColumnName("ItemCodePD");

                entity.Property(e => e.ItemName).HasMaxLength(500);

                entity.Property(e => e.ItemNamePd).HasColumnName("ItemNamePD");

                entity.Property(e => e.ItemNo).HasMaxLength(50);

                entity.Property(e => e.Lot).HasMaxLength(20);

                entity.Property(e => e.PlantCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.PoNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Project)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.QtyGsize).HasColumnName("QtyGSize");

                entity.Property(e => e.QtyLotGsize)
                    .HasColumnType("decimal(16, 6)")
                    .HasColumnName("QtyLotGSize");

                entity.Property(e => e.QtySaleOrderGsize)
                    .HasColumnType("decimal(16, 6)")
                    .HasColumnName("QtySaleOrderGSize");

                entity.Property(e => e.SaleTypes)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ShipToDesc)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SizeNo).HasMaxLength(10);

                entity.Property(e => e.SoNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Width)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwWebTpapproval>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_WebTPApproval");

                entity.Property(e => e.Condition)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.Department)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.Email)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("email")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.Levels).HasColumnName("levels");

                entity.Property(e => e.Name)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("name")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.Program)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.RowId).HasColumnName("rowId");

                entity.Property(e => e.Site)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.Types)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");
            });

            modelBuilder.Entity<VwWebUser>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_WebUser");

                entity.Property(e => e.AuthDate)
                    .HasColumnType("datetime")
                    .HasColumnName("authDate");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate");

                entity.Property(e => e.EmpCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empCode")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpDepartment)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empDepartment")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empEmail")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpLname)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("empLname")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("empName")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpNameTh)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("empNameTH")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpPassword)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empPassword")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpPosition)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empPosition")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpSection)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empSection")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpSex)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empSex")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empStatus")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.EmpUsername)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("empUsername")
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.RowId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("rowID");

                entity.Property(e => e.Site)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.UpdateBy)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("Thai_100_CI_AS");

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
