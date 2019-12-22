using System;

namespace AzureUpskill.Models
{
    public class Category
    {
        public string Name { get; set; }

        public int CategoryId { get; set; }

        public string Type { get; set; } = nameof(Category);
    }
}
