using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeArchive.Models.DataViewModels
{
    public class RecipeBookViewModel
    {
        public IEnumerable<MealType> mealTypes { get; set; }

        public List<IEnumerable<MealDTO>> meals { get; set; }
    }
}
