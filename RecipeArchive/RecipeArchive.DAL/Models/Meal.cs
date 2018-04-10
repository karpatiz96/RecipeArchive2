using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeArchive.Models
{
    public class Meal
    {

        public enum DifficultyStates
        {
            Easy = 0,
            Normal = 1,
            Hard = 2
        }

        public int MealID { get; set; }

        public int MealTypeID { get; set; }

        public DifficultyStates Difficulty { get; set; }

        public string Name { get; set; }

        public int? MakeTime { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public virtual MealType MealType { get; set; }

        public virtual ICollection<Ingredient> Ingredients { get; set; }

        public virtual ICollection<UserMeal> UserMeals { get; set; }

    }
}
