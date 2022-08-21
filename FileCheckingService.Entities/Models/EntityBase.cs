using System.ComponentModel.DataAnnotations;


namespace FileCheckingService.Entities.Models
{
    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; set; }
    }
}
