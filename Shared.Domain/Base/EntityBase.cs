using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Base
{
    public abstract class BaseEntity
    {
        // Common properties for all entities
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public BaseEntity()
        {
            Id = Guid.NewGuid(); // Automatically assign a new unique Id
            CreatedAt = DateTime.UtcNow; // Automatically set creation timestamp
            UpdatedAt = DateTime.UtcNow; // Initially the same as CreatedAt
        }

        // Optional method to update the 'UpdatedAt' timestamp when entity is modified
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
