using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        [DisplayName("Type")]
        public int MealTypeID { get; set; }

        [DisplayName("Difficulty")]
        public DifficultyStates Difficulty { get; set; }

        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Time")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Invalide")]
        [Range(0, int.MaxValue)]
        public int? MakeTime { get; set; }

        [DisplayName("Preparation")]
        public string Description { get; set; }

        [DisplayName("Picture")]
        public string Picture { get; set; }

        public virtual MealType MealType { get; set; }

        public virtual ICollection<Ingredient> Ingredients { get; set; }

        public virtual ICollection<UserMeal> UserMeals { get; set; }

    }
}
