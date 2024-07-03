namespace EduSpaceEngine.Model.Social.Request
{
    public class EditCommentReqeustModel
    {
        public int CommentId { get; set; }

        public string? Content { get; set; }

        public string? Picture { get; set; }

        public string? Video { get; set; }
    }
}
