using System;
using System.Collections.Generic;
using System.Text;
using static RecipeArchive.Models.Meal;

namespace RecipeArchive.Models.DataViewModels
{
    public class MealViewModel
    {
        public int MealID { get; set; }

        public string MealTypeName { get; set; }

        public DifficultyStates Difficulty { get; set; }

        public string Name { get; set; }

        public int? MakeTime { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public float Stars { get; set; }

        public IEnumerable<Ingredient> Ingredients { get; set; }
    }
}
