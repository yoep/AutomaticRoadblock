using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugPursuitToggleComponent : IMenuComponent<UIMenuListScrollerItem<DebugPursuitToggleComponent.PursuitType>>
    {
        private static readonly Random Random = new();

        private static readonly IReadOnlyList<string> VehicleModels = new List<string>
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
            "landstalker",
            "baller6",
            "oracle2",
            "pounder2"
        };

        private readonly IGame _game;

        private LHandle _currentPursuit;

        public DebugPursuitToggleComponent(IGame game)
        {
            _game = game;
            Events.OnPursuitEnded += OnPursuitEnded;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<PursuitType> MenuItem { get; } = new(AutomaticRoadblocksPlugin.StartPursuit,
            AutomaticRoadblocksPlugin.StartPursuitDescription, PursuitType.Values);

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

                var type = MenuItem.SelectedItem;
                var vehicleNode = RoadQuery.FindClosestRoad(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) * 25f,
                    EVehicleNodeType.AllNodes);
                var driver = new Ped(vehicleNode.Position);
                var passenger = new Ped(vehicleNode.Position);
                var vehicle = new Vehicle(new Model(VehicleModels[Random.Next(VehicleModels.Count)]), vehicleNode.Position, _game.PlayerHeading);

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
                Functions.GetPedPursuitAttributes(driver).MinDrivingSpeed = 10f;
                Functions.GetPedPursuitAttributes(driver).MaxDrivingSpeed = type.MaxDrivingSpeed;

                if (type.Aggressive)
                {
                    Functions.GetPedPursuitAttributes(driver).AverageFightTime = 5;
                    Functions.GetPedPursuitAttributes(passenger).AverageFightTime = 5;
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

        public class PursuitType
        {
            public static readonly PursuitType Slow = new("Slow",15f, false);
            public static readonly PursuitType SlowAggressive = new("Slow aggressive",15f, true);
            public static readonly PursuitType Normal = new("Normal",45f, false);
            public static readonly PursuitType NormalAggressive = new("Normal aggressive",45f, true);
            public static readonly PursuitType Fast = new("Fast",80f, false);
            public static readonly PursuitType FastAggressive = new("Fast aggressive",80f, true);

            public static readonly IEnumerable<PursuitType> Values = new[]
            {
                Slow,
                SlowAggressive,
                Normal,
                NormalAggressive,
                Fast,
                FastAggressive
            };
            
            private PursuitType(string displayText, float maxDrivingSpeed, bool aggressive)
            {
                DisplayText = displayText;
                MaxDrivingSpeed = maxDrivingSpeed;
                Aggressive = aggressive;
            }

            public string DisplayText { get; }
            
            public float MaxDrivingSpeed { get; }

            public bool Aggressive { get; }

            public override string ToString()
            {
                return $"{DisplayText}";
            }
        }
    }
}