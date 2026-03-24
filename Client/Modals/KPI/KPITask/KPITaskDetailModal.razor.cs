using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.KPI.KPITasks;
using Microsoft.AspNetCore.Components;


namespace AquaSolution.Client.Modals.KPI.KPITask
{
    public partial class KPITaskDetailModal
    {
        private KPITaskDto Detail = new();
        private bool IsModalVisible {  get; set; }=false;
        private List<AttachmentDto> Attachment = new();
        [Inject] private HttpClient Http { get; set; }
        public async Task ShowModal(KPITaskDto kpiTaskDto)
        {
            Detail = kpiTaskDto;
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
     
        private async Task Close()
        {
            IsModalVisible = false;
            await InvokeAsync(StateHasChanged);
        }
       
    }
}
