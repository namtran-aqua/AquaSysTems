using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Shared.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data
{
    public static class DbSeeder
    {
        public static void SeedData(AquaDbContext context)
        {
            if (context.tbl_Users.Any()) return; // tránh seed lại

            // ----- MENU & PAGE -----
            //var menuUser = new Menu { Id = Guid.NewGuid(), Name = "Administration", Icon = "global", Order = 1 };
            //var menuRole = new Menu { Id = Guid.NewGuid(), Name = "Quản lý Role", Icon = "role-icon", Order = 2 };
            //var menuPermission = new Menu { Id = Guid.NewGuid(), Name = "Quản lý Permission", Icon = "perm-icon", Order = 3 };

            //var pages = new List<Page>
            //    {
            //        new Page { Id = Guid.NewGuid(), MenuId = menuUser.Id, Name = "Roles Management", Url = "/roles-management" ,Order = 2},
            //        new Page { Id = Guid.NewGuid(), MenuId = menuUser.Id, Name = "Menu - Page Management", Url = "/menu-management",Order =4},
            //        new Page { Id = Guid.NewGuid(), MenuId = menuUser.Id, Name = "Permission Management", Url = "/permission-management",Order =3 },
            //        new Page { Id = Guid.NewGuid(), MenuId = menuUser.Id, Name = "User Management", Url = "/user-management" ,Order =1}

            //    };



            //// ----- PERMISSIONS -----
            //var permissions = new List<Permission>();

            //// Thêm permission cấp Menu (View menu)
            //permissions.AddRange(new[]
            //{
            //                    new Permission
            //                    {
            //                        Id = Guid.NewGuid(),
            //                        Action = PermissionActionType.View,
            //                        Type = PermissionType.Menu,
            //                        MenuId = menuUser.Id,
            //                        PageId = null // Không cần PageId cho PermissionType.Menu
            //                    }
            //                });

            // Thêm permission cấp Page (View, Add, Edit, Delete)
            //permissions.AddRange(pages.SelectMany(page => new[]
            //{
            //                    new Permission
            //                    {
            //                        Id = Guid.NewGuid(),
            //                        Action = PermissionActionType.View,
            //                        Type = PermissionType.Page,
            //                        MenuId = page.MenuId,
            //                        PageId = page.Id
            //                    },
            //                    new Permission
            //                    {
            //                        Id = Guid.NewGuid(),
            //                        Action = PermissionActionType.Add,
            //                        Type = PermissionType.Page,
            //                        MenuId = page.MenuId,
            //                        PageId = page.Id
            //                    },
            //                    new Permission
            //                    {
            //                        Id = Guid.NewGuid(),
            //                        Action = PermissionActionType.Edit,
            //                        Type = PermissionType.Page,
            //                        MenuId = page.MenuId,
            //                        PageId = page.Id
            //                    },
            //                    new Permission
            //                    {
            //                        Id = Guid.NewGuid(),
            //                        Action = PermissionActionType.Delete,
            //                        Type = PermissionType.Page,
            //                        MenuId = page.MenuId,
            //                        PageId = page.Id
            //                    }
            //                }));


            //    //// ----- ROLE -----
            var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };


                var roles = new[] { adminRole };

            //    //// ----- USERS -----
            var hasher = new PasswordHasher<object>();
            string defaultPassword = "1";

            var users = new List<User>
                {
                    new User { Id = Guid.NewGuid(), WorkDayId = "Admin", FirstName = "Admin", LastName = "One", FullName = "Admin One", Email = "admin1@example.com", NormalizedEmail = "ADMIN1@EXAMPLE.COM", PhoneNumber = "123456",CreatedBy="Admin", CreatedTime = DateTime.UtcNow, IsActive = true, IsDeleted = false, PasswordHash = hasher.HashPassword(null, defaultPassword) },

                };


            var userRoles = new List<UserRole>
                {
                    new UserRole { Id = Guid.NewGuid(), UserId = users[0].Id, RoleId = adminRole.Id },
                };

            //    //// ----- ROLE PERMISSION -----
            //    var rolePermissions = new List<RolePermission>();

            //    foreach (var perm in permissions)
            //    {
            //        rolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = adminRole.Id, PermissionId = perm.Id });
            //    }

            //    //// Save to context
              //  context.tbl_Menus.AddRange(menuUser, menuRole, menuPermission);
            //    context.tbl_Pages.AddRange(pages);
            //    context.tbl_Permissions.AddRange(permissions);
                  context.tbl_Roles.AddRange(roles);
                  context.tbl_Users.AddRange(users);
                  context.tbl_UserRoles.AddRange(userRoles);
            //    context.tbl_RolePermissions.AddRange(rolePermissions);

            context.SaveChanges();
        }
    }
}
