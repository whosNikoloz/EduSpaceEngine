using EduSpaceEngine.Model;
using GraphQL.Types;

namespace EduSpaceEngine.GraphQL.Types
{
    public class UserType : ObjectGraphType<UserModel>
    {
        public UserType()
        {
            Field(x => x.UserId).Description("The ID of the user.");
            Field(x => x.UserName, nullable: true).Description("The username of the user.");
            Field(x => x.FirstName, nullable: true).Description("The first name of the user.");
            Field(x => x.LastName, nullable: true).Description("The last name of the user.");
            Field(x => x.Email).Description("The email of the user.");
            Field(x => x.PhoneNumber, nullable: true).Description("The phone number of the user.");
            Field(x => x.Picture, nullable: true).Description("The profile picture URL of the user.");
            Field(x => x.VerificationToken, nullable: true).Description("The verification token of the user.");
            Field(x => x.VerifiedAt, nullable: true).Description("The date and time when the user was verified.");
            Field(x => x.Role, nullable: true).Description("The role of the user.");
            Field(x => x.OAuthProvider, nullable: true).Description("The OAuth provider (e.g., Google) used for authentication.");
            Field(x => x.OAuthProviderId, nullable: true).Description("The unique identifier provided by the OAuth provider.");
            Field(x => x.LastActivity).Description("The date and time of the user's last activity.");
        }
    }
}
