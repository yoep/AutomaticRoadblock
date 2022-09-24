using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugPursuitToggleComponent : IMenuComponent<UIMenuItem>
    {
        private static readonly Random Random = new();

        private static readonly IReadOnlyList<string> RaceVehicleModels = new List<string>
        {
            "penumbra2",
            "coquette4",
            "sugoi",
            "sultan2",
            "imorgon",
            "komoda",
            "jugular",
            "neo",
            "issi7",
            "drafter",
            "paragon2",
            "italigto",
        };

        private readonly IGame _game;
        private readonly Random _random = new();

        private LHandle _currentPursuit;

        public DebugPursuitToggleComponent(IGame game)
        {
            _game = game;
            Events.OnPursuitEnded += OnPursuitEnded;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.StartPursuit);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

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

                var road = (Road)RoadQuery.FindClosestRoad(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) * 25f,
                    EVehicleNodeType.AllNodes);
                var lane = road.Lanes.First();
                var driver = new Ped(road.Position);
                var passenger = new Ped(road.Position);
                var vehicle = new Vehicle(new Model(RaceVehicleModels[Random.Next(RaceVehicleModels.Count)]), lane.Position, lane.Heading);

                driver.RelationshipGroup = RelationshipGroup.Gang1;
                passenger.RelationshipGroup = RelationshipGroup.Gang1;

                AddWeaponsToPed(driver);
                AddWeaponsToPed(passenger);
                driver.WarpIntoVehicle(vehicle, (int)EVehicleSeat.Driver);
                passenger.WarpIntoVehicle(vehicle, (int)EVehicleSeat.RightFront);

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
            var weaponDescriptor = ped.Inventory.GiveNewWeapon(new WeaponAsset("weapon_pistol"), -1, true);
            ped.Inventory.GiveNewWeapon(new WeaponAsset("weapon_pumpshotgun"), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset("weapon_heavyrifle"), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset("weapon_microsmg"), -1, false);
            ped.Inventory.GiveNewWeapon(new WeaponAsset("weapon_smg"), -1, false);
            ped.Inventory.EquippedWeapon = weaponDescriptor;
        }
    }
}