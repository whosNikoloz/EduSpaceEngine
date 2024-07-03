using AutoMapper;
using EduSpaceEngine.Model.User;

namespace EduSpaceEngine.Profiles
{
    public class MainProfile : Profile
    {
        public MainProfile()
        {
            CreateMap<UserModel, UserRegisterRequest>();
            CreateMap<UserRegisterRequest, UserModel>();


            CreateMap<OAuthUserRegisterRequest, UserModel>();
            CreateMap<UserModel, OAuthUserRegisterRequest>();
        }
    }
}
