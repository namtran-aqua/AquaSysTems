using AntDesign;
using AntDesign.Select.Internal;
using AquaSolution.Shared.Administration.UserManagements;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace AquaSolution.Client.Components.ManageMedicalRooms.RequestClinics
{
    public partial class MyRequestClinicModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsVisible { get; set; }
        private bool IsView { get; set; }
        private UserDto CurrenUser { get; set; }
        private bool IsPrescription { get; set; }
        private string? DepartmentName { get; set; }
        private List<UserDto> Listusers = new();
        private List<UserSelectedDto> ListSelectedcUsers = new();
        private List<PurposeType> ListPurposeType = new List<PurposeType>();
        private HandleMyRequestClinicDto HandleMyRequestClinic { get; set; } = new HandleMyRequestClinicDto();
        private Form<HandleMyRequestClinicDto> formRef;
        private bool IsEdit { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        #endregion
        #region Innit
        public async Task ShowModalAsync(ParamShowModal paramShowModal)
        {
            HandleMyRequestClinic = new();
            CurrenUser = paramShowModal.currenUser;
            Listusers = paramShowModal.users;
            IsEdit = paramShowModal.IsEdit;
            HandleMyRequestClinic = paramShowModal.handleMyRequestClinicDto;
            DepartmentName = CurrenUser.DepartmentName;
            await LoadUserByDepartment();
            await LoadPurposeType();
            IsVisible = true;
            await InvokeAsync(StateHasChanged);

        }
        private Task LoadUserByDepartment()
        {
            ListSelectedcUsers = new();
            var listUser = Listusers.ToList();
            if (listUser.Any())
            {
                foreach (var user in listUser)
                {
                    ListSelectedcUsers.Add(new UserSelectedDto
                    {
                        Id = user.Id,
                        Name = user.FullName,
                        WorkDayId = user.WorkDayId,
                        DepartmentId = user.DepartmentId,
                        ManagerId = user.ManagerId,
                        ManagerName = user.ManagerName,
                        WorkDayManager = user.ManagerWorkDay,
                        Email = user.Email
                    });
                }

                UserValue = ListSelectedcUsers.First(x => x.Id == CurrenUser.Id);
            }

            return Task.CompletedTask;
        }
        private UserSelectedDto _userValue;
        private UserSelectedDto UserValue
        {
            get => _userValue;
            set
            {
                if (_userValue != value)
                {
                    _userValue = value;
                    HandleMyRequestClinic.WorkDayUserRequestId = value.WorkDayId;
                    HandleMyRequestClinic.UserRequestName = value.Name;
                    HandleMyRequestClinic.UserRequestId = value.Id;
                    HandleMyRequestClinic.ManagerId = value.ManagerId;
                    HandleMyRequestClinic.ManagerName = value.ManagerName;
                    HandleMyRequestClinic.WorkDayManager = value.WorkDayManager;
                    HandleMyRequestClinic.EmailRequestter = value.Email;

                }
            }
        }
        private PurposeType _purposeType;
        private PurposeType PurposeType
        {
            get => _purposeType;
            set
            {
                if (_purposeType != value)
                {
                    _purposeType = value;
                    if (value == PurposeType.MedicationRequest)
                    {
                        IsPrescription = true;
                    }
                    else
                    {
                        IsPrescription = false;
                    }
                    HandleMyRequestClinic.PurposeType = value;
                    StateHasChanged();
                }

            }
        }
        private string GetPurposeTypeLabel(PurposeType type)
        {
            return type switch
            {
                PurposeType.SickLeave => "SickLeave/Nghỉ Bệnh",
                PurposeType.MedicationRequest => "MedicationRequest/Đề nghị cấp thuốc",
                _ => type.ToString()
            };
        }
        private async Task LoadPurposeType()
        {
            ListPurposeType = Enum.GetValues(typeof(PurposeType))
                    .Cast<PurposeType>()
                    .ToList();
            PurposeType = ListPurposeType
                    .First();
            if (HandleMyRequestClinic != null)
            {
                PurposeType = ListPurposeType
                         .FirstOrDefault(x => x == HandleMyRequestClinic.PurposeType);
            }
            else
            {
                PurposeType = ListPurposeType
                        .First();
            }
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var validation = formRef.Validate();
            if (!validation) { return; }
            if (!IsEdit)
            {
                await CreatedAsync();
            }
            else
            {
                await UpdatedAsync();
            }
            await OnSaved.InvokeAsync();
            IsVisible = false;
            StateHasChanged();
        }
        private void Close()
        {
            IsVisible = false;
            StateHasChanged();

        }
        #endregion
        #region HandleData
        private async Task CreatedAsync()
        {
            HandleMyRequestClinic.Id = Guid.NewGuid();
            HandleMyRequestClinic.Status = StatusClinicType.New;
            HandleMyRequestClinic.CreatedBy = CurrenUser.Id;
            HandleMyRequestClinic.CreatedName = CurrenUser.FullName;
            HandleMyRequestClinic.CreatedWorkDay = CurrenUser.WorkDayId;

            var response = await httpClient.PostAsJsonAsync("api/myrequestclinic/create-request", HandleMyRequestClinic);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Success!");
                IsVisible = false;
                await OnSaved.InvokeAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(errorContent);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("message", out var messageProp))
                    {
                        var message = messageProp.GetString();
                        await Message.Error(message);
                    }
                    else
                    {
                        await Message.Error("An unexpected error occurred.");
                    }
                }
                catch
                {
                    await Message.Error("An unexpected error occurred.");
                }
            }
        }
        private async Task UpdatedAsync()
        {

        }
        #endregion
    }
}
