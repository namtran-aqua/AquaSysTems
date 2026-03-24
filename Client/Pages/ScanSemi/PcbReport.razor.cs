using AntDesign;
using AquaSolution.Client.Common.ExcelHelper;
using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ExcelHelper;
using AquaSolution.Client.Modals.Administration.Users;
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
    public partial class PcbReport : IAsyncDisposable
    {

        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<PcbReportDto>? _pcb = new();
        private List<PcbReportDto>? _pcbFilter = new();
        private HubConnection? _hubConnection;
        [Inject] NavigationManager NavigationManager { get; set; }
        private bool _isLoading = true;
        private bool _isInitialized;
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

            _hubConnection.Remove("LoadPcbReport");

            _hubConnection.On("LoadPcbReport", async () =>
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
                var data = await Http.GetFromJsonAsync<List<PcbReportDto>>(
                    "api/pcbreport/get-all-pcb-data");

                _pcb = data ?? new List<PcbReportDto>();
                _pcbFilter = _pcb.ToList();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Search
        private string? PcbCode { get; set; }
        private string? IntermediateCode { get; set; }
        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private void PcbCodeInputChanged(ChangeEventArgs e)
        {
            PcbCode = e.Value?.ToString();
        }
        private void IntermediateCodeInputChanged(ChangeEventArgs e)
        {
            IntermediateCode = e.Value?.ToString();
        }
        private Task Search()
        {
            try
            {
                var pcbCode = StringHelper.NormalizeText(PcbCode?.Trim());
                var intermediateCode = StringHelper.NormalizeText(IntermediateCode?.Trim());

                if (_pcb != null)
                {
                    var filtered = _pcb
                        .Where(x =>
                            (string.IsNullOrWhiteSpace(pcbCode) || (!string.IsNullOrEmpty(x.PcbCode) && StringHelper.NormalizeText(x.PcbCode).Contains(pcbCode))) &&
                            (string.IsNullOrWhiteSpace(intermediateCode) || (!string.IsNullOrEmpty(x.IntermediateCode) && StringHelper.NormalizeText(x.IntermediateCode).Contains(intermediateCode)))
                            )
                        .ToList();

                    if (string.IsNullOrWhiteSpace(pcbCode) &&
                        string.IsNullOrWhiteSpace(intermediateCode))
                    {
                        filtered = _pcb;
                    }

                    _pcbFilter = filtered;
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
            PcbCode = null;
            IntermediateCode = null;
            _pcbFilter = _pcb;
            _tableRef?.ReloadData();
            await InvokeAsync(StateHasChanged);
        }
        private Table<PcbReportDto>? _tableRef;
        #endregion
        #region Export
        private async Task ExportExcel()
        {
            try
            {
                var data = _pcbFilter;
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
