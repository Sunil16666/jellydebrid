#pragma warning disable CS1591,SA1402
using System;
using System.Collections.Generic;
using Jellyfin.Data.Enums;

namespace Jellyfin.Api.Models.ExternalMetadataDtos
{
    /// <summary>
    /// DTO for external movie details compatible with Jellyfin's API.
    /// </summary>
    public class ExternalMovieDetailsDto
    {
        public int TmdbId { get; set; }

        public string? ImdbId { get; set; }

        public string? Title { get; set; }

        public string? OriginalTitle { get; set; }

        public string? Overview { get; set; }

        public string? Tagline { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int? RuntimeMinutes { get; set; }

        public string? OfficialRating { get; set; }

        public double CommunityRating { get; set; }

        public int VoteCount { get; set; }

        public IReadOnlyList<string>? Genres { get; set; }

        public IReadOnlyList<ExternalProductionCompanyDto>? ProductionCompanies { get; set; }

        public string? Homepage { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public IReadOnlyList<ExternalImageDto>? Images { get; set; }

        public IReadOnlyList<ExternalPersonDto>? Cast { get; set; }

        public IReadOnlyList<ExternalPersonDto>? Crew { get; set; }

        public IReadOnlyList<ExternalVideoDto>? Trailers { get; set; }

        public ExternalCollectionDto? BelongsToCollection { get; set; }

        public IReadOnlyList<ExternalMovieDto>? SimilarMovies { get; set; }
    }

    public class ExternalProductionCompanyDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? LogoPath { get; set; }

        public string? OriginCountry { get; set; }
    }

    public class ExternalImageDto
    {
        public string? FilePath { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string? Iso6391 { get; set; } // Renamed from Iso_639_1

        public double VoteAverage { get; set; }

        public int VoteCount { get; set; }

        public string? ImageType { get; set; }
    }

    public class ExternalPersonDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Character { get; set; }

        public string? Job { get; set; }

        public string? Department { get; set; }

        public PersonKind PersonType { get; set; }

        public string? ProfilePath { get; set; }
    }

    public class ExternalVideoDto
    {
        public string? Id { get; set; }

        public string? Key { get; set; }

        public string? Name { get; set; }

        public string? Site { get; set; }

        public string? Type { get; set; }

        public int Size { get; set; }
    }

    public class ExternalCollectionDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }
    }

    public class ExternalMovieDto // For similar movies list
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? PosterPath { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public double CommunityRating { get; set; }
    }
}

#pragma warning restore CS1591,SA1402
