﻿using System;
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
                MealType soup = new MealType { Name = "Soup" };
                MealType pasta = new MealType { Name = "Pasta" };
                MealType fish = new MealType { Name = "Fish" };
                MealType beef = new MealType { Name = "Beef" };
                MealType chicken = new MealType { Name = "Chicken" };
                MealType pork = new MealType { Name = "Pork" };
                MealType dessert = new MealType { Name = "Dessert" };
                MealType cake = new MealType { Name = "Cake" };

                context.MealType.AddRange(
                    salad,
                    soup,
                    pasta,
                    fish,
                    beef,
                    chicken,
                    pork,
                    dessert,
                    cake
                    );

                context.SaveChanges();

                /*Meal meal11 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal11", Picture = "text_0.svg" };
                Meal meal12 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal12", Picture = "text_0.svg" };
                Meal meal13 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = salad, Name = "meal13", Picture = "text_0.svg" };
                Meal meal21 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal21", Picture = "text_0.svg" };
                Meal meal22 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal22", Picture = "text_0.svg" };
                Meal meal23 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = cake, Name = "meal23", Picture = "text_0.svg" };
                Meal meal31 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal31", Picture = "text_0.svg" };
                Meal meal32 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal32", Picture = "text_0.svg" };
                Meal meal33 = new Meal { Difficulty = Meal.DifficultyStates.Easy, Description = "desc", MealType = soup, Name = "meal33", Picture = "text_0.svg" };

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

                context.SaveChanges();*/
            }
        }
    }
}
