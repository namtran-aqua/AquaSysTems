using System;

namespace AquaSolution.Data.Data.Entities.Admin
{
    public class UserSection
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid SectionId { get; set; }
        public Section Section { get; set; }
    }
}
