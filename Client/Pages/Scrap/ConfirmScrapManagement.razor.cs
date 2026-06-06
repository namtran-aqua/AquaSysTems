using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ScrapManagement.Scrap;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ScrapManagement.Scrap;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Scrap
{
    public partial class ConfirmScrapManagement
    {
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public IMessageService Message { get; set; } = default!;

        private List<HistoryScrapDto> ListHistory { get; set; } = new();
        private List<HistoryScrapDto> ListFiltered { get; set; } = new();

        // Chỉ 2 nhóm: Approved (chờ xác nhận) và Done (đã xác nhận)
        private Dictionary<string, List<HistoryScrapDto>> GroupedScraps { get; set; } = new();

        // Mặc định mở nhóm cần xác nhận
        private HashSet<string> ExpandedGroups { get; set; } = new() { "PendingConfirm" };

        private bool IsLoading { get; set; } = false;
        private UserDto? CurrenUser { get; set; }
        private RegisterScrapModal registerScrapModal { get; set; } = default!;

        private string? _filterFactory;
        private string? _filterDepartment;
        private List<string> _factoryOptions = new();
        private List<string> _departmentOptions = new();
        private HubConnection? _hubConnection;
        #region Init
        protected override async Task OnInitializedAsync()
        {
            var currenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await currenUserClass.LoadCurrenUser();
            await SignalRReload();
            await LoadHistory();
        }
        private async Task SignalRReload()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(Navigation.BaseUri + "signalrhub"))
                .Build();

            _hubConnection.On("LoadScrap", async () =>
            {
                await LoadHistory();
                await InvokeAsync(StateHasChanged);
            });

            await _hubConnection.StartAsync();
        }

        public async Task LoadHistory()
        {
            IsLoading = true;
            try
            {
                // Chỉ load phiếu đã Approved hoặc Done
                var result = await Http.GetFromJsonAsync<List<HistoryScrapDto>>(
                    "api/scrap/get-scrap-for-confirm");

                if (result != null)
                {
                    ListHistory = result;

                    _factoryOptions = ListHistory
                        .Where(x => !string.IsNullOrEmpty(x.FactoryName))
                        .Select(x => x.FactoryName).Distinct().OrderBy(x => x).ToList();

                    _departmentOptions = ListHistory
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
            _departmentOptions = ListHistory
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
            _departmentOptions = ListHistory
                .Where(x => !string.IsNullOrEmpty(x.DepartmentName))
                .Select(x => x.DepartmentName).Distinct().OrderBy(x => x).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            ListFiltered = ListHistory
                .Where(x =>
                    (string.IsNullOrEmpty(_filterFactory) || x.FactoryName == _filterFactory) &&
                    (string.IsNullOrEmpty(_filterDepartment) || x.DepartmentName == _filterDepartment))
                .ToList();
            BuildGroups();
            StateHasChanged();
        }

        /// <summary>
        /// Approved  = cần xác nhận (PendingConfirm)
        /// Done      = đã xác nhận
        /// Thứ tự: PendingConfirm trước, Done sau
        /// </summary>
        private void BuildGroups()
        {
            GroupedScraps = ListFiltered
                .GroupBy(x => x.Status switch
                {
                    StatusScrap.Approved => "PendingConfirm",
                    StatusScrap.Done => "Done",
                    _ => "Other"
                })
                .OrderBy(g => g.Key switch
                {
                    "PendingConfirm" => 0,
                    "Done" => 1,
                    _ => 2
                })
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private void ToggleGroup(string key)
        {
            if (ExpandedGroups.Contains(key))
                ExpandedGroups.Remove(key);
            else
                ExpandedGroups.Add(key);
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
            {
                // Cả Approved (chờ confirm) lẫn Done (đã confirm) đều mở Mode = Confirm
                // Modal tự xử lý: Done → readonly + ẩn nút xác nhận
                bool isConfirm = row.Status == StatusScrap.Approved
                              || row.Status == StatusScrap.Done;

                await registerScrapModal.ShowModal(
                    CurrenUser!, scrapHistory,
                    isEdit: false, isDetail: false,
                    isApproval: false, isConfirm: isConfirm);
            }
        }
        #endregion
    }
}
