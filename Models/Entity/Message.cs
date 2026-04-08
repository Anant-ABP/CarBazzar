using System;
using System.Collections.Generic;

namespace CarBazzar.Models.Entity;

public partial class Message
{
    public int Id { get; set; }

    public string? SenderId { get; set; }

    public string? ReceiverId { get; set; }

    public int CarId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ApplicationUser? Receiver { get; set; }

    public virtual ApplicationUser? Sender { get; set; }
}
