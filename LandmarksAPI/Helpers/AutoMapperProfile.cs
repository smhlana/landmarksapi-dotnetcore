using AutoMapper;
using LandmarksAPI.Models.Users;

namespace LandmarksAPI.Helpers
{
	public class AutoMapperProfile : Profile
	{
        public AutoMapperProfile()
        {
            CreateMap<Entities.User, User>();
            CreateMap<Register, Entities.User>();
        }
    }
}
