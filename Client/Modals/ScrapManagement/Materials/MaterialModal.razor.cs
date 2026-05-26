using AntDesign;
using AquaSolution.Shared.ScrapManagement.Materials;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Modals.ScrapManagement.Materials
{
    public partial class MaterialModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IMessageService Message { get; set; }

        [Parameter] public EventCallback OnSaved { get; set; }

        private Form<UpdateWeightDto> formRef;
        private bool IsModalVisible { get; set; } = false;

        private bool isSaving { get; set; } = false;

        private MaterialDto? SelectedMaterial { get; set; }
        private UpdateWeightDto UpdateWeight { get; set; } = new UpdateWeightDto();
        #endregion

        #region Init
        public async Task ShowModal(MaterialDto? material = null, Guid? currentUserId = null)
        {
            SelectedMaterial = material;
            IsModalVisible = true;

            if (material != null)
            {
                UpdateWeight = new UpdateWeightDto
                {
                    MaterialId = material.Id,
                    WeightId = material.WeightId ?? Guid.Empty,
                    WeightValue = material.WeightValue ,
                    StartDate = material.StartDate ?? DateTime.Now,
                    CreatedBy = currentUserId ?? Guid.Empty
                };
            }
            else
            {
                UpdateWeight = new UpdateWeightDto
                {
                    StartDate = DateTime.Now,
                    CreatedBy = currentUserId ?? Guid.Empty
                };
            }

            StateHasChanged();
        }
        #endregion

        #region Actions
        private async Task SaveAsync()
        {
            if (isSaving) return;

            try
            {
                isSaving = true;
                
                var response = await Http.PostAsJsonAsync("api/material/update-weight", UpdateWeight);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Save successfully!");
                    IsModalVisible = false;
                    if (OnSaved.HasDelegate)
                    {
                        await OnSaved.InvokeAsync();
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Save failed: {error}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"System error: {ex.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
        }
        #endregion
    }
}
