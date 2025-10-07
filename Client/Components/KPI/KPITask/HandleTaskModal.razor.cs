using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Administration.UserManagements;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.KPITask
{
    public partial class HandleTaskModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private HandleTaskDto HandleTaskDto = new HandleTaskDto();
        private Form<HandleTaskDto> formRef;
        private List<UserContributerDto> ListUser = new List<UserContributerDto>();
        private UserDto CurrentUser = new UserDto();
        private List<BaseDto> ListDepartment = new List<BaseDto>();
        private List<BaseDto> ListFactory = new List<BaseDto>();
        private bool IsEdit = false;
        private List<KPICategoryType> ListKPICategoryType = new List<KPICategoryType>();
        private List<KPIIndexType> ListKPIIndexType = new List<KPIIndexType>();

        #endregion
        #region Init
        public async Task ShowModal(UserDto user, bool isEdit = false, HandleTaskDto? handleTaskDto = null)
        {
            IsEdit = isEdit;

            if (isEdit)
            {
                HandleTaskDto = handleTaskDto ?? new HandleTaskDto(); 
            }
            else
            {
                HandleTaskDto = new HandleTaskDto();
                HandleTaskDto.CreatedById = user.Id;
                HandleTaskDto.CreatedDate = DateTime.Now;
            }

            IsModalVisible = true;
            CurrentUser = user;
            await LoadUser();
            await LoadDepartment();
            await LoadFactory();
            GetEnum();
            StateHasChanged();
        }

        private async Task LoadUser()
        {
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            ListUser = data.ToList();

        }
        private async Task LoadDepartment()
        {
     
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

        private async Task LoadFactory()
        {
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
        private void GetEnum()
        {
            ListKPICategoryType = Enum.GetValues(typeof(KPICategoryType))
                    .Cast<KPICategoryType>()
                    .ToList();
            ListKPIIndexType = Enum.GetValues(typeof(KPIIndexType))
                      .Cast<KPIIndexType>()
                      .ToList();
            //if (HandleTaskDto != null)
            //{
            //    DepartmentType = ListDepartmentType
            //             .FirstOrDefault(x => x == _model.DepartmentType);
            //}

        }
        #endregion
        #region Actions
        private async Task SaveAsync()
        {
            var a = HandleTaskDto;
        }
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
