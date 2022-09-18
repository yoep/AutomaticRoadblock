using System;
using System.Linq;
using AutomaticRoadblocks.Localization;
using Rage;

namespace AutomaticRoadblocks.Models
{
    public abstract class AbstractModel : IModel
    {
        #region Properties

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string ScriptName { get; }

        /// <inheritdoc />
        public Model? Model { get; protected set; }

        /// <inheritdoc />
        public LocalizationKey LocalizationKey { get; protected set; }

        /// <inheritdoc />
        public virtual bool IsNone => Model == null;

        #endregion

        protected static float DimensionOf(Model model)
        {
            return model.Dimensions.X;
        }

        protected static string ToCamelCase(string snakeCase)
        {
            return snakeCase.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToUpper(x[0]) + x.Substring(1))
                .Aggregate((current, next) => current + next);
        }
    }
}