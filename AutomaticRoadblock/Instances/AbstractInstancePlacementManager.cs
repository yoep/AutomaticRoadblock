using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public abstract class AbstractInstancePlacementManager<T> : IPreviewSupport, IDisposable where T : IPlaceableInstance
    {
        protected readonly IGame Game;
        protected readonly ILogger Logger;

        protected VehicleNodeInfo LastDeterminedStreet;

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
            lock (Instances)
            {
                LastDeterminedStreet = null;

                var instancesToRemove = Instances.Where(x => x.IsPreviewActive).ToList();

                foreach (var instance in instancesToRemove)
                {
                    DisposeInstance(instance);
                    Instances.Remove(instance);
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
                Instances.ForEach(DisposeInstance);
                Instances.Clear();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Create a new placeable instance for the given <see cref="Road"/>.
        /// </summary>
        /// <param name="street">The road to create the instance for.</param>
        /// <returns>Returns the created instance on success, else null.</returns>
        [CanBeNull]
        protected abstract T CreateInstance(IVehicleNode street);

        /// <summary>
        /// Calculate the preview position to place within the world.
        /// </summary>
        /// <returns></returns>
        protected abstract Vector3 CalculatePreviewPosition();

        /// <summary>
        /// Create a preview for the current properties.
        /// </summary>
        /// <param name="force">Force a redraw of the preview.</param>
        protected void DoInternalPreviewCreation(bool force)
        {
            var road = CalculateNewLocationForInstance(CalculatePreviewPosition());

            // ignore intersections and wait for the player to move
            if (road.GetType() == typeof(Intersection))
                return;

            if (IsHologramPreviewEnabled)
            {
                if (force)
                    Logger.Info($"Forcing creation of preview placement for type {GetType()}");

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

            toBoRemoved.ForEach(DisposeInstance);
        }

        /// <summary>
        /// Dispose the given instance.
        /// </summary>
        /// <param name="instance">The instance to dispose.</param>
        protected void DisposeInstance(T instance)
        {
            if (instance == null)
                return;

            if (!instance.IsPreviewActive)
            {
                Logger.Debug($"Releasing instance placement to LSPDFR for {instance}");
                instance.Release(true);
            }

            Logger.Debug($"Disposing instance placement for {instance}");
            instance.Dispose();
        }

        protected VehicleNodeInfo CalculateNewLocationForInstance(Vector3 position)
        {
            return RoadQuery.FindClosestNode(position, EVehicleNodeType.AllRoadNoJunctions);
        }

        private void DoHologramPreviewCreation(VehicleNodeInfo street, bool force)
        {
            if (!force && Equals(street, LastDeterminedStreet))
                return;

            // remove any existing previews first
            DeletePreview();

            lock (Instances)
            {
                Logger.Trace($"Creating new instance hologram preview at {street} for type {GetType()}");
                LastDeterminedStreet = street;

                var instance = CreateInstance(RoadQuery.ToVehicleNode(street));
                if (instance == null)
                {
                    Logger.Warn($"Failed to create placement instance at {street}");
                    return;
                }

                Logger.Debug($"Created instance {instance} for type {GetType()}");
                instance.CreatePreview();
                Instances.Add(instance);
            }
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

        private static void CreatePreviewMarker(VehicleNodeInfo street)
        {
            GameUtils.CreateMarker(street.Position, EMarkerType.MarkerTypeVerticalCylinder, Color.White, 2.5f, 1.5f, false);
        }

        #endregion
    }
}