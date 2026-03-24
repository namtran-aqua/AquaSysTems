using AntDesign;
using AquaSolution.Shared.Enum;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using AquaSolution.Shared.Factory;

namespace AquaSolution.Client.Modals.Administration.Factory
{
    public partial class FactoryModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private FactoryDto _model = new FactoryDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<FactoryDto> formRef = new();
        private bool IsEdit {  get; set; }
        private string Title { get; set; }
        private List<FactoryType> ListFactoryType = new List<FactoryType>();
        #endregion
        #region Innit
        public async Task Showmodal(FactoryDto factoryDto,bool isEdit)
        {
            IsEdit = isEdit;
            if (IsEdit)
            {
                Title = "Edit Factory";
            }
            else
            {
                Title = "Created Factory";
            }
            _model = factoryDto;
            GetFactoryType();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private void GetFactoryType()
        {
            ListFactoryType = Enum.GetValues(typeof(FactoryType))
                    .Cast<FactoryType>()
                    .ToList();
            if (_model != null)
            {
                FactoryType = ListFactoryType
                         .FirstOrDefault(x => x == _model.FactoryType);
            }
            else
            {
                FactoryType = ListFactoryType
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
                _model.FactoryType = FactoryType;
                await UpdateAsync(_model);
            }
            else
            {
                _model.FactoryType = FactoryType;
                await CreatedAsync(_model);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        private FactoryType _factoryType;
        private FactoryType FactoryType
        {
            get => _factoryType;
            set
            {
                if (_factoryType != value)
                {
                    _factoryType = value;
                }

            }
        }
        #endregion
        #region Handle Data
        private async Task CreatedAsync(FactoryDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/factory/create", createdUserDto);
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
        private async Task UpdateAsync(FactoryDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/factory/update", updateUserDto);
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
