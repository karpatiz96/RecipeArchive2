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
                        select m;

            meals = meals.Include(m => m.UserMeals).Include(m => m.MealType);

            List<MealDTO> mealDTOs = new List<MealDTO>();

            foreach (Meal item in meals) {
                mealDTOs.Add(GetMealDTO(item));
            }

            IEnumerable<MealDTO> MealDTOs = mealDTOs.AsEnumerable();

            if (!String.IsNullOrEmpty(mealType))
            {
                MealDTOs = MealDTOs.Where(m => m.MealTypeName == mealType);
            }

            return MealDTOs.AsQueryable();
        }

        public MealDTO GetMealDTO(Meal meal) {
            int count = meal.UserMeals.Count(us => us.Stars != null && us.Stars != 0);
            if (count <= 0) {
                count = 1;
            }
            MealDTO mealDTO = new MealDTO {
                MealID = meal.MealID,
                Name = meal.Name,
                Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / count,
                MealTypeName = meal.MealType.Name,
                Picture = meal.Picture
            };
            return mealDTO;
        }

        // GET: Meals
        public async Task<IActionResult> Index()
        {

            RecipeBookViewModel recipeBookViewModel = new RecipeBookViewModel();

            var mealTypes = from mt in _context.MealType
                            select mt;

            recipeBookViewModel.mealTypes = mealTypes;
            recipeBookViewModel.meals = new List<IEnumerable<MealDTO>>();

            int size = 9;

            foreach (var type in mealTypes)
            {
                IQueryable<MealDTO> mealDTOs = GetMeals(type.Name);
                mealDTOs = mealDTOs.OrderByDescending(m => m.Stars);
                mealDTOs = mealDTOs.Take(size).AsQueryable();
                recipeBookViewModel.meals.Add(mealDTOs);
            }

            return View(recipeBookViewModel);
        }

        public IActionResult Filter() {
            var difficultyStates = new List<SelectListItem>();
            difficultyStates.Add(new SelectListItem
            {
                Text = "",
                Value = null
            });

            foreach (DifficultyStates item in Enum.GetValues(typeof(DifficultyStates))) {
                difficultyStates.Add(new SelectListItem { Text = Enum.GetName(typeof(DifficultyStates), item), Value = ((int)item).ToString() });
            }

            var mealTypes = new List<SelectListItem>();
            mealTypes.Add(new SelectListItem {
                Text = "",
                Value = null
            });

            var mealType = _context.MealType;

            foreach (MealType type in mealType) {
                mealTypes.Add(new SelectListItem { Text = type.Name, Value = type.Name.ToString() });
            }

            ViewBag.difficultyStates = difficultyStates;
            ViewBag.mealTypes = mealTypes;
            return View();
        }

        public async Task<IActionResult> Search([Bind("Name,MealType,MakeTime,Difficulty")] FilterViewModel filter, int? page, 
            string currentName, int currentTime, string currentType, int currentDifficulty) {

            if (ModelState.IsValid)
            {
                var meals = from m in _context.Meal
                            select m;

                meals = meals.Include(m => m.MealType).Include(m => m.UserMeals);

                if (page == null)
                {
                    page = 1;
                }
                else
                {
                    filter.Name = currentName;
                    if (currentTime >= 0)
                    {
                        filter.MakeTime = currentTime;
                    }
                    filter.MealType = currentType;
                    if (currentDifficulty >= 0)
                    {
                        filter.Difficulty = currentDifficulty;
                    }
                }

                if (!String.IsNullOrEmpty(filter.Name))
                {
                    meals = meals.Where(m => m.Name.Contains(filter.Name));
                    ViewBag.Name = filter.Name;
                }
                else
                {
                    ViewBag.Name = "";
                }
                if (!String.IsNullOrEmpty(filter.MealType))
                {
                    meals = meals.Where(m => m.MealType.Name == filter.MealType);
                    ViewBag.Type = filter.MealType;
                }
                else
                {
                    ViewBag.Type = "";
                }
                if (filter.Difficulty >= 0)
                {
                    meals = meals.Where(m => (int)m.Difficulty == filter.Difficulty);
                    ViewBag.Difficulty = filter.Difficulty;
                }
                else
                {
                    ViewBag.Difficulty = -1;
                }
                if (filter.MakeTime != null)
                {
                    meals = meals.Where(m => m.MakeTime == filter.MakeTime);
                    ViewBag.Time = filter.MakeTime;
                }
                else
                {
                    ViewBag.Time = -1;
                }

                int size = 9;

                List<MealDTO> mealItems = new List<MealDTO>();

                foreach (Meal meal in meals)
                {
                    mealItems.Add(GetMealDTO(meal));
                }

                IAsyncEnumerable<MealDTO> mealDTOs = mealItems.ToAsyncEnumerable();

                return View(await PaginatedList<MealDTO>.CreateAsync(mealDTOs, page ?? 1, size));

            }

            return RedirectToAction(nameof(Filter), "Meals");
        }

        public async Task<IActionResult> SearchName(string searchName, int? page, string currentName) {
            var meals = from m in _context.Meal
                        select m;

            meals = meals.Include(m => m.MealType).Include(m => m.UserMeals);

            if (searchName != null)
            {
                page = 1;
            } else {
                searchName = currentName;
            }

            if (!String.IsNullOrEmpty(searchName))
            {
                meals = meals.Where(m => m.Name.Contains(searchName));
                ViewBag.Name = searchName;
            } else {
                ViewBag.Name = "";
            }

            int size = 9;

            List<MealDTO> mealItems = new List<MealDTO>();

            foreach (Meal meal in meals)
            {
                mealItems.Add(GetMealDTO(meal));
            }

            IAsyncEnumerable<MealDTO> mealDTOs = mealItems.ToAsyncEnumerable();

            return View(await PaginatedList<MealDTO>.CreateAsync(mealDTOs, page ?? 1, size));
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

            IAsyncEnumerable<MealDTO> meals = GetMeals(type).ToAsyncEnumerable();
            int size = 9;
            return View(await PaginatedList<MealDTO>.CreateAsync(meals, page ?? 1 , size));
        }

        public async Task<IActionResult> RecipeBook()
        {
            RecipeBookViewModel recipeBookViewModel = new RecipeBookViewModel();

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var mealTypes = from mt in _context.MealType
                            select mt;

            recipeBookViewModel.mealTypes = mealTypes;
            recipeBookViewModel.meals = new List<IEnumerable<MealDTO>>();

            foreach (MealType type in mealTypes) {
                var userMeals = await _context.UserMeal.Include(us => us.Meal).ThenInclude(m => m.MealType).Where(us => us.RecipeBook == true && us.UserID == user.Id).AsNoTracking().ToListAsync();
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

        public async Task<IActionResult> Favourite()
        {
            RecipeBookViewModel recipeBookViewModel = new RecipeBookViewModel();

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var mealTypes = from mt in _context.MealType
                            select mt;

            recipeBookViewModel.mealTypes = mealTypes;
            recipeBookViewModel.meals = new List<IEnumerable<MealDTO>>();

            foreach (MealType type in mealTypes)
            {
                var userMeals = await _context.UserMeal.Include(us => us.Meal).ThenInclude(m => m.MealType).Where(us => us.Favourite == true && us.UserID == user.Id).AsNoTracking().ToListAsync();
                List<MealDTO> meals = new List<MealDTO>();
                foreach (UserMeal userMeal in userMeals)
                {
                    if (userMeal.Meal.MealTypeID == type.MealTypeID)
                    {
                        meals.Add(GetMealDTO(userMeal.Meal));
                    }
                }
                recipeBookViewModel.meals.Add(meals);
            }

            return View(recipeBookViewModel);
        }

        public async Task<IActionResult> AlreadyMade()
        {
            RecipeBookViewModel recipeBookViewModel = new RecipeBookViewModel();

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var mealTypes = from mt in _context.MealType
                            select mt;

            recipeBookViewModel.mealTypes = mealTypes;
            recipeBookViewModel.meals = new List<IEnumerable<MealDTO>>();

            foreach (MealType type in mealTypes)
            {
                var userMeals = await _context.UserMeal.Include(us => us.Meal).ThenInclude(m => m.MealType).Where(us => us.AlreadyMade == true && us.UserID == user.Id).AsNoTracking().ToListAsync();
                List<MealDTO> meals = new List<MealDTO>();
                foreach (UserMeal userMeal in userMeals)
                {
                    if (userMeal.Meal.MealTypeID == type.MealTypeID)
                    {
                        meals.Add(GetMealDTO(userMeal.Meal));
                    }
                }
                recipeBookViewModel.meals.Add(meals);
            }

            return View(recipeBookViewModel);
        }

        public async Task<IActionResult> Rate(int? mealID, int Rating) {
            if (mealID == null) {
                return NotFound();
            }

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            UserMeal userMeal = await _context.UserMeal.Where(us => us.UserID == user.Id && us.MealID == mealID).SingleOrDefaultAsync();

            Meal meal = await _context.Meal.Where(m => m.MealID == mealID).SingleOrDefaultAsync();

            if (userMeal != null)
            {
                userMeal.Stars = Rating;
                _context.Update(userMeal);
                await _context.SaveChangesAsync();
            } else {
                try
                {
                    userMeal = new UserMeal();
                    userMeal.AlreadyMade = false;
                    userMeal.Favourite = false;
                    userMeal.RecipeBook = false;
                    userMeal.Stars = Rating;
                    userMeal.MealID = meal.MealID;
                    userMeal.Meal = meal;
                    if (user != null)
                    {
                        userMeal.UserID = user.Id;
                        userMeal.User = user;
                    }

                    _context.Add(userMeal);
                    await _context.SaveChangesAsync();

                    user.UserMeals.Add(userMeal);
                    meal.UserMeals.Add(userMeal);

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                }
            }

            return RedirectToAction(nameof(Details), "Meals", new { id = mealID });
        }

        // GET: Meals/Details/5
        public async Task<IActionResult> Details(int? id, int? ingredientID)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ingredientID != null)
            {
                ViewBag.ingredientID = ingredientID;
            } else {
                ViewBag.ingredientID = -1;
            }

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var meal = await _context.Meal
                .Include(m => m.MealType)
                .Include(m => m.UserMeals)
                .SingleOrDefaultAsync(m => m.MealID == id);

            var Ingredients = await _context.Ingredient.Where(i => i.MealID == id).ToListAsync();

            int count = meal.UserMeals.Count(us => us.Stars != null && us.Stars != 0);
            if (count <= 0)
            {
                count = 1;
            }

            MealViewModel mealView = new MealViewModel();

            mealView.Ingredients = Ingredients;
            mealView.Description = meal.Description;
            mealView.Difficulty = meal.Difficulty;
            mealView.MakeTime = meal.MakeTime;
            mealView.MealID = meal.MealID;
            mealView.MealTypeName = meal.MealType.Name;
            mealView.Name = meal.Name;
            mealView.Picture = meal.Picture;
            mealView.Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / count;

            if (meal == null)
            {
                return NotFound();
            }

            var ratingStars = new List<SelectListItem>();

            for (int i = 1; i <= 5; i++) {
                ratingStars.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            if (user != null)
            {
                ViewBag.In = true;
                UserMeal userMeal = await _context.UserMeal.Where(us => us.MealID == meal.MealID && us.RecipeBook == true).SingleOrDefaultAsync();

                UserMeal userMeal2 = await _context.UserMeal.Where(us => us.MealID == meal.MealID  && us.UserID == user.Id).SingleOrDefaultAsync();

                if (userMeal2 != null ) {
                    if (userMeal2.Stars != null && userMeal2.Stars <= 0) {
                        ViewBag.Stars = true;
                        ViewBag.Ratings = ratingStars;
                    } else {
                        ViewBag.Stars = false;
                    }

                    if (userMeal2.Favourite == true)
                    {
                        ViewBag.Favourite = false;
                    } else {
                        ViewBag.Favourite = true;
                    }

                    if (userMeal2.AlreadyMade == true)
                    {
                        ViewBag.AlreadyMade = false;
                    } else {
                        ViewBag.AlreadyMade = true;
                    }

                } else {
                    ViewBag.Stars = true;
                    ViewBag.Ratings = ratingStars;
                    ViewBag.Favourite = true;
                    ViewBag.AlreadyMade = true;
                }

                if (userMeal != null && userMeal.UserID == user.Id)
                {
                    ViewBag.owner = true;
                } else {
                    ViewBag.owner = false;
                }
            }
            else
            {
                ViewBag.owner = false;
                ViewBag.Favourite = false;
                ViewBag.AlreadyMade = false;
                ViewBag.In = false;
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

            ViewBag.MealTypeID = new SelectList(_context.MealType, "MealTypeID", "Name");
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

                if (Picture != null && Picture.Length > 0)
                {

                    var file = Picture;
                    var upload = Path.Combine(_hosting.WebRootPath, "images\\imgMeal");
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = Path.GetFileName(file.FileName);
                    if (file.Length > 0)
                    {
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfileName = name + '_' + meal.MealID + extension;

                        using (var fileStream = new FileStream(Path.Combine(upload, myfileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                            meal.Picture = myfileName;
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

            ViewBag.MealTypeID = new SelectList(_context.MealType, "MealTypeID", "Name", meal.MealTypeID);
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

            ViewBag.MealTypeID = new SelectList(_context.MealType, "MealTypeID", "Name", meal.MealTypeID);

            return View(meal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MealID, MealTypeID,Difficulty,Name,MakeTime,Description,Picture")] Meal meal, IFormFile Picture)
        {
            if (id != meal.MealID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Picture != null && Picture.Length > 0)
                    {
                        var file = Picture;
                        var upload = Path.Combine(_hosting.WebRootPath, "images\\imgMeal");
                        var extension = Path.GetExtension(file.FileName);
                        string fileName = Path.GetFileName(file.FileName);
                        string currentName = Path.GetFileName(meal.Picture);

                            if (currentName != null && currentName.Length > 0)
                            {
                                string libary = Path.Combine(_hosting.WebRootPath, "images\\imgMeal");
                                string fullPath = Path.Combine(libary, currentName);

                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            if (file.Length > 0)
                            {
                                string name = Path.GetFileNameWithoutExtension(fileName);
                                string myfileName = name + '_' + meal.MealID + extension;

                                using (var fileStream = new FileStream(Path.Combine(upload, myfileName), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                    meal.Picture = myfileName;
                                }
                            }

                    }

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
                return RedirectToAction(nameof(Details), new { id = id });
            }

            ViewBag.MealTypeID = new SelectList(_context.MealType, "MealTypeID", "Name", meal.MealTypeID);

            return View(meal);
        }

        // GET: Meals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var meal = await _context.Meal
                .Include(m => m.MealType)
                .Include(m => m.UserMeals)
                .SingleOrDefaultAsync(m => m.MealID == id);

            var Ingredients = await _context.Ingredient.Where(i => i.MealID == id).ToListAsync();

            int count = meal.UserMeals.Count(us => us.Stars != null && us.Stars != 0);
            if (count <= 0)
            {
                count = 1;
            }

            MealViewModel mealView = new MealViewModel();

            mealView.Ingredients = Ingredients;
            mealView.Description = meal.Description;
            mealView.Difficulty = meal.Difficulty;
            mealView.MakeTime = meal.MakeTime;
            mealView.MealID = meal.MealID;
            mealView.MealTypeName = meal.MealType.Name;
            mealView.Name = meal.Name;
            mealView.Picture = meal.Picture;
            mealView.Stars = meal.UserMeals.Sum(um => (float)(um.Stars)) / count;

            if (meal == null)
            {
                return NotFound();
            }

            return View(mealView);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userMeals = await _context.UserMeal.Include(us => us.User).Where(us => us.MealID == id).ToListAsync();
            foreach (UserMeal us in userMeals) {
                us.User.UserMeals.Remove(us);
            }
            var meal = await _context.Meal.Include(m => m.MealType).SingleOrDefaultAsync(m => m.MealID == id);
            meal.MealType.Meals.Remove(meal);
            var ingredients = await _context.Ingredient.Where(i => i.MealID == id).ToListAsync();
            string fileName = meal.Picture;

            try {
                if (fileName != null && fileName.Length > 0) {
                    string libary = Path.Combine(_hosting.WebRootPath, "images\\imgMeal");
                    string fullPath = Path.Combine(libary, fileName);

                    if (System.IO.File.Exists(fullPath)) {
                        System.IO.File.Delete(fullPath);
                    }
                }
            } catch (Exception ex) {

            }

            _context.RemoveRange(userMeals);
            _context.RemoveRange(ingredients);
            _context.Meal.Remove(meal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DropAlreadyMade(int? id){
            if (id == null) {
                return NotFound();
            }
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var userMeal = await _context.UserMeal.Where(us => us.AlreadyMade == true && us.MealID == id && us.UserID == user.Id).SingleOrDefaultAsync();
            if (userMeal == null) {
                return NotFound();
            }
            try {
                userMeal.AlreadyMade = false;
                _context.Update(userMeal);
                await _context.SaveChangesAsync();
            } catch (Exception)
            {

            }

            return RedirectToAction(nameof(AlreadyMade));
        }

        public async Task<IActionResult> DropFavourite(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var userMeal = await _context.UserMeal.Where(us => us.Favourite == true && us.MealID == id && us.UserID == user.Id).SingleOrDefaultAsync();
            if (userMeal == null)
            {
                return NotFound();
            }
            try
            {
                userMeal.Favourite = false;
                _context.Update(userMeal);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }

            return RedirectToAction(nameof(Favourite));
        }

        public async Task<IActionResult> AddFavourite(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Meal meal = await _context.Meal.Where(m => m.MealID == id).SingleOrDefaultAsync();
            UserMeal userMeal = await _context.UserMeal.Where(us => us.MealID == id && us.UserID == user.Id).SingleOrDefaultAsync();
            if (userMeal == null)
            {
                try
                {
                    userMeal = new UserMeal();
                    userMeal.AlreadyMade = false;
                    userMeal.Favourite = true;
                    userMeal.RecipeBook = false;
                    userMeal.Stars = 0;
                    userMeal.MealID = meal.MealID;
                    userMeal.Meal = meal;
                    if (user != null)
                    {
                        userMeal.UserID = user.Id;
                        userMeal.User = user;
                    }

                    _context.Add(userMeal);
                    await _context.SaveChangesAsync();

                    user.UserMeals.Add(userMeal);
                    meal.UserMeals.Add(userMeal);

                    await _context.SaveChangesAsync();
                } catch (Exception) {

                }

                return RedirectToAction(nameof(Favourite));
            }
            try
            {
                userMeal.Favourite = true;
                _context.Update(userMeal);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }

            return RedirectToAction(nameof(Favourite));
        }

        public async Task<IActionResult> AddAlreadyMade(int? id) {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Meal meal = await _context.Meal.Where(m => m.MealID == id).SingleOrDefaultAsync();
            UserMeal userMeal = await _context.UserMeal.Where(us => us.MealID == id && us.UserID == user.Id).SingleOrDefaultAsync();
            if (userMeal == null)
            {
                try
                {
                    userMeal = new UserMeal();
                    userMeal.AlreadyMade = true;
                    userMeal.Favourite = false;
                    userMeal.RecipeBook = false;
                    userMeal.Stars = 0;
                    userMeal.MealID = meal.MealID;
                    userMeal.Meal = meal;
                    if (user != null)
                    {
                        userMeal.UserID = user.Id;
                        userMeal.User = user;
                    }

                    _context.Add(userMeal);
                    await _context.SaveChangesAsync();

                    user.UserMeals.Add(userMeal);
                    meal.UserMeals.Add(userMeal);

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                }

                return RedirectToAction(nameof(AlreadyMade));
            }
            try
            {
                userMeal.AlreadyMade = true;
                _context.Update(userMeal);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }

            return RedirectToAction(nameof(AlreadyMade));
        }

        private bool MealExists(int id)
        {
            return _context.Meal.Any(e => e.MealID == id);
        }
    }
}
