using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class StartPursuitComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;

        private LHandle _currentPursuit;

        public StartPursuitComponent(IGame game)
        {
            _game = game;
            Events.OnPursuitEnded += OnPursuitEnded;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.StartPursuit);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_currentPursuit == null)
            {
                StartPursuit();
            }
            else
            {
                EndPursuit();
            }
        }

        [Conditional("DEBUG")]
        private void StartPursuit()
        {
            _game.NewSafeFiber(() =>
            {
                _currentPursuit = Functions.CreatePursuit();

                var road = RoadUtils.FindClosestRoad(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) * 25f, RoadType.All);
                var lane = road.Lanes.First();
                var ped = new Ped(road.Position);
                var vehicle = EntityUtils.CreateVehicle(new Model("Buffalo3"), lane.Position, lane.Heading);

                var weaponDescriptor = ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Pistol), -1, true);
                ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.HeavyRifle), -1, false);
                ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Shotgun), -1, false);
                ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Smg), -1, false);
                ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.MicroSmg), -1, false);
                ped.WarpIntoVehicle(vehicle, (int)VehicleSeat.Driver);
                ped.Inventory.EquippedWeapon = weaponDescriptor;

                Functions.AddPedToPursuit(_currentPursuit, ped);
                Functions.SetPursuitIsActiveForPlayer(_currentPursuit, true);
                MenuItem.Text = AutomaticRoadblocksPlugin.EndPursuit;
            }, "StartPursuitComponent.StartPursuit");
        }

        private void OnPursuitEnded(LHandle handle)
        {
            MenuItem.Text = AutomaticRoadblocksPlugin.StartPursuit;
            _currentPursuit = null;
        }

        [Conditional("DEBUG")]
        private void EndPursuit()
        {
            if (Functions.IsPursuitStillRunning(_currentPursuit))
                Functions.ForceEndPursuit(_currentPursuit);
        }
    }
}