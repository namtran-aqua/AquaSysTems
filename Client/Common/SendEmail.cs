using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.RequestSuport;
using System.Net.Http.Json;
using System.Text;

namespace AquaSolution.Client.Common
{

    public static class SendEmail
    {
        private static async Task HandleEmail(SendEmailDto sendEmailDto)
        {
            var url = "https://prod-15.southeastasia.logic.azure.com:443/workflows/2dcf2a5ca6a74e7ca0565bc92a03c5b1/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wVvZUqoCZW7gvxjgfiLGLJHdZDndFg9oeOKjGxquIKM";
            var payload = new
            {
                Id = sendEmailDto.RequestId,
                Status = "No Status",
                To = sendEmailDto.ToAdress,
                Subject = sendEmailDto.Subject,
                Body = sendEmailDto.Body.ToString()
            };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, payload);
        }
        public static async Task SendEmailStatusRequestAsync(RequestSuportDto requestSuportDto)
        {
            var data = await EmailStatusRequestTemplate(requestSuportDto);
            await HandleEmail(data);
        }

        private static Task<SendEmailDto> EmailStatusRequestTemplate(RequestSuportDto requestSuportDto)
        {
            var sendEmailDto = new SendEmailDto();
            var bodyEmail = new StringBuilder();
            sendEmailDto.RequestId = requestSuportDto.Id;
            sendEmailDto.ToAdress = requestSuportDto.RequestByEmail;
            switch (requestSuportDto.Status)
            {
                case RequestSuportStatusType.InProgress:
                    bodyEmail.AppendLine($"<strong>Yêu cầu \"{requestSuportDto.RequestTitle}\" của bạn đang được giải quyết.</strong>");
                    bodyEmail.AppendLine("<ul>");
                    bodyEmail.AppendLine($"<li><strong>Request Description: </strong> {requestSuportDto.RequestDescription}</li>");
                    bodyEmail.AppendLine($"<li><strong>Technician: </strong> {requestSuportDto.TechnicianName}</li>");
                    bodyEmail.AppendLine($"<li><p><strong>Status: </strong> <span style='color:#1890ff;'>InProgress</span></p></li>");
                    bodyEmail.AppendLine($"<li><strong>Created Time: </strong> {requestSuportDto.CreatedDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine($"<li><strong>Start Date: </strong> {requestSuportDto.InProgessDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine($"<li><strong>Due Date: </strong> {requestSuportDto.DueDate:yyyy-MM-dd}</li>");
                    bodyEmail.AppendLine("</ul>");
                    break;
                case RequestSuportStatusType.Resolved:
                    bodyEmail.AppendLine($"<strong>Yêu cầu \"{requestSuportDto.RequestTitle}\" của bạn đã được giải quyết.</strong>");
                    bodyEmail.AppendLine("<ul>");
                    bodyEmail.AppendLine($"<li><strong>Request Description: </strong> {requestSuportDto.RequestDescription}</li>");
                    bodyEmail.AppendLine($"<li><strong>Request Solution: </strong> {requestSuportDto.RequestSolution}</li>");
                    bodyEmail.AppendLine($"<li><strong>Technician: </strong> {requestSuportDto.TechnicianName}</li>");
                    bodyEmail.AppendLine($"<li> <p><strong>Status: </strong> <span style='color:#52c41a;'>Resolved</span></p> </li>");
                    bodyEmail.AppendLine($"<li><strong>Created Time: </strong> {requestSuportDto.CreatedDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine($"<li><strong>Start Date: </strong> {requestSuportDto.InProgessDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine($"<li><strong>Due Date: </strong> {requestSuportDto.DueDate:yyyy-MM-dd}</li>");
                    bodyEmail.AppendLine($"<li><strong>Resolved Date: </strong> {requestSuportDto.ResolveDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine("</ul>");
                    break;
                case RequestSuportStatusType.Cancel:
                    bodyEmail.AppendLine($"<strong>Yêu cầu <strong>\"{requestSuportDto.RequestTitle}\"</strong> của bạn đã bị <span style='color:red;'>Cancel</span>.</strong>");
                    bodyEmail.AppendLine($"<li><p><strong>RequestDescription: </strong> {requestSuportDto.RequestDescription}</p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Technician: </strong> {requestSuportDto.TechnicianName}</p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Status: </strong> <span style='color:red;'>Cancel</span></p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Created Time: </strong> {requestSuportDto.CreatedDate:yyyy-MM-dd HH:mm}</p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Cancel Date: </strong> {requestSuportDto.CancelDate:yyyy-MM-dd HH:mm}</p></li>");
                    bodyEmail.AppendLine("</ul>");
                    break;
                case RequestSuportStatusType.Open:
                    bodyEmail.AppendLine($"<strong>Yêu cầu \"{requestSuportDto.RequestTitle}\" của bạn đã được IT tiếp nhận.</strong>");
                    bodyEmail.AppendLine($"<li><p><strong>RequestDescription: </strong> {requestSuportDto.RequestDescription}</p></li>");
                    bodyEmail.AppendLine($"<li><strong>Technician: </strong> {requestSuportDto.TechnicianName}</li>");
                    bodyEmail.AppendLine($"<li><p><strong>Status: </strong> <span style='color:#000000;'>Open</span></p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Created Time: </strong> {requestSuportDto.CreatedDate:yyyy-MM-dd HH:mm}</p></li>");
                    break;
                case RequestSuportStatusType.OnHold:
                    bodyEmail.AppendLine($"<strong>Yêu cầu \"{requestSuportDto.RequestTitle}\" của bạn đã bị tạm dừng xử lý.</strong>");
                    bodyEmail.AppendLine($"<li><p><strong>RequestDescription: </strong> {requestSuportDto.RequestDescription}</p></li>");
                    bodyEmail.AppendLine($"<li><strong>Technician: </strong> {requestSuportDto.TechnicianName}</li>");
                    bodyEmail.AppendLine($"<li><p><strong>Status: </strong> <span style='color:#fa8c16;'>OnHold</span></p></li>");
                    bodyEmail.AppendLine($"<li><p><strong>Created Time: </strong> {requestSuportDto.CreatedDate:yyyy-MM-dd HH:mm}</p></li>");
                    bodyEmail.AppendLine($"<li><strong>Start Date: </strong> {requestSuportDto.OnHoldDate:yyyy-MM-dd HH:mm}</li>");
                    bodyEmail.AppendLine("</ul>");
                    break;
            }
            bodyEmail.AppendLine("<p><a href='http://server14/ITSM/SysRequest/Index'>Xem chi tiết tại đây</a></p>");
            sendEmailDto.Body = bodyEmail;
            return Task.FromResult(sendEmailDto);
        }
    }
}
