using System;
using System.Collections.Generic;

namespace CarBazzar.Models.Entity;

public partial class CarImage
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public virtual Car Car { get; set; } = null!;
}
