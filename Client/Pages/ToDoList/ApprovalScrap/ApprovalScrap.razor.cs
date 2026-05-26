using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ScrapManagement.Scrap;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ScrapManagement.Scrap;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ToDoList.ApprovalScrap
{
    public partial class ApprovalScrap
    {
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public IMessageService Message { get; set; } = default!;

        private List<HistoryScrapDto> ListScrap { get; set; } = new();
        private List<HistoryScrapDto> ListFiltered { get; set; } = new();
        private bool IsLoading { get; set; } = false;
        private UserDto CurrenUser { get; set; } = new();
        private RegisterScrapModal registerScrapModal { get; set; } = default!;

        private string? _filterFactory;
        private string? _filterDepartment;
        private List<string> _factoryOptions = new();
        private List<string> _departmentOptions = new();

        private bool IsRejectModalVisible = false;
        private string? RejectComment;
        private HistoryScrapDto? RowToReject;

        #region Init
        protected override async Task OnInitializedAsync()
        {
            var currenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await currenUserClass.LoadCurrenUser();
            await LoadScraps();

            await LoadScraps();
        }

        public async Task LoadScraps()
        {
            if (CurrenUser.Id == Guid.Empty) return;

            IsLoading = true;
            try
            {
           
                var res = await Http.GetFromJsonAsync<List<HistoryScrapDto>>(
                    $"api/scrap/get-scrap-for-approval/{CurrenUser.Id}");

                if (res != null)
                {
                    ListScrap = res;
                    _factoryOptions = ListScrap
                        .Where(x => !string.IsNullOrEmpty(x.FactoryName))
                        .Select(x => x.FactoryName).Distinct().OrderBy(x => x).ToList();
                    _departmentOptions = ListScrap
                        .Where(x => !string.IsNullOrEmpty(x.DepartmentName))
                        .Select(x => x.DepartmentName).Distinct().OrderBy(x => x).ToList();
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi load dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
        #endregion

        #region Filter
        private void OnFilterFactory(string? factory)
        {
            _filterFactory = factory;
            _filterDepartment = null;
            _departmentOptions = ListScrap
                .Where(x => string.IsNullOrEmpty(factory) || x.FactoryName == factory)
                .Where(x => !string.IsNullOrEmpty(x.DepartmentName))
                .Select(x => x.DepartmentName).Distinct().OrderBy(x => x).ToList();
            ApplyFilter();
        }

        private void OnFilterDepartment(string? department)
        {
            _filterDepartment = department;
            ApplyFilter();
        }

        private void ClearFilter()
        {
            _filterFactory = null;
            _filterDepartment = null;
            _departmentOptions = ListScrap
                .Where(x => !string.IsNullOrEmpty(x.DepartmentName))
                .Select(x => x.DepartmentName).Distinct().OrderBy(x => x).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            ListFiltered = ListScrap
                .Where(x =>
                    (string.IsNullOrEmpty(_filterFactory) || x.FactoryName == _filterFactory) &&
                    (string.IsNullOrEmpty(_filterDepartment) || x.DepartmentName == _filterDepartment))
                .ToList();
            StateHasChanged();
        }
        #endregion

        #region Actions
        private async Task DetailScrap(HistoryScrapDto row)
        {
            var scrapHistory = new HandleScrapDto
            {
                Id = row.Id,
                Title = row.Title,
                Status = row.Status,
                Description = row.Description,
                CreatedDate = row.CreatedDate,
                CreatedById = row.CreatedById,
                CreatedByName = row.CreatedByName,
                FactoryId = row.FactoryId,
                FactoryName = row.FactoryName,
                DepartmentId = row.DepartmentId,
                DepartmentName = row.DepartmentName,
                TotalAmount = row.TotalAmount,
                ConfirmAmount = row.ConfirmAmount,
                ConfirmationStatusType = row.ConfirmationStatusType,
                Notes = row.Notes,
                HistoryDetails = row.HistoryDetails,
                Approvals = row.Approvals
            };

            if (registerScrapModal != null)
                await registerScrapModal.ShowModal(
                    CurrenUser, scrapHistory,
                    isEdit: false, isDetail: false, isApproval: true);
        }

        private async Task ApproveRow(HistoryScrapDto row)
        {
            try
            {
                var req = new ApprovalActionDto
                {
                    HistoryScrapId = row.Id,
                    ActionBy = CurrenUser.Id,
                    IsApproved = true
                };

                var res = await Http.PostAsJsonAsync("api/scrap/action-approval", req);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Đã duyệt phiếu thành công!");
                    await LoadScraps();
                }
                else
                {
                    await Message.Error("Lỗi: " + await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Message.Error("Lỗi: " + ex.Message);
            }
        }

        private void OpenRejectModal(HistoryScrapDto row)
        {
            RowToReject = row;
            RejectComment = string.Empty;
            IsRejectModalVisible = true;
            StateHasChanged();
        }

        private void CloseRejectModal()
        {
            IsRejectModalVisible = false;
            RowToReject = null;
            StateHasChanged();
        }

        private async Task ConfirmRejectRow()
        {
            if (string.IsNullOrWhiteSpace(RejectComment))
            {
                await Message.Warning("Vui lòng nhập lý do từ chối!");
                return;
            }

            try
            {
                var req = new ApprovalActionDto
                {
                    HistoryScrapId = RowToReject!.Id,
                    ActionBy = CurrenUser.Id,
                    IsApproved = false,
                    Comment = RejectComment
                };

                var res = await Http.PostAsJsonAsync("api/scrap/action-approval", req);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Đã từ chối phiếu thành công!");
                    CloseRejectModal();
                    await LoadScraps();
                }
                else
                {
                    await Message.Error("Lỗi: " + await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Message.Error("Lỗi: " + ex.Message);
            }
        }
        #endregion
    }
}