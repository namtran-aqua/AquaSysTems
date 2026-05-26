using AntDesign;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Shared.ScrapManagement.Scrap;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.ScrapManagement.Scrap
{
    public enum ScrapModalMode { Create, Detail, Approval, Confirm }

    public partial class RegisterScrapModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IMessageService Message { get; set; } = default!;

        [Parameter] public EventCallback OnActionCompleted { get; set; }

        private UserDto? CurrenUser { get; set; }
        private HandleScrapDto? HandleScrap { get; set; }
        private ScrapModalMode Mode { get; set; } = ScrapModalMode.Create;

        private bool IsModalVisible { get; set; } = false;
        private bool IsRejectModalVisible { get; set; } = false;
        private string? RejectComment { get; set; }
        private bool _loading = false;
        private bool _submitting = false;

        private List<MaterialDto> _materials = new();
        private List<HistoryDetailScrapDto> _details = new();

        // Confirm fields
        private decimal _confirmAmount { get; set; } = 0;
        private ConfirmationStatusType _confirmStatus { get; set; } = ConfirmationStatusType.Received;
        private string? _confirmNote { get; set; }

        private string ModalTitle => Mode switch
        {
            ScrapModalMode.Create => "Tạo phiếu Scrap",
            ScrapModalMode.Detail => "Chi tiết phiếu Scrap",
            ScrapModalMode.Approval => "Duyệt phiếu Scrap",
            ScrapModalMode.Confirm => "Xác nhận nhận hàng",
            _ => "Scrap"
        };
        #endregion

        #region Init
        public async Task ShowModal(UserDto user, HandleScrapDto? handleScrap = null,
            bool isEdit = false, bool isDetail = false,
            bool isApproval = false, bool isConfirm = false)
        {
            CurrenUser = user;

            if (isConfirm) Mode = ScrapModalMode.Confirm;
            else if (isApproval) Mode = ScrapModalMode.Approval;
            else if (isDetail) Mode = ScrapModalMode.Detail;
            else Mode = ScrapModalMode.Create;

            if (Mode != ScrapModalMode.Create && handleScrap != null)
            {
                HandleScrap = handleScrap;
                _details = HandleScrap.HistoryDetails ?? new();

                // Pre-fill confirm amount nếu có
                if (Mode == ScrapModalMode.Confirm)
                {
                    _confirmAmount = HandleScrap.ConfirmAmount ?? HandleScrap.TotalAmount ?? 0;
                    _confirmStatus = HandleScrap.ConfirmationStatusType;
                    _confirmNote = HandleScrap.Notes;
                }
            }
            else
            {
                HandleScrap = new HandleScrapDto
                {
                    Title = $"SCRAP - {user.FactoryName} - {user.DepartmentName} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}".ToUpper(),
                    CreatedById = user.Id,
                    HistoryDetails = new(),
                    DepartmentId = user.DepartmentId ?? Guid.Empty,
                    FactoryId = user.FactoryId ?? Guid.Empty,
                    CreatedDate = DateTime.Now
                };
                _details = HandleScrap.HistoryDetails;
            }

            await LoadMaterials();
            IsModalVisible = true;
            StateHasChanged();
        }

        private async Task LoadMaterials()
        {
            try
            {
                _loading = true;
                var result = await Http.GetFromJsonAsync<List<MaterialDto>>("api/Material/get-all-materials");
                _materials = result ?? new();
            }
            catch
            {
                await Message.Error("Không thể tải danh sách vật tư!");
            }
            finally
            {
                _loading = false;
            }
        }
        #endregion

        #region Material Actions
        private void OnMaterialIdChanged(HistoryDetailScrapDto row, Guid id)
        {
            row.MaterialId = id;
            var mat = _materials.FirstOrDefault(m => m.Id == id);
            if (mat is null)
            {
                row.Code = null; row.Name = null; row.BOMHead = null;
                row.BOMDescription = null; row.Unit = null; row.TYPE = null;
                row.Plant = default; row.Weight = default;
            }
            else
            {
                row.Code = mat.Code; row.Name = mat.Name; row.BOMHead = mat.BOMHead;
                row.BOMDescription = mat.BOMDescription; row.Unit = mat.Unit;
                row.TYPE = mat.TYPE; row.Plant = mat.Plant; row.Weight = mat.WeightValue;
            }
            StateHasChanged();
        }

        private void AddRow()
        {
            _details.Add(new HistoryDetailScrapDto());
            StateHasChanged();
        }

        private void RemoveRow(HistoryDetailScrapDto row)
        {
            _details.Remove(row);
            StateHasChanged();
        }
        #endregion

        #region Submit Actions
        private async Task SubmitAsync()
        {
            if (!_details.Any())
            {
                await Message.Warning("Vui lòng thêm ít nhất 1 dòng detail!");
                return;
            }
            if (_details.Any(d => d.Quantity <= 0))
            {
                await Message.Warning("Vui lòng nhập Quantity lớn hơn 0 cho tất cả các dòng!");
                return;
            }

            HandleScrap!.HistoryDetails = _details;

            try
            {
                _submitting = true;
                var response = await Http.PostAsJsonAsync("api/scrap/create-scrap", HandleScrap);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Tạo phiếu scrap thành công!");
                    await OnActionCompleted.InvokeAsync();
                    Close();
                }
                else
                {
                    await Message.Error($"Thất bại: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _submitting = false;
            }
        }

        private async Task ApproveAction()
        {
            try
            {
                _submitting = true;
                var req = new ApprovalActionDto
                {
                    HistoryScrapId = HandleScrap!.Id,
                    ActionBy = CurrenUser!.Id,
                    IsApproved = true
                };
                var res = await Http.PostAsJsonAsync("api/scrap/action-approval", req);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Đã duyệt phiếu thành công!");
                    await OnActionCompleted.InvokeAsync();
                    Close();
                }
                else
                {
                    await Message.Error("Lỗi: " + await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Message.Error("Lỗi: " + ex.Message);
            }
            finally
            {
                _submitting = false;
            }
        }

        private void RejectAction()
        {
            RejectComment = string.Empty;
            IsModalVisible = false;
            IsRejectModalVisible = true;
            StateHasChanged();
        }

        private void CloseRejectModal()
        {
            IsRejectModalVisible = false;
            IsModalVisible = true;
            StateHasChanged();
        }

        private async Task ConfirmRejectAction()
        {
            if (string.IsNullOrWhiteSpace(RejectComment))
            {
                await Message.Warning("Vui lòng nhập lý do từ chối!");
                return;
            }
            try
            {
                _submitting = true;
                var req = new ApprovalActionDto
                {
                    HistoryScrapId = HandleScrap!.Id,
                    ActionBy = CurrenUser!.Id,
                    IsApproved = false,
                    Comment = RejectComment
                };
                var res = await Http.PostAsJsonAsync("api/scrap/action-approval", req);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Đã từ chối phiếu thành công!");
                    IsRejectModalVisible = false;
                    await OnActionCompleted.InvokeAsync();
                }
                else
                {
                    await Message.Error("Lỗi: " + await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Message.Error("Lỗi: " + ex.Message);
            }
            finally
            {
                _submitting = false;
            }
        }

        private async Task ConfirmAction()
        {
            if (_confirmAmount <= 0)
            {
                await Message.Warning("Vui lòng nhập số lượng thực tế nhận!");
                return;
            }
            try
            {
                _submitting = true;
                var req = new ConfirmScrapDto
                {
                    HistoryScrapId = HandleScrap!.Id,
                    ConfirmerId = CurrenUser!.Id,
                    ConfirmAmount = _confirmAmount,
                    ConfirmationStatusType = _confirmStatus,
                    Notes = _confirmNote
                };
                var res = await Http.PostAsJsonAsync("api/scrap/confirm-scrap", req);
                if (res.IsSuccessStatusCode)
                {
                    await Message.Success("Xác nhận nhận hàng thành công!");
                    await OnActionCompleted.InvokeAsync();
                    Close();
                }
                else
                {
                    await Message.Error("Lỗi: " + await res.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Message.Error("Lỗi: " + ex.Message);
            }
            finally
            {
                _submitting = false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            IsRejectModalVisible = false;
            _details = new();
            _confirmAmount = 0;
            _confirmNote = null;
            StateHasChanged();
        }
        #endregion
    }
}