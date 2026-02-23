using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.KPI.KPITask;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.HRMS;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.KPITask
{
    public partial class KPITaskManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<KPITaskDto> DataSource = new();
        private List<KPITaskDto> DataFilter = new();
        private Table<KPITaskDto> tableRef;
        private bool Created { get;set; }
        private bool Edit { get;set; }
        private bool Delete { get;set; }
        private Guid PageId { get; set; }
        private UserDto CurrenUser { get; set; }
        private HandleTaskModal handleTaskModal;
        private KPITaskDetailModal KPITaskDetailModal;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await CheckPermission();
            await LoadData();
            await LoadDataFilter();


        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<KPITaskDto>>("api/KPITask/get-list");

            if (result is not null)
            {
                DataSource = result;
            }
            else
            {
                DataSource = new();
            }
            DataFilter = DataSource;
            StateHasChanged();
        }
        private async Task GetPage()
        {

            var url = "task-management";
            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");

        }
        private async Task CheckPermission()
        {
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            Created = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Add);
            Edit = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Edit);
            Delete = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Delete);
        }
        #endregion
        #region Actions
        private async Task CreatedAsync()
        {

            await handleTaskModal.ShowModal(CurrenUser);
        }
        private async Task EditAsync(KPITaskDto kPITaskDto)
        {
            var handleDto = new HandleTaskDto
            {
                Id = kPITaskDto.Id,
                TaskName = kPITaskDto.TaskName ?? string.Empty,
                KPICategory = kPITaskDto.KPICategory,
                TaskDescription = kPITaskDto.TaskDescription ?? string.Empty,
                CalculatedMdethod = kPITaskDto.CalculatedMdethod ?? string.Empty,
                DataSource = kPITaskDto.DataSource ?? string.Empty,
                PIC = kPITaskDto.PIC,
                KPIIndexType = kPITaskDto.KPIIndexType,
                FormulaId = kPITaskDto.FormulaId,
                Max = kPITaskDto.Max,
                Bottom = kPITaskDto.Bottom,
                FactoryId = kPITaskDto.FactoryId,
                Unit = kPITaskDto.Unit ?? string.Empty,
                DepartmentId = kPITaskDto.DepartmentId,
                CalculatedId = kPITaskDto.CalculatedId,
            };
            await handleTaskModal.ShowModal(CurrenUser,true, handleDto);
        }
        private async Task ViewAsync(KPITaskDto kPITaskDto)
        {
            await KPITaskDetailModal.ShowModal(kPITaskDto);
        }
        private async Task DeleteAsync(KPITaskDto kPITaskDto)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            var response = await Http.DeleteAsync($"api/KPITask/delete/{kPITaskDto.Id}");

            if (response.IsSuccessStatusCode)
            {
                await LoadData();
                await Message.Success("Deleted successfully");
            }
            else
            {
                await Message.Error("An unexpected error occurred");
            }
        }

        #endregion
        #region Search
        private string? TaskName { get; set; }
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                DataFilter = DataSource;
            }
            else
            {
                var keyword = TaskName.Trim().ToLower();
                DataFilter = DataSource
                    .Where(x => x.TaskName != null && x.TaskName.ToLower().Contains(keyword))
                    .ToList();
            }
        }
        private async Task Reset()
        {
            TaskName =null;
            tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
            DataFilter = DataSource;
        }   
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void TaskNameInputChanged(ChangeEventArgs e)
        {
            TaskName = e.Value?.ToString();
        }
        #endregion
        #region Filter
        TableFilter<KPICategoryType>[] _kPICategoryFilter = Array.Empty<TableFilter<KPICategoryType>>();
        TableFilter<KPIIndexType>[] _kPIIndexTypeFilter = Array.Empty<TableFilter<KPIIndexType>>();
        TableFilter<string>[] _departmentFilter = Array.Empty<TableFilter<string>>();
        TableFilter<string>[] _factoryFilter = Array.Empty<TableFilter<string>>();
        private List<DepartmentDto> _listDepartment = new();
        private List<FactoryDto> _listFactory = new();
        private async Task LoadDataFilter()
        {
            if (Http != null)
            {
                _listDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ??
                                  new List<DepartmentDto>();
                _departmentFilter = _listDepartment
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();

                _listFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ??
                               new List<FactoryDto>();
                _factoryFilter = _listFactory
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => new TableFilter<string>
                    {
                        Text = x.Name,
                        Value = x.Name,
                        Selected = false
                    })
                    .ToArray();
                _kPICategoryFilter = Enum.GetValues(typeof(KPICategoryType))
                  .Cast<KPICategoryType>()
                  .Select(e => new TableFilter<KPICategoryType>
                  {
                      Text = EnumHelper.GetDisplayName(e),
                      Value = e,
                      Selected = false
                  })
                  .ToArray();
                _kPIIndexTypeFilter = Enum.GetValues(typeof(KPIIndexType))
                     .Cast<KPIIndexType>()
                     .Select(e => new TableFilter<KPIIndexType>
                     {
                         Text = EnumHelper.GetDisplayName(e),
                         Value = e,
                         Selected = false
                     })
                     .ToArray();
            }

        }
        #endregion
        #region Import
        private static KPIIndexType MapKPIIndexType(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return KPIIndexType.KPI;

            var key = text
                .Replace(" ", "")
                .Replace("-", "")
                .ToLower();

            return key switch
            {
                "kpi" => KPIIndexType.KPI,
                "keytask" => KPIIndexType.KeyTask,
                "omg" => KPIIndexType.OMG,
                _ => KPIIndexType.KPI
            };
        }
        private static KPICategoryType MapKPICategory(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return KPICategoryType.KeyTask;

            var key = text
                .Replace(" ", "")
                .Replace("-", "")
                .ToLower();

            return key switch
            {
                "market" => KPICategoryType.Market,
                "financial" => KPICategoryType.Financial,
                "operation" => KPICategoryType.Operation,
                "keytask" => KPICategoryType.KeyTask,
                "omg" => KPICategoryType.OMG,
                "it" => KPICategoryType.IT,
                "safety" => KPICategoryType.Safety,
                "health" => KPICategoryType.Health,
                "security" => KPICategoryType.Security,
                "environment" or "env"
                                 => KPICategoryType.Environmen,
                "iso" => KPICategoryType.ISO,
                _ => KPICategoryType.KeyTask
            };
        }

        private static Guid MapFormulaId(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Guid.Empty;

            var key = text
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("/", "")
                .ToLower();

            return key switch
            {
                "actualtarget"
                    => Guid.Parse("FF6A2841-E032-4174-81BC-07C9CCF50640"),

                "netprofit"
                    => Guid.Parse("119789D0-FE1A-40D0-8644-B3BBD43FED73"),

                "2actualtarget"
                    => Guid.Parse("251E20DC-BBA6-47F1-A91C-E872AC1AE188"),

                _ => Guid.Empty
            };
        }


        private static Guid MapCalculatedId(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Guid.Empty;

            var key = text
                .Replace(" ", "")
                .Replace("-", "")
                .ToLower();

            return key switch
            {
                var k when k.Contains("avgof3months")
                    => Guid.Parse("7135A5B1-2845-4673-9207-4E37D8312CDB"),

                var k when k.Contains("lastmonth")
                    => Guid.Parse("3950D78A-A55B-45A2-933F-8753C609472D"),

                var k when k.Contains("systematic")
                    => Guid.Parse("A10AAFDC-BFAB-4C70-96CA-983C7CA28FF3"),

                var k when k.Contains("sumof3months")
                    => Guid.Parse("53D10EA0-9D69-4B5A-A5C8-C5A653A985A4"),

                _ => Guid.Empty
            };
        }


        private List<HandleTaskDto> KPITasks = new();

        private async Task HandleKPITaskFile(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file == null)
                    return;

                using var ms = await file.ToMemoryStreamAsync();

                KPITasks = await ExcelImportHelper.ReadFromExcelAsync<HandleTaskDto>(
                    ms,
                    (sheet, row) =>
                    {
                        if (string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text))
                            return null;

                        return new HandleTaskDto
                        {
                            TaskName = sheet.Cells[row, 1].Text.Trim(),

                            // CategoryName (cột 2)
                            KPICategory = MapKPICategory(
                                sheet.Cells[row, 2].Text
                            ),

      
                            KPIIndexType = MapKPIIndexType(
                                sheet.Cells[row, 3].Text
                            ),

                            FormulaId = MapFormulaId(
                                    sheet.Cells[row, 5].Text
                                ),
                       
                             CalculatedId = MapCalculatedId(sheet.Cells[row, 6].Text),

                            Max = decimal.TryParse(
                                sheet.Cells[row, 7].Text,
                                out var max)
                                ? max / 100
                                : 0,

                            Bottom = decimal.TryParse(
                                sheet.Cells[row, 8].Text,
                                out var bottom)
                                ? bottom / 100
                                : 0,

                            TaskDescription = sheet.Cells[row, 9].Text.Trim(),
                            CalculatedMdethod = sheet.Cells[row, 10].Text.Trim(),
                            DataSource = sheet.Cells[row, 11].Text.Trim(),
                            PIC = sheet.Cells[row, 12].Text.Trim(),
                            DepartmentId =new Guid("36AD907A-5948-417E-8FF8-813E7EED9496"),
                            FactoryId = new Guid("3C6F5EE1-1C4A-42E9-8033-ED16CEA2C327"),
                            CreatedById  =new Guid("B3A87D42-4CD2-4882-A1EE-EAEDE1707AC6"),
                            CreatedDate = DateTime.Now,
                            Unit = sheet.Cells[row, 13].Text.Trim()
                        };
                    });
                var list = new List<HandleTaskDto>();
                foreach(var item in KPITasks)
                {
                    var response = await Http.PostAsJsonAsync("api/KPITask/create", item);

                    if (response.IsSuccessStatusCode)
                    {
                        await Message.Success($"Import thành công {KPITasks.Count} KPI Tasks");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        await Message.Error(error);
                    }
                }
          
            }
            catch (Exception ex)
            {
                await Message.Error(ex.Message);
            }
        }


        #endregion
    }
}
