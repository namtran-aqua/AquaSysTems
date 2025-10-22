using AntDesign;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.UserTask
{
    public partial class AddTaskByUserModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private List<KPITaskDto> DataSource = new();
        private bool IsModalVisible = false;
        private Table<KPITaskDto> tableRef;
        private IEnumerable<KPITaskDto> Selected;
        private List<KPITaskDto> DataFilter = new();
        private List<HandleUserTaskDto> HandleUserTaskDto = new();
        private List<CreatedTargetDto> CreatedTarget = new();
        private HandleUserTaskAndTargetDto HandleUserTaskAndTarget = new();
        private Guid UserId { get; set; }
        private string activeTabKey = "1";
        #endregion
        #region Innit
        public async Task ShowModal(Guid userId)
        {
            await LoadData();
            await LoadDataFilter();
            Selected = null;
            activeTabKey = "1";
            TitleButton = "Next";
            HandleUserTaskDto = new();
            UserId = userId;
            await AddTaskByUser();
            IsModalVisible = true;
            StateHasChanged();
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
        private async Task LoadTaskAndTarget()
        {
            var monthProperties = new Dictionary<int, Action<CreatedTargetDto, decimal>>
            {
                [1] = (dto, value) => dto.TargetValue1 = value,
                [2] = (dto, value) => dto.TargetValue2 = value,
                [3] = (dto, value) => dto.TargetValue3 = value,
                [4] = (dto, value) => dto.TargetValue4 = value,
                [5] = (dto, value) => dto.TargetValue5 = value,
                [6] = (dto, value) => dto.TargetValue6 = value,
                [7] = (dto, value) => dto.TargetValue7 = value,
                [8] = (dto, value) => dto.TargetValue8 = value,
                [9] = (dto, value) => dto.TargetValue9 = value,
                [10] = (dto, value) => dto.TargetValue10 = value,
                [11] = (dto, value) => dto.TargetValue11 = value,
                [12] = (dto, value) => dto.TargetValue12 = value,
            };

            await Task.WhenAll(CreatedTarget.Select(async item =>
            {
                var results = await Http.GetFromJsonAsync<List<GetUserTaskAndTargetDto>>(
                    $"api/KPIMonthlyTarget/get-kpi-target/{item.TaskId}/{item.UserId}") ?? new List<GetUserTaskAndTargetDto>();
                var first = results.FirstOrDefault();
                if (first != null)
                {
                    item.Index = first.Index;
                    item.Weight = first.Weight;
                }
                // Gán giá trị theo từng tháng
                results
                    .Where(x => x.Month.HasValue)
                    .ToList()
                    .ForEach(target =>
                    {
                        if (monthProperties.TryGetValue(target.Month.Value, out var setProp))
                        {
                            setProp(item, target.TargetValue);
                        }
                    });
            }));
        }
        private async Task AddTaskByUser()
        {
            var result = await Http.GetFromJsonAsync<List<UserTaskDto>>($"api/UserTask/list-taskIds/{UserId}");
            if (DataSource is not null && result is not null)
            {
                var listId = result.Select(x => x.KPITaskId).ToList();
                var listSelected = DataSource
                      .Where(x => listId.Contains(x.Id))
                      .ToList();
                Selected = listSelected;
            }
        }
        #endregion
        #region Handle Data
        private List<UpdateTargetDto> CulatedTarget(List<UpdateTargetDto> updateTarget, KPIQuarterCalculateType type)
        {
            // Tạo danh sách mới để lưu kết quả
            var result = new List<UpdateTargetDto>(updateTarget);

            // Bước 2: Tính quý (Q1 → Q4)
            for (int quarter = 0; quarter < 4; quarter++)
            {
                var months = updateTarget
                    .Where(x => x.Month >= quarter * 3 + 1 && x.Month <= quarter * 3 + 3)
                    .OrderBy(x => x.Month)
                    .ToList();
                decimal value = CalculateValue(months, type);
                result.Add(new UpdateTargetDto
                {
                    TaskId = updateTarget.First().TaskId,
                    UserId = updateTarget.First().UserId,
                    Quarter = quarter + 1,
                    CreatedDate = DateTime.Now,
                    Year = updateTarget.First().Year,
                    TargetValue = value
                });
            }
            // Bước 3: Tính nửa năm (2 quý)
            for (int half = 0; half < 2; half++)
            {
                var quarters = result
                    .Where(x => x.Quarter >= half * 2 + 1 && x.Quarter <= half * 2 + 2)
                    .ToList();

                decimal value = CalculateValue(quarters, type);
                result.Add(new UpdateTargetDto
                {
                    TaskId = updateTarget.First().TaskId,
                    UserId = updateTarget.First().UserId,
                    HalfYear = half + 1,
                    CreatedDate = DateTime.Now,
                    Year = updateTarget.First().Year,
                    TargetValue = value
                });
            }
            // Bước 4: Tính cả năm
            var allMonths = updateTarget.Where(x => x.Month >= 1 && x.Month <= 12).ToList();
            decimal yearValue = CalculateValue(allMonths, type);

            result.Add(new UpdateTargetDto
            {
                TaskId = updateTarget.First().TaskId,
                UserId = updateTarget.First().UserId,
                CreatedDate = DateTime.Now,
                Year = updateTarget.First().Year,
                TargetValue = yearValue,
            });

            return result;
        }
        private decimal CalculateValue(List<UpdateTargetDto> list, KPIQuarterCalculateType type)
        {
            if (list == null || !list.Any()) return 0;

            switch (type)
            {
                case KPIQuarterCalculateType.CALCULATE1:
                    return list.Sum(x => x.TargetValue);

                case KPIQuarterCalculateType.CALCULATE2:
                    return list.Average(x => x.TargetValue);

                case KPIQuarterCalculateType.CALCULATE3:
                    return list
                        .Where(x => x.Month != null)
                        .OrderByDescending(x => x.Month)
                        .FirstOrDefault()?.TargetValue ?? 0;

                default:
                    return 0;
            }
        }
        public List<UpdateTargetDto> ConvertCreatedToUpdateTargets(CreatedTargetDto created)
        {
            var list = new List<UpdateTargetDto>();

            for (int month = 1; month <= 12; month++)
            {
                decimal targetValue = month switch
                {
                    1 => created.TargetValue1,
                    2 => created.TargetValue2,
                    3 => created.TargetValue3,
                    4 => created.TargetValue4,
                    5 => created.TargetValue5,
                    6 => created.TargetValue6,
                    7 => created.TargetValue7,
                    8 => created.TargetValue8,
                    9 => created.TargetValue9,
                    10 => created.TargetValue10,
                    11 => created.TargetValue11,
                    12 => created.TargetValue12,
                    _ => 0
                };

                list.Add(new UpdateTargetDto
                {
                    TaskId = created.TaskId,
                    UserId = created.UserId,
                    Year = DateTime.Now.Year,
                    Month = month,
                    TargetValue = targetValue,
                    CreatedDate = created.CreatedDate,
                });
            }

            return list;
        }
        private async Task<bool> ValidateWieght()
        {
            var invalidGroups = CreatedTarget
                .GroupBy(x => x.KPIIndexType)
                .Where(g => Math.Round(g.Sum(x => x.Weight), 4) != 1m)
                .ToList();

            if (invalidGroups.Any())
            {
                RenderFragment content = builder =>
                {
                    int seq = 0;
                    builder.AddContent(seq++, "⚠️ The following types have invalid total weights:");
                    builder.AddMarkupContent(seq++, "<br/>");

                    foreach (var group in invalidGroups)
                    {
                        var totalWeight = group.Sum(x => x.Weight);
                        builder.AddContent(seq++, $"❌ Type '{group.Key}' has a total Weight of {totalWeight}, which is not equal to 1.");
                        builder.AddMarkupContent(seq++, "<br/>");
                    }
                };

                await Message.Error(content);
                return false;
            }

            return true;
        }
        #endregion
        #region Action
        private void Close()
        {
            IsModalVisible = false;
        }
        private async Task SaveAsync()
        {
            //var validate =await ValidateWieght();
            //if (!validate) return;

            HandleUserTaskAndTarget = new();
            foreach (var item in CreatedTarget)
            {
                var userTask = new HandleUserTaskDto();
                var listTarget = ConvertCreatedToUpdateTargets(item);
                HandleUserTaskAndTarget.UpdateTargetDtos.Add(listTarget);
                userTask.Weight = item.Weight;
                userTask.Index = item.Index;
                userTask.TaskIds = item.TaskId;
                userTask.UserId = UserId;
                HandleUserTaskDto.Add(userTask);
            }
            HandleUserTaskAndTarget.HandleUserTaskDtos = HandleUserTaskDto;
            var response = await Http.PostAsJsonAsync("api/UserTask/create", HandleUserTaskAndTarget);
            IsModalVisible = false;
            await OnSave.InvokeAsync();
            activeTabKey = "1";
        }
        private async Task ActionTab()
        {
            if (activeTabKey == "1")
            {
                CreatedTarget = new List<CreatedTargetDto>();
                activeTabKey = "2";
                TitleButton = "Back";
                foreach (var item in Selected)
                {
                    CreatedTarget.Add(new CreatedTargetDto
                    {
                        TaskId = item.Id,
                        TaskName = item.TaskName,
                        KPIIndexType = item.KPIIndexType,
                        KPICategory = item.KPICategory,
                        Max = item.Max,
                        Bottom = item.Bottom,
                        DataSource = item.DataSource,
                        Formula = item.Formula,
                        Department = item.Department,
                        Factory = item.Factory,
                        OwnerName = item.OwnerName,
                        Unit = item.Unit,
                        UserId = UserId,
                        CreatedDate = DateTime.Now
                    });
                }
                await LoadTaskAndTarget();
            }
            else if (activeTabKey == "2")
            {
                activeTabKey = "1";
                TitleButton = "Next";
            }
        }
        private string TitleButton =string.Empty;
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
            TaskName = null;
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
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name)) 
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
    }
}
