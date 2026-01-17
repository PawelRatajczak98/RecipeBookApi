using Domain.Entities;
using System;

namespace Application.Utilities
{
    public static class IngredientPriceCalculator
    {
        /// <summary>
        /// Oblicza całkowitą cenę składnika na podstawie ilości i jednostki.
        /// </summary>
        /// <param name="ingredient">Składnik zawierający informacje o cenach</param>
        /// <param name="quantity">Ilość składnika</param>
        /// <param name="unit">Jednostka miary (gramy lub sztuki)</param>
        /// <returns>Obliczona cena</returns>
        public static decimal CalculatePrice(Ingredient ingredient, decimal quantity, string unit)
        {
            if (ingredient == null)
                return 0;

            // Normalizuj jednostkę do małych liter dla porównania
            var normalizedUnit = unit?.ToLower()?.Trim();

            // Jeśli jednostka to "sztuki", użyj PriceForSingle
            if (normalizedUnit == "sztuka" || normalizedUnit == "sztuki")
            {
                if (ingredient.PriceForSingle.HasValue)
                {   
                    return ingredient.PriceForSingle.Value * quantity;
                }
                // Fallback do PriceFor100Grams jeśli PriceForSingle nie jest ustawione
                return ingredient.PriceFor100Grams * quantity / 100;
            }

            // Dla "gramy" lub innych jednostek użyj PriceFor100Grams
            return ingredient.PriceFor100Grams * quantity / 100;
        }

        /// <summary>
        /// Oblicza całkowitą cenę na podstawie bezpośrednich wartości cen.
        /// </summary>
        /// <param name="priceFor100Grams">Cena za 100 gramów</param>
        /// <param name="priceForSingle">Opcjonalna cena za sztukę</param>
        /// <param name="quantity">Ilość</param>
        /// <param name="unit">Jednostka miary</param>
        /// <returns>Obliczona cena</returns>
        public static decimal CalculatePrice(decimal priceFor100Grams, decimal? priceForSingle, decimal quantity, string unit)
        {
            var normalizedUnit = unit?.ToLower()?.Trim();

            if (normalizedUnit == "sztuki" || normalizedUnit == "sztuki")
            {
                if (priceForSingle.HasValue)
                {
                    return priceForSingle.Value * quantity;
                }
                return priceFor100Grams * quantity / 100;
            }

            return priceFor100Grams * quantity / 100;
        }
    }
}
