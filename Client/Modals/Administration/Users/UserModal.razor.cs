using AntDesign;
using AquaSolution.Shared.ApprovalFlows;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.Users
{
    public partial class UserModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private UserDto CurrenUser { get; set; }
        private bool IsModalVisible = false;
        private Form<CreatedAndUpdateUserDto> formRef;
        private CreatedAndUpdateUserDto CreatedUserDto = new CreatedAndUpdateUserDto();
        private bool IsEdit { get; set; }
        private List<BaseDto> ListDepartment = new List<BaseDto>();
        private List<BaseDto> ListFactory = new List<BaseDto>();
        private List<BaseDto> ListPosition = new List<BaseDto>();
        private List<BaseDto> ListSection = new List<BaseDto>();
        private List<UserContributerDto> AllManagers = new();
        private List<ApprovalFlowDto>? ListApprovalFlow = new();
        private bool _isInitializing = false;
        #endregion
        #region Innit
        
        public async Task ShowModelAsync(bool isEdit, CreatedAndUpdateUserDto createdAndUpdateUserDto, UserDto currenUser)
        {
            _isInitializing = true;
            IsEdit = isEdit;
            CurrenUser = currenUser;
            
            await LoadDepartment();
            await LoaPosition();
            await LoadFactory();
            await LoadManager();
            await FlowApproval();

            if (createdAndUpdateUserDto.DepartmentId.HasValue)
            {
                await LoadSections(createdAndUpdateUserDto.DepartmentId.Value);
            }
            else
            {
                ListSection.Clear();
            }

            if (IsEdit)
            {
                CreatedUserDto = createdAndUpdateUserDto;
                if (CreatedUserDto.SectionIds == null)
                {
                    CreatedUserDto.SectionIds = new List<Guid>();
                }
            }
            else
            {
                CreatedUserDto = new();
                CreatedUserDto.SectionIds = new List<Guid>();
            }

            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
            _isInitializing = false;
        }
        private async Task LoadDepartment()
        {
            try
            {
                ListDepartment = new List<BaseDto>();
                var data = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        ListDepartment.Add(new BaseDto
                        {
                            Id = item.Id,
                            Name = item.Name,
                        });
                    }
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }

        private async Task LoadSections(Guid departmentId)
        {
            try
            {
                ListSection = new List<BaseDto>();
                var data = await Http.GetFromJsonAsync<List<AquaSolution.Shared.Administration.Sections.SectionDto>>($"api/section/by-department/{departmentId}");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        ListSection.Add(new BaseDto
                        {
                            Id = item.Id,
                            Name = item.Name,
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private async Task OnDepartmentChanged(BaseDto item)
        {
            if (_isInitializing) return;

            if (item != null && item.Id.HasValue)
            {
                await LoadSections(item.Id.Value);
                CreatedUserDto.SectionIds = new List<Guid>();
            }
            else
            {
                ListSection.Clear();
                CreatedUserDto.SectionIds = new List<Guid>();
            }
        }
        private async Task LoaPosition()
        {
            ListPosition = new List<BaseDto>();
            var data = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all");
            if (data != null)
            {
                foreach (var item in data)
                {
                    ListPosition.Add(new BaseDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                    });
                }
            }
        }
        private async Task LoadFactory()
        {
            ListFactory = new List<BaseDto>();
            var data = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all");
            if (data != null)
            {
                foreach (var item in data)
                {
                    ListFactory.Add(new BaseDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                    });
                }
            }
        }
        private async Task LoadManager()
        {
            AllManagers = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
        }
        private async Task FlowApproval()
        {
            var data = await Http.GetFromJsonAsync<List<ApprovalFlowDto>>("api/approvalFlow/get-all");
            if(data == null)
            {
                return;
            }
            ListApprovalFlow = data
                .DistinctBy(x => x.FlowApproval)
                .ToList();
        }


        #endregion
        #region Action
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            if (IsEdit)
            {
                bool exists = AllManagers
                   .Any(x => x.WorkDayId == CreatedUserDto.WorkDayId && x.Id != CreatedUserDto.Id);
                if (exists)
                {
                    await Message.Error("WorkDayId exists !");
                    return;
                }
            }
            else
            {
                bool exists = AllManagers
                                 .Any(x => x.WorkDayId == CreatedUserDto.WorkDayId );
                if (exists)
                {
                    await Message.Error("WorkDayId exists !");
                    return;
                }
            }
            CreatedUserDto.FullName = $"{CreatedUserDto.LastName} {CreatedUserDto.FirstName}";
            if (IsEdit)
            {
                CreatedUserDto.UpdateBy = CurrenUser.FullName;
                CreatedUserDto.UpdatedTime = DateTime.Now;
                await UpdateAsync(CreatedUserDto);
            }
            else
            {
                CreatedUserDto.CreatedTime = DateTime.Now;
                CreatedUserDto.CreatedBy = CurrenUser.FullName;
                await CreatedAsync(CreatedUserDto);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region HandleData
        private async Task CreatedAsync(CreatedAndUpdateUserDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/user/create", CreatedUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        private async Task UpdateAsync(CreatedAndUpdateUserDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/user/update", updateUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Cập nhật thành công!");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        #endregion

    }
}
