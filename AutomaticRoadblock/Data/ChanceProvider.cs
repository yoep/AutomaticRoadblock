using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomaticRoadblocks.Data
{
    /// <summary>
    /// The chance provider allows easy selection of an item from a list based on a chance. 
    /// </summary>
    public static class ChanceProvider
    {
        private static readonly Random Random = new();

        /// <summary>
        /// Retrieve a single item based on the chance of each item in the list.
        /// </summary>
        /// <param name="items">The items to retrieve one from.</param>
        /// <typeparam name="T">The chance data instance.</typeparam>
        /// <returns>Returns the selected item based on the chance.</returns>
        /// <exception cref="ChanceException{T}">Is thrown when the chance total is invalid.</exception>
        public static T Retrieve<T>(IList<T> items) where T : IChanceData
        {
            var tressHold = Random.Next(101);
            var totalChance = 0;

            foreach (var item in items.OrderBy(x => x.Chance))
            {
                totalChance += item.Chance;

                if (tressHold <= totalChance)
                {
                    return item;
                }
            }

            throw new ChanceException<T>(items);
        }
    }
}