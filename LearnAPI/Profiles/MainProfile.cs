using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using LearnAPI.Model.User;

namespace LearnAPI.Profiles
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
