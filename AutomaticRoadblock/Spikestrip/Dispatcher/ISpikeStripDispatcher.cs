using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip.Dispatcher
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface ISpikeStripDispatcher : IDisposable
    {
        /// <summary>
        /// Spawn a spike strip on the given road.
        /// This will spawn an undeployed spike strip.
        /// </summary>
        /// <param name="street">The road the spike strip should be placed on.</param>
        /// <param name="stripLocation">The location of the spike strip.</param>
        /// <param name="offset">The vertical offset of the spike strip on the lane.</param>
        /// <returns>Returns the spike strip instance when spawned, else null.</returns>
        ISpikeStrip Spawn(Road street, ESpikeStripLocation stripLocation, float offset = 0f);

        /// <summary>
        /// Spawn a spike strip on the given lane.
        /// This will spawn an undeployed spike strip.
        /// </summary>
        /// <param name="street">The road to which the lane belongs.</param>
        /// <param name="lane">The lane the spike strip should be placed on.</param>
        /// <param name="stripLocation">The location of the spike strip.</param>
        /// <param name="targetVehicle">The vehicle to monitor.</param>
        /// <param name="offset">The vertical offset of the spike strip on the lane.</param>
        /// <returns>Returns the spike strip instance when spawned, else null.</returns>
        ISpikeStrip Spawn(Road street, Road.Lane lane, ESpikeStripLocation stripLocation, Vehicle targetVehicle, float offset = 0f);
        
        /// <summary>
        /// Deploy a spike strip on the nearby road for the given location.
        /// This will spawn an undeployed spike strip which will directly be deployed upon creation.
        /// </summary>
        /// <param name="position">The position to deploy a spike strip at.</param>
        /// <param name="stripLocation">The location of the spike strip on the road.</param>
        /// <returns>Returns the deployed spike strip.</returns>
        ISpikeStrip Deploy(Vector3 position, ESpikeStripLocation stripLocation);

        /// <summary>
        /// Deploy a spike strip on the given road.
        /// This will spawn an undeployed spike strip which will directly be deployed upon creation.
        /// </summary>
        /// <param name="street">The road to deploy a spike strip at.</param>
        /// <param name="stripLocation">The location of the spike strip on the road.</param>
        /// <returns>Returns the deployed spike strip.</returns>
        ISpikeStrip Deploy(Road street, ESpikeStripLocation stripLocation);

        /// <summary>
        /// Deploy a spike strip on the given road for the target vehicle.
        /// This spike strip will monitor if the spike strip is hit/bypassed by the target vehicle.
        /// </summary>
        /// <param name="street">The road to deploy a spike strip at.</param>
        /// <param name="stripLocation">The location of the spike strip on the road.</param>
        /// <param name="targetVehicle">The vehicle to monitor.</param>
        /// <returns>Returns the deployed spike strip.</returns>
        ISpikeStrip Deploy(Road street, ESpikeStripLocation stripLocation, Vehicle targetVehicle);

        /// <summary>
        /// Create a spike strip preview on the nearby road for the given position.
        /// </summary>
        /// <param name="position">The position to create the spike strip on.</param>
        /// <param name="stripLocation">The placement location of the spike strip.</param>
        /// <returns></returns>
        ISpikeStrip CreatePreview(Vector3 position, ESpikeStripLocation stripLocation);

        /// <summary>
        /// Remove all spike strips.
        /// </summary>
        void RemoveAll();
    }
}