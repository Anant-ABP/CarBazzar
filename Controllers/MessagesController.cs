using CarBazzar.Models.Cars;
using CarBazzar.Models.Entity;
using CarBazzar.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CarBazzar.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly CarBazaarContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(CarBazaarContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? carId, string? otherUserId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null) return Challenge();

        var model = new MessagesViewModel { CurrentUserId = currentUserId };

        // 1. Gather all messages user is involved in
        var allMyMessages = await _context.Messages
            .Include(m => m.Car)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        // Group into conversations by (CarId, TheOtherPersonId)
        var grouped = allMyMessages
            .GroupBy(m => new 
            { 
                CarId = m.CarId, 
                OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId 
            })
            .ToList();

        // 2. Identify the active conversation
        int activeCarId = 0;
        string activeOtherUserId = "";

        if (carId.HasValue && !string.IsNullOrEmpty(otherUserId))
        {
            activeCarId = carId.Value;
            activeOtherUserId = otherUserId;
        }
        else if (carId.HasValue)
        {
            // Usually means navigated from Details page as buyer
            var car = await _context.Cars.FindAsync(carId.Value);
            if (car != null && car.SellerId != currentUserId)
            {
                activeCarId = carId.Value;
                activeOtherUserId = car.SellerId ?? "";
            }
        }
        
        // If still no active selected, try to pop the most recent conversation
        if (activeCarId == 0 && grouped.Any())
        {
            var latest = grouped.First();
            activeCarId = latest.Key.CarId;
            activeOtherUserId = latest.Key.OtherUserId ?? "";
        }

        // Build Conversation Summary List
        foreach (var gp in grouped)
        {
            var firstMsg = gp.First();
            string oName = "Unknown User";
            if (firstMsg.SenderId == gp.Key.OtherUserId)
                oName = firstMsg.Sender != null ? $"{firstMsg.Sender.FirstName} {firstMsg.Sender.LastName}" : "Unknown";
            else
                oName = firstMsg.Receiver != null ? $"{firstMsg.Receiver.FirstName} {firstMsg.Receiver.LastName}" : "Unknown";

            model.Conversations.Add(new ConversationSummary
            {
                CarId = gp.Key.CarId,
                OtherUserId = gp.Key.OtherUserId ?? "",
                OtherUserName = oName,
                CarTitle = firstMsg.Car?.Title ?? "Unknown Car",
                LastMessage = firstMsg.MessageText,
                IsActive = (gp.Key.CarId == activeCarId && gp.Key.OtherUserId == activeOtherUserId)
            });
        }

        // 3. Load Active Conversation
        if (activeCarId != 0 && !string.IsNullOrEmpty(activeOtherUserId))
        {
            var car = await _context.Cars.Include(c => c.Seller).FirstOrDefaultAsync(c => c.Id == activeCarId);
            if (car != null)
            {
                // If it's a brand new conversation from a buyer, it might not be in 'grouped' yet, so add it to summaries
                if (!model.Conversations.Any(c => c.IsActive))
                {
                    string oName = "Unknown";
                    if (car.SellerId == activeOtherUserId && car.Seller != null)
                        oName = $"{car.Seller.FirstName} {car.Seller.LastName}";
                    
                    model.Conversations.Insert(0, new ConversationSummary
                    {
                        CarId = activeCarId,
                        OtherUserId = activeOtherUserId,
                        OtherUserName = oName,
                        CarTitle = car.Title,
                        LastMessage = "Start of conversation...",
                        IsActive = true
                    });
                }
                
                var activePartner = await _userManager.FindByIdAsync(activeOtherUserId);
                string partnerName = activePartner != null ? $"{activePartner.FirstName} {activePartner.LastName}" : "Unknown";

                var chatDbMsgs = allMyMessages
                    .Where(m => m.CarId == activeCarId && 
                                (m.SenderId == activeOtherUserId || m.ReceiverId == activeOtherUserId))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                var convDetail = new ConversationDetail
                {
                    CarId = activeCarId,
                    OtherUserId = activeOtherUserId,
                    OtherUserName = partnerName,
                    CarInfo = MapToCard(car),
                    Messages = chatDbMsgs.Select(m => new MessageDto
                    {
                        SenderId = m.SenderId ?? "",
                        MessageText = m.MessageText,
                        SentAtStr = m.SentAt.ToLocalTime().ToString("t")
                    }).ToList()
                };

                // Add starter message if completely empty
                if (!convDetail.Messages.Any())
                {
                    convDetail.Messages.Add(new MessageDto
                    {
                        SenderId = activeOtherUserId,
                        MessageText = $"Hi there! I am the seller for the {car.Title}. Do you have any questions?",
                        SentAtStr = "System"
                    });
                }

                model.ActiveConversation = convDetail;
            }
        }

        return View(model);
    }

    private CarCard MapToCard(Car c)
    {
        return new CarCard
        {
            Id = c.Id,
            Title = c.Title,
            Specs = c.Model,
            Price = "₹" + c.Price.ToString("N0"),
            ImageUrl = Url.Action("GetImage", "Cars", new { id = c.Id }) ?? "",
            Year = c.Year,
            FuelType = c.FuelType,
            Mileage = c.Mileage + " km",
            Transmission = c.Transmission,
            BodyType = c.Brand,
            Color = "Unknown",
            Description = c.Description ?? "",
            Features = new List<string>(),
            DealerName = c.Seller != null && !string.IsNullOrEmpty(c.Seller.FirstName) 
                ? $"{c.Seller.FirstName} {c.Seller.LastName}" 
                : "Unknown Seller",
            DealerPhone = c.Seller?.PhoneNumber ?? "",
            DealerLocation = c.Location,
            SellerId = c.SellerId ?? ""
        };
    }
}
