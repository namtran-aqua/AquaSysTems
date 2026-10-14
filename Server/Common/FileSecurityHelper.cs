using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AquaSolution.Server.Common
{
    /// <summary>
    /// Tiện ích kiểm tra an toàn tệp đính kèm:
    /// - Quét Magic Bytes (chữ ký nhị phân)
    /// - Chặn file thực thi giả mạo (Windows PE .exe, Linux ELF, Scripts)
    /// - Xác minh định dạng thực tế khớp với đuôi mở rộng
    /// </summary>
    public static class FileSecurityHelper
    {
        // Danh sách phần mở rộng được phép tải lên
        private static readonly string[] AllowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp",
            ".pdf", ".docx", ".xlsx", ".doc", ".xls", ".txt"
        };

        public static (bool IsValid, string Reason, string DetectedMimeType) ValidateFile(byte[] fileBytes, string declaredFileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                return (false, "Tệp trống (0 bytes).", "application/octet-stream");

            // 1. Giới hạn dung lượng tối đa (25 MB)
            if (fileBytes.Length > 25 * 1024 * 1024)
                return (false, "Tệp vượt quá dung lượng cho phép (tối đa 25MB).", "application/octet-stream");

            // 2. Chặn tuyệt đối tệp thực thi Windows PE (.exe, .dll, .sys, .com...) bắt đầu bằng "MZ"
            if (fileBytes.Length >= 2 && fileBytes[0] == 0x4D && fileBytes[1] == 0x5A)
            {
                return (false, "CẢNH BÁO: Phát hiện tệp thực thi nhị phân độc hại (Windows Executable / MZ signature).", "application/x-msdownload");
            }

            // 3. Chặn tệp thực thi Linux ELF bắt đầu bằng 0x7F 'E' 'L' 'F'
            if (fileBytes.Length >= 4 && fileBytes[0] == 0x7F && fileBytes[1] == 0x45 && fileBytes[2] == 0x4C && fileBytes[3] == 0x46)
            {
                return (false, "CẢNH BÁO: Phát hiện tệp thực thi nhị phân độc hại (Linux ELF binary).", "application/x-executable");
            }

            // 4. Quét mã độc dạng Script (Web shell, PHP, ASPX, Shell script) trong 512 bytes đầu
            try
            {
                var checkLength = Math.Min(fileBytes.Length, 512);
                var headerText = Encoding.UTF8.GetString(fileBytes, 0, checkLength).ToLowerInvariant();
                if (headerText.Contains("<?php") || 
                    headerText.Contains("<%@") || 
                    headerText.Contains("<script") || 
                    headerText.Contains("#!/bin/sh") || 
                    headerText.Contains("#!/bin/bash") ||
                    headerText.Contains("eval(") ||
                    headerText.Contains("base64_decode("))
                {
                    return (false, "CẢNH BÁO: Phát hiện mã lệnh / script nguy hiểm được nhúng trong tệp.", "text/plain");
                }
            }
            catch { }

            // 5. Kiểm tra phần mở rộng tệp
            var ext = Path.GetExtension(declaredFileName)?.ToLowerInvariant() ?? "";
            if (!string.IsNullOrEmpty(ext) && !AllowedExtensions.Contains(ext))
            {
                return (false, $"Phần mở rộng '{ext}' không nằm trong danh sách định dạng được phép.", "application/octet-stream");
            }

            // 6. Nhận diện Magic Bytes của các định dạng an toàn
            // PDF: %PDF- (0x25, 0x50, 0x44, 0x46)
            if (fileBytes.Length >= 4 && fileBytes[0] == 0x25 && fileBytes[1] == 0x50 && fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
            {
                return (true, "Hợp lệ", "application/pdf");
            }

            // JPEG: FF D8 FF
            if (fileBytes.Length >= 3 && fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
            {
                return (true, "Hợp lệ", "image/jpeg");
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (fileBytes.Length >= 8 && fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47 &&
                fileBytes[4] == 0x0D && fileBytes[5] == 0x0A && fileBytes[6] == 0x1A && fileBytes[7] == 0x0A)
            {
                return (true, "Hợp lệ", "image/png");
            }

            // WEBP: RIFF....WEBP
            if (fileBytes.Length >= 12 && fileBytes[0] == 0x52 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46 && fileBytes[3] == 0x46 &&
                fileBytes[8] == 0x57 && fileBytes[9] == 0x45 && fileBytes[10] == 0x42 && fileBytes[11] == 0x50)
            {
                return (true, "Hợp lệ", "image/webp");
            }

            // GIF: GIF87a hoặc GIF89a
            if (fileBytes.Length >= 6 && fileBytes[0] == 0x47 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46 && fileBytes[3] == 0x38)
            {
                return (true, "Hợp lệ", "image/gif");
            }

            // ZIP / OpenOffice XML (.docx, .xlsx)
            if (fileBytes.Length >= 4 && fileBytes[0] == 0x50 && fileBytes[1] == 0x4B && fileBytes[2] == 0x03 && fileBytes[3] == 0x04)
            {
                var mime = ext == ".docx" 
                    ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
                    : (ext == ".xlsx" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/zip");
                return (true, "Hợp lệ", mime);
            }

            // Nếu người gửi đổi tên file giả vờ là .jpg hay .pdf nhưng nội dung nhị phân không khớp
            if (ext is ".jpg" or ".jpeg" or ".png" or ".pdf")
            {
                return (false, $"CẢNH BÁO: Tệp có phần mở rộng '{ext}' nhưng cấu trúc nhị phân (Magic Bytes) bên trong không đúng định dạng.", "application/octet-stream");
            }

            return (true, "Hợp lệ", "application/octet-stream");
        }
    }
}
