using EduSpaceEngine.Model.Social;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduSpaceEngine.Hubs
{
    public class NotificationHub : Hub
    {
        public static readonly ConcurrentDictionary<string, string> userConnectionMap = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdFromContext();

            // Retrieve userId from query parameters if it's not available from claims
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.GetHttpContext().Request.Query["userId"];
            }

            var connectionId = Context.ConnectionId;


            AddOrUpdateConnectionId(userId, connectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserIdFromContext();

            RemoveConnectionId(userId);

            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserIdFromContext()
        {
            return Context.GetHttpContext().Request.Query["userId"];
        }


        private void AddOrUpdateConnectionId(string userId, string connectionId)
        {
            userConnectionMap.AddOrUpdate(userId, connectionId, (_, existingConnectionId) => connectionId);
        }

        private void RemoveConnectionId(string userId)
        {
            userConnectionMap.TryRemove(userId, out _);
        }
        
    }
}
