using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AquaSolution.Shared.PasswordHelpers
{
    public class UserPersonalInfo
    {
        public string? WorkDayId { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static PasswordValidationResult Success() => new() { IsValid = true };
        public static PasswordValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    public static class PasswordValidator
    {
        private static readonly string[] KeyboardPatterns = new[]
        {
            "qwertyuiop",
            "poiuytrewq",
            "asdfghjkl",
            "lkjhgfdsa",
            "zxcvbnm",
            "mnbvcxz",
            "1234567890",
            "0987654321"
        };

        private static readonly string[] CommonWeakKeywords = new[]
        {
            "password", "p@ssword", "passw0rd", "admin", "administrator",
            "qwerty", "welcome", "aquavietnam", "haier", "itsm", "default"
        };

        public static PasswordValidationResult Validate(
            string? password,
            string? confirmPassword = null,
            string? oldPassword = null,
            UserPersonalInfo? personalInfo = null)
        {
            // 0. Null / Empty check
            if (string.IsNullOrWhiteSpace(password))
            {
                return PasswordValidationResult.Failure("Mật khẩu không được để trống.");
            }

            // 1. Length check: The password must be at least 10 characters long
            if (password.Length < 10)
            {
                return PasswordValidationResult.Failure("Mật khẩu phải có độ dài ít nhất 10 ký tự (The password must be at least 10 characters long).");
            }

            // 2. Complexity check: It must contain at least three of the following: uppercase letters, lowercase letters, numbers, and special characters
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            int categoriesCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
            if (categoriesCount < 3)
            {
                return PasswordValidationResult.Failure("Mật khẩu phải chứa ít nhất 3 trong 4 nhóm ký tự: chữ hoa (A-Z), chữ thường (a-z), chữ số (0-9) và ký tự đặc biệt (!@#$%^&*...).");
            }

            // 3. Prohibitions:
            // 3a. Repeated characters check (3 or more identical characters in a row: aaa, 111, @@@)
            for (int i = 0; i <= password.Length - 3; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                {
                    string repeated = password.Substring(i, 3);
                    return PasswordValidationResult.Failure($"Mật khẩu không được chứa các ký tự lặp lại liên tiếp ('{repeated}').");
                }
            }

            // Repeated 2-character patterns (e.g. ababab, 121212)
            if (Regex.IsMatch(password, @"(..)\1\1", RegexOptions.IgnoreCase))
            {
                return PasswordValidationResult.Failure("Mật khẩu không được chứa chuỗi lặp lại liên tiếp (ví dụ: 'ababab', '121212').");
            }

            // 3b. Consecutive characters check (3 or more consecutive chars in alphabetic, numeric, or keyboard order)
            if (HasConsecutiveCharacters(password, out string detectedSequence))
            {
                return PasswordValidationResult.Failure($"Mật khẩu không được chứa chuỗi ký tự liên tiếp ('{detectedSequence}', ví dụ: 123, abc, qwe...).");
            }

            // 3c. Weak passwords: Common weak dictionary keywords
            foreach (var weak in CommonWeakKeywords)
            {
                if (password.IndexOf(weak, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return PasswordValidationResult.Failure($"Mật khẩu không được chứa từ khóa thông dụng dễ đoán ('{weak}').");
                }
            }

            // 3d. Weak passwords related to personal information
            if (personalInfo != null)
            {
                var personalCheckResult = CheckPersonalInformation(password, personalInfo);
                if (!personalCheckResult.IsValid)
                {
                    return personalCheckResult;
                }
            }

            // 4. Compare with old password if provided
            if (!string.IsNullOrEmpty(oldPassword) && string.Equals(password, oldPassword))
            {
                return PasswordValidationResult.Failure("Mật khẩu mới không được trùng với mật khẩu cũ.");
            }

            // 5. Compare with confirm password if provided
            if (confirmPassword != null && !string.Equals(password, confirmPassword))
            {
                return PasswordValidationResult.Failure("Xác nhận mật khẩu không trùng khớp với mật khẩu mới.");
            }

            return PasswordValidationResult.Success();
        }

        private static bool HasConsecutiveCharacters(string password, out string detected)
        {
            detected = string.Empty;
            if (string.IsNullOrEmpty(password) || password.Length < 3)
                return false;

            for (int i = 0; i <= password.Length - 3; i++)
            {
                char c1 = char.ToLowerInvariant(password[i]);
                char c2 = char.ToLowerInvariant(password[i + 1]);
                char c3 = char.ToLowerInvariant(password[i + 2]);

                // Numeric consecutive (e.g. 123, 789, 321, 987, 890, 098)
                if (char.IsDigit(c1) && char.IsDigit(c2) && char.IsDigit(c3))
                {
                    if ((c2 - c1 == 1 && c3 - c2 == 1) || (c1 - c2 == 1 && c2 - c3 == 1))
                    {
                        detected = password.Substring(i, 3);
                        return true;
                    }

                    string numSub = $"{c1}{c2}{c3}";
                    if (numSub == "890" || numSub == "901" || numSub == "098" || numSub == "109")
                    {
                        detected = password.Substring(i, 3);
                        return true;
                    }
                }

                // Alphabetical consecutive (e.g. abc, xyz, cba, zyx)
                if (char.IsLetter(c1) && char.IsLetter(c2) && char.IsLetter(c3))
                {
                    if ((c2 - c1 == 1 && c3 - c2 == 1) || (c1 - c2 == 1 && c2 - c3 == 1))
                    {
                        detected = password.Substring(i, 3);
                        return true;
                    }
                }

                // Keyboard sequence (e.g. qwe, asd, zxc)
                string sub = $"{c1}{c2}{c3}";
                foreach (var pattern in KeyboardPatterns)
                {
                    if (pattern.Contains(sub))
                    {
                        detected = password.Substring(i, 3);
                        return true;
                    }
                }
            }

            return false;
        }

        private static PasswordValidationResult CheckPersonalInformation(string password, UserPersonalInfo personalInfo)
        {
            var candidates = new List<(string Label, string Value)>();

            void AddCandidate(string label, string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return;
                var trimmed = raw.Trim();
                if (trimmed.Length >= 3)
                {
                    candidates.Add((label, trimmed));
                    var normalized = RemoveDiacritics(trimmed);
                    if (!string.Equals(trimmed, normalized, StringComparison.OrdinalIgnoreCase) && normalized.Length >= 3)
                    {
                        candidates.Add((label, normalized));
                    }
                }
            }

            AddCandidate("Mã nhân viên / Tên đăng nhập", personalInfo.WorkDayId);
            AddCandidate("Tên", personalInfo.FirstName);
            AddCandidate("Họ", personalInfo.LastName);

            if (!string.IsNullOrWhiteSpace(personalInfo.FullName))
            {
                var words = personalInfo.FullName.Split(new[] { ' ', ',', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var w in words)
                {
                    AddCandidate("Họ tên", w);
                }
            }

            if (!string.IsNullOrWhiteSpace(personalInfo.Email))
            {
                var emailPrefix = personalInfo.Email.Split('@')[0];
                AddCandidate("Email", emailPrefix);
                var emailParts = emailPrefix.Split(new[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in emailParts)
                {
                    AddCandidate("Email", part);
                }
            }

            if (!string.IsNullOrWhiteSpace(personalInfo.PhoneNumber))
            {
                var digitsOnly = new string(personalInfo.PhoneNumber.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length >= 4)
                {
                    candidates.Add(("Số điện thoại", digitsOnly));
                    if (digitsOnly.Length > 6)
                    {
                        candidates.Add(("Số điện thoại", digitsOnly.Substring(digitsOnly.Length - 6)));
                    }
                }
            }

            string normalizedPassword = RemoveDiacritics(password);

            foreach (var (label, val) in candidates)
            {
                if (val.Length >= 3)
                {
                    if (password.IndexOf(val, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        normalizedPassword.IndexOf(val, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return PasswordValidationResult.Failure($"Mật khẩu không được chứa thông tin cá nhân ({label}: '{val}').");
                    }
                }
            }

            return PasswordValidationResult.Success();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }
    }
}

