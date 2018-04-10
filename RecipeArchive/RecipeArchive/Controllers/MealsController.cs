using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeArchive.Data;
using RecipeArchive.Extensions;
using RecipeArchive.Models;
using RecipeArchive.Models.DataViewModels;

namespace RecipeArchive.Controllers
{
    public class MealsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MealsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<MealDTO> GetMeals(string mealType)
        {

            var meals = from m in _context.Meal
                        select new MealDTO()
                        {
                            MealID = m.MealID,
                            Name = m.Name,
                            Stars = m.UserMeals.Sum(um =>(float) (um.Stars)) / m.UserMeals.Count,
                            MealTypeName = m.MealType.Name,
                            Picture = m.Picture
                        };

            if (!String.IsNullOrEmpty(mealType))
            {
                meals = meals.Where(m => m.MealTypeName == mealType);
            }

            return meals;
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
            int size = 2;
            return View(await PaginatedList<MealDTO>.CreateAsync(meals, page ?? 1 , size));
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
            mealView.Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / meal.UserMeals.Count;

            if (meal == null)
            {
                return NotFound();
            }

            return View(mealView);
        }

        // GET: Meals/Create
        public IActionResult Create()
        {
            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "MealTypeID");
            return View();
        }

        // POST: Meals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MealID,MealTypeID,Difficulty,Name,MakeTime,Description,Picture")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MealTypeID"] = new SelectList(_context.MealType, "MealTypeID", "MealTypeID", meal.MealTypeID);
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
