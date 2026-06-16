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
                var user = new OnlineUserDto
                {
                    ConnectionId = connectionId,
                    UserName = userName,
                    FullName = $"{currentUser?.FullName}",
                    ProfileImage = currentUser?.ProfileImage,
                };
                onlineUsers.TryAdd(userName, user);

                await Clients.AllExcept(connectionId).SendAsync("Notify", user);
            }
            if (!string.IsNullOrEmpty(receiverId))
            {
                await LoadMessages(receiverId);
            }
            
            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());

        }

        public async Task SendMessage(MessageRequestDto messageDto)
        {
            var senderId = Context.User?.Identity?.Name;
            var recipientId = messageDto.ReceiverId;

            var newMessage = new Message
            {
                Sender = await userManager.FindByNameAsync(senderId),
                Receiver = await userManager.FindByIdAsync(messageDto.ReceiverId),
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                Content = messageDto.Content,
            };

            context.Messages.Add(newMessage);
            await context.SaveChangesAsync();

            await Clients.User(recipientId).SendAsync("ReceiveNewMessage", newMessage);

        }

        public async Task SendTypingNotification(string recipientUserName)
        {
            var senderUserName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(senderUserName))
            {
                await Clients.User(recipientUserName).SendAsync("ReceiveTypingNotification", senderUserName);
            }
            else
            {
                return;
                // Handle the case where senderUserName is null or empty, if necessary
            }
            var connectionId = onlineUsers.Values.FirstOrDefault(u => u.UserName == recipientUserName)?.ConnectionId;
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveTypingNotification", senderUserName);
            }

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            onlineUsers.TryRemove(userName, out _); // Remove the user from the online users list
            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());

        }
        public async Task LoadMessages(string recipieentId, int pageNumber = 1)//,int pageSize=10
        {
            int pageSize = 10;
            var username = Context.User?.Identity?.Name;
            var currentUser = await userManager.FindByNameAsync(username);
            if (currentUser == null) return;
            List<MessageResponseDto> messages = await context.Messages
                .Where(m => m.ReceiverId == currentUser.Id && (m.SenderId == currentUser.Id || m.SenderId == recipieentId) && (m.ReceiverId == currentUser.Id || m.ReceiverId == recipieentId))
                .OrderByDescending(m => m.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).OrderBy(m => m.CreatedDate)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            foreach (var message in messages)
            {
                var msg = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
                if (msg != null && msg.ReceiverId == currentUser.Id && !msg.IsRead)
                {
                    msg.IsRead = true;
                    await context.SaveChangesAsync();
                }
            }

            await Clients.User(currentUser.Id).SendAsync("ReceiveMessagesList", messages);
        }

        public async Task MarkAsRead(string senderId)
        {
            var receiverId = Context.User?.Identity?.Name;
            var messages = await context.Messages
                .Where(m => m.Sender.UserName == senderId && m.Receiver.UserName == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
        {
            var username = Context.User?.GetUserName();

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