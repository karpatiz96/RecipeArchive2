using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RecipeArchive.Models.DataViewModels
{
    public class HomeMealViewModel
    {

        public IEnumerable<MealType> mealTypes { get; set; }

        public List<IEnumerable<MealDTO>> meals { get; set; }

    }
}
