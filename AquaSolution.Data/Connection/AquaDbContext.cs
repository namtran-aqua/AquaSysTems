using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.MappingConfigurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Connection
{
    public class AquaDbContext : DbContext
    {
        public AquaDbContext(DbContextOptions<AquaDbContext> options) : base(options)
        {
        }
       // public DbSet<Button> Buttons { get; set; }
        public DbSet<Menu> tbl_Menus { get; set; }
        public DbSet<Page> tbl_Pages { get; set; }
        public DbSet<Permission> tbl_Permissions { get; set; }
        public DbSet<Role> tbl_Roles { get; set; }
        public DbSet<RolePermission> tbl_RolePermissions { get; set; }
        public DbSet<User> tbl_Users { get; set; }
        public DbSet<UserRole> tbl_UserRoles { get; set; }
        public DbSet<Groups> tbl_Groups { get; set; }
        public DbSet<Department> tbl_Departments { get; set; }
        public DbSet<Factory> tbl_Factorys { get; set; }
        public DbSet<Position> tbl_Positions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MenuConfiguration());
            modelBuilder.ApplyConfiguration(new PageConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new GroupsConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());   
            modelBuilder.ApplyConfiguration(new FactoryConfiguration());
            modelBuilder.ApplyConfiguration(new PositionConfiguration());
        }
    }
}
