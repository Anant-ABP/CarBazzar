using System.Collections.Generic;
using CarBazzar.Models.Cars;

namespace CarBazzar.Models.Messages;

public class MessagesViewModel
{
    public List<ConversationSummary> Conversations { get; set; } = new();
    public ConversationDetail? ActiveConversation { get; set; }
    public string CurrentUserId { get; set; } = "";
}

public class ConversationSummary
{
    public int CarId { get; set; }
    public string OtherUserId { get; set; } = "";
    public string OtherUserName { get; set; } = "";
    public string CarTitle { get; set; } = "";
    public string LastMessage { get; set; } = "";
    public bool IsActive { get; set; }
}

public class ConversationDetail
{
    public int CarId { get; set; }
    public string OtherUserId { get; set; } = "";
    public string OtherUserName { get; set; } = "";
    public CarCard CarInfo { get; set; } = null!;
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public string SenderId { get; set; } = "";
    public string MessageText { get; set; } = "";
    public string SentAtStr { get; set; } = "";
}
