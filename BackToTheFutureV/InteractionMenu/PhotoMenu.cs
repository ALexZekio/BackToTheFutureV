﻿using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.InteractionMenu
{
    public class PhotoMenu : UIMenu
    {
        private UIMenuCheckboxItem _wormhole;
        private UIMenuCheckboxItem _coils;
        private UIMenuCheckboxItem _ice;
        private UIMenuCheckboxItem _fluxCapacitor;
        private UIMenuCheckboxItem _hideHUD;

        private PhotoHandler _photoHandler => DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<PhotoHandler>();
        
        public PhotoMenu() : base(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu"), Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            OnCheckboxChange += PhotoMenu_OnCheckboxChange;
            OnItemSelect += PhotoMenu_OnItemSelect;
            OnListChange += PhotoMenu_OnListChange;
            OnMenuClose += PhotoMenu_OnMenuClose;
            OnMenuOpen += PhotoMenu_OnMenuOpen;

            AddItem(_wormhole = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole"), false, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole_Description")));
            AddItem(_coils = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils"), false, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils_Description")));
            AddItem(_ice = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice"), false, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice_Description")));
            AddItem(_fluxCapacitor = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor"), false, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor_Description")));
            AddItem(_hideHUD = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD"), false, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD_Description")));
        }

        private void PhotoMenu_OnMenuOpen(UIMenu sender)
        {
            _wormhole.Checked = _photoHandler.WormholeActive;
            _coils.Checked = _photoHandler.GlowingCoilsActive;
            _ice.Checked = _photoHandler.IceActive;
            _fluxCapacitor.Checked = _photoHandler.FluxCapacitorActive;
            _hideHUD.Checked = Main.HideGui;
        }

        public void Process()
        {

        }

        private void PhotoMenu_OnMenuClose(UIMenu sender)
        {
            
        }

        private void PhotoMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            
        }

        private void PhotoMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            
        }

        private void PhotoMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == _wormhole)
            {
                _photoHandler.WormholeActive = Checked;                
            }

            if (checkboxItem == _coils)
            {
                _photoHandler.GlowingCoilsActive = Checked;
            }

            if (checkboxItem == _ice)
            {
                _photoHandler.IceActive = Checked;
            }

            if (checkboxItem == _fluxCapacitor)
            {
                _photoHandler.FluxCapacitorActive = Checked;
            }

            if (checkboxItem == _hideHUD)
            {
                Main.HideGui = _hideHUD.Checked;
            }

            _photoHandler.SetPhotoMode();
        }
    }
}
