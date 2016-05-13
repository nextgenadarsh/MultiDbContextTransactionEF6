using System.Data.Entity;
using System.Reflection;
using ConsumerTestApplication.DomainModel;

namespace ConsumerTestApplication.DatabaseContexts
{
	public class SchoolDbContext : DbContext
	{
// Map our 'Student' model by convention
		public DbSet<Student> Students{ get; set; }

		public SchoolDbContext() : base("SchoolDB")
		{}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// Overrides for the convention-based mappings.
			// We're assuming that all our fluent mappings are declared in this assembly.
			modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(SchoolDbContext)));

			base.OnModelCreating(modelBuilder);
		}
	}
}
