using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.AuthModels
{
    public class ChangePassRequest
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
