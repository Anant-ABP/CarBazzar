using System;
using System.Threading.Tasks;
using CarBazzar.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarBazzar.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly CarBazaarContext _context;

        public ChatHub(CarBazaarContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int carId, string receiverId, string messageText)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId)) return;

            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                CarId = carId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            string timeStr = msg.SentAt.ToLocalTime().ToString("t");
            await Clients.Users(receiverId, senderId).SendAsync("ReceiveMessage", carId, senderId, messageText, timeStr);
        }
    }
}
