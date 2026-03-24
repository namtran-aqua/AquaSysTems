using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.KPI.KPISubmit;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using OneOf.Types;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ToDoList.Approvalkpi
{
    public partial class ApprovalKPI
    {
        #region Declaration
        private UserDto? CurrenUser { get; set; }
        [Inject] private HttpClient? Http { get; set; }
        private List<GroupViewKPIForApproval> _groupedList { get; set; } = new();
        Table<ViewKPITotalScoreDto>? TableRef;
        private ApprovalTaskModal ApprovalTaskModalRef = new();
        private HubConnection? _hubConnection;
        private HandleApprovalTask HandleApproval = new();
        private Table<ViewKPIForApprovalDto>? _tableRef;
        TableFilter<int?>[] _monthFilter = Array.Empty<TableFilter<int?>>();
        #endregion
        #region Init
        protected override async Task OnInitializedAsync()
        {
            await LoadCurrenUser();
            await LoadData();
            await InitSignalRAsync();
        }
        private async Task InitSignalRAsync()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri("signalrhub"))
                    .Build();
                _hubConnection.On("ReloadKPIForUserApproval", async () =>
                {
                    await InvokeAsync(async () =>
                    {
                        await LoadData();
                        StateHasChanged();
                    });
                });
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR error: {ex.Message}");
            }
        }
        private async Task LoadCurrenUser()
        {
            if (Http != null)
            {
                var currenUserClass = new CurrenUser(Http, AuthStateProvider);
                CurrenUser = await currenUserClass.LoadCurrenUser();
            }
        }

        private async Task LoadData()
        {
            try
            {
                if (Http == null || CurrenUser == null)
                    return;

                var data = await Http.GetFromJsonAsync<List<ViewKPIForApprovalDto>>("api/KPISubmit/get-kpi-approval");

                if (CurrenUser.Roles.Any(x => x.Name == "Admin"))
                    data = data.OrderByDescending(x=>x.CreatedDate).ToList();
                else
                    data = data.Where(x => x.DecisionMaker == CurrenUser.Id &&
                                           x.EApprovalStatusType != EApprovalStatusType.Pending).OrderByDescending(x => x.CreatedDate).ToList();
                _groupedList.Clear();
                _groupedList.AddRange(new[]
                {
                    new GroupViewKPIForApproval { ApprovalStatusType = EApprovalStatusType.InReview, StatusName = "Pending" },
                    new GroupViewKPIForApproval { ApprovalStatusType = EApprovalStatusType.Approved, StatusName = "Approved" },
                    new GroupViewKPIForApproval { ApprovalStatusType = EApprovalStatusType.Rejected, StatusName = "Rejected" }
                });
     
                foreach (var group in _groupedList)
                {
                    group.Items = data
                        .Where(x => x.EApprovalStatusType == group.ApprovalStatusType)
                        .OrderByDescending(x => x.Month)
                        .ThenBy(x => x.Step)
                        .ToList();
                    _monthFilter = group.Items
                         .Where(x => x.Month !=0)
                         .Select(x => new TableFilter<int?>
                         {
                             Text = $"Month {x.Month}",
                             Value = x.Month,
                             Selected = false
                         })
                         .GroupBy(f => f.Value)
                         .Select(g => g.First())
                         .ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        #endregion
        #region Actions
        private async Task EvenCallback(ApprovalInfo approvalInfo)
        {
            if (approvalInfo == null) return;
            if (approvalInfo.IsApproved)
            {
                await ApprovalAsync(approvalInfo.SubmitId, approvalInfo.RequestTaskId, approvalInfo.DecisionMaker);
            }
            else
            {
                await RejectedAsync(approvalInfo.SubmitId, approvalInfo.RequestTaskId, approvalInfo.DecisionMaker);
            }
        }
        private async Task ApprovalAsync(Guid submitId, Guid requestTaskId, Guid? decisionMaker)
        {
            var approvalInfo = new ApprovalInfo();
            approvalInfo.SubmitId = submitId;
            approvalInfo.RequestTaskId = requestTaskId;
            approvalInfo.IsApproved = true;
            var Confirm = await MessageBox.Confirm(modal, "Are you sure you want to be approved?");
            if (!Confirm) return;
            if (CurrenUser.Roles.Any(x => x.Name == "Admin"))
            {
                approvalInfo.DecisionMaker = CurrenUser.Id;
            }
            else
            {
                approvalInfo.DecisionMaker = decisionMaker;
            }
            var response = await Http.PutAsJsonAsync("api/kpiSubmit/update-status-request-kpi", approvalInfo);

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Update successfully !");

            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Update failed");

            }
        }
        private async Task RejectedAsync(Guid submitId, Guid requestTaskId, Guid? decisionMaker)
        {
            var approvalInfo = new ApprovalInfo();
            approvalInfo.SubmitId = submitId;
            approvalInfo.RequestTaskId = requestTaskId;
            if (CurrenUser.Roles.Any(x => x.Name == "Admin"))
            {
                approvalInfo.DecisionMaker = CurrenUser.Id;
            }
            else
            {
                approvalInfo.DecisionMaker = decisionMaker;
            }
            approvalInfo.IsApproved = false;
            await HandleApproval.ShowModal(approvalInfo);
        }
        private async Task ViewDetail(ViewKPIForApprovalDto item)
        {
            var approvalInfo = new ApprovalInfo();
            approvalInfo.SubmitId = item.SubmitId;
            approvalInfo.DecisionMaker = item.DecisionMaker;
            approvalInfo.RequestTaskId = item.Id;
            bool isView;
            if (item.EApprovalStatusType == EApprovalStatusType.InReview)
            {
                isView = false;
            }
            else
            {
                isView = true;
            }
            await ApprovalTaskModalRef.ShowModalAsync(approvalInfo, isView);
        }
        #endregion
    }
}
