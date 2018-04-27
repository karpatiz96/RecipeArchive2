using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public int? MakeTime { get; set; }

        [DisplayName("Difficulty")]
        public int? Difficulty { get; set; }
    }
}
