using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsumerTestApplication.DatabaseContexts;
using MultiDbContextTransactionEF6.Core.Interface;

namespace ConsumerTestApplication.BusinessLogic
{

public class StudentScoreManager
	{
		private readonly IDbContextScopeFactory _dbContextScopeFactory;

		public StudentScoreManager(IDbContextScopeFactory dbContextScopeFactory)
		{
			if (dbContextScopeFactory == null) throw new ArgumentNullException("dbContextScopeFactory");
			_dbContextScopeFactory = dbContextScopeFactory;
		}

		public void UpdateCreditScoreForAllUsers()
		{
			/*
			 * Demo of DbContextScope + parallel programming.
			 */

			using (var dbContextScope = _dbContextScopeFactory.Create())
			{
				//-- Get all users
				var dbContext = dbContextScope.DbContexts.Get<SchoolDbContext>();
				var userIds = dbContext.Students.Select(u => u.Id).ToList();

				Console.WriteLine("Found {0} users in the database. Will calculate and store their credit scores in parallel.", userIds.Count);

				//-- Calculate and store the credit score of each user
				// We're going to imagine that calculating a credit score of a user takes some time. 
				// So we'll do it in parallel.

				// You MUST call SuppressAmbientContext() when kicking off a parallel execution flow 
				// within a DbContextScope. Otherwise, this DbContextScope will remain the ambient scope
				// in the parallel flows of execution, potentially leading to multiple threads
				// accessing the same DbContext instance.
				using (_dbContextScopeFactory.SuppressAmbientContext())
				{
					Parallel.ForEach(userIds, UpdateCreditScore);
				}

				// Note: SaveChanges() isn't going to do anything in this instance since all the changes
				// were actually made and saved in separate DbContextScopes created in separate threads.
				dbContextScope.SaveChanges();
			}
		}

		public void UpdateCreditScore(Guid userId)
		{
			using (var dbContextScope = _dbContextScopeFactory.Create())
			{
				var dbContext = dbContextScope.DbContexts.Get<SchoolDbContext>();
				var user = dbContext.Students.Find(userId);
				if (user == null)
					throw new ArgumentException(String.Format("Invalid userId provided: {0}. Couldn't find a User with this ID.", userId));

				// Simulate the calculation of a credit score taking some time
				var random = new Random(Thread.CurrentThread.ManagedThreadId);
				Thread.Sleep(random.Next(300, 1000));

				user.Score = random.Next(1, 100);
				dbContextScope.SaveChanges();
			}
		}
	}
}
