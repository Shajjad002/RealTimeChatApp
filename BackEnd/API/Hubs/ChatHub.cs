namespace API.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using API.Data;
    using API.DTOs;
    using API.Extenions;
    using API.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
    {
        public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var receiverId = httpContext?.Request.Query["senderId"].ToString();
            var userName = Context.User?.Identity?.Name;
            var currentUser = await userManager.FindByNameAsync(userName);
            var connectionId = Context.ConnectionId;
            if (onlineUsers.ContainsKey(userName))
            {
                onlineUsers[userName].ConnectionId = connectionId;
                //onlineUsers[userName].IsOnline = true;
            }
            else
            {
                var user =new OnlineUserDto
                {
                    ConnectionId = connectionId,
                    UserName = userName,
                    FullName = $"{currentUser?.FullName}",
                    ProfileImage = currentUser?.ProfileImage,
                };
                onlineUsers.TryAdd(userName, user);

                await Clients.AllExcept(connectionId).SendAsync("Notify", user);
            }
            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());


           
        }

        private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
        {
            var username= Context.User?.GetUserName();

            var OnlineUsers = new HashSet<string>(onlineUsers.Keys);
            var users = await userManager.Users.Select(u => new OnlineUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                ProfileImage = u.ProfileImage,
                IsOnline = OnlineUsers.Contains(u.UserName),
                UnreadCount = context.Messages.Count(m => m.ReceiverId == username && m.SenderId == u.Id && !m.IsRead)
                
            }).OrderByDescending(u => u.IsOnline).ToListAsync();
            return users;
        }
    }
}