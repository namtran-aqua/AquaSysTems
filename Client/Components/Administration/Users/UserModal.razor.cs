using AntDesign;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration.Users
{
    public partial class UserModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private UserDto CurrenUser { get; set; }
        private Func<Task> ResetListManager { get; set; }
        private bool IsModalVisible = false;
        private Form<CreatedAndUpdateUserDto> formRef;
        private CreatedAndUpdateUserDto CreatedUserDto = new CreatedAndUpdateUserDto();
        private bool IsEdit { get; set; }
        private List<BaseDto> ListDepartment = new List<BaseDto>();
        private List<BaseDto> ListFactory = new List<BaseDto>();
        private List<BaseDto> ListPosition = new List<BaseDto>();
        private List<UserContributerDto> Listmanager = new List<UserContributerDto>();

        #endregion
        #region Innit
        private BaseDto? _valuePosition;
        private BaseDto? ValuePosition
        {
            get => _valuePosition;
            set
            {
                if (_valuePosition != value)
                {
                    _valuePosition = value;
                }
            }
        }
        private BaseDto? _valueFactory;
        private BaseDto? ValueFactory
        {
            get => _valueFactory;
            set
            {
                if (_valueFactory != value)
                {
                    _valueFactory = value;
                    ResetListManager?.Invoke();
                }
            }
        }
        private BaseDto? _valueDepartment;
        private BaseDto? ValueDepartment
        {
            get => _valueDepartment;
            set
            {
                if (_valueDepartment != value)
                {
                    _valueDepartment = value;
                    ResetListManager?.Invoke();
                }
     
            }
        }
        private UserContributerDto? _valueManager;
        private UserContributerDto? ValueManager
        {
            get => _valueManager;
            set
            {
                if (_valueManager != value)
                {
                    _valueManager = value;
                }

            }
        }
        public async Task ShowModelAsync(bool isEdit, CreatedAndUpdateUserDto createdAndUpdateUserDto, UserDto currenUser)
        {
            IsEdit = isEdit;
            CurrenUser = currenUser;
            if (IsEdit)
            {
                CreatedUserDto = createdAndUpdateUserDto;
            }
            else
            {
                CreatedUserDto = new();
            }
            await LoadDepartment();
            await LoaPosition();
            await LoadFactory();
            await LoadManager();
            ResetListManager += LoadManager;
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
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
                    if (CreatedUserDto.DepartmentId != null)
                    {
                        ValueDepartment = ListDepartment.FirstOrDefault(x => x.Id == CreatedUserDto.DepartmentId);
                    }
                    else
                    {
                        ValueDepartment = ListDepartment.FirstOrDefault();
                    }

                }

            }
            catch(Exception ex)
            {
                throw ex;
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
                if (CreatedUserDto.PositionId != null)
                {
                    ValuePosition = ListPosition.FirstOrDefault(x => x.Id == CreatedUserDto.PositionId);
                }
                else
                {
                    ValuePosition = ListPosition.FirstOrDefault();
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
                if (CreatedUserDto.FactoryId != null)
                {
                    ValueFactory = ListFactory.FirstOrDefault(x => x.Id == CreatedUserDto.FactoryId);
                }
                else
                {
                    ValueFactory = ListFactory.FirstOrDefault();
                }
            }
        }
        private async Task LoadManager()
        {
            Listmanager = new List<UserContributerDto>();
            Listmanager = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");


            if (Listmanager != null)
            {
                Listmanager = Listmanager
                    .Where(d =>
                        (ValueFactory != null && ValueDepartment != null && d.FactoryId == ValueFactory.Id && d.DepartmentId == ValueDepartment.Id) ||
                        (ValueFactory != null && ValueDepartment == null && d.FactoryId == ValueFactory.Id) ||
                        (ValueFactory == null && ValueDepartment != null && d.DepartmentId == ValueDepartment.Id) ||
                        (ValueFactory == null && ValueDepartment == null)
                    )
                    .ToList();
                if (CreatedUserDto.ManagerId != null)
                {
                    ValueManager = Listmanager.FirstOrDefault(x => x.Id == CreatedUserDto.ManagerId);
                }
                else
                {
                    ValueManager = Listmanager.FirstOrDefault();
                }
            }
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
            CreatedUserDto.DepartmentId = ValueDepartment?.Id;
            CreatedUserDto.FactoryId = ValueFactory?.Id;
            CreatedUserDto.ManagerId = ValueManager?.Id;
            CreatedUserDto.PositionId = ValuePosition?.Id;
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
