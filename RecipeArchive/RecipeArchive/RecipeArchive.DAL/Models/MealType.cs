using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeArchive.Models
{
    public class MealType
    {
        
        public int MealTypeID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Meal> Meals { get; set; }

    }
}
