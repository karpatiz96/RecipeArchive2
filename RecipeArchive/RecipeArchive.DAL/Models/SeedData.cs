using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using RecipeArchive.Data;
using RecipeArchive.Models;

namespace RecipeArchive.DAL.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider) {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>())
                )
            {
                if (context.MealType.Any()) {
                    return;
                }

                MealType salad = new MealType { Name = "Salad" };
                MealType cake = new MealType { Name = "Cake" };
                MealType soup = new MealType { Name = "Soup" };

                context.MealType.AddRange(
                    salad,
                    cake,
                    soup
                    );

                context.SaveChanges();

                Meal meal11 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal11" };
                Meal meal12 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal12" };
                Meal meal13 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal13" };
                Meal meal21 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal21" };
                Meal meal22 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal22" };
                Meal meal23 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal23" };
                Meal meal31 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal31" };
                Meal meal32 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal32" };
                Meal meal33 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal33" };

                context.Meal.AddRange(
                    meal11,meal12,meal13,
                    meal21, meal22, meal23,
                    meal31, meal32, meal33
                    );

                context.SaveChanges();

                UserMeal userMeal11 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 5, Meal = meal11  };
                UserMeal userMeal12 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 4, Meal = meal12 };
                UserMeal userMeal13 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 3, Meal = meal13 };
                UserMeal userMeal21 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 5, Meal = meal21 };
                UserMeal userMeal22 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 4, Meal = meal22 };
                UserMeal userMeal23 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 3, Meal = meal23 };
                UserMeal userMeal31 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 5, Meal = meal31 };
                UserMeal userMeal32 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 4, Meal = meal32 };
                UserMeal userMeal33 = new UserMeal { AlreadyMade = false, Favourite = false, RecipeBook = false, Stars = 3, Meal = meal33 };

                context.UserMeal.AddRange(
                    userMeal11, userMeal12, userMeal13,
                    userMeal21, userMeal22, userMeal23,
                    userMeal31, userMeal32, userMeal33
                    );

                context.SaveChanges();

                meal11.UserMeals.Add(userMeal11);
                meal12.UserMeals.Add(userMeal12);
                meal13.UserMeals.Add(userMeal13);
                meal21.UserMeals.Add(userMeal21);
                meal22.UserMeals.Add(userMeal22);
                meal23.UserMeals.Add(userMeal23);
                meal31.UserMeals.Add(userMeal31);
                meal32.UserMeals.Add(userMeal32);
                meal33.UserMeals.Add(userMeal33);

                context.SaveChanges();
            }
        }
    }
}
