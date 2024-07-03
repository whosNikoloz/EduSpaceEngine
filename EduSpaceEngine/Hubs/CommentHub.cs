using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace EduSpaceEngine.Hubs
{
    public class CommentHub : Hub
    {
        // Dictionary to store user-post connections
        public static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> userPostConnectionMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdFromContext();
            var postId = GetPostIdFromContext();

            // Retrieve userId from query parameters if it's not available from claims
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.GetHttpContext().Request.Query["userId"];
            }

            // Ensure postId is provided
            if (string.IsNullOrEmpty(postId))
            {
                throw new ArgumentException("postId is required.");
            }

            var connectionId = Context.ConnectionId;

            // Add or update the connectionId for the user-post pair
            AddOrUpdateConnectionId(userId, postId, connectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserIdFromContext();
            var postId = GetPostIdFromContext();

            // Remove the connectionId for the user-post pair
            RemoveConnectionId(userId, postId);

            await base.OnDisconnectedAsync(exception);
        }

        // Method to add or update connectionId for a user-post pair
        private void AddOrUpdateConnectionId(string userId, string postId, string connectionId)
        {
            // Ensure the inner ConcurrentDictionary for the user exists
            userPostConnectionMap.TryGetValue(userId, out ConcurrentDictionary<string, string> postConnectionMap);
            if (postConnectionMap == null)
            {
                postConnectionMap = new ConcurrentDictionary<string, string>();
                userPostConnectionMap.TryAdd(userId, postConnectionMap);
            }

            // Update the connectionId for the given postId
            postConnectionMap.AddOrUpdate(postId, connectionId, (_, existingConnectionId) => connectionId);
        }

        // Method to remove connectionId for a user-post pair
        private void RemoveConnectionId(string userId, string postId)
        {
            if (userPostConnectionMap.TryGetValue(userId, out ConcurrentDictionary<string, string> postConnectionMap))
            {
                postConnectionMap.TryRemove(postId, out _);
            }
        }

        // Method to get userId from context
        private string GetUserIdFromContext()
        {
            return Context.GetHttpContext().Request.Query["userId"];
        }

        // Method to get postId from context
        private string GetPostIdFromContext()
        {
            return Context.GetHttpContext().Request.Query["postId"];
        }
    }
}
