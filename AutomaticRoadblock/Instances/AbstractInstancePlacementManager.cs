using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public abstract class AbstractInstancePlacementManager<T> : IPreviewSupport, IDisposable where T : IPlaceableInstance
    {
        protected readonly IGame Game;
        protected readonly ILogger Logger;

        protected Road LastDeterminedRoad;

        protected AbstractInstancePlacementManager(IGame game, ILogger logger)
        {
            Game = game;
            Logger = logger;
        }

        #region Properties

        /// <summary>
        /// The instances which are/can be placed.
        /// </summary>
        protected List<T> Instances { get; } = new();

        /// <summary>
        /// Indicates if a hologram preview should be shown or a marker.
        /// </summary>
        protected abstract bool IsHologramPreviewEnabled { get; }
        
        /// <summary>
        /// The distance in front of the player for which a location must be determined.
        /// </summary>
        protected abstract float DistanceInFrontOfPlayer { get; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances.Any(x => x.IsPreviewActive);

        /// <inheritdoc />
        public void CreatePreview()
        {
            DoInternalPreviewCreation(false);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            LastDeterminedRoad = null;
            
            lock (Instances)
            {
                var instancesToRemove = Instances.Where(x => x.IsPreviewActive).ToList();

                foreach (var redirectTraffic in instancesToRemove)
                {
                    redirectTraffic.DeletePreview();
                    Instances.Remove(redirectTraffic);
                }
            }
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            lock (Instances)
            {
                Instances.ForEach(x => x.Dispose());
                Instances.Clear();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Create a new placeable instance for the given <see cref="Road"/>.
        /// </summary>
        /// <param name="road">The road to create the instance for.</param>
        /// <returns>Returns the created instance.</returns>
        protected abstract T CreateInstance(Road road);

        /// <summary>
        /// Create a preview for the current properties.
        /// </summary>
        /// <param name="force">Force a redraw of the preview.</param>
        protected void DoInternalPreviewCreation(bool force)
        {
            var road = CalculateNewLocationForInstance();

            if (IsHologramPreviewEnabled)
            {
                DoHologramPreviewCreation(road, force);
            }
            else
            {
                CreatePreviewMarker(road);
            }
        }

        /// <summary>
        /// Remove instance based on the given removal condition type.
        /// </summary>
        /// <param name="removeType">The removal condition.</param>
        protected void DoInternalInstanceRemoval(RemoveType removeType)
        {
            Logger.Debug($"Removing placed instances with criteria {removeType}");
            var toBoRemoved = new List<T>();

            lock (Instances)
            {
                if (removeType == RemoveType.All)
                {
                    toBoRemoved = Instances
                        .Where(x => !x.IsPreviewActive)
                        .ToList();
                }
                else if (removeType == RemoveType.ClosestToPlayer)
                {
                    var closestRoadblock = FindInstanceClosestToPlayer();

                    if (closestRoadblock != null)
                        toBoRemoved.Add(closestRoadblock);
                }
                else if (removeType == RemoveType.LastPlaced)
                {
                    toBoRemoved = new List<T> { Instances[Instances.Count - 1] };
                }
                else
                {
                    Logger.Warn($"Remove placed roadblocks has not been implemented for {removeType}");
                }

                Instances.RemoveAll(x => toBoRemoved.Contains(x));
            }

            toBoRemoved.ForEach(x => x.Dispose());
        }

        protected Road CalculateNewLocationForInstance()
        {
            var position = Game.PlayerPosition + MathHelper.ConvertHeadingToDirection(Game.PlayerHeading) * DistanceInFrontOfPlayer;

            return RoadUtils.FindClosestRoad(position, ERoadType.All);
        }

        private void DoHologramPreviewCreation(Road road, bool force)
        {
            if (!force && Equals(road, LastDeterminedRoad))
                return;

            // remove any existing previews first
            DeletePreview();

            LastDeterminedRoad = road;
            Game.NewSafeFiber(() =>
            {
                Logger.Trace($"Creating new instance hologram preview at {road}");
                var instance = CreateInstance(road);
                Logger.Debug($"Created instance {instance}");
                instance.CreatePreview();

                lock (Instances)
                {
                    Instances.Add(instance);
                }
            }, "AbstractInstancePlacement.DoHologramPreviewCreation");
        }

        private T FindInstanceClosestToPlayer()
        {
            T closestInstance = default;
            var closestInstanceDistance = 9999f;
            var playerPosition = Game.PlayerPosition;

            foreach (var instance in Instances.Where(x => !x.IsPreviewActive))
            {
                var distance = playerPosition.DistanceTo(instance.Position);

                if (distance > closestInstanceDistance)
                    continue;

                closestInstanceDistance = distance;
                closestInstance = instance;
            }

            return closestInstance;
        }

        private static void CreatePreviewMarker(Road road)
        {
            GameUtils.CreateMarker(road.Position, EMarkerType.MarkerTypeVerticalCylinder, Color.White, 2.5f, false);
        }

        #endregion
    }
}