using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeArchive.Data;
using RecipeArchive.Extensions;
using RecipeArchive.Models;
using RecipeArchive.Models.DataViewModels;
using static RecipeArchive.Models.Meal;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Identity;

namespace RecipeArchive.Controllers
{
    public class MealsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IHostingEnvironment _hosting;

        private readonly UserManager<ApplicationUser> _userManager;

        public MealsController(ApplicationDbContext context, IHostingEnvironment hosting, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hosting = hosting;
            _userManager = userManager;
        }

        public IQueryable<MealDTO> GetMeals(string mealType)
        {

            var meals = from m in _context.Meal
                        select new MealDTO()
                        {
                            MealID = m.MealID,
                            Name = m.Name,
                            Stars = m.UserMeals.Sum(um =>(float) (um.Stars)) / m.UserMeals.Count(us => us.Stars != null),
                            MealTypeName = m.MealType.Name,
                            Picture = m.Picture
                        };

            if (!String.IsNullOrEmpty(mealType))
            {
                meals = meals.Where(m => m.MealTypeName == mealType);
            }

            return meals;
        }

        public MealDTO GetMealDTO(Meal meal) {
            MealDTO mealDTO = new MealDTO {
                MealID = meal.MealID,
                Name = meal.Name,
                Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / meal.UserMeals.Count(us => us.Stars != null),
                MealTypeName = meal.MealType.Name,
                Picture = meal.Picture
            };
            return mealDTO;
        }

        // GET: Meals
        public async Task<IActionResult> Index()
        {

            HomeMealViewModel homeMealViewModel = new HomeMealViewModel();

            var mealTypes = from mt in _context.MealType
                            select mt;

            homeMealViewModel.mealTypes = mealTypes;
            homeMealViewModel.meals = new List<IEnumerable<MealDTO>>();

            foreach (var type in mealTypes)
            {
                homeMealViewModel.meals.Add(GetMeals(type.Name));
            }

            return View(homeMealViewModel);
        }

        public async Task<IActionResult> Type(string type, int? page, string currentFilter)
        {

            if (type != null)
            {
                page = 1;
            }
            else
            {
                type = currentFilter;
            }

            ViewData["CurrentFilter"] = type;

            IQueryable<MealDTO> meals = GetMeals(type);
            int size = 3;
            return View(await PaginatedList<MealDTO>.CreateAsync(meals, page ?? 1 , size));
        }

        public async Task<IActionResult> RecipeBook()
        {
            RecipeBookViewModel recipeBookViewModel = new RecipeBookViewModel();

            var mealTypes = from mt in _context.MealType
                            select mt;

            recipeBookViewModel.mealTypes = mealTypes;
            recipeBookViewModel.meals = new List<IEnumerable<MealDTO>>();

            foreach (MealType type in mealTypes) {
                var userMeals = await _context.UserMeal.Include(us => us.Meal).ThenInclude(m => m.MealType).Where(us => us.RecipeBook == true).AsNoTracking().ToListAsync();
                List<MealDTO> meals = new List<MealDTO>();
                foreach (UserMeal userMeal in userMeals) {
                    if (userMeal.Meal.MealTypeID == type.MealTypeID) {
                        meals.Add(GetMealDTO(userMeal.Meal));
                    }
                }
                recipeBookViewModel.meals.Add(meals);
            }

            return View(recipeBookViewModel);
        }



        // GET: Meals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meal
                .Include(m => m.MealType)
                .Include(m => m.UserMeals)
                .SingleOrDefaultAsync(m => m.MealID == id);

            var Ingredients = await _context.Ingredient.Where(i => i.MealID == id).ToListAsync();

            MealViewModel mealView = new MealViewModel();

            mealView.Ingredients = Ingredients;
            mealView.Description = meal.Description;
            mealView.Difficulty = meal.Difficulty;
            mealView.MakeTime = meal.MakeTime;
            mealView.MealID = meal.MealID;
            mealView.MealTypeName = meal.MealType.Name;
            mealView.Name = meal.Name;
            mealView.Picture = meal.Picture;
            mealView.Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / meal.UserMeals.Count(us => us.Stars != null);

            if (meal == null)
            {
                return NotFound();
            }

            return View(mealView);
        }

        // GET: Meals/Create
        public IActionResult Create()
        {

            var difficultyStates = new List<SelectListItem>();
            difficultyStates.Add(new SelectListItem {
                Text = "Select",
                Value = ""
            });

            foreach (DifficultyStates item in Enum.GetValues(typeof(DifficultyStates))) {
                difficultyStates.Add(new SelectListItem { Text = Enum.GetName(typeof(DifficultyStates), item), Value = item.ToString() });
            }

            ViewBag.diffState = difficultyStates;

            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "Name");
            return View();
        }

        // POST: Meals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MealTypeID,Difficulty,Name,MakeTime,Description,Picture")] Meal meal, IFormFile Picture)
        {

            if (ModelState.IsValid)
            {

                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

                MealType mealType = _context.MealType.SingleOrDefault(mt => mt.MealTypeID == meal.MealTypeID);
                meal.MealType = mealType;
                _context.Add(meal);
                await _context.SaveChangesAsync();

                if (Picture != null && Picture.Length > 0) {

                    var file = Picture;
                    var upload = Path.Combine(_hosting.WebRootPath, "images\\imgMeal");
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = Path.GetFileName(file.FileName);
                    if (file.Length > 0) {
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfileName = name + '_' + meal.MealID + extension;

                        using (var fileStream = new FileStream(Path.Combine(upload, myfileName), FileMode.Create)) {
                            await file.CopyToAsync(fileStream);
                            meal.Picture = myfileName;//Path.Combine(upload, myfileName);
                            await _context.SaveChangesAsync();
                        }
                    }

                }

                UserMeal userMeal = new UserMeal();
                userMeal.AlreadyMade = false;
                userMeal.Favourite = false;
                userMeal.RecipeBook = true;
                userMeal.Stars = 0;
                userMeal.MealID = meal.MealID;
                if (user != null)
                {
                    userMeal.UserID = user.Id;
                }
                _context.Add(userMeal);
                await _context.SaveChangesAsync();

                if (user != null)
                {
                    userMeal.User = user;
                    user.UserMeals.Add(userMeal);
                }

                meal.UserMeals.Add(userMeal);
                userMeal.Meal = meal;

                mealType.Meals.Add(meal);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "Name", meal.MealTypeID);
            return View(meal);
        }

        // GET: Meals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meal.SingleOrDefaultAsync(m => m.MealID == id);
            if (meal == null)
            {
                return NotFound();
            }
            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "MealTypeID", meal.MealTypeID);
            return View(meal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MealID,MealTypeID,Difficulty,Name,MakeTime,Description,Picture")] Meal meal)
        {
            if (id != meal.MealID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealExists(meal.MealID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "MealTypeID", meal.MealTypeID);
            return View(meal);
        }

        // GET: Meals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meal
                .Include(m => m.MealType)
                .SingleOrDefaultAsync(m => m.MealID == id);
            if (meal == null)
            {
                return NotFound();
            }

            return View(meal);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meal = await _context.Meal.SingleOrDefaultAsync(m => m.MealID == id);
            _context.Meal.Remove(meal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MealExists(int id)
        {
            return _context.Meal.Any(e => e.MealID == id);
        }
    }
}
