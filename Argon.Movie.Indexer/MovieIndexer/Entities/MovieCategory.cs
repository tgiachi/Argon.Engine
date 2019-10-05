using System;
using Argon.Api.Interfaces.Entities;

namespace Argon.Movie.Indexer.MovieIndexer.Entities
{
	public class MovieCategory : IArgonEntity
	{
		public string Id { get; set; }
		public DateTime CreateDateTime { get; set; }
		public DateTime UpdateDateTime { get; set; }

		public string Name { get; set; }
	}
}
