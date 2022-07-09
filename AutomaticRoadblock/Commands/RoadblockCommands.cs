using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.Roadblock;
using Rage.Attributes;

namespace AutomaticRoadblocks.Commands
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RoadblockCommands
    {
        [ConsoleCommand(Name = "CreateRoadblock", Description = "Create a new roadblock")]
        public static void Create(
            [ConsoleCommandParameter(Description = "Force the roadblock to be created")]
            bool force)
        {
            var pursuitManager = IoC.Instance.GetInstance<IPursuitManager>();
            var roadblockDispatcher = IoC.Instance.GetInstance<IRoadblockDispatcher>();
            var game = IoC.Instance.GetInstance<IGame>();

            if (!force && pursuitManager.IsPursuitActive)
            {
                pursuitManager.DispatchNow();
            }
            else
            {
                roadblockDispatcher.Dispatch(RoadblockLevel.Level1, game.PlayerVehicle, true);
            }
        }
    }
}