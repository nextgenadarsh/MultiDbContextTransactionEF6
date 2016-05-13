using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MultiDbContextTransactionEF6.Core.Interface;

namespace MultiDbContextTransactionEF6.Core.Implementation
{
	public class DbContextLocator : IDbContextLocator
	{
		public TDbContext Get<TDbContext>() where TDbContext : DbContext
		{
			var ambientDbContextScope = DbContextScope.GetAmbientScope();
			return ambientDbContextScope == null ? null : ambientDbContextScope.DbContexts.Get<TDbContext>();
		}
	}
}
