
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace AquaSolution.Client.Components.ITSuport.RequestITSuport
{
    public partial class RequestITSuportDetailModal
    {
        private RequestSuportDto RequestSuport = new();
        private bool IsModalVisible {  get; set; }=false;
        private List<AttachmentDto> Attachment = new();
        [Inject] private HttpClient Http { get; set; }
        public async Task ShowModal(RequestSuportDto requestSuport)
        {
            RequestSuport = requestSuport;
            await LoadAttachment();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadAttachment()
        {
            var data = await Http.GetFromJsonAsync<List<AttachmentDto>>($"api/RequestITSuport/get-attechment/{RequestSuport.Id}");
            Attachment = data.ToList();
        }
        private string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{Math.Round(bytes / (1024.0 * 1024.0))} MB";
            return $"{Math.Round(bytes / 1024.0)} KB";
        }

        private async Task Close()
        {
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }
        private async Task Download(string filePath)
        {
            if (!filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = Navigation.BaseUri; // vd: https://localhost:7195/
                filePath = $"{baseUrl}{filePath.TrimStart('/')}";
            }

            await JSRuntime.InvokeVoidAsync("open_blank", filePath);
        }
    }
}
