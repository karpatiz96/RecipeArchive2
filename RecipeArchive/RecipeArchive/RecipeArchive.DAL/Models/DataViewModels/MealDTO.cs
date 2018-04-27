using RecipeArchive.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeArchive.Models
{
    public class MealDTO
    {
        public int MealID { get; set; }

        public string Name { get; set; }

        public string MealTypeName { get; set; }

        public float Stars { get; set; }

        public string Picture { get; set; }
    }
}
