using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeArchive.Data;
using RecipeArchive.Models;

namespace RecipeArchive.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ingredient.Include(i => i.Meal);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredient
                .Include(i => i.Meal)
                .SingleOrDefaultAsync(m => m.IngredientID == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredients/Create
        public IActionResult Create(int mealid)
        {
            ViewData["MealID"] = new SelectList(_context.Meal, "MealID", "MealID");
            ViewBag.MealID = mealid;
            return View();
        }

        // POST: Ingredients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MealID,Name,Quantity")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {

                Meal meal = _context.Meal.SingleOrDefault(m => m.MealID == ingredient.MealID);

                if (meal != null)
                {
                    ingredient.Meal = meal;
                    _context.Add(ingredient);
                    await _context.SaveChangesAsync();

                    meal.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Edit), "Ingredients", new { id = ingredient.IngredientID });
                //return RedirectToAction(nameof(Details),"Meals",new { id = ingredient.MealID });
            }
            ViewData["MealID"] = new SelectList(_context.Meal, "MealID", "MealID", ingredient.MealID);
            ViewBag.MealID = ingredient.MealID;
            return RedirectToAction(nameof(Details),"Meals",new { id = ingredient.MealID });
        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredient.SingleOrDefaultAsync(m => m.IngredientID == id);
            if (ingredient == null)
            {
                return NotFound();
            }
            ViewData["MealID"] = new SelectList(_context.Meal, "MealID", "MealID", ingredient.MealID);
            return RedirectToAction(nameof(Details), "Meals", new { id = ingredient.MealID, ingredientID = ingredient.IngredientID });
        }

        // POST: Ingredients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IngredientID,MealID,Name,Quantity")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(ingredient.IngredientID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), "Meals", new { id = ingredient.MealID, ingredientID = -1 });
            }
            ViewData["MealID"] = new SelectList(_context.Meal, "MealID", "MealID", ingredient.MealID);
            return View(ingredient);
        }

        // GET: Ingredients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredient
                .Include(i => i.Meal)
                .SingleOrDefaultAsync(m => m.IngredientID == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            Meal meal = await _context.Meal.SingleOrDefaultAsync(m => m.MealID == ingredient.MealID);
            meal.Ingredients.Remove(ingredient);
            _context.Ingredient.Remove(ingredient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), "Meals", new { id = ingredient.MealID, ingredientID = -1 });
        }

        // POST: Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var ingredient = await _context.Ingredient.SingleOrDefaultAsync(m => m.IngredientID == id);

            if (ingredient == null)
            {
                return NotFound();
            }

            Meal meal = await _context.Meal.SingleOrDefaultAsync(m => m.MealID == ingredient.MealID );
            meal.Ingredients.Remove(ingredient);
            _context.Ingredient.Remove(ingredient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), "Meals", new { id = ingredient.MealID, ingredientID = -1 });
        }

        private bool IngredientExists(int id)
        {
            return _context.Ingredient.Any(e => e.IngredientID == id);
        }
    }
}
