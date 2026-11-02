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
        private List<WasteItemDto> _wasteItems = new();
        private List<string> _wasteCategories = new();
        private List<RequestApprovalDto> _previewApprovals = new();

        private string? SelectedResponsibilityOption { get; set; }
        private string? CustomResponsibility { get; set; }
        private string? _previewImageUrl;
        private static readonly string[] StandardResponsibilities = new[]
        {
            "Lỗi nhà cung cấp",
            "Thay đổi kỹ thuật",
            "Lỗi sản xuất",
            "Thiếu hụt"
        };



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

                if (!string.IsNullOrWhiteSpace(HandleScrap.Responsibility))
                {
                    if (StandardResponsibilities.Contains(HandleScrap.Responsibility))
                    {
                        SelectedResponsibilityOption = HandleScrap.Responsibility;
                        CustomResponsibility = string.Empty;
                    }
                    else
                    {
                        SelectedResponsibilityOption = "Khác";
                        CustomResponsibility = HandleScrap.Responsibility;
                    }
                }
                else
                {
                    SelectedResponsibilityOption = null;
                    CustomResponsibility = string.Empty;
                }
            }
            else
            {
                SelectedResponsibilityOption = null;
                CustomResponsibility = string.Empty;
                var defaultSectionId = user.Sections?.FirstOrDefault()?.Id;
                var sectionPart = user.Sections?.FirstOrDefault()?.Name;
                var titleParts = new[] { "SCRAP", user.FactoryName, user.DepartmentName, sectionPart, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                HandleScrap = new HandleScrapDto
                {
                    Title = string.Join(" - ", titleParts).ToUpper(),
                    CreatedById = user.Id,
                    HistoryDetails = new(),
                    DepartmentId = user.DepartmentId ?? Guid.Empty,
                    FactoryId = user.FactoryId ?? Guid.Empty,
                    SectionId = defaultSectionId,
                    CreatedDate = DateTime.Now
                };
                _details = HandleScrap.HistoryDetails;
            }

            await LoadMaterials();
            if (Mode == ScrapModalMode.Create)
            {
                await LoadPreviewFlow();
            }
            if (Mode == ScrapModalMode.Confirm || isConfirm)
            {
                await LoadWasteItems();
            }
            IsModalVisible = true;
            StateHasChanged();
        }

        private async Task LoadPreviewFlow()
        {
            if (CurrenUser == null || CurrenUser.FactoryId == null || CurrenUser.DepartmentId == null) return;
            try
            {
                var factoryId = HandleScrap?.FactoryId ?? CurrenUser.FactoryId.Value;
                var deptId = HandleScrap?.DepartmentId ?? CurrenUser.DepartmentId.Value;
                var secId = HandleScrap?.SectionId;
                var creatorId = CurrenUser.Id;

                var url = $"api/scrap/preview-flow?factoryId={factoryId}&departmentId={deptId}&creatorId={creatorId}";
                if (secId.HasValue) url += $"&sectionId={secId.Value}";

                var res = await Http.GetFromJsonAsync<List<RequestApprovalDto>>(url);
                _previewApprovals = res ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading preview flow: {ex.Message}");
            }
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

        private async Task LoadWasteItems()
        {
            try
            {
                if (!_wasteItems.Any())
                {
                    var result = await Http.GetFromJsonAsync<List<WasteItemDto>>("api/scrap/get-all-waste-items");
                    _wasteItems = result ?? new();
                    _wasteCategories = _wasteItems
                        .Select(x => x.Category)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                }
            }
            catch
            {
                await Message.Error("Không thể tải danh mục phế liệu & chất thải!");
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

        private void OnWasteTypeChanged(HistoryDetailScrapDto row, string? type)
        {
            row.WasteType = type;
            row.WasteCode = null;
            row.WasteName = null;
            StateHasChanged();
        }

        private void OnWasteCodeChanged(HistoryDetailScrapDto row, string? code)
        {
            row.WasteCode = code;
            var item = _wasteItems.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
            row.WasteName = item?.Name;
            StateHasChanged();
        }

        private async Task OnSectionChanged(AquaSolution.Shared.Administration.Sections.SectionDto section)
        {
            if (Mode == ScrapModalMode.Create && HandleScrap != null && CurrenUser != null)
            {
                var sectionName = section?.Name ?? CurrenUser.SectionName;
                var parts = new[] { "SCRAP", CurrenUser.FactoryName, CurrenUser.DepartmentName, sectionName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                HandleScrap.Title = string.Join(" - ", parts).ToUpper();
                HandleScrap.SectionId = section?.Id;
                await LoadPreviewFlow();
                StateHasChanged();
            }
        }

        private void OnResponsibilityChanged(string val)
        {
            SelectedResponsibilityOption = val;
            if (val == "Khác")
            {
                HandleScrap!.Responsibility = CustomResponsibility;
            }
            else
            {
                HandleScrap!.Responsibility = val;
            }
            StateHasChanged();
        }

        private void PreviewImage(string? url)
        {
            _previewImageUrl = url;
            StateHasChanged();
        }

        private async Task HandleImageUpload(HistoryDetailScrapDto row, Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                await Message.Warning("Vui lòng chọn tệp hình ảnh (.jpg, .png, .webp, ...)");
                return;
            }

            if (file.Size > 15 * 1024 * 1024)
            {
                await Message.Warning("Dung lượng ảnh tối đa 15MB!");
                return;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream(15 * 1024 * 1024);
                using var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);

                var response = await Http.PostAsync("api/upload/scrap-image", content);
                if (response.IsSuccessStatusCode)
                {
                    var url = await response.Content.ReadAsStringAsync();
                    url = url.Trim('"', ' ');
                    row.ImageUrl = url;
                    await Message.Success("Tải ảnh lên thành công!");
                    StateHasChanged();
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Tải ảnh thất bại: {err}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi khi tải ảnh: {ex.Message}");
            }
        }
        #endregion

        #region Submit Actions
        private async Task SubmitAsync()
        {
            // Chỉ bắt buộc chọn Section nếu user được gán section
            // Nếu user không có section nào → phòng ban không phân section, cho phép tạo
            var userHasSections = CurrenUser?.Sections != null && CurrenUser.Sections.Any();
            if (userHasSections && HandleScrap!.SectionId == null)
            {
                await Message.Warning("Vui lòng chọn Section!");
                return;
            }

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

            if (SelectedResponsibilityOption == "Khác")
            {
                HandleScrap!.Responsibility = CustomResponsibility?.Trim();
            }
            else
            {
                HandleScrap!.Responsibility = SelectedResponsibilityOption?.Trim();
            }

            if (string.IsNullOrWhiteSpace(HandleScrap!.Responsibility))
            {
                await Message.Warning("Vui lòng chọn hoặc nhập phần Trách nhiệm!");
                return;
            }

            HandleScrap.HistoryDetails = _details;

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

        private async Task ResubmitAsync()
        {
            if (HandleScrap == null) return;

            if (string.IsNullOrWhiteSpace(SelectedResponsibilityOption))
            {
                await Message.Warning("Vui lòng chọn hoặc nhập phần Trách nhiệm!");
                return;
            }

            if (SelectedResponsibilityOption == "Khác")
            {
                HandleScrap.Responsibility = CustomResponsibility?.Trim();
            }
            else
            {
                HandleScrap.Responsibility = SelectedResponsibilityOption?.Trim();
            }

            if (string.IsNullOrWhiteSpace(HandleScrap.Responsibility))
            {
                await Message.Warning("Vui lòng chọn hoặc nhập phần Trách nhiệm!");
                return;
            }

            if (_details == null || !_details.Any())
            {
                await Message.Warning("Vui lòng thêm ít nhất một dòng vật tư!");
                return;
            }

            if (_details.Any(d => d.MaterialId == Guid.Empty))
            {
                await Message.Warning("Có dòng chưa chọn vật tư!");
                return;
            }

            if (_details.Any(d => d.Quantity <= 0))
            {
                await Message.Warning("Số lượng phải lớn hơn 0!");
                return;
            }

            try
            {
                _submitting = true;
                HandleScrap.HistoryDetails = _details;
                var response = await Http.PostAsJsonAsync("api/scrap/resubmit-scrap", HandleScrap);
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Cập nhật và nộp lại thành công!");
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
            if (_details.Any(d => string.IsNullOrWhiteSpace(d.WasteType)))
            {
                await Message.Warning("Vui lòng chọn Loại (Phế liệu / Chất thải) cho tất cả các dòng!");
                return;
            }

            if (_details.Any(d => string.IsNullOrWhiteSpace(d.WasteCode)))
            {
                await Message.Warning("Vui lòng chọn Mã phế liệu / Chất thải cho tất cả các dòng!");
                return;
            }

            if (_details.Any(d => (d.ConfirmAmount ?? 0) <= 0))
            {
                await Message.Warning("Vui lòng nhập thực nhận lớn hơn 0 cho tất cả chi tiết!");
                return;
            }

            if (_details.Any(d => d.ConfirmUnitType == ConfirmUnitType.Unit && (d.ConfirmAmount ?? 0) % 1 != 0))
            {
                await Message.Warning("Vui lòng nhập số nguyên cho các chi tiết nhận theo PCS!");
                return;
            }

            try
            {
                _submitting = true;
                var req = new ActionConfirmScrapDto
                {
                    HistoryScrapId = HandleScrap!.Id,
                    ConfirmerId = CurrenUser!.Id,
                    Details = _details.Select(d => new ConfirmDetailDto
                    {
                        HistoryDetailId = d.Id,
                        ConfirmAmount = d.ConfirmAmount ?? 0,
                        ConfirmUnitType = d.ConfirmUnitType,
                        ConfirmNote = d.ConfirmNote,
                        WasteType = d.WasteType,
                        WasteCode = d.WasteCode,
                        WasteName = d.WasteName
                    }).ToList()
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

        private (string StatusText, string Color, string CardBorder, string CardBg) GetStepStatusDisplay(RequestApprovalDto step, ScrapModalMode mode)
        {
            var title = step.Title?.Trim() ?? string.Empty;
            bool isCreator = step.Step == 1 || title.Equals("Người tạo", StringComparison.OrdinalIgnoreCase);
            bool isReceiver = title.Equals("Người nhận hàng", StringComparison.OrdinalIgnoreCase);
            bool isFinalApprover = title.Equals("Người xác nhận cuối", StringComparison.OrdinalIgnoreCase);
            bool isApprover = !isCreator && !isReceiver && !isFinalApprover;

            // 1. Khi đang tạo mới phiếu (chưa bấm tạo phiếu)
            if (mode == ScrapModalMode.Create)
            {
                if (isCreator)
                {
                    // Người tạo đang xử lý -> Màu xanh dương, status Pending
                    return ("Pending", "#1677ff", "1.5px solid #1677ff", "#ffffff");
                }
                else
                {
                    // Các bước sau chưa tới lượt -> Màu xám, status Pending
                    return ("Pending", "#bfbfbf", "0.5px solid #e8e8e8", "#fafafa");
                }
            }

            // 2. Nếu bước bị Rejected
            if (step.Status == StatusScrap.Rejected)
            {
                return ("Từ chối", "#ff4d4f", "1px solid #ff4d4f", "#fff1f0");
            }

            // 3. Nếu bước đã hoàn thành (Approved)
            if (step.Status == StatusScrap.Approved)
            {
                string text = "Approved";
                if (isCreator) text = "Created";
                else if (isReceiver) text = "Received";
                else if (isFinalApprover || isApprover) text = "Approved";

                return (text, "#52c41a", "1px solid #52c41a", "#ffffff");
            }

            // 4. Nếu bước đang xử lý (InterView - đang ở bước này)
            if (step.Status == StatusScrap.InterView)
            {
                string text = "Waiting for Approval";
                if (isCreator) text = "Pending";
                else if (isReceiver) text = "Pending";
                else if (isFinalApprover || isApprover) text = "Waiting for Approval";

                return (text, "#1677ff", "1.5px solid #1677ff", "#ffffff");
            }

            // 5. Mặc định chưa tới lượt (Pending)
            return ("Pending", "#bfbfbf", "0.5px solid #e8e8e8", "#fafafa");
        }

        private void Close()
        {
            IsModalVisible = false;
            IsRejectModalVisible = false;
            _previewImageUrl = null;
            _details = new();
            StateHasChanged();
        }
        #endregion
    }
}