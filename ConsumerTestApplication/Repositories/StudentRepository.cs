using System;
using System.Threading.Tasks;
using ConsumerTestApplication.DatabaseContexts;
using ConsumerTestApplication.DomainModel;
using ConsumerTestApplication.Repositories;
using MultiDbContextTransactionEF6.Core.Interface;

namespace ConsumerTestApplication
{
	/*
	 * An example "repository" relying on an ambient DbContext instance.
	 * 
	 * Since we use EF to persist our data, the actual repository is of course the EF DbContext. This
	 * class is called a "repository" for old time's sake but is merely just a collection 
	 * of pre-built Linq-to-Entities queries. This avoids having these queries copied and 
	 * pasted in every service method that need them and facilitates unit testing. 
	 * 
	 * Whether your application would benefit from using this additional layer or would
	 * be better off if its service methods queried the DbContext directly or used some sort of query 
	 * object pattern is a design decision for you to make.
	 * 
	 * DbContextScope is agnostic to this and will happily let you use any approach you
	 * deem most suitable for your application.
	 * 
	 */
	public class StudentRepository : IRepository<Student>
	{
		private readonly IDbContextLocator _dbContextLocator;

		private SchoolDbContext DbContext
		{
			get
			{
				var dbContext = _dbContextLocator.Get<SchoolDbContext>();

				if (dbContext == null)
					throw new InvalidOperationException("No ambient DbContext of type StudentManagementDbContext found. This means that this repository method has been called outside of the scope of a DbContextScope. A repository must only be accessed within the scope of a DbContextScope, which takes care of creating the DbContext instances that the repositories need and making them available as ambient contexts. This is what ensures that, for any given DbContext-derived type, the same instance is used throughout the duration of a business transaction. To fix this issue, use IDbContextScopeFactory in your top-level business logic service method to create a DbContextScope that wraps the entire business transaction that your service method implements. Then access this repository within that scope. Refer to the comments in the IDbContextScope.cs file for more details.");
				
				return dbContext;
			}
		}

		public StudentRepository(IDbContextLocator ambientDbContextLocator)
		{
			if (ambientDbContextLocator == null) throw new ArgumentNullException("ambientDbContextLocator");
			_dbContextLocator = ambientDbContextLocator;
		}

		public Student Get(Guid studentId)
		{
			return DbContext.Students.Find(studentId);
		}

		public Task<Student> GetAsync(Guid studentId)
		{
			return DbContext.Students.FindAsync(studentId);
		}

		public void Add(Student student)
		{
			DbContext.Students.Add(student);
		}
	}
}
