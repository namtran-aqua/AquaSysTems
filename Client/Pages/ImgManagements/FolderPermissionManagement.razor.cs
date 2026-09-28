using AquaSolution.Shared.Imgs;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using AntDesign;
using System;

namespace AquaSolution.Client.Pages.ImgManagements
{
    public class FolderItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public partial class FolderPermissionManagement
    {
        [Inject] public HttpClient Http { get; set; }
        [Inject] public MessageService Message { get; set; }

        private List<UserDto> allUsers = new();
        private List<FolderItem> foldersList = new();
        
        // Maps folderId to its assigned permission WorkDayIds
        private Dictionary<string, HashSet<string>> folderPermissionsMap = new();

        // Maps folderId to currently selected users in the UI
        private Dictionary<string, List<UserDto>> selectedUsersMap = new();
        
        // Maps folderId to search term
        private Dictionary<string, string> searchTermsMap = new();

        private bool loading = false;
        private Dictionary<string, bool> savingMap = new();
        
        // Dummy values for Select component to reset after selection
        private Dictionary<string, string> selectValueMap = new();

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            try
            {
                try
                {
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    await Message.Error("Lỗi tải users: " + ex.Message);
                }

                try
                {
                    await LoadAllPermissions();
                }
                catch (Exception ex)
                {
                    await Message.Error("Lỗi tải permissions: " + ex.Message);
                }

                try
                {
                    var foldersTask = await Http.GetFromJsonAsync<Dictionary<string, string>>("api/Img/get-folders");
                    if (foldersTask != null)
                    {
                        foldersList = foldersTask.Select(x => new FolderItem { Id = x.Key, Name = x.Value }).ToList();
                        
                        // Initialize maps
                        foreach(var f in foldersList)
                        {
                            searchTermsMap[f.Id] = "";
                            savingMap[f.Id] = false;
                            selectValueMap[f.Id] = null;
                            
                            // Initialize selections based on existing permissions
                            if (folderPermissionsMap.TryGetValue(f.Id, out var permIds))
                            {
                                selectedUsersMap[f.Id] = allUsers.Where(u => permIds.Contains(u.WorkDayId)).ToList();
                            }
                            else
                            {
                                selectedUsersMap[f.Id] = new List<UserDto>();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Message.Error("Lỗi tải danh sách folders: " + ex.Message);
                }
            }
            finally
            {
                loading = false;
            }
        }

        private async Task LoadUsers()
        {
            var data = await Http.GetFromJsonAsync<List<UserDto>>("api/user/get-all");
            if (data != null)
            {
                allUsers = data;
            }
        }

        private async Task LoadAllPermissions()
        {
            folderPermissionsMap.Clear();
            var perms = await Http.GetFromJsonAsync<List<FolderPermissionDto>>("api/FolderPermission/get-all");
            if (perms != null)
            {
                foreach (var group in perms.GroupBy(p => p.FolderId))
                {
                    folderPermissionsMap[group.Key] = group.Select(p => p.WorkDayId).ToHashSet();
                }
            }
        }

        private bool UserFilter(UserDto item, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue)) return true;
            return (item.FullName != null && item.FullName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                   (item.WorkDayId != null && item.WorkDayId.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
        }

        private void OnUserSelected(string folderId, UserDto user)
        {
            if (user != null)
            {
                if (!selectedUsersMap[folderId].Any(u => u.WorkDayId == user.WorkDayId))
                {
                    selectedUsersMap[folderId].Add(user);
                }
                // Reset select value so it can be used again
                selectValueMap[folderId] = null;
            }
        }

        private void RemoveUser(string folderId, UserDto user)
        {
            selectedUsersMap[folderId].Remove(user);
        }

        private async Task SavePermissions(FolderItem folder)
        {
            savingMap[folder.Id] = true;
            try
            {
                var selected = selectedUsersMap.GetValueOrDefault(folder.Id) ?? new List<UserDto>();
                var userIds = selected.Select(x => x.WorkDayId).ToList();
                
                var res = await Http.PostAsJsonAsync($"api/FolderPermission/update-by-folder/{folder.Id}?folderName={Uri.EscapeDataString(folder.Name)}", userIds);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success($"Đã lưu phân quyền cho thư mục {folder.Name}!");
                    
                    // Update header counts
                    folderPermissionsMap[folder.Id] = userIds.ToHashSet();
                }
                else
                {
                    var errorMsg = await res.Content.ReadAsStringAsync();
                    await Message.Error($"Lỗi khi lưu phân quyền: {errorMsg}");
                }
            }
            catch (System.Exception ex)
            {
                await Message.Error("Lỗi khi lưu phân quyền: " + ex.Message);
            }
            finally
            {
                savingMap[folder.Id] = false;
            }
        }
    }
}
