using System;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.RedirectTraffic
{
    internal class RedirectTrafficDispatcher : AbstractInstancePlacementManager<RedirectTraffic>, IRedirectTrafficDispatcher
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IModelProvider _modelProvider;

        private float _coneDistance = 2f;
        private EBackupUnit _backupType = EBackupUnit.LocalPatrol;
        private BarrierModel _coneType;
        private RedirectTrafficType _type = RedirectTrafficType.Lane;
        private bool _enableRedirectionArrow = true;
        private float _offset;

        public RedirectTrafficDispatcher(IGame game, ILogger logger, ISettingsManager settingsManager, IModelProvider modelProvider)
            : base(game, logger)
        {
            _settingsManager = settingsManager;
            _modelProvider = modelProvider;
            _coneType = ConeTypeFromSettings(settingsManager.RedirectTrafficSettings.DefaultCone);
        }

        #region Properties

        /// <inheritdoc />
        public float ConeDistance
        {
            get => _coneDistance;
            set => UpdateConeDistance(value);
        }

        /// <inheritdoc />
        public EBackupUnit BackupType
        {
            get => _backupType;
            set => UpdateBackupType(value);
        }

        /// <inheritdoc />
        public BarrierModel ConeType
        {
            get => _coneType;
            set => UpdateConeType(value);
        }

        /// <inheritdoc />
        public RedirectTrafficType Type
        {
            get => _type;
            set => UpdateType(value);
        }

        /// <inheritdoc />
        public bool EnableRedirectionArrow
        {
            get => _enableRedirectionArrow;
            set => UpdateRedirectArrow(value);
        }

        /// <inheritdoc />
        public float Offset
        {
            get => _offset;
            set => UpdateOffset(value);
        }

        /// <inheritdoc />
        protected override bool IsHologramPreviewEnabled => _settingsManager.RedirectTrafficSettings.EnablePreview;

        /// <inheritdoc />
        protected override float DistanceInFrontOfPlayer => _settingsManager.RedirectTrafficSettings.DistanceFromPlayer;

        #endregion

        #region IRedirectTrafficDispatcher

        /// <inheritdoc />
        public void DispatchRedirection()
        {
            RedirectTraffic redirectTraffic;

            lock (Instances)
            {
                redirectTraffic = Instances.FirstOrDefault(x => x.IsPreviewActive);

                if (redirectTraffic == null)
                {
                    redirectTraffic = CreateInstance(RoadQuery.ToVehicleNode(LastDeterminedStreet ?? CalculateNewLocationForInstance(Game.PlayerPosition + MathHelper.ConvertHeadingToDirection(Game.PlayerHeading) * DistanceInFrontOfPlayer)));
                    Instances.Add(redirectTraffic);
                }
            }

            Logger.Trace($"Spawning traffic redirection {redirectTraffic}");
            var success = redirectTraffic.Spawn();

            if (success)
            {
                Logger.Info($"Traffic redirection has been spawned with success, {redirectTraffic}");
            }
            else
            {
                Logger.Warn($"Traffic redirection was unable to be spawned correctly, {redirectTraffic}");
            }
        }

        /// <inheritdoc />
        public void RemoveTrafficRedirects(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Function

        protected override RedirectTraffic CreateInstance(IVehicleNode street)
        {
            Assert.NotNull(street, "street cannot be null");
            if (street.GetType() == typeof(Intersection))
                return null;

            Logger.Trace(
                $"Creating a redirect traffic instance for {nameof(BackupType)}: {BackupType}, {nameof(ConeType)}: {ConeType}, {nameof(Type)}: {Type}, {nameof(ConeDistance)}: {ConeDistance}");
            var redirectTraffic = new RedirectTraffic(new RedirectTraffic.Request
            {
                Road = (Road)street,
                BackupType = BackupType,
                ConeType = ConeType,
                Type = Type,
                ConeDistance = ConeDistance,
                EnableRedirectionArrow = EnableRedirectionArrow,
                EnableLights = ShouldAddLights(),
                Offset = Offset
            });
            return redirectTraffic;
        }

        private bool ShouldAddLights()
        {
            return _settingsManager.RedirectTrafficSettings.EnableLights &&
                   GameUtils.TimePeriod is ETimePeriod.Evening or ETimePeriod.Night;
        }

        private void UpdateConeDistance(float newDistance)
        {
            _coneDistance = newDistance;
            DoInternalPreviewCreation(true);
        }

        private void UpdateBackupType(EBackupUnit value)
        {
            _backupType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateConeType(BarrierModel value)
        {
            _coneType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateType(RedirectTrafficType value)
        {
            _type = value;
            DoInternalPreviewCreation(true);
        }


        private void UpdateRedirectArrow(bool value)
        {
            _enableRedirectionArrow = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateOffset(float offset)
        {
            _offset = offset;
            DoInternalPreviewCreation(true);
        }

        private BarrierModel ConeTypeFromSettings(string defaultCone)
        {
            Logger.Debug($"Using cone {defaultCone} for traffic redirection if found");
            return _modelProvider.BarrierModels
                .Where(x => string.Equals(x.ScriptName, defaultCone, StringComparison.OrdinalIgnoreCase))
                .DefaultIfEmpty(BarrierModel.None)
                .First();
        }

        #endregion
    }
}