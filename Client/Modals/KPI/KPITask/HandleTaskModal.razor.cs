using AntDesign;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.QuaterCalculated;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.KPITask
{
    public partial class HandleTaskModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private HandleTaskDto HandleTaskDto = new HandleTaskDto();
        private Form<HandleTaskDto> formRef;
        private List<QuaterCalculatedDto> ListCalculated = new List<QuaterCalculatedDto>();
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
            await LoadCalculated();
            await LoadDepartment();
            await LoadFactory();
            await LoadFormula();
            GetEnum();
            StateHasChanged();
        }

        //private async Task LoadUser()
        //{
        //    var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
        //    ListUser = data.ToList();

        //}
        private async Task LoadCalculated()
        { 
            var result = await Http.GetFromJsonAsync<List<QuaterCalculatedDto>>("api/QuarterCalculated/get-list");
            if (result != null)
            {
                ListCalculated = result;
            }
        }

        private async Task LoadDepartment()
        {
            ListDepartment = new();

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
            ListFactory = new();
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
        }
        private List<FormulaDto> ListFormula = new();
        private async Task LoadFormula()
        {
            var result = await Http.GetFromJsonAsync<List<FormulaDto>>("api/formula/get-list");

            if (result is not null)
            {
                ListFormula = result;
            }
            else
            {
                ListFormula = new();
            }
        }

        #endregion
        #region Actions
        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            bool isSave = false;
            if (IsEdit)
            {
                isSave = await UpdateAsync();
            }
            else
            {
                isSave = await CreatedAsync();
            }
            if (isSave)
            {
                await OnSave.InvokeAsync();
                IsModalVisible = false;
            }
        }
        private async Task<bool> CreatedAsync()
        {

            var response = await Http.PostAsJsonAsync("api/KPITask/create", HandleTaskDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                return true;
            }
            else
            {
                await Message.Error("Created failed.");
                return false;
            }
        }
        private async Task<bool> UpdateAsync()
        {
            var response = await Http.PutAsJsonAsync("api/KPITask/update", HandleTaskDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Updated successfully.");
                return true;
            }
            else
            {
                await Message.Error("Updated failed.");
                return false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
