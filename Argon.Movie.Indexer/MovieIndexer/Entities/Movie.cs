using System;
using Argon.Api.Interfaces.Entities;

namespace Argon.Movie.Indexer.MovieIndexer.Entities
{
	public class Movie : IArgonEntity
	{
		public string Id { get; set; }
		public DateTime CreateDateTime { get; set; }
		public DateTime UpdateDateTime { get; set; }

		public string Title { get; set; }

		public string MovieCategoryId { get; set; }

	}
}
