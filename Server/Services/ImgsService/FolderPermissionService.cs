using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities.Imgs;
using AquaSolution.Shared.Imgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public class FolderPermissionService : IFolderPermissionService
    {
        private readonly AquaDbContext _context;

        public FolderPermissionService(AquaDbContext context)
        {
            _context = context;
        }

        public async Task<List<FolderPermissionDto>> GetPermissionsByWorkDayIdAsync(string workDayId)
        {
            var result = await _context.tbl_FolderPermissions
                .Where(x => x.WorkDayId == workDayId)
                .Select(x => new FolderPermissionDto
                {
                    Id = x.Id,
                    WorkDayId = x.WorkDayId,
                    FolderId = x.FolderId,
                    FolderName = x.FolderName
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<FolderPermissionDto>> GetAllPermissionsAsync()
        {
            var result = await _context.tbl_FolderPermissions
                .Select(x => new FolderPermissionDto
                {
                    Id = x.Id,
                    WorkDayId = x.WorkDayId,
                    FolderId = x.FolderId,
                    FolderName = x.FolderName
                })
                .ToListAsync();

            return result;
        }

        public async Task<bool> UpdatePermissionsAsync(string workDayId, List<FolderPermissionDto> permissions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xoá tất cả quyền hiện tại của user này
                var existing = await _context.tbl_FolderPermissions
                    .Where(x => x.WorkDayId == workDayId)
                    .ToListAsync();
                    
                _context.tbl_FolderPermissions.RemoveRange(existing);

                // Thêm các quyền mới
                if (permissions != null && permissions.Any())
                {
                    var newEntities = permissions.Select(x => new FolderPermission
                    {
                        Id = Guid.NewGuid(),
                        WorkDayId = workDayId,
                        FolderId = x.FolderId ?? string.Empty,
                        FolderName = x.FolderName ?? string.Empty
                    });

                    await _context.tbl_FolderPermissions.AddRangeAsync(newEntities);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi lưu phân quyền User: {ex.Message}", ex);
            }
        }
        public async Task<List<FolderPermissionDto>> GetPermissionsByFolderIdAsync(string folderId)
        {
            var result = await _context.tbl_FolderPermissions
                .Where(x => x.FolderId == folderId)
                .Select(x => new FolderPermissionDto
                {
                    Id = x.Id,
                    WorkDayId = x.WorkDayId,
                    FolderId = x.FolderId,
                    FolderName = x.FolderName
                })
                .ToListAsync();

            return result;
        }

        public async Task<bool> UpdatePermissionsByFolderAsync(string folderId, string folderName, List<string> workDayIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xoá tất cả quyền hiện tại của folder này
                var existing = await _context.tbl_FolderPermissions
                    .Where(x => x.FolderId == folderId)
                    .ToListAsync();
                    
                _context.tbl_FolderPermissions.RemoveRange(existing);

                // Thêm các quyền mới
                if (workDayIds != null && workDayIds.Any())
                {
                    var validWorkDayIds = workDayIds.Where(wd => !string.IsNullOrEmpty(wd)).ToList();
                    var newEntities = validWorkDayIds.Select(wd => new FolderPermission
                    {
                        Id = Guid.NewGuid(),
                        WorkDayId = wd,
                        FolderId = folderId ?? string.Empty,
                        FolderName = folderName ?? string.Empty
                    });

                    await _context.tbl_FolderPermissions.AddRangeAsync(newEntities);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi lưu phân quyền: {ex.Message}", ex);
            }
        }
    }
}
