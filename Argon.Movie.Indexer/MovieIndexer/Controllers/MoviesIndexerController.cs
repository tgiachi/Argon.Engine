using System.Collections.Generic;
using Argon.Movie.Indexer.MovieIndexer.Entities;
using Argon.Movie.Indexer.MovieIndexer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Argon.Movie.Indexer.MovieIndexer.Controllers
{
	[ApiController]
	[Route("api/plugins/moviesindexer/")]
	public class MoviesIndexerController : ControllerBase
	{
		private readonly IMoviesIndexerService _moviesIndexerService;

		public MoviesIndexerController(IMoviesIndexerService moviesIndexerService)
		{
			_moviesIndexerService = moviesIndexerService;
		}


		[HttpGet]
		[Route("available/indexers")]
		public ActionResult<List<string>> GetAvailableIndexers()
		{
			return _moviesIndexerService.AvailableIndexers;
		}

		[HttpPost]
		[Route("start/{name}")]
		public ActionResult<bool> StartIndexer(string name)
		{
			return Ok(_moviesIndexerService.StartIndexer(name));
		}

		[HttpGet]
		[Route("movies/categories")]
		public ActionResult<List<MovieCategory>> GetCategories()
		{
			return Ok(_moviesIndexerService.GetMovieCategories());
		}

		[HttpGet]
		[Route("movies/category/{id}")]
		public ActionResult<List<MovieCategory>> GetMovieCategory(string id)
		{
			return Ok(_moviesIndexerService.GetMoviesByCategoryId(id));
		}

		[HttpGet]
		[Route("movies/links/{id}")]
		public ActionResult<List<MovieLink>> GetMovieLink(string id)
		{
			return Ok(_moviesIndexerService.GetMovieLinkById(id));
		}
	}
}
