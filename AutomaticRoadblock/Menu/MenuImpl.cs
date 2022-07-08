using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Settings;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Menu
{
    public class MenuImpl : IMenu
    {
        private readonly ILogger _logger;
        private readonly INotification _notification;
        private readonly ISettingsManager _settingsManager;
        
        private static readonly MenuPool MenuPool = new MenuPool();
        private static readonly IDictionary<MenuType, UIMenu> Menus = new Dictionary<MenuType, UIMenu>();
        private static readonly List<IMenuComponent> MenuItems = new List<IMenuComponent>();
        
        private UIMenuSwitchMenusItem _menuSwitcher;

        public MenuImpl(ILogger logger, INotification notification, ISettingsManager settingsManager)
        {
            _logger = logger;
            _notification = notification;
            _settingsManager = settingsManager;
        }

        #region Properties

        /// <inheritdoc />
        public bool IsMenuInitialized { get; private set; }

        /// <inheritdoc />
        public bool IsShown { get; private set; }

        /// <inheritdoc />
        public int TotalItems => MenuItems.Count;

        #endregion

        #region IMenu
        
        public void RegisterComponent(IMenuComponent component)
        {
            Assert.NotNull(component, "component cannot be null");
            var uiMenu = Menus[component.Type];

            uiMenu.AddItem(component.MenuItem);
            uiMenu.RefreshIndex();

            MenuItems.Add(component);
        }

        #endregion

        #region IDisposable
        
        public void Dispose()
        {
            Game.FrameRender -= Process;
        }
        
        #endregion
        
        #region Functions
        
        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _logger.Trace("Initializing menu");
            
            try
            {
                _logger.Trace("Creating sub-menu's");
                Menus.Add(MenuType.PURSUIT, CreateMenu());
                Menus.Add(MenuType.MANUAL_PLACEMENT, CreateMenu());
                AddDebugMenu();

                _logger.Trace("Creating menu switcher");
                CreateMenuSwitcher();

                _logger.Trace("Initializing sub-menu's");
                foreach (var menu in Menus)
                {
                    menu.Value.AddItem(_menuSwitcher, 0);
                    menu.Value.RefreshIndex();
                    menu.Value.OnItemSelect += ItemSelectionHandler;
                }

                _logger.Trace("Adding MenuImpl.Process to FrameRender handler...");
                Game.FrameRender += Process;
                _logger.Trace("MenuImpl.Process added to FrameRender handler");

                IsMenuInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while initializing the menu with error {ex.Message}", ex);
                _notification.DisplayPluginNotification("an unexpected error occurred");
            }
        }

        private void Process(object sender, GraphicsEventArgs e)
        {
            try
            {
                if (IsMenuKeyPressed())
                {
                    _menuSwitcher.CurrentMenu.Visible = !_menuSwitcher.CurrentMenu.Visible;
                    IsShown = _menuSwitcher.CurrentMenu.Visible;
                }

                MenuPool.ProcessMenus();
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while processing the menu with error {ex.Message}", ex);
                _notification.DisplayPluginNotification("an unexpected error occurred");
            }
        }

        private void ItemSelectionHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            var menuComponent = MenuItems.FirstOrDefault(x => x.MenuItem == selectedItem);

            try
            {
                if (menuComponent == null)
                    throw new MenuException("No menu item action found for the selected menu item", selectedItem);

                menuComponent.OnMenuActivation(this);

                if (menuComponent.IsAutoClosed)
                    CloseMenu();
            }
            catch (MenuException ex)
            {
                _logger.Error(ex.Message, ex);
                _notification.DisplayPluginNotification("could not invoke menu item, see log files for more info");
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while activating the menu item {ex.Message}", ex);
                _notification.DisplayPluginNotification("an unexpected error occurred while invoking the menu action");
            }
        }

        private bool IsMenuKeyPressed()
        {
            var generalSettings = _settingsManager.GeneralSettings;
            var secondKey = generalSettings.OpenMenuModifierKey;
            var secondKeyDown = secondKey == Keys.None;

            if (!secondKeyDown && secondKey == Keys.ShiftKey && Game.IsShiftKeyDownRightNow)
                secondKeyDown = true;

            if (!secondKeyDown && secondKey == Keys.ControlKey && Game.IsControlKeyDownRightNow)
                secondKeyDown = true;

            return Game.IsKeyDown(generalSettings.OpenMenuKey) && secondKeyDown;
        }

        private void CloseMenu()
        {
            _menuSwitcher.CurrentMenu.Visible = false;
        }

        private static UIMenu CreateMenu()
        {
            var menu = new UIMenu("Automatic Roadblocks", "~b~Dispatch roadblocks");
            MenuPool.Add(menu);
            return menu;
        }

        private void CreateMenuSwitcher()
        {
            _menuSwitcher = new UIMenuSwitchMenusItem("Mode", null,
                new DisplayItem(Menus[MenuType.PURSUIT], "Pursuit"),
                new DisplayItem(Menus[MenuType.MANUAL_PLACEMENT], "Manual Placement"),
                new DisplayItem(Menus[MenuType.DEBUG], "Debug"));
        }
        
        [Conditional("DEBUG")]
        private static void AddDebugMenu()
        {
            Menus.Add(MenuType.DEBUG, CreateMenu());
        }

        #endregion
    }
}