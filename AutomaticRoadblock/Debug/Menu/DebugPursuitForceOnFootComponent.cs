using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugPursuitForceOnFootComponent : IMenuComponent<UIMenuItem>, IDisposable
    {
        private readonly ILogger _logger;

        private bool _active = true;

        public DebugPursuitForceOnFootComponent(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.ForcePursuitOnFoot, AutomaticRoadblocksPlugin.ForcePursuitOnFootDescription);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            try
            {
                var handle = Functions.GetActivePursuit();
                Functions.GetPursuitPeds(handle)
                    .Where(x => x.CurrentVehicle != null)
                    .ToList()
                    .ForEach(x => x.Tasks.LeaveVehicle(LeaveVehicleFlags.BailOut));
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to force pursuit on foot, {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _active = false;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            GameUtils.NewSafeFiber(() =>
            {
                while (_active)
                {
                    MenuItem.Enabled = Functions.GetActivePursuit() != null;
                   GameFiber.Yield();
                }
            }, "PursuitForceOnFootComponent.PursuitListener");
        }
    }
}