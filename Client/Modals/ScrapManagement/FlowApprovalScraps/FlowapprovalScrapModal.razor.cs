using AntDesign;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.ScrapManagement.FlowApprovals;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.ScrapManagement.FlowApprovalScraps
{
    public partial class FlowApprovalScrapModal : ComponentBase
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IMessageService Message { get; set; }

        [Parameter] public EventCallback OnSaved { get; set; }

        private bool IsModalVisible { get; set; } = false;
        private bool _loading = false;
        private bool _submitting = false;
        private bool _isEdit = false;

        private Guid _selectedFactoryId = Guid.Empty;
        private Guid _selectedDepartmentId = Guid.Empty;
        private string _editFactoryName = string.Empty;
        private string _editDepartmentName = string.Empty;

        private List<FactoryDto> _factories = new();
        private List<DepartmentDto> _departments = new();
        private List<UserDto> _users = new();
        private List<CreateFlowStepRequest> _steps = new();
        #endregion

        #region Init

        // Gọi khi Create
        public async Task ShowModal()
        {
            _isEdit = false;
            _selectedFactoryId = Guid.Empty;
            _selectedDepartmentId = Guid.Empty;
            _editFactoryName = string.Empty;
            _editDepartmentName = string.Empty;
            _steps = new();

            await LoadInitialData();
            IsModalVisible = true;
            StateHasChanged();
        }

        // Gọi khi Edit
        public async Task ShowModalEdit(FlowApprovalResponse flow)
        {
            _isEdit = true;
            _selectedFactoryId = flow.FactoryId;
            _selectedDepartmentId = flow.DepartmentId;
            _editFactoryName = flow.FactoryName;
            _editDepartmentName = flow.DepartmentName;

            // Map sang CreateFlowStepRequest để bind vào table
            _steps = flow.Steps.Select(s => new CreateFlowStepRequest
            {
                Name = s.Name,
                DecisionMaker = s.DecisionMaker,
                Description = s.Description,
                Step = s.Step,
                // Giữ Id để update đúng record
                Id = s.Id
            }).OrderBy(s => s.Step).ToList();

            await LoadUsers();
            IsModalVisible = true;
            StateHasChanged();
        }

        private async Task LoadInitialData()
        {
            try
            {
                _loading = true;
                StateHasChanged();

                var factoryTask = Http.GetFromJsonAsync<List<FactoryDto>>("api/Factory/get-all");
                var departmentTask = Http.GetFromJsonAsync<List<DepartmentDto>>("api/Department/get-all");
                var userTask = Http.GetFromJsonAsync<List<UserDto>>("api/User/get-all");

                await Task.WhenAll(factoryTask, departmentTask, userTask);

                _factories = factoryTask.Result ?? new();
                _departments = departmentTask.Result ?? new();
                var user = userTask.Result ?? new();
                _users = user.Where(x => x.IsActive==true).ToList();
            }
            catch
            {
                await Message.Error("Không thể tải dữ liệu!");
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                _loading = true;
                StateHasChanged();

                var result = await Http.GetFromJsonAsync<List<UserDto>>("api/User/get-all");
                _users = result ?? new();
            }
            catch
            {
                await Message.Error("Không thể tải danh sách người dùng!");
            }
            finally
            {
                _loading = false;
            }
        }
        #endregion

        #region Actions
        private void OnUserSelected(CreateFlowStepRequest step, Guid userId)
        {
            step.DecisionMaker = userId;
            StateHasChanged();
        }

        private void AddStep()
        {
            _steps.Add(new CreateFlowStepRequest
            {
                Step = _steps.Count + 1
            });
            StateHasChanged();
        }

        private void RemoveStep(CreateFlowStepRequest step)
        {
            _steps.Remove(step);
            for (int i = 0; i < _steps.Count; i++)
                _steps[i].Step = i + 1;
            StateHasChanged();
        }

        private async Task SubmitAsync()
        {
            if (!_isEdit && (_selectedFactoryId == Guid.Empty || _selectedDepartmentId == Guid.Empty))
            {
                await Message.Warning("Vui lòng chọn Factory và Department!");
                return;
            }
            if (!_steps.Any())
            {
                await Message.Warning("Vui lòng thêm ít nhất 1 bước duyệt!");
                return;
            }
            if (_steps.Any(s => s.DecisionMaker == Guid.Empty))
            {
                await Message.Warning("Vui lòng chọn người duyệt cho tất cả các bước!");
                return;
            }
            if (_steps.Any(s => string.IsNullOrWhiteSpace(s.Name)))
            {
                await Message.Warning("Vui lòng nhập tên cho tất cả các bước!");
                return;
            }

            if (_isEdit)
                await SubmitEdit();
            else
                await SubmitCreate();
        }

        private async Task SubmitCreate()
        {
            var request = new CreateFlowApprovalRequest
            {
                FactoryId = _selectedFactoryId,
                DepartmentId = _selectedDepartmentId,
                Steps = _steps
            };

            try
            {
                _submitting = true;
                var response = await Http.PostAsJsonAsync("api/FlowApprovalScrap/create", request);

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Tạo luồng duyệt thành công!");
                    await OnSaved.InvokeAsync();
                    Close();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Thất bại: {error}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _submitting = false;
            }
        }

        private async Task SubmitEdit()
        {
            try
            {
                _submitting = true;
                var tasks = _steps.Select(s =>
                    Http.PutAsJsonAsync($"api/FlowApprovalScrap/update-step/{s.Id}", new UpdateFlowStepRequest
                    {
                        Id = s.Id,
                        DecisionMaker = s.DecisionMaker,
                        Step = s.Step
                    })
                ).ToList();

                var responses = await Task.WhenAll(tasks);

                if (responses.All(r => r.IsSuccessStatusCode))
                {
                    await Message.Success("Cập nhật luồng duyệt thành công!");
                    await OnSaved.InvokeAsync();
                    Close();
                }
                else
                {
                    await Message.Error("Một số bước cập nhật thất bại!");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _submitting = false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            _isEdit = false;
            _steps = new();
            StateHasChanged();
        }
        #endregion
    }
}