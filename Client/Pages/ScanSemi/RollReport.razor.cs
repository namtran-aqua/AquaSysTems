using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ExcelHelper;
using AquaSolution.Client.Components.Administration.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.SemiReport;
using AquaSolution.Shared.UserManagements;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Org.BouncyCastle.Bcpg;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ScanSemi
{
    public partial class RollReport: IAsyncDisposable
    {

        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<RollReportDto>? _roll = new();
        private List<RollReportDto>? _rollFilter = new();
        private HubConnection? _hubConnection;
        [Inject] NavigationManager NavigationManager { get; set; }
        private bool _isLoading = true;
        private bool _isInitialized;
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

            _hubConnection.Remove("LoadRollReport");

            _hubConnection.On("LoadRollReport", async () =>
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
        private async Task LoadData()
        {
            _isLoading = true;

            try
            {
                var data = await Http.GetFromJsonAsync<List<RollReportDto>>(
                    "api/rollreport/get-all-roll-data");

                _roll = data ?? new List<RollReportDto>();
                _rollFilter = _roll.ToList();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Search
        private string? PackNo { get; set; }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void PackNoInputChanged(ChangeEventArgs e)
        {
            PackNo = e.Value?.ToString();
        }
        private Task Search()
        {
            try
            {
                var packNo = StringHelper.NormalizeText(PackNo?.Trim());

                if (_roll != null)
                {
                    var filtered = _roll
                        .Where(x =>
                            (string.IsNullOrWhiteSpace(packNo) || (!string.IsNullOrEmpty(x.PackNo) && StringHelper.NormalizeText(x.PackNo).Contains(packNo)))                         )
                        .ToList();

                    if (string.IsNullOrWhiteSpace(packNo))
                    {
                        filtered = _roll;
                    }

                    _rollFilter = filtered;
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
            PackNo = null;
            _rollFilter = _roll;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        private Table<RollReportDto>? _tableRef;
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
                var data = _rollFilter;
                string[] excludeProperties = { "" };
                var excelBytes = ExcelFileGenerator.GenerateExcelFile(data, null, excludeProperties);
                await JS.InvokeVoidAsync("saveAsFileRoll", $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                    Convert.ToBase64String(excelBytes));

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

        }
        #endregion

    }
}
