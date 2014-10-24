using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        /// <summary>
        /// Implementera metoden Load()
        /// </summary>
        public void Load()
        {
            List<IRecipe> recipes = new List<IRecipe>(); //Skapa lista som innehåller referenser till receptobjektet
            RecipeReadStatus status = RecipeReadStatus.Indefinite; //Läs av statusen för nästkommande rad
            Recipe recipe = null; //Ge den lokala variabeln ett värde
            
            try
            {
                using (StreamReader reader = new StreamReader(_path))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (line == SectionRecipe)
                            {
                                status = RecipeReadStatus.New; //status är enum. RecipeReadStatus.New innehåller recept-namnet
                            }
                            else if (line == SectionIngredients)
                            {
                                status = RecipeReadStatus.Ingredient;
                            }
                            else if (line == SectionInstructions)
                            {
                                status = RecipeReadStatus.Instruction;
                            }
                            else
                            {
                                if (status == RecipeReadStatus.New)
                                {
                                    recipe = new Recipe(line);
                                    //skulle ha kunnat skriva recipes.Add(new Recipe(line) för att slippa lägga till variabeln recipe
                                }
                                else if (status == RecipeReadStatus.Ingredient)
                                {
                                    string[] values = line.Split(new char[] {';'});

                                    if (values.Length != 3)
                                    {
                                        throw new FileFormatException();
                                    }

                                    //Instansiera ett ingrediensobjekt
                                    //Initiera mängd, mått och namn
                                    Ingredient ingredient = new Ingredient();
                                    ingredient.Amount = values[0];
                                    ingredient.Measure = values[1];
                                    ingredient.Name = values[2];

                                    //Lägg till ingrediensen till receptets lista med ingredienser
                                    recipe.Add(ingredient);

                                }
                                else if (status == RecipeReadStatus.Instruction)
                                {
                                    recipe.Add(line);
                                }
                                else
                                {
                                    throw new FileFormatException();
                                }
                            }
                            //Sortera listan med recept baserat på namn
                            //recipes.Sort();
                            //recipes.OrderBy<RecipeReadStatus>(); //http://msdn.microsoft.com/en-us/library/bb534966.aspx
                            IEnumerable<IRecipe> sortedRecipes = recipes.OrderBy(ReadRecipeStatus => ReadRecipeStatus.Name);

                            //Tilldela avsett fält i klassen, _recipes, en referens till listan
                            _recipes = new List<IRecipe>

                            //Tilldela avsedd egenskap i klassen, IsModified, ett värde som indikerar
                            //att listan med recept är oförändrad
                            IsModified = 

                            //Utlös händelse om att recept har lästs in genom att anropa metoden OnRecipeChanged
                            //och skicka med parametern EventArgs.Empty
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Implementera metoden Save()
        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
