using System;

namespace AquaSolution.Data.Data.Entities.Imgs
{
    public class FolderPermission
    {
        public Guid Id { get; set; }
        public string WorkDayId { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
    }
}
