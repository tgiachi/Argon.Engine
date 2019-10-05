using System.Collections.Generic;
using System.Net.Http;
using Argon.Api.Interfaces.Base;
using Argon.Movie.Indexer.MovieIndexer.Entities;

namespace Argon.Movie.Indexer.MovieIndexer.Interfaces.Services
{
	public interface IMoviesIndexerService : IArgonService
	{
		List<string> AvailableIndexers { get; }

		MovieCategory AddMovieCategory(string name);

		Argon.Movie.Indexer.MovieIndexer.Entities.Movie AddMovie(MovieCategory category, string name);

		MovieLink AddMovieLink(Argon.Movie.Indexer.MovieIndexer.Entities.Movie movie, string provider, string link);

		List<MovieCategory> GetMovieCategories();

		List<Argon.Movie.Indexer.MovieIndexer.Entities.Movie> GetMoviesByCategoryId(string categoryId);

		List<MovieLink> GetMovieLinkById(string movieId);
		bool StartIndexer(string name);

		IHttpClientFactory HttpClientFactory { get; }
	}
}
