using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ScrapManagement.Materials;
using AquaSolution.Client.Modals.ScrapManagement.Scrap;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.HRMS;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Shared.ScrapManagement.Scrap;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
namespace AquaSolution.Client.Pages.Scrap
{
    public partial class ScrapManagement
    {

        #region Declaration
        public List<ImportExcelDto> ImportedData { get; set; } = new List<ImportExcelDto>();
        private UserDto? CurrenUser { get; set; }
        private HubConnection? _hubConnection;
        private UpdateWeightDto? UpdateWeight { get; set; }
        [Inject]
        public HttpClient Http { get; set; }
        [Inject]
        public IMessageService Message { get; set; }
        private List<HistoryScrapDto> ListHistory { get; set; } = new List<HistoryScrapDto>();

        private Table<HistoryScrapDto> _tableRef;
        private RegisterScrapModal registerScrapModal;
        private bool Edit { get; set; } = true;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            ImportedData = new List<ImportExcelDto>();

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
        private async Task LoadHistory()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<HistoryScrapDto>>("api/scrap/get-all-scraps");
                if (result != null)
                {
                    if (CurrenUser.Roles.Any(x => x.Name == "Admin"))
                    {

                        ListHistory = result;
                    }
                    else
                    {
                        ListHistory = result.Where(x => x.CreatedById == CurrenUser.Id).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                await Message.Error(ex.Message);
            }
        }
        #endregion

        #region Action
        private async Task CreatedScrap()
        {
            if (registerScrapModal != null)
            {
                registerScrapModal.ShowModal(CurrenUser, null, false);
            }
        }
        private async Task DetailScrap(HistoryScrapDto historyScrap)
        {
            var scrapHistory = new HandleScrapDto
            {
                Id = historyScrap.Id,
                Title = historyScrap.Title,
                Status = historyScrap.Status,
                Description = historyScrap.Description,
                CreatedDate = historyScrap.CreatedDate,
                CreatedById = historyScrap.CreatedById,
                FactoryId = historyScrap.FactoryId,
                FactoryName = historyScrap.FactoryName,
                DepartmentId = historyScrap.DepartmentId,
                DepartmentName = historyScrap.DepartmentName,
                HistoryDetails = historyScrap.HistoryDetails,
                Approvals = historyScrap.Approvals
            };

            if (registerScrapModal != null)
            {
                await registerScrapModal.ShowModal(CurrenUser, scrapHistory, isEdit: false, isDetail: true);
            }
        }

        #endregion
        #region Helper
        private PlantType? ConvertStringToPlantType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return input.Trim().ToUpper() switch
            {
                "9A61" => PlantType.REF,
                "9A62" => PlantType.WM,
                "9A60" => PlantType.PL,
                _ => null
            };
        }
        #endregion
        #region Excel
        //private async Task HandleImportExcelFile(InputFileChangeEventArgs e)
        //{
        //    try
        //    {
        //        var file = e.File;
        //        if (file == null)
        //            return;

        //        using var ms = await file.ToMemoryStreamAsync();

        //        var parsedData = await ExcelImportHelper.ReadFromExcelAsync<ImportExcelDto>(
        //            ms,
        //            (sheet, row) =>
        //            {
        //                // Bỏ qua dòng 1 vì là dòng tiêu đề (Title)
        //                if (row == 1)
        //                    return null;

        //                if (string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text))
        //                    return null;

        //                var plant = ConvertStringToPlantType(sheet.Cells[row, 7].Text);
        //                decimal.TryParse(sheet.Cells[row, 8].Text?.Trim(), out var weightValue);

        //                DateTime? startDateValue = null;
        //                if (DateTime.TryParse(sheet.Cells[row, 9].Text?.Trim(), out var startDate))
        //                {
        //                    startDateValue = startDate;
        //                }

        //                DateTime? endDateValue = null;
        //                if (DateTime.TryParse(sheet.Cells[row, 10].Text?.Trim(), out var endDate))
        //                {
        //                    endDateValue = endDate;
        //                }

        //                bool.TryParse(sheet.Cells[row, 11].Text?.Trim(), out var isActive);

        //                return new ImportExcelDto
        //                {
        //                    BOMHead = sheet.Cells[row, 1].Text?.Trim(),
        //                    BOMDescription = sheet.Cells[row, 2].Text?.Trim(),
        //                    Code = sheet.Cells[row, 3].Text?.Trim(),
        //                    Name = sheet.Cells[row, 4].Text?.Trim(),
        //                    TYPE = sheet.Cells[row, 5].Text?.Trim(),
        //                    Unit = sheet.Cells[row, 6].Text?.Trim(),
        //                    Plant = plant,
        //                    WeightValue = weightValue,
        //                    StartDate = DateTime.Now,
        //                    EndDate = null,
        //                    IsActive = true,
        //                    CreatedBy = CurrenUser?.Id ?? Guid.Empty,

        //                };
        //            });

        //        ImportedData = parsedData.Where(x => x != null).ToList();

        //        var response = await Http.PostAsJsonAsync(
        //           "api/material/import-bom",
        //           ImportedData);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            await Message.Success($"Import thành công {ImportedData.Count} dòng");
        //        }
        //        else
        //        {
        //            var error = await response.Content.ReadAsStringAsync();
        //            await Message.Error(error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Message.Error(ex.Message);
        //    }
        //}
        #endregion
    }
}
