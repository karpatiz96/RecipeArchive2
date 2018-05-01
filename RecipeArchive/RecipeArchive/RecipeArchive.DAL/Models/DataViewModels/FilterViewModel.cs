using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using RecipeArchive.DAL.Models;

namespace RecipeArchive.Models.DataViewModels
{
    public class FilterViewModel
    {
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Type")]
        public string MealType { get; set; }

        [DisplayName("Time")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Invalide Time >= 0")]
        [Range(0, int.MaxValue)]
        public int? MakeTime { get; set; }

        [DisplayName("Difficulty")]
        public int? Difficulty { get; set; }
    }
}
