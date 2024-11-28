using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inftrastructure.Repositories
{
    [Table("Commit")]
    public class CommitRecord
    {
        public required string Sha { get; set; }
        [Required]
        public required string Owner { get; set; }
        [Required]
        public required string Repo { get; set; }
        [Required]
        public required string Message { get; set; }
        [Required]
        public required string Committer { get; set; }
    }
}
