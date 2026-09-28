using AquaSolution.Shared.Imgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public interface IFolderPermissionService
    {
        Task<List<FolderPermissionDto>> GetPermissionsByWorkDayIdAsync(string workDayId);
        Task<List<FolderPermissionDto>> GetAllPermissionsAsync();
        Task<bool> UpdatePermissionsAsync(string workDayId, List<FolderPermissionDto> permissions);
        Task<List<FolderPermissionDto>> GetPermissionsByFolderIdAsync(string folderId);
        Task<bool> UpdatePermissionsByFolderAsync(string folderId, string folderName, List<string> workDayIds);
    }
}
