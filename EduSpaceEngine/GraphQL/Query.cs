using EduSpaceEngine.Dto;
using EduSpaceEngine.GraphQL.Types;
using EduSpaceEngine.Model;
using EduSpaceEngine.Services.User;
using HotChocolate.Authorization;


namespace EduSpaceEngine.GraphQL
{
    public class Query
    {
        private readonly IUserService _userService;

        public Query(IUserService userService)
        {
            _userService = userService;
        }

        public string Hello()
        {
            return "Hello";
        }

        [Authorize(Roles = new[] { "admin" })]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }

    }
}
