﻿using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity;
using MovieOpinions.Domain.Response;
using MovieOpinions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Service.Implementations
{
    public class FilmsServices : IFilmsServices
    {
        private readonly IFilmRepository _filmRepository;

        public FilmsServices(IFilmRepository filmRepository)
        {
            _filmRepository = filmRepository;
        }

        public async Task<BaseResponse<List<Film>>> GetFilms()
        {
            var GetAllFilms = await _filmRepository.GetAll();

            if (GetAllFilms.StatusCode == Domain.Enum.StatusCode.OK)
            {
                if (GetAllFilms.Data != null)
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.OK,
                        Data = GetAllFilms.Data
                    };
                }
                else
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільмів не знайдено"
                    };
                }
            }
            else
            {
                return new BaseResponse<List<Film>>()
                {
                    StatusCode = Domain.Enum.StatusCode.InternalServerError,
                    Description = GetAllFilms.Description
                };
            }
        }

        public async Task<BaseResponse<Film>> GetFilmId(int Id)
        {
            var FilmIdResponse = await _filmRepository.GetMovieId(Id);

            if (FilmIdResponse.StatusCode == Domain.Enum.StatusCode.OK && FilmIdResponse.Data != null)
            {
                return new BaseResponse<Film>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = FilmIdResponse.Data
                };
            }
            else
            {
                if (FilmIdResponse.Data == null)
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільм не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = FilmIdResponse.Description
                    };
                }
            }
        }

        public async Task<BaseResponse<Film>> GetFilmName(string NameFilm)
        {
            var FilmName = await _filmRepository.GetMovieName(NameFilm);

            if (FilmName.StatusCode == Domain.Enum.StatusCode.OK || FilmName.Data != null)
            {
                return new BaseResponse<Film>
                {
                    StatusCode = Domain.Enum.StatusCode.OK,
                    Data = FilmName.Data
                };
            }
            else
            {
                if (FilmName.Data == null)
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільм не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<Film>
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = FilmName.Description
                    };
                }
            }
        }

        public async Task<BaseResponse<List<Film>>> GetFilmByGenre(IEnumerable<int> IdGenre)
        {
            var GetMoviesByGenre = await _filmRepository.GetMovieGenre(IdGenre);

            if(GetMoviesByGenre.StatusCode == Domain.Enum.StatusCode.OK && GetMoviesByGenre.Data.Count > 0)
            {
                return new BaseResponse<List<Film>>()
                {
                    Data = GetMoviesByGenre.Data,
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            else
            {
                if(GetMoviesByGenre.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільмів не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = GetMoviesByGenre.Description
                    };
                }
            }
        }

        public async Task<BaseResponse<List<Film>>> GetFilmByYear(IEnumerable<string> Year)
        {
            var GetMoviesByYear = await _filmRepository.GetMovieYear(Year);

            if (GetMoviesByYear.StatusCode == Domain.Enum.StatusCode.OK && GetMoviesByYear.Data.Count > 0)
            {
                return new BaseResponse<List<Film>>()
                {
                    Data = GetMoviesByYear.Data,
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            else
            {
                if (GetMoviesByYear.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільмів не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = "Помилка серверу" + " " + GetMoviesByYear.Description
                    };
                }
            }
        }

        public async Task<BaseResponse<List<Film>>> SearchByPartialName(string PartialName)
        {
            var SearchFilm = await _filmRepository.SearchByPartialName(PartialName);

            if(SearchFilm.Data != null && SearchFilm.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return new BaseResponse<List<Film>>()
                {
                    Data = SearchFilm.Data,
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            else
            {
                if(SearchFilm.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільмів не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = "Помилка серверу спробуйте пізніше"
                    };
                }
            }
        }

        public async Task<BaseResponse<List<Film>>> SortingFilm(string SortElement)
        {
            var GetAllFilm = await _filmRepository.GetAll();

            if(GetAllFilm.StatusCode == Domain.Enum.StatusCode.OK && GetAllFilm.Data.Count > 0)
            {
                switch (SortElement)
                {
                    case "Alphabetical":
                        var AlphabeticalSortedFilms = GetAllFilm.Data.OrderBy(film => film.NameFilm).ToList();
                        return new BaseResponse<List<Film>>()
                        {
                            Data = AlphabeticalSortedFilms,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                    case "YearOld":
                        var YearOldSortedFilms = GetAllFilm.Data.OrderBy(film => film.YearFilm).ToList();
                        return new BaseResponse<List<Film>>()
                        {
                            Data = YearOldSortedFilms,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                    case "YearNew":
                        var YearNewSortedFilms = GetAllFilm.Data.OrderByDescending(film => film.YearFilm).ToList();
                        return new BaseResponse<List<Film>>()
                        {
                            Data = YearNewSortedFilms,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                    case "Popularity":
                        var PopularitySortedFilm = GetAllFilm.Data.OrderBy(film => film.RatingFilm).ToList();
                        return new BaseResponse<List<Film>>()
                        {
                            Data = PopularitySortedFilm,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                    default:
                        return new BaseResponse<List<Film>>()
                        {
                            Data = GetAllFilm.Data,
                            StatusCode = Domain.Enum.StatusCode.OK
                        };
                }
            }
            else
            {
                if(GetAllFilm.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.NotFound,
                        Description = "Фільмів не знайдено"
                    };
                }
                else
                {
                    return new BaseResponse<List<Film>>()
                    {
                        StatusCode = Domain.Enum.StatusCode.InternalServerError,
                        Description = "Помилка серверу!"
                    };
                }
            }
        }
    }
}