using Microsoft.Data.Entity;

namespace Loial
{
    public class LoialDb : DbContext
    {
        public DbSet<Project> Projects { get; set; }
    };
}
