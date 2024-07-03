namespace EduSpaceEngine.Model.Social.Request
{
    public class EditPostRequestModel
    {
        public int PostId { get; set; }


        public string? Subject { get; set; }

        public string? Content { get; set; }

        public string? Video { get; set; }

        public string? Picture { get; set; }
    }
}
