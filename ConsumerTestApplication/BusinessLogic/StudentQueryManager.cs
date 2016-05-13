using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ConsumerTestApplication.DatabaseContexts;
using ConsumerTestApplication.DomainModel;
using ConsumerTestApplication.Repositories;
using MultiDbContextTransactionEF6.Core.Interface;

namespace ConsumerTestApplication.BusinessLogic
{
	/*
	 * Example business logic service implementing query functionalities (i.e. read actions).
	*/
	public class StudentQueryManager
	{
		private readonly IDbContextScopeFactory _dbContextScopeFactory;
		private readonly IRepository<Student> _studentRepository;

		public StudentQueryManager(IDbContextScopeFactory dbContextScopeFactory, IRepository<Student> studentRepository)
		{
			if (dbContextScopeFactory == null) throw new ArgumentNullException("dbContextScopeFactory");
			if (studentRepository == null) throw new ArgumentNullException("studentRepository");
			_dbContextScopeFactory = dbContextScopeFactory;
			_studentRepository = studentRepository;
		}

		public Student GetStudent(Guid studentId)
		{
			/*
			 * An example of using DbContextScope for read-only queries. 
			 * Here, we access the Entity Framework DbContext directly from 
			 * the business logic service class.
			 * 
			 * Calling SaveChanges() is not necessary here (and in fact not 
			 * possible) since we created a read-only scope.
			 */
			using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
			{
				var dbContext = dbContextScope.DbContexts.Get<SchoolDbContext>();
				var student = dbContext.Students.Find(studentId);

				if (student == null)
					throw new ArgumentException(String.Format("Invalid value provided for studentId: [{0}]. Couldn't find a student with this ID.", studentId));

				return student;
			}
		}

		public IEnumerable<Student> GetStudents(params Guid[] studentIds)
		{
			using (var dbContextScope = _dbContextScopeFactory.CreateReadOnly())
			{
				var dbContext = dbContextScope.DbContexts.Get<SchoolDbContext>();
				return dbContext.Students.Where(u => studentIds.Contains(u.Id)).ToList();
			}
		}

		public Student GetStudentViaRepository(Guid studentId)
		{
			/*
			 * Same as GetStudents() but using a repository layer instead of accessing the 
			 * EF DbContext directly.
			 * 
			 * Note how we don't have to worry about knowing what type of DbContext the 
			 * repository will need, about creating the DbContext instance or about passing
			 * DbContext instances around. 
			 * 
			 * The DbContextScope will take care of creating the necessary DbContext instances
			 * and making them available as ambient contexts for our repository layer to use.
			 * It will also guarantee that only one instance of any given DbContext type exists
			 * within its scope ensuring that all persistent entities managed within that scope
			 * are attached to the same DbContext. 
			 */
			using (_dbContextScopeFactory.CreateReadOnly())
			{
				var student = _studentRepository.Get(studentId);

				if (student == null)
					throw new ArgumentException(String.Format("Invalid value provided for studentId: [{0}]. Couldn't find a student with this ID.", studentId));

				return student;
			}
		}

		public async Task<IList<Student>> GetTwoStudentsAsync(Guid studentId1, Guid studentId2)
		{
			/*
			 * A very contrived example of ambient DbContextScope within an async flow.
			 * 
			 * Note that the ConfigureAwait(false) calls here aren't strictly necessary 
			 * and are unrelated to DbContextScope. You can remove them if you want and 
			 * the code will run in the same way. It is however good practice to configure
			 * all your awaitables in library code to not continue 
			 * on the captured synchronization context. It avoids having to pay the overhead 
			 * of capturing the sync context and running the task continuation on it when 
			 * library code doesn't need that context. If also helps prevent potential deadlocks 
			 * if the upstream code has been poorly written and blocks on async tasks. 
			 * 
			 * "Library code" is any code in layers under the presentation tier. Typically any code
			 * other that code in ASP.NET MVC / WebApi controllers or Window Form / WPF forms.
			 * 
			 * See http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx for 
			 * more details.
			 */

			using (_dbContextScopeFactory.CreateReadOnly())
			{
				var student1 = await _studentRepository.GetAsync(studentId1).ConfigureAwait(false);

				// We're now in the continuation of the first async task. This is most
				// likely executing in a thread from the ThreadPool, i.e. in a different
				// thread that the one where we created our DbContextScope. Our ambient
				// DbContextScope is still available here however, which allows the call 
				// below to succeed.

				var student2 = await _studentRepository.GetAsync(studentId2).ConfigureAwait(false);

				// In other words, DbContextScope works with async execution flow as you'd expect: 
				// It Just Works.  

				return new List<Student> {student1, student2}.Where(u => u != null).ToList();
			}
		}

		public Student GetStudentUncommitted(Guid studentId)
		{
			/*
			 * An example of explicit database transaction. 
			 * 
			 * Read the comment for CreateReadOnlyWithTransaction() before using this overload
			 * as there are gotchas when doing this!
			 */
			using (_dbContextScopeFactory.CreateReadOnlyWithTransaction(IsolationLevel.ReadUncommitted))
			{
				return _studentRepository.Get(studentId);
			}
		}
	}
}
