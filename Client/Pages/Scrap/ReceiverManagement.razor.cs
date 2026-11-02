using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Factory;
using AquaSolution.Shared.ScrapManagement.Receivers;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Scrap
{
    public partial class ReceiverManagement
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IMessageService Message { get; set; } = default!;

        private UserDto? CurrentUser { get; set; }
        private List<ScrapReceiverDto> _receivers = new();
        private List<ScrapReceiverDto> _filteredReceivers = new();
        private List<FactoryDto> _factoryList = new();
        private List<UserDto> _userList = new();

        private string? _filterFactory;
        private bool _loading = false;
        private bool _saving = false;
        private bool _isModalVisible = false;
        private bool _isEdit = false;

        private SaveScrapReceiverDto _editingModel = new();

        protected override async Task OnInitializedAsync()
        {
            var userHelper = new CurrenUser(Http, AuthStateProvider);
            CurrentUser = await userHelper.LoadCurrenUser();

            await Task.WhenAll(LoadReceivers(), LoadFactories(), LoadUsers());
        }

        private async Task LoadReceivers()
        {
            try
            {
                _loading = true;
                var res = await Http.GetFromJsonAsync<List<ScrapReceiverDto>>("api/scrap/get-receivers");
                _receivers = res ?? new();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi tải danh sách người nhận hàng: {ex.Message}");
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        private async Task LoadFactories()
        {
            try
            {
                var res = await Http.GetFromJsonAsync<List<FactoryDto>>("api/Factory/get-all");
                _factoryList = res ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading factories: {ex.Message}");
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                var res = await Http.GetFromJsonAsync<List<UserDto>>("api/User/get-all");
                _userList = (res ?? new()).Where(u => u != null).OrderBy(u => u.FullName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private void OnFilterFactoryChanged(string? factory)
        {
            _filterFactory = factory;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(_filterFactory))
            {
                _filteredReceivers = _receivers.ToList();
            }
            else
            {
                _filteredReceivers = _receivers
                    .Where(x => string.Equals(x.FactoryName, _filterFactory, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private void OpenCreateModal()
        {
            _isEdit = false;
            _editingModel = new SaveScrapReceiverDto
            {
                FactoryId = _factoryList.FirstOrDefault()?.Id ?? Guid.Empty,
                IsActive = true
            };
            _isModalVisible = true;
        }

        private void OpenEditModal(ScrapReceiverDto row)
        {
            _isEdit = true;
            _editingModel = new SaveScrapReceiverDto
            {
                Id = row.Id,
                FactoryId = row.FactoryId,
                UserId = row.UserId,
                Note = row.Note,
                IsActive = row.IsActive
            };
            _isModalVisible = true;
        }

        private async Task SaveAsync()
        {
            if (_editingModel.FactoryId == Guid.Empty)
            {
                await Message.Warning("Vui lòng chọn Factory!");
                return;
            }

            if (_editingModel.UserId == Guid.Empty)
            {
                await Message.Warning("Vui lòng chọn Nhân viên nhận hàng!");
                return;
            }

            try
            {
                _saving = true;
                var res = await Http.PostAsJsonAsync("api/scrap/save-receiver", _editingModel);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Lưu cấu hình thành công!");
                    _isModalVisible = false;
                    await LoadReceivers();
                }
                else
                {
                    await Message.Error($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _saving = false;
            }
        }

        private async Task DeleteAsync(Guid id)
        {
            try
            {
                var res = await Http.DeleteAsync($"api/scrap/delete-receiver/{id}");
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Xóa cấu hình thành công!");
                    await LoadReceivers();
                }
                else
                {
                    await Message.Error($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
        }
    }
}
