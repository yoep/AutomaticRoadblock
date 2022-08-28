using System;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class StartPursuitComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;
        private readonly Random _random = new();

        private LHandle _currentPursuit;

        public StartPursuitComponent(IGame game)
        {
            _game = game;
            Events.OnPursuitEnded += OnPursuitEnded;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.StartPursuit);

        /// <inheritdoc />
        public MenuType Type => MenuType.Debug;

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
                var driver = new Ped(road.Position);
                var passenger = new Ped(road.Position);
                var vehicle = new Vehicle(ModelUtils.Vehicles.GetRaceVehicle(), lane.Position, lane.Heading);

                driver.RelationshipGroup = RelationshipGroup.Gang1;
                passenger.RelationshipGroup = RelationshipGroup.Gang1;

                AddWeaponsToPed(driver);
                AddWeaponsToPed(passenger);
                driver.WarpIntoVehicle(vehicle, (int)VehicleSeat.Driver);
                passenger.WarpIntoVehicle(vehicle, (int)VehicleSeat.RightFront);

                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1.Name, RelationshipGroup.Cop.Name, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Cop.Name, RelationshipGroup.Gang1.Name, Relationship.Hate);

                Functions.AddPedToPursuit(_currentPursuit, driver);
                Functions.AddPedToPursuit(_currentPursuit, passenger);
                Functions.GetPedPursuitAttributes(driver).MaxDrivingSpeed = 80f;
                Functions.GetPedPursuitAttributes(driver).MinDrivingSpeed = 20f;

                // randomize if the suspect will be shooting at the cops or not
                if (_random.Next(2) == 1)
                {
                    Functions.GetPedPursuitAttributes(driver).AverageFightTime = 0;
                    Functions.GetPedPursuitAttributes(passenger).AverageFightTime = 0;
                }

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

        [Conditional("DEBUG")]
        private static void AddWeaponsToPed(Ped ped)
        {
            var weaponDescriptor = ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Pistol), -1, true);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.HeavyRifle), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Shotgun), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.Smg), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(ModelUtils.Weapons.MicroSmg), -1, false);
            ped.Inventory.EquippedWeapon = weaponDescriptor;
        }
    }
}