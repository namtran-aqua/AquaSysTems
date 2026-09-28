using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Imgs
{
    public partial class GroupImg
    {
        public string WorkId { get; set; }
        public string FolderName { get; set; }
        public  List<CloudinaryImageDto> CloudinaryImageDtos { get; set; } =new List<CloudinaryImageDto>();
        public List<GoogleDriveImageDto> GoogleDriveImageDtos { get; set; } = new List<GoogleDriveImageDto>();
    }
}
