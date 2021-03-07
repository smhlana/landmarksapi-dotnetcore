using AutoMapper;
using LandmarksAPI.Entities;
using LandmarksAPI.Models.Users;

namespace LandmarksAPI.Helpers
{
	public class AutoMapperProfile : Profile
	{
        public AutoMapperProfile()
        {
            CreateMap<Entities.User, Models.Users.User>();
            CreateMap<RegisterRequest, Entities.User>();

            CreateMap<Account, AccountResponse>();

            CreateMap<Account, AuthenticateResponse>();

            CreateMap<RegisterRequest, Account>();

            CreateMap<CreateRequest, Account>();

        }
    }
}
