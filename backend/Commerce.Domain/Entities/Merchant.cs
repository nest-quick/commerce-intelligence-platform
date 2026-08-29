using System;
using System.Collections.Generic;
using System.Text;

namespace Commerce.Domain.Entities;

public class Merchant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<Store> Stores { get; set; } = new List<Store>();
}
