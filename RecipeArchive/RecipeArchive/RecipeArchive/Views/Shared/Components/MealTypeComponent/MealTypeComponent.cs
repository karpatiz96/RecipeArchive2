using Microsoft.AspNetCore.Mvc;
using RecipeArchive.Data;
using RecipeArchive.Models.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeArchive.Views.Shared.Components.MealTypeComponent
{

    [ViewComponent(Name = "MealTypeComponent")]
    public class MealTypeComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public MealTypeComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var mealType = from mt in _context.MealType
                           select mt;

            MealTypeViewModel mealTypeViewModel = new MealTypeViewModel();
            mealTypeViewModel.MealTypes = await mealType.ToAsyncEnumerable().ToList();
            return View(mealTypeViewModel);
        }
    }
}
