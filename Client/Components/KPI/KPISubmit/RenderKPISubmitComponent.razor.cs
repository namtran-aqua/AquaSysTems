using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Common.ConvertNumber;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.CeilingLevel;
using AquaSolution.Shared.KPI.DealineManagement;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.KPI.KPISubmit
{
    public partial class RenderKPISubmitComponent
    {
        #region DECLARATION
        [Parameter]
        public List<HandleActualDto> HandleActual { get; set; } = new();
        [Parameter]
        public string Title { get; set; } =string.Empty;
        #endregion

    }
}
