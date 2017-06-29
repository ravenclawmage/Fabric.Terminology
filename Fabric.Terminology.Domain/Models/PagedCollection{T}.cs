﻿using System.Collections.Generic;
using System.Linq;

namespace Fabric.Terminology.Domain.Models
{
    public class PagedCollection<T> 
    {
        public int TotalItems { get; set; } = 0;
        public PagerSettings PagerSettings { get; set; } = new PagerSettings();
        public int TotalPages { get; set; } = 0;
        public IReadOnlyCollection<T> Items { get; set; } = new List<T>().AsReadOnly();

        public static PagedCollection<T> Empty()
        {
            return new PagedCollection<T>();
        }
    }
}
