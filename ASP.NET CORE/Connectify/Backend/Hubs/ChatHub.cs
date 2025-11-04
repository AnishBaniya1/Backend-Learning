using System;
using System.Collections.Concurrent;
using Backend.Data;
using Backend.DTOs;
using Backend.Extensions;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Hubs;

public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
{
    //ConcurrentDictionary → Thread-safe dictionary (safe to use in multi-threaded environments like SignalR hubs).
    //Key = username (string)
    //Value = OnlineUserDto (contains info like connection ID, full name, profile picture, etc.)
    //Keeps track of all currently connected users.Static Dictionary of online users
    public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();

    //s automatically called when a client connects to the hub.
    public override async Task OnConnectedAsync()
    {
        //Get the HTTP context:Sometimes you need query parameters or headers when a client connects.
        var httpContext = Context.GetHttpContext();
        //Get receiverId query param:ptional, maybe used for redirecting messages
        var receiverId = httpContext?.Request.Query["receiverId"].ToString();
        //SignalR sets Context.User automatically if your app uses authentication.
        var userName = Context.User!.Identity!.Name;
        //Fetches the AppUser object from the database.
        var currentUser = await userManager.FindByNameAsync(userName!);
        //Get connection ID:Each SignalR client connection has a unique ID.
        var connectionId = Context.ConnectionId;

        //Update onlineUsers dictionary:
        if (onlineUsers.ContainsKey(userName!))
        {
            //If the user already exists in onlineUsers → update their connection ID (reconnect scenario).
            onlineUsers[userName!].ConnectionId = connectionId;
        }
        else //If new user doesn't exist → create OnlineUserDto 
        {
            var user = new OnlineUserDto
            {
                ConnectionId = connectionId,
                UserName = userName,
                ProfilePicture = currentUser!.ProfileImage,
                FullName = currentUser!.FullName
            };
            //add to dictionary.
            onlineUsers.TryAdd(userName!, user);
            //Notify all other users that this user is online.
            await Clients.AllExcept(connectionId).SendAsync("Notify", currentUser);
        }
        // If receiverId is provided → automatically load chat messages for that conversation
        if (!string.IsNullOrEmpty(receiverId))
        {
            await LoadMessages(receiverId);
        }
        //Send the updated list of online users to all clients:Calls GetAllUsers()→ sends the list of users with their online/offline status.
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }

    // Loads chat messages between the current user and a recipient, with pagination support
    public async Task LoadMessages(string recipientId, int pageNumber = 1)
    {
        // Number of messages to load per request (for pagination)
        int pageSize = 10;
        // Get the currently connected user's info
        var username = Context.User!.Identity!.Name;
        var currentUser = await userManager.FindByNameAsync(username!);
        // Stop if user not found
        if (currentUser is null)
        {
            return;
        }
        // Fetch paginated chat messages between current user and recipient
        // Filters messages between the current user and the recipient (both directions).
        //Ensures both sent and received messages are included in the conversation.
        List<MessageResponseDto> messages = await context.Messages.Where(x => x.ReceiverId == currentUser!.Id
        && x.SenderId == recipientId
        || x.SenderId == currentUser!.Id && x.ReceiverId == recipientId)
        .OrderByDescending(x => x.CreatedDate) // Get latest first
        .Skip((pageNumber - 1) * pageSize)// Skip previous pages
        .Take(pageSize)// Take only one page worth of messages
        .OrderBy(x => x.CreatedDate)// Reorder oldest → newest
        .Select(x => new MessageResponseDto
        {
            Id = x.Id,
            Content = x.Content,
            SenderId = x.SenderId,
            ReceiverId = x.ReceiverId,
            CreatedDate = x.CreatedDate
        }).ToListAsync();
        // Mark all messages received by this user as read
        foreach (var message in messages)
        {
            var msg = await context.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
            if (msg != null && msg.ReceiverId == currentUser.Id)
            {
                msg.IsRead = true;
                await context.SaveChangesAsync();
            }
        }
        // Send the message list back to the current user
        await Clients.User(currentUser.Id).SendAsync("ReceiveMessageList", messages);
    }

    //Purpose: Handle sending a message from one user to another.
    public async Task SendMessage(MessageRequestDto message)
    {
        //Get sender username → from authenticated Context.User.
        var senderId = Context.User!.Identity!.Name;
        //Get recipient ID → from MessageRequestDto.
        var recipientId = message.ReceiverId;

        //Create new message → set sender, receiver, content, timestamp.
        var newMsg = new Message
        {
            Sender = await userManager.FindByNameAsync(senderId!),
            Receiver = await userManager.FindByIdAsync(recipientId!),
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            Content = message.Content
        };
        //Save message to database → context.Messages.Add + SaveChangesAsync.
        context.Messages.Add(newMsg);
        await context.SaveChangesAsync();
        //Send message to recipient
        await Clients.User(recipientId!).SendAsync("ReceiveNewMessage", newMsg);
    }

    // Notifies a specific user in real-time that another user has started typing
    public async Task NotifyTyping(string recipientUserName)
    {
        // Get the username of the current (typing) user from the SignalR context
        var senderUserName = Context.User!.Identity!.Name;
        // Safety check — stop if the sender is not authenticated
        if (senderUserName is null)
        {
            return;
        }
        // Find the recipient's SignalR connection ID from the online users list
        var connectionId = onlineUsers.Values.FirstOrDefault(x => x.UserName == recipientUserName)?.ConnectionId;
        // If the recipient is online, send them a real-time typing notification
        if (connectionId != null)
        {
            await Clients.Client(connectionId).SendAsync("NotifyTypingToUser", senderUserName);
        }
    }

    // Triggered automatically when a client disconnects from the SignalR hub
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Get the username of the disconnected user from the connection context
        var username = Context.User!.Identity!.Name;

        // Safely remove the user from the online users dictionary
        onlineUsers.TryRemove(username!, out _);

        // Notify all remaining clients with the updated online user list
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }
    //Returns a list of all users, marking which ones are online and their unread messages count.
    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        //Represents the current authenticated user connected to SignalR.
        var username = Context.User!.GetUserName();
        // Create a set of online usernames for fast lookup
        var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);

        //Query all users and map to OnlineUserDto
        //Allows you to query users directly without loading them all into memory, and Maps each AppUser to an OnlineUserDto object
        var users = await userManager.Users.Select(u => new OnlineUserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            ProfilePicture = u.ProfileImage,
            //checks if the user is online.
            IsOnline = onlineUsersSet.Contains(u.UserName!),
            //counts messages that are sent to the current user but not read.
            UnreadCount = context.Messages.Count(x => x.ReceiverId == username && x.SenderId == u.Id && !x.IsRead)
        }).OrderByDescending(u => u.IsOnline).ToListAsync();//Sorts users so online users appear first.

        //Sends the list of all users with online status and unread count back to whoever called GetAllUsers()
        return users;
    }
}
