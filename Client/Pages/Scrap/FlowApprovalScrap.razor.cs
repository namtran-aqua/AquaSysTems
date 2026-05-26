using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ScrapManagement.FlowApprovalScraps;
using AquaSolution.Shared.ScrapManagement.FlowApprovals;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Scrap
{
    public partial class FlowApprovalScrap
    {
        #region Declaration
        [Inject] public HttpClient Http { get; set; }
        [Inject] public IMessageService Message { get; set; }

        private UserDto? CurrentUser { get; set; }
        private List<FlowApprovalResponse> ListFlowApproval { get; set; } = new();
        private List<FlowApprovalResponse> ListFiltered { get; set; } = new();
        private FlowApprovalScrapModal FlowApprovalScrapModal { get; set; }
        private EditStepModal EditStepModal { get; set; }

        private bool IsLoading { get; set; } = false;

        private List<string> _factoryOptions { get; set; } = new();
        private List<string> _departmentOptions { get; set; } = new();
        private string _filterFactory { get; set; } = string.Empty;
        private string _filterDepartment { get; set; } = string.Empty;
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            var currentUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrentUser = await currentUserClass.LoadCurrenUser();
            await LoadFlows();
        }

        private async Task LoadFlows()
        {
            try
            {
                IsLoading = true;

                var result = await Http.GetFromJsonAsync<List<FlowApprovalResponse>>(
                    "api/FlowApprovalScrap/get-all");

                if (result != null)
                {
                    ListFlowApproval = result;
                    _factoryOptions = result
                        .Select(x => x.FactoryName)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    _departmentOptions = result
                        .Select(x => x.DepartmentName)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                await Message.Error(ex.Message);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
        #endregion

        #region Filter
        private void ApplyFilter()
        {
            ListFiltered = ListFlowApproval
                .Where(x =>
                    (string.IsNullOrEmpty(_filterFactory) || x.FactoryName == _filterFactory) &&
                    (string.IsNullOrEmpty(_filterDepartment) || x.DepartmentName == _filterDepartment))
                .ToList();
            StateHasChanged();
        }

        private void OnFilterFactory(string value)
        {
            _filterFactory = value;
            _filterDepartment = string.Empty;
            _departmentOptions = ListFlowApproval
                .Where(x => string.IsNullOrEmpty(value) || x.FactoryName == value)
                .Select(x => x.DepartmentName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            ApplyFilter();
        }

        private void OnFilterDepartment(string value)
        {
            _filterDepartment = value;
            ApplyFilter();
        }

        private void ClearFilter()
        {
            _filterFactory = string.Empty;
            _filterDepartment = string.Empty;
            _departmentOptions = ListFlowApproval
                .Select(x => x.DepartmentName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            ApplyFilter();
        }
        #endregion

        #region Action
        private async Task CreateFlow()
        {
            if (FlowApprovalScrapModal != null)
                await FlowApprovalScrapModal.ShowModal();
        }

        // Sửa UpdateStep trong code-behind
        private async Task UpdateStep(FlowApprovalScrapDto flow)
        {
            if (FlowApprovalScrapModal != null)
                await EditStepModal.ShowModal(flow);
        }
        private async Task UpdateFlow(FlowApprovalResponse flow)
        {
            if (FlowApprovalScrapModal != null)
                await FlowApprovalScrapModal.ShowModalEdit(flow);
        }
        private async Task DeleteStep(Guid stepId)
        {
            try
            {
                var response = await Http.DeleteAsync(
                    $"api/FlowApprovalScrap/delete-step/{stepId}");

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Xóa step thành công");
                    await LoadFlows();
                }
                else
                {
                    await Message.Error("Xóa thất bại");
                }
            }
            catch (Exception ex)
            {
                await Message.Error(ex.Message);
            }
        }

        private async Task DeleteFlow(Guid departmentId, Guid factoryId)
        {
            try
            {
                var response = await Http.DeleteAsync(
                    $"api/FlowApprovalScrap/delete-flow/{departmentId}/{factoryId}");

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Xóa luồng duyệt thành công");
                    await LoadFlows();
                }
                else
                {
                    await Message.Error("Xóa thất bại");
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