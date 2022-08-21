using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileCheckingService.Entities.Models
{
    [Table("File")]
    public class FileEntity : EntityBase
    {

        [Required]
        public string Path { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
    }
}
