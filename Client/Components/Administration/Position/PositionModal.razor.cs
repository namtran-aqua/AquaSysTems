using AntDesign;
using AquaSolution.Shared.Enum;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AquaSolution.Shared.Position;

namespace AquaSolution.Client.Components.Administration.Position
{
    public partial class PositionModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private PositionDto _model = new PositionDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<PositionDto> formRef = new();
        private bool IsEdit {  get; set; }
        private string Title { get; set; }
        private List<PositionType> ListPositionType = new List<PositionType>();
        #endregion
        #region Innit
        public async Task Showmodal(PositionDto positionDto,bool isEdit)
        {
            IsEdit = isEdit;
            if (IsEdit)
            {
                Title = "Edit Position";
            }
            else
            {
                Title = "Created Position";
            }
            _model = positionDto;
            GetPositionType();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private void GetPositionType()
        {
            ListPositionType = Enum.GetValues(typeof(PositionType))
                    .Cast<PositionType>()
                    .ToList();
            if (_model != null)
            {
                PositionType = ListPositionType
                         .FirstOrDefault(x => x == _model.PositionType);
            }
            else
            {
                PositionType = ListPositionType
                        .First();
            }
        }
        #endregion
        #region Action
        private void HandleCancel() => IsModalVisible = false;

        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            if (IsEdit)
            {
                _model.PositionType = PositionType;
                await UpdateAsync(_model);
            }
            else
            {
                _model.PositionType = PositionType;
                await CreatedAsync(_model);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        private PositionType _positionType;
        private PositionType PositionType
        {
            get => _positionType;
            set
            {
                if (_positionType != value)
                {
                    _positionType = value;
                }

            }
        }
        #endregion
        #region Handle Data
        private async Task CreatedAsync(PositionDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/position/create", createdUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        private async Task UpdateAsync(PositionDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/position/update", updateUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Update successfully!");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        #endregion
    }
}
