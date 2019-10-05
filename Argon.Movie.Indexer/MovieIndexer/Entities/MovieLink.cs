using System;
using Argon.Api.Interfaces.Entities;

namespace Argon.Movie.Indexer.MovieIndexer.Entities
{
	public class MovieLink : IArgonEntity
	{
		public string Id { get; set; }
		public DateTime CreateDateTime { get; set; }
		public DateTime UpdateDateTime { get; set; }

		public string MovieId { get; set; }

		public string Provider { get;set; }

		public string Link { get; set; }
	}
}
