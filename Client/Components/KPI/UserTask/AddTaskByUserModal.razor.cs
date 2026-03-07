
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.UserTask;
using AquaSolution.Shared.UserManagements;
using Force.DeepCloner;
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
        private List<CreatedTargetDto> CreatedTargetCompare = new();

        private HandleUserTaskAndTargetDto HandleUserTaskAndTarget = new();
        private UserDto User { get; set; }
        private string activeTabKey = "1";
        private string TitleButton = string.Empty;
        private string TitleButton2 = string.Empty;

        #endregion

        #region Init
        public async Task ShowModal(UserDto user, List<KPITaskDto> kPITaskDtos)
        {
            CreatedTarget.Clear();
            CreatedTargetCompare.Clear();
            HandleUserTaskDto.Clear();
            User = user;
            if(User.Roles.Any(x => x.Name == "KPIUSER_DepartmentViewer"))
            {
                DataSource = kPITaskDtos
                              .Where(x => x.DepartmentId == user.DepartmentId)
                              .ToList();
            }
            else
            {
                DataSource = kPITaskDtos
                    .Where(x => x.FactoryId == user.FactoryId && x.DepartmentId == user.DepartmentId)
                    .ToList();
            }
            DataFilter = DataSource;
            await LoadDataFilter();

            Selected = null;
            activeTabKey = "1";
            TitleButton = "Next ⟶";
            TitleButton2 = "⟵ Back";
            HandleUserTaskDto = new();
            await AddTaskByUser();
            IsModalVisible = true;
            StateHasChanged();
        }
        #endregion

        #region Handle Data
        private async Task LoadTaskAndTarget()
        {
            var monthProperties = new Dictionary<int, Action<CreatedTargetDto, decimal>>
            {
                [1] = (d, v) => d.TargetValue1 = v,
                [2] = (d, v) => d.TargetValue2 = v,
                [3] = (d, v) => d.TargetValue3 = v,
                [4] = (d, v) => d.TargetValue4 = v,
                [5] = (d, v) => d.TargetValue5 = v,
                [6] = (d, v) => d.TargetValue6 = v,
                [7] = (d, v) => d.TargetValue7 = v,
                [8] = (d, v) => d.TargetValue8 = v,
                [9] = (d, v) => d.TargetValue9 = v,
                [10] = (d, v) => d.TargetValue10 = v,
                [11] = (d, v) => d.TargetValue11 = v,
                [12] = (d, v) => d.TargetValue12 = v,
            };

            await Task.WhenAll(CreatedTarget.Select(async item =>
            {
                var results = await Http.GetFromJsonAsync<List<GetUserTaskAndTargetDto>>(
                    $"api/KPIMonthlyTarget/get-kpi-target/{item.TaskId}/{item.UserId}") ?? new();

                var first = results.FirstOrDefault();
                if (first != null)
                {
                    item.Index = first.Index;
                    if(first.Weight != 0)
                    {
                        item.Weight = first.Weight;
                    }
                    item.QuarterCalculateType = first.QuarterCalculateType;

                }

                foreach (var target in results.Where(x => x.Month.HasValue))
                {
                    if (monthProperties.TryGetValue(target.Month.Value, out var set))
                        set(item, target.TargetValue);
                }
            }));
            //CreatedTargetCompare = CreatedTarget.DeepClone();
        }

        private async Task AddTaskByUser()
        {
            var result = await Http.GetFromJsonAsync<List<UserTaskDto>>(
                $"api/UserTask/list-taskIds/{User.Id}");

            if (result != null)
            {
                var ids = result.Select(x => x.KPITaskId).ToList();
                Selected = DataSource.Where(x => ids.Contains(x.Id)).ToList();
            }
        }

        private List<UpdateTargetDto> CulatedTarget(
            List<UpdateTargetDto> updateTarget,
            QuarterCalculateType type)
        {
            var result = new List<UpdateTargetDto>(updateTarget);

            // Quarter
            for (int q = 0; q < 4; q++)
            {
                var months = updateTarget
                    .Where(x => x.Month >= q * 3 + 1 && x.Month <= q * 3 + 3)
                    .ToList();

                result.Add(new UpdateTargetDto
                {
                    TaskId = updateTarget.First().TaskId,
                    UserId = updateTarget.First().UserId,
                    Year = updateTarget.First().Year,
                    Quarter = q + 1,
                    TargetValue = CalculateValue(months, type),
                    CreatedDate = DateTime.Now
                });
            }

            // HalfYear
            for (int h = 0; h < 2; h++)
            {
                var quarters = result
                    .Where(x => x.Quarter >= h * 2 + 1 && x.Quarter <= h * 2 + 2)
                    .ToList();

                result.Add(new UpdateTargetDto
                {
                    TaskId = updateTarget.First().TaskId,
                    UserId = updateTarget.First().UserId,
                    Year = updateTarget.First().Year,
                    HalfYear = h + 1,
                    TargetValue = CalculateValue(quarters, type),
                    CreatedDate = DateTime.Now
                });
            }

            // Year
            result.Add(new UpdateTargetDto
            {
                TaskId = updateTarget.First().TaskId,
                UserId = updateTarget.First().UserId,
                Year = updateTarget.First().Year,
                TargetValue = CalculateValue(updateTarget, type),
                CreatedDate = DateTime.Now
            });

            return result;
        }

        private decimal CalculateValue(List<UpdateTargetDto> list, QuarterCalculateType type)
        {
            if (!list.Any()) return 0;

            return type switch
            {
                QuarterCalculateType.CAL1 => list.Sum(x => x.TargetValue),
                QuarterCalculateType.CAL2 => list.Average(x => x.TargetValue),
                QuarterCalculateType.CAL3 => list.Last().TargetValue,
                _ => 0
            };
        }
        private void FillQuarterHalfYearAndYear(
            CreatedTargetDto created,
            List<UpdateTargetDto> calculated)
        {
            created.TargetQarter1 = calculated.FirstOrDefault(x => x.Quarter == 1)?.TargetValue ?? 0;
            created.TargetQarter2 = calculated.FirstOrDefault(x => x.Quarter == 2)?.TargetValue ?? 0;
            created.TargetQarter3 = calculated.FirstOrDefault(x => x.Quarter == 3)?.TargetValue ?? 0;
            created.TargetQarter4 = calculated.FirstOrDefault(x => x.Quarter == 4)?.TargetValue ?? 0;

            created.TargetHaftYear1 = calculated.FirstOrDefault(x => x.HalfYear == 1)?.TargetValue ?? 0;
            created.TargetHaftYear2 = calculated.FirstOrDefault(x => x.HalfYear == 2)?.TargetValue ?? 0;

            created.TargetYear = calculated
                .FirstOrDefault(x => x.Month == null && x.Quarter == null && x.HalfYear == null)
                ?.TargetValue ?? 0;
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
        #endregion

        #region Action
        private async Task ActionTab()
        {
            if (activeTabKey == "1")
            {
                CreatedTarget.Clear();
                activeTabKey = "2";
                TitleButton = "NEXT ⟶";

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
                        Calculated = item.Calculated,
                        Department = item.Department,
                        Factory = item.Factory,
                        PIC = item.PIC,
                        Unit = item.Unit,
                        UserId = User.Id,
                        CreatedDate = DateTime.Now,
                        QuarterCalculateType = item.QuarterCalculateType
                    });
                }

                await LoadTaskAndTarget();
            }
            else if (activeTabKey == "2")
            {
                var validate = await ValidateWieght();
                if (!validate) return;
                HandleUserTaskAndTarget = new();
      
                //bool isDifferent = CreatedTarget
                //.Where((x, i) => !TargetEquals(x, CreatedTargetCompare[i]))
                //.Any();
                //if (isDifferent)
                //{
                //    CreatedTargetCompare = CreatedTarget
                //        .Select(x => CloneTarget(x))
                //        .ToList();
                //}
                foreach (var item in CreatedTarget)
                {
    
                    var monthTargets = ConvertCreatedToUpdateTargets(item);
                    var fullTargets = CulatedTarget(monthTargets, item.QuarterCalculateType);
                    FillQuarterHalfYearAndYear(item, fullTargets);
                    HandleUserTaskAndTarget.UpdateTargetDtos.Add(fullTargets);
                }
                activeTabKey = "3";
                TitleButton = "⟵ Back";
            }
            else
            {
                activeTabKey = "2";
                TitleButton = "NEXT ⟶";
            }
        }
        private CreatedTargetDto CloneTarget(CreatedTargetDto x)
        {
            return new CreatedTargetDto
            {
                TaskId = x.TaskId,
                UserId = x.UserId,
                Year = x.Year,
                Index = x.Index,
                Weight = x.Weight,

                TargetValue1 = x.TargetValue1,
                TargetValue2 = x.TargetValue2,
                TargetValue3 = x.TargetValue3,
                TargetValue4 = x.TargetValue4,
                TargetValue5 = x.TargetValue5,
                TargetValue6 = x.TargetValue6,
                TargetValue7 = x.TargetValue7,
                TargetValue8 = x.TargetValue8,
                TargetValue9 = x.TargetValue9,
                TargetValue10 = x.TargetValue10,
                TargetValue11 = x.TargetValue11,
                TargetValue12 = x.TargetValue12,

                TargetQarter1 = x.TargetQarter1,
                TargetQarter2 = x.TargetQarter2,
                TargetQarter3 = x.TargetQarter3,
                TargetQarter4 = x.TargetQarter4,

                TargetHaftYear1 = x.TargetHaftYear1,
                TargetHaftYear2 = x.TargetHaftYear2,
                TargetYear = x.TargetYear,

                QuarterCalculateType = x.QuarterCalculateType
            };
        }

        private bool TargetEquals(CreatedTargetDto a, CreatedTargetDto b)
        {
            if (a == null || b == null) return false;

            return
                a.TargetValue1 == b.TargetValue1 &&
                a.TargetValue2 == b.TargetValue2 &&
                a.TargetValue3 == b.TargetValue3 &&
                a.TargetValue4 == b.TargetValue4 &&
                a.TargetValue5 == b.TargetValue5 &&
                a.TargetValue6 == b.TargetValue6 &&
                a.TargetValue7 == b.TargetValue7 &&
                a.TargetValue8 == b.TargetValue8 &&
                a.TargetValue9 == b.TargetValue9 &&
                a.TargetValue10 == b.TargetValue10 &&
                a.TargetValue11 == b.TargetValue11 &&
                a.TargetValue12 == b.TargetValue12 &&

                a.TargetQarter1 == b.TargetQarter1 &&
                a.TargetQarter2 == b.TargetQarter2 &&
                a.TargetQarter3 == b.TargetQarter3 &&
                a.TargetQarter4 == b.TargetQarter4 &&

                a.TargetHaftYear1 == b.TargetHaftYear1 &&
                a.TargetHaftYear2 == b.TargetHaftYear2 &&

                a.TargetYear == b.TargetYear;
        }


        private async Task ActionTab2()
        {
            if (activeTabKey == "2")
            {
                activeTabKey = "1";
 
            }    
        }
        private async Task SaveAsync()
        {
            var validate = await ValidateWieght();
            if (!validate) return;
            foreach (var item in CreatedTarget)
            {
                var userTask = new HandleUserTaskDto();
                var listTarget = ConvertCreatedToUpdateTargets(item);
                HandleUserTaskAndTarget.UpdateTargetDtos.Add(listTarget);
                userTask.Weight = item.Weight;
                userTask.Index = item.Index;
                userTask.TaskIds = item.TaskId;
                userTask.UserId = User.Id;
                HandleUserTaskDto.Add(userTask);
            }
            HandleUserTaskAndTarget.HandleUserTaskDtos = HandleUserTaskDto;

            await Http.PostAsJsonAsync("api/UserTask/create", HandleUserTaskAndTarget);
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }

        private async Task<bool> ValidateWieght()
        {
            var invalidGroups = CreatedTarget
                .GroupBy(x => x.KPIIndexType)
                .Where(g => Math.Round(g.Sum(x => x.Weight), 4) != 1m)
                .ToList();
            foreach(var item in CreatedTarget)
            {
                if(item.Weight <=0)
                {
                     await Message.Error("Weight must be greater than zero");
                }
            }
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
        private void Close()
        {
            IsModalVisible = false;
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
