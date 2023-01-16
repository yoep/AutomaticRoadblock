using System.Diagnostics;
using AutomaticRoadblocks.CloseRoad.Menu;
using AutomaticRoadblocks.Debug.Menu;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.ManualPlacement.Menu;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Pursuit.Menu;
using AutomaticRoadblocks.RedirectTraffic.Menu;
using AutomaticRoadblocks.Roadblock.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks
{
    internal class MenuInitializer
    {
        internal static void InitializeMenu()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var menu = IoC.Instance.GetInstance<IMenu>();
            var menuComponents = IoC.Instance.GetInstances<IMenuComponent<UIMenuItem>>();
        
            logger.Trace("Initializing menu components");
            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }
        
            logger.Debug($"Initialized a total of {menu.TotalItems} menu component(s)");
        }
        
        internal static void InitializeMenuComponents()
        {
            IoC.Instance
                // menu switchers
                .Register<IMenuSwitchItem>(typeof(RoadblockMenuSwitchItem))
                .Register<IMenuSwitchItem>(typeof(ManualPlacementMenuSwitchItem))
                .Register<IMenuSwitchItem>(typeof(RedirectTrafficMenuSwitchItem))
                .Register<IMenuSwitchItem>(typeof(CloseRoadMenuSwitchItem))
                // pursuit components
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitLevelComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchNowComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableDuringPursuitComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableAutomaticLevelIncreaseComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableSpikeStripComponentItem))
                // manual placement components
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualRoadblockPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementRemoveComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PlacementTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableCopsComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementDirectionComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementMainBarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementSecondaryBarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementLightTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementVehicleTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementOffsetComponentItem))
                // redirect traffic components
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficRemoveComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficLaneTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeDistanceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficOffsetComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficVehicleTypeComponentType))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableRedirectArrowComponentItem))
                // close road components
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadNearbyComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadUnitTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadLightSourceComponentItem));
        }
        
        [Conditional("DEBUG")]
        internal static void InitializeDebugComponents()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
        
            logger.Debug("Registering debug menu components");
            IoC.Instance
                .Register<IMenuSwitchItem>(typeof(DebugMenuSwitchItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPursuitToggleComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPursuitForceOnFootComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugRoadPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugDeploySpikeStripComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPreviewSpikeStripComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugReloadSettingsComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugReloadDataFilesComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchSpawnComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CleanRoadblocksComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugCleanEntitiesComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadNearbyPreviewComponentItem));
        }
    }
}