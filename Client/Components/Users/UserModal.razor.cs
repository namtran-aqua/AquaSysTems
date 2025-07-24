using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Users
{
    public partial class UserModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private UserDto CurrenUser { get; set; }
    
        private bool IsModalVisible = false;
        private Form<CreatedAndUpdateUserDto> formRef;
        private CreatedAndUpdateUserDto CreatedUserDto = new CreatedAndUpdateUserDto();
        private bool IsEdit { get;set; }
        #endregion
        #region Innit
        public async Task ShowModelAsync(bool isEdit, CreatedAndUpdateUserDto createdAndUpdateUserDto,UserDto currenUser)
        {
            IsEdit =  isEdit;
            CurrenUser = currenUser;
            if (IsEdit)
            {
                CreatedUserDto = createdAndUpdateUserDto;
            }
            else 
            {
                CreatedUserDto = new();
            }
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }

        #endregion
        #region Action
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return; // Nếu không hợp lệ thì không lưu
            }
            CreatedUserDto.FullName = $"{CreatedUserDto.LastName} {CreatedUserDto.FirstName}";
            if (IsEdit)
            {
                CreatedUserDto.UpdateBy = CurrenUser.FullName;
                CreatedUserDto.UpdatedTime = DateTime.Now;
                await UpdateAsync(CreatedUserDto);
            }
            else
            {
                CreatedUserDto.CreatedTime = DateTime.Now;
                CreatedUserDto.CreatedBy = CurrenUser.FullName;
                await CreatedAsync(CreatedUserDto);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region HandleData
        private async Task CreatedAsync(CreatedAndUpdateUserDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/user/create", CreatedUserDto);
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
        private async Task UpdateAsync(CreatedAndUpdateUserDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/user/update", updateUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Cập nhật thành công!");
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
