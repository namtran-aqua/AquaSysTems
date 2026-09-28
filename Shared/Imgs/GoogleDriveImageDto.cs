using System;

namespace AquaSolution.Shared.Imgs
{
    public class GoogleDriveImageDto
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public long? FileSize { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string ThumbnailLink { get; set; }
    }
}
