using System.Threading.Tasks;

namespace Argon.Movie.Indexer.MovieIndexer.Interfaces
{
	public interface IMoviesIndexer
	{
		Task<bool> Start();

	}

}
