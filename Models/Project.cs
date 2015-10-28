using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loial
{
    [Table("Project")]
    public class Project
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(50), Required]
        public string Name { get; set; }

        [StringLength(100), Required]
        public string Repository { get; set; }

        [StringLength(50), Required]
        public string Branch { get; set; }

        [MaxLength, Required]
        public string Command { get; set; }

        public int BuildNumber { get; set; }

        public bool IsActive { get; set; }

        public bool IsRunning { get; set; }
    };
}
