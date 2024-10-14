using AutoMapper;
using EduSpaceEngine.Dto;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Dto.User;
using EduSpaceEngine.Model;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.Test;

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

            CreateMap<UserModel, UserDto>();
            CreateMap<UserDto, UserModel>();

            CreateMap<LevelModel, LevelDto>();
            CreateMap<LevelDto, LevelModel>();

            CreateMap<CourseDto, CourseModel>();
            CreateMap<CourseModel, CourseDto>();

            CreateMap<SubjectDto, SubjectModel>();
            CreateMap<SubjectModel, SubjectDto>();

            CreateMap<LessonDto, LessonModel>();
            CreateMap<LessonModel, LessonDto>();

            CreateMap<LearnModel, LearnMaterialDto>();
            CreateMap<LearnMaterialDto, LearnModel>();

            CreateMap<TestModel, TestDto>();
            CreateMap<TestDto, TestModel>();

            CreateMap<TestAnswerDto, TestAnswerModel>();
            CreateMap<TestAnswerModel, TestAnswerDto>();

        }
    }
}
