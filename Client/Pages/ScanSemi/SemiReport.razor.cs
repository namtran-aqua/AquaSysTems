
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ExcelHelper;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Client.Pages.ITSuport.RequestSuport;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.SemiReport;
using AquaSolution.Shared.UserManagements;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace AquaSolution.Client.Pages.ScanSemi
{
    public partial class SemiReport: IAsyncDisposable
    {

        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<SemiReportDto>? _semi = new();
        private List<SemiReportDto>? _semiFilter = new();
        private HubConnection? _hubConnection;
        public class SemiResponse
        {
            public List<SemiReportDto> Data { get; set; } = new();
            public bool IsInner4Hour { get; set; }
        }
        [Inject] NavigationManager NavigationManager { get; set; }
        private bool _isLoading = true;
        private bool _isInitialized;
        private bool _isInner4Hour;
        //private RoleManagerDialog? _roleManagerDialog;
        //private UserModal? _userModal;
        //private UserDto? CurrenUser { get; set; }
        //private UserDetailModal? _detailModal;
        private Guid PageId { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            await LoadData();
            await SignalRReload();
        }

        private async Task SignalRReload()
        {
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri("signalrhub"))
                    .WithAutomaticReconnect()
                    .Build();

                await _hubConnection.StartAsync();
            }

            _hubConnection.Remove("LoadSemiReport");

            _hubConnection.On("LoadSemiReport", async () =>
            {
                await LoadData();
                await Search();
                await InvokeAsync(StateHasChanged);
            });
        }
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        private async Task GetPage()
        {
            var url = "/report-scan";
            if (Http != null) PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{url}");
        }
        //private async Task LoadData()
        //{
        //    _semi = new();
        //    if (Http != null)
        //    {
        //        var data = await Http.GetFromJsonAsync<List<SemiReportDto>>("api/semi/get-all-semi-data");
        //        if (data != null) _semi = data.ToList();
        //    }
        //    _semiFilter = _semi.ToList();
        //}

        private async Task LoadData()
        {
            _isLoading = true;
            try
            {
                var response = await Http.GetFromJsonAsync<SemiResponse>("api/semi/get-all-semi-data");

                if (response != null)
                {
                    _semi = response.Data;
                    _semiFilter = _semi.ToList();
                    _isInner4Hour = response.IsInner4Hour; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }
        //private async Task CheckPermission()
        //{
        //if (Http != null)
        //{
        //    var currenUserClass = new CurrenUser(Http, AuthStateProvider);
        //    CurrenUser = await currenUserClass.LoadCurrenUser();
        //}

        //EditRole = await permissionService.HasPermissionAsync(PageId, PermissionActionType.EditRole);
        //Created = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Add);
        //Edit = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Edit);
        //Delete = await permissionService.HasPermissionAsync(PageId, PermissionActionType.Delete);
        //}
        #endregion

        #region Search
        private string? InnerBarcode { get; set; }
        private string? OuterBarcode { get; set; }
        private string? MotorBarcode { get; set; }
        private string? ScrapDescription { get; set; }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void InnerBarcodeInputChanged(ChangeEventArgs e)
        {
            InnerBarcode = e.Value?.ToString();
        }
        private void OuterBarcodeInputChanged(ChangeEventArgs e)
        {
            OuterBarcode = e.Value?.ToString();
        }
        private void MotorBarcodeInputChanged(ChangeEventArgs e)
        {
            MotorBarcode = e.Value?.ToString();
        }
        private void ScrapInputChanged(ChangeEventArgs e)
        {
            ScrapDescription = e.Value?.ToString();
        }
        private Task Search()
        {
            try
            {
                var innerBarcode = StringHelper.NormalizeText(InnerBarcode?.Trim());
                var outerBarcode = StringHelper.NormalizeText(OuterBarcode?.Trim());
                var motorBarcode = StringHelper.NormalizeText(MotorBarcode?.Trim());
                var scrap = StringHelper.NormalizeText(ScrapDescription?.Trim());

                if (_semi != null)
                {
                    var filtered = _semi
                        .Where(x =>
                            (string.IsNullOrWhiteSpace(innerBarcode) || (!string.IsNullOrEmpty(x.InnerBarcode) && StringHelper.NormalizeText(x.InnerBarcode).Contains(innerBarcode))) &&
                            (string.IsNullOrWhiteSpace(outerBarcode) || (!string.IsNullOrEmpty(x.OuterBarcode) && StringHelper.NormalizeText(x.OuterBarcode).Contains(outerBarcode))) &&
                            (string.IsNullOrWhiteSpace(motorBarcode) || (!string.IsNullOrEmpty(x.MotorBarcode) && StringHelper.NormalizeText(x.MotorBarcode).Contains(motorBarcode))) &&
                            (string.IsNullOrWhiteSpace(scrap) || (!string.IsNullOrEmpty(x.ScrapDescription) && StringHelper.NormalizeText(x.ScrapDescription).Contains(scrap)))
                        )
                        .ToList();

                    if (string.IsNullOrWhiteSpace(innerBarcode) &&
                        string.IsNullOrWhiteSpace(outerBarcode) &&
                        string.IsNullOrWhiteSpace(motorBarcode) &&
                        string.IsNullOrWhiteSpace(scrap))
                    {
                        filtered = _semi;
                    }

                    _semiFilter = filtered;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong Search(): " + ex.Message);
            }

            return Task.CompletedTask;
        }
        private async Task Reset()
        {
            InnerBarcode = null;
            OuterBarcode = null;
            MotorBarcode = null;
            ScrapDescription = null;
            _semiFilter = _semi;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        private Table<SemiReportDto>? _tableRef;
        //private List<DepartmentDto> _listDepartment = new();
        //TableFilter<string>[] _positionFilter = Array.Empty<TableFilter<string>>();
        #endregion
        #region Load Filter Data
        private async Task LoadDataFilterAsync()
        {
            //    if (Http != null)
            //    {
            //        _listDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all") ??
            //                          new List<DepartmentDto>();
            //        _departmentFilter = _listDepartment
            //            .Where(x => !string.IsNullOrWhiteSpace(x.Name)) // loại bỏ null/empty
            //            .Select(x => new TableFilter<string>
            //            {
            //                Text = x.Name,
            //                Value = x.Name,
            //                Selected = false
            //            })
            //            .ToArray();

            //        _listFactory = await Http.GetFromJsonAsync<List<FactoryDto>>("api/factory/get-all") ??
            //                       new List<FactoryDto>();
            //        _factoryFilter = _listFactory
            //            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            //            .Select(x => new TableFilter<string>
            //            {
            //                Text = x.Name,
            //                Value = x.Name,
            //                Selected = false
            //            })
            //            .ToArray();

            //        _listPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all") ??
            //                        new List<PositionDto>();
            //    }

            //    _positionFilter = _listPosition
            //        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            //        .Select(x => new TableFilter<string>
            //        {
            //            Text = x.Name,
            //            Value = x.Name,
            //            Selected = false
            //        })
            //        .ToArray();
            //    if (_users != null)
            //        foreach (var user in _users)
            //        {
            //            user.FactoryName ??= string.Empty;
            //            user.DepartmentName ??= string.Empty;
            //            user.PositionName ??= string.Empty;
            //        }
        }
        #endregion
        #region Export
        private async Task ExportExcel()
        {
            try
            {
                var data = _semiFilter;
                string[] excludeProperties = { "" };
                var excelBytes = ExcelFileGenerator.GenerateExcelFile(data, null, excludeProperties);
                await JS.InvokeVoidAsync("saveAsFileSemi", $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                    Convert.ToBase64String(excelBytes));

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        private async Task OnSwitchChanged(bool args)
        {
            _isInner4Hour = args;
            try
            {
                // Gọi API cập nhật database
                var response = await Http.PostAsJsonAsync("api/semi/update-active-status", args);

                if (!response.IsSuccessStatusCode)
                {
                    _isInner4Hour = !args;
                    Console.WriteLine("Lỗi: Không thể lưu vào database.");
                }
            }
            catch (Exception ex)
            {
                _isInner4Hour = !args;
                Console.WriteLine($"Network Error: {ex.Message}");
            }

        }
    }
}
