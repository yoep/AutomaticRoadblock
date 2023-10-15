using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugPursuitToggleComponent : IMenuComponent<UIMenuListScrollerItem<DebugPursuitToggleComponent.PursuitType>>
    {
        private static readonly Random Random = new();

        private static readonly IReadOnlyList<string> LunaticsVehicleModels = new List<string>
        {
            "burrito3",
            "gburrito2",
            "baller5"
        };

        private static readonly IReadOnlyList<string> VehicleModels = LunaticsVehicleModels.Concat(new List<string>
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
        }).ToList();

        private LHandle _currentPursuit;

        public DebugPursuitToggleComponent()
        {
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
            GameUtils.NewSafeFiber(() =>
            {
                _currentPursuit = Functions.CreatePursuit();

                var type = MenuItem.SelectedItem;
                var vehicleNode = RoadQuery.FindClosestRoad(GameUtils.PlayerPosition + MathHelper.ConvertHeadingToDirection(GameUtils.PlayerHeading) * 50f,
                    EVehicleNodeType.AllNodes);
                var vehicleModels = type == PursuitType.Lunatics ? LunaticsVehicleModels : VehicleModels;
                var vehicle = new Vehicle(new Model(vehicleModels[Random.Next(vehicleModels.Count)]), vehicleNode.Position, GameUtils.PlayerHeading)
                {
                    IsEngineOn = true
                };
                var peds = CreatePeds(type, vehicle);

                for (var i = 0; i < peds.Count; i++)
                {
                    var ped = peds[i];

                    ped.WarpIntoVehicle(vehicle, (int)(i == 0 ? EVehicleSeat.Driver : EVehicleSeat.Any));
                    Functions.AddPedToPursuit(_currentPursuit, ped);

                    if (i == 0)
                    {
                        Functions.GetPedPursuitAttributes(ped).MinDrivingSpeed = 10f;
                        Functions.GetPedPursuitAttributes(ped).MaxDrivingSpeed = type.MaxDrivingSpeed;
                    }

                    if (type.Aggressive)
                    {
                        Functions.GetPedPursuitAttributes(ped).AverageFightTime = 20;
                    }

                    if (type == PursuitType.Lunatics)
                    {
                        ped.Armor = 100;
                        Functions.GetPedPursuitAttributes(ped).AverageFightTime = 0;
                        Functions.GetPedPursuitAttributes(ped).SurrenderChancePittedAndCrashed = 0.05f;
                        Functions.GetPedPursuitAttributes(ped).SurrenderChanceTireBurstAndCrashed = 0.05f;
                        Functions.GetPedPursuitAttributes(ped).SurrenderChancePitted = 0.05f;
                    }
                }

                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Gang1.Name, RelationshipGroup.Cop.Name, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Cop.Name, RelationshipGroup.Gang1.Name, Relationship.Hate);

                Functions.SetPursuitIsActiveForPlayer(_currentPursuit, true);
                MenuItem.Text = AutomaticRoadblocksPlugin.EndPursuit;
            }, "StartPursuitComponent.StartPursuit");
        }

        private void OnPursuitEnded(LHandle handle)
        {
            MenuItem.Text = AutomaticRoadblocksPlugin.StartPursuit;
            _currentPursuit = null;
        }

        private List<Ped> CreatePeds(PursuitType pursuitType, Vehicle vehicle)
        {
            var peds = new List<Ped>();
            var maxVehicleOccupants = vehicle.FreeSeatsCount;
            var totalOccupants = pursuitType.MaxOccupants > maxVehicleOccupants ? maxVehicleOccupants : pursuitType.MaxOccupants;

            for (var i = 0; i < totalOccupants; i++)
            {
                var instance = new Ped(vehicle.Position);
                AddWeaponsToPed(instance);
                instance.RelationshipGroup = RelationshipGroup.Gang1;
                peds.Add(instance);
            }

            return peds;
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
            public static readonly PursuitType Slow = new("Slow", 15f, false, 1);
            public static readonly PursuitType SlowAggressive = new("Slow aggressive", 15f, true, 2);
            public static readonly PursuitType Normal = new("Normal", 45f, false, 2);
            public static readonly PursuitType NormalAggressive = new("Normal aggressive", 45f, true, 2);
            public static readonly PursuitType Fast = new("Fast", 80f, false, 2);
            public static readonly PursuitType FastAggressive = new("Fast aggressive", 80f, true, 2);
            public static readonly PursuitType Lunatics = new("Lunatics", 80f, true, 4);

            public static readonly IEnumerable<PursuitType> Values = new[]
            {
                Slow,
                SlowAggressive,
                Normal,
                NormalAggressive,
                Fast,
                FastAggressive,
                Lunatics
            };

            private PursuitType(string displayText, float maxDrivingSpeed, bool aggressive, int maxOccupants)
            {
                DisplayText = displayText;
                MaxDrivingSpeed = maxDrivingSpeed;
                Aggressive = aggressive;
                MaxOccupants = maxOccupants;
            }

            public string DisplayText { get; }

            public float MaxDrivingSpeed { get; }

            public bool Aggressive { get; }

            public int MaxOccupants { get; }

            public override string ToString()
            {
                return $"{DisplayText}";
            }
        }
    }
}