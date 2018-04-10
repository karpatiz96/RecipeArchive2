using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeArchive.Extensions
{
    public class PaginatedList<T> : List<T>
    {

        public int Index { get; set; }

        public int Total { get; set; }

        public PaginatedList(List<T> items, int count, int index, int size) {
            Index = index;
            Total = (int)Math.Ceiling(count / (double)size);

            this.AddRange(items);
        }

        public bool HasPrevious { get { return (Index > 1); } }

        public bool HasNext { get { return (Index < Total); } }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int index, int size) {
            var count = await source.CountAsync();
            var items = await source.Skip((index - 1) * size).Take(size).ToListAsync();
            return new PaginatedList<T>(items, count, index, size);
        }

    }
}
