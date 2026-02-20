using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class UserRelationships
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppUser User { get; set; }
        [ForeignKey("RelatedUser")]
        public string RelatedUserId { get; set; }
        public AppUser RelatedUser { get; set; }
        public string RelationshipType { get; set; }
    }
}
