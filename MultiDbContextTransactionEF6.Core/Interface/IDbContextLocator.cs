using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDbContextTransactionEF6.Core.Interface
{
	/// <summary>
	/// Convenience methods to retrieve ambient DbContext instances. 
	/// </summary>
	public interface IDbContextLocator
	{
		/// <summary>
		/// If called within the scope of a DbContextScope, gets or creates 
		/// the ambient DbContext instance for the provided DbContext type. 
		/// 
		/// Otherwise returns null. 
		/// </summary>
		TDbContext Get<TDbContext>() where TDbContext : DbContext;
	}
}
