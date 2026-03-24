using AquaSolution.Client.Common;
using AquaSolution.Shared.KPI.KPISubmit;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.KPISubmit
{
    public partial class ApprovalTaskModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsVisibleModal { get; set; } = false;
        private bool IsView { get; set; }
        [Parameter] public EventCallback<ApprovalInfo> IsApprovalEvent { get; set; }

        [Parameter] public EventCallback<ApprovalInfo> IsRejectEvent { get; set; }
        private string ModalTitle { get; set; } = string.Empty;
        private List<ProcessApprovalDto> ProcessApprovalList { get; set; } = new();
        private ViewDetailApprovalKPI ViewDetailApprovalKPI { get; set; } = new();
        private ApprovalInfo ApprovalInfo { get; set; } = new();
        #endregion
        #region Innit
        public async Task ShowModalAsync(ApprovalInfo approvalInfo , bool isView)
        {
            ViewDetailApprovalKPI = new();
            IsView = isView;
            ApprovalInfo = approvalInfo;
            await LoadDataDetail();
            IsVisibleModal = true;
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action
        private async Task ApprovalAsync()
        {
            ApprovalInfo.IsApproved = true;
            await IsApprovalEvent.InvokeAsync(ApprovalInfo);
            IsVisibleModal = false;
            StateHasChanged();
        }
        private async Task RejectedAsync()
        {
            ApprovalInfo.IsApproved = false;
            await IsApprovalEvent.InvokeAsync(ApprovalInfo);
            IsVisibleModal = false;
            StateHasChanged();

        }
        private void CloseModal()
        {
            IsVisibleModal = false;
            StateHasChanged();
        }
        #endregion
        #region Handlers
        private async Task LoadDataDetail()
        {
            await LoadProcess();
            await LoadDetailScore();
        }
        private async Task LoadProcess()
        {
            var result = await Http.GetFromJsonAsync<List<ProcessApprovalDto>>(
                                $"api/kpiSubmit/get-process-by-submitid/{ApprovalInfo.SubmitId}");
            if (result != null)
            {
                ProcessApprovalList = result;
            }
        }
        private async  Task LoadDetailScore()
        {
            var result = await Http.GetFromJsonAsync<ViewDetailApprovalKPI>(
                   $"api/kpiSubmit/get-detail-by-submitid/{ApprovalInfo.SubmitId}");
            if (result != null)
            {
                ViewDetailApprovalKPI = result;
                ModalTitle = ViewDetailApprovalKPI?.TotalScore?.FirstOrDefault(x => x.Month != null)?.Title;
            }
        }
        #endregion

    }
}
