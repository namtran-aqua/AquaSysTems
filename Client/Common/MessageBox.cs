using AntDesign;

namespace AquaSolution.Client.Common
{
    public static class MessageBox
    {
        public static async Task<bool> Confirm(ModalService modal, string message)
        {
            var result = await modal.ConfirmAsync(new ConfirmOptions
            {
                Title = "Confirm",
                OkText = "Yes",
                CancelText = "No",
                Content = message
            });
            return result;
        }
        public static async Task<bool> Error(ModalService modal, string message)
        {
            var result = await modal.ErrorAsync(new ConfirmOptions
            {
                Title = "Error",
                Content = message,
                OkText = "Ok",
            });
            return result;
        }
        public static async Task Warning(ModalService modal, string message)
        {
            await modal.WarningAsync(new ConfirmOptions
            {
                Title = "Warning!",
                Content = message,
                OkText = "Ok",
            });
        }

        public static async Task Success(ModalService modal, string message)
        {
            await modal.SuccessAsync(new ConfirmOptions
            {
                Title = "Thành Công",
                Content = message,
                OkText = "Ok",
            });
        }

        internal static async Task ConfErrorirm(ModalService modal, string v)
        {
            throw new NotImplementedException();
        }
    }
}
