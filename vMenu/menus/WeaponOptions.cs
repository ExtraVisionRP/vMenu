using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class WeaponOptions
    {
        // Variables
        private UIMenu menu;

        public bool UnlimitedAmmo { get; private set; } = UserDefaults.WeaponsUnlimitedAmmo;
        public bool NoReload { get; private set; } = UserDefaults.WeaponsNoReload;
        public bool AutoEquipChute { get; private set; } = UserDefaults.AutoEquipChute;

        public static Dictionary<string, uint> AddonWeapons = new Dictionary<string, uint>();

        private Dictionary<UIMenu, ValidWeapon> weaponInfo = new Dictionary<UIMenu, ValidWeapon>();
        private Dictionary<UIMenuItem, string> weaponComponents = new Dictionary<UIMenuItem, string>();

        #region Create Menu
        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create main weapon options menu and add items
            // Create the menu.
            menu = new UIMenu(Game.Player.Name, "Weapon Options", RightAlignMenus());

            UIMenuItem getAllWeapons = new UIMenuItem("Get All Weapons", "Get all weapons.");
            UIMenuItem removeAllWeapons = new UIMenuItem("Remove All Weapons", "Removes all weapons in your inventory.");
            UIMenuCheckboxItem unlimitedAmmo = new UIMenuCheckboxItem("Unlimited Ammo", UnlimitedAmmo, "Unlimited ammonition supply.");
            UIMenuCheckboxItem noReload = new UIMenuCheckboxItem("No Reload", NoReload, "Never reload.");
            UIMenuItem setAmmo = new UIMenuItem("Set All Ammo Count", "Set the amount of ammo in all your weapons.");
            UIMenuItem refillMaxAmmo = new UIMenuItem("Refill All Ammo", "Give all your weapons max ammo.");
            ValidWeapons vw = new ValidWeapons();
            UIMenuItem spawnByName = new UIMenuItem("Spawn Weapon By Name", "Enter a weapon mode name to spawn.");

            // Add items based on permissions
            if (IsAllowed(Permission.WPGetAll))
            {
                menu.AddItem(getAllWeapons);
            }
            if (IsAllowed(Permission.WPRemoveAll))
            {
                menu.AddItem(removeAllWeapons);
            }
            if (IsAllowed(Permission.WPUnlimitedAmmo))
            {
                menu.AddItem(unlimitedAmmo);
            }
            if (IsAllowed(Permission.WPNoReload))
            {
                menu.AddItem(noReload);
            }
            if (IsAllowed(Permission.WPSetAllAmmo))
            {
                menu.AddItem(setAmmo);
                menu.AddItem(refillMaxAmmo);
            }
            if (IsAllowed(Permission.WPSpawnByName))
            {
                menu.AddItem(spawnByName);
            }
            #endregion

            #region addonweapons submenu
            UIMenuItem addonWeaponsBtn = new UIMenuItem("Addon Weapons", "Equip / remove addon weapons available on this server.");
            UIMenu addonWeaponsMenu = new UIMenu("Addon Weapons", "Equip/Remove Addon Weapons", RightAlignMenus());
            menu.AddItem(addonWeaponsBtn);

            #region manage creating and accessing addon weapons menu
            if (IsAllowed(Permission.WPSpawn) && AddonWeapons != null && AddonWeapons.Count > 0)
            {
                menu.BindMenuToItem(addonWeaponsMenu, addonWeaponsBtn);
                foreach (KeyValuePair<string, uint> weapon in AddonWeapons)
                {
                    string name = weapon.Key.ToString();
                    uint model = weapon.Value;
                    var item = new UIMenuItem(name, $"Click to add/remove this weapon ({name}) to/from your inventory.");
                    addonWeaponsMenu.AddItem(item);
                    if (!IsWeaponValid(model))
                    {
                        item.Enabled = false;
                        item.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        item.Description = "This model is not available. Please ask the server owner to verify it's being streamed correctly.";
                    }
                }
                addonWeaponsMenu.OnItemSelect += (sender, item, index) =>
                {
                    var weapon = AddonWeapons.ElementAt(index);
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, weapon.Value, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, weapon.Value);
                    }
                    else
                    {
                        var maxAmmo = 200;
                        GetMaxAmmo(Game.PlayerPed.Handle, weapon.Value, ref maxAmmo);
                        GiveWeaponToPed(Game.PlayerPed.Handle, weapon.Value, maxAmmo, false, true);
                    }
                };
                addonWeaponsBtn.SetRightLabel("→→→");
            }
            else
            {
                addonWeaponsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                addonWeaponsBtn.Enabled = false;
                addonWeaponsBtn.Description = "This option is not available on this server because you don't have permission to use it, or it is not setup correctly.";
            }
            #endregion

            addonWeaponsMenu.RefreshIndex();
            addonWeaponsMenu.UpdateScaleform();
            #endregion

            #region parachute options menu
            #region parachute buttons and submenus
            UIMenuItem parachuteBtn = new UIMenuItem("Parachute Options", "All parachute related options can be changed here.");
            UIMenu parachuteMenu = new UIMenu("Parachute Options", "Parachute Options", RightAlignMenus());

            UIMenu primaryChute = new UIMenu("Parachute Options", "Select A Primary Parachute", RightAlignMenus());

            UIMenu secondaryChute = new UIMenu("Parachute Options", "Select A Reserve Parachute", RightAlignMenus());

            UIMenuItem chute = new UIMenuItem("No Style", "Default parachute.");
            UIMenuItem chute0 = new UIMenuItem(GetLabelText("PM_TINT0"), GetLabelText("PD_TINT0"));             // Rainbow Chute
            UIMenuItem chute1 = new UIMenuItem(GetLabelText("PM_TINT1"), GetLabelText("PD_TINT1"));             // Red Chute
            UIMenuItem chute2 = new UIMenuItem(GetLabelText("PM_TINT2"), GetLabelText("PD_TINT2"));             // Seaside Stripes Chute
            UIMenuItem chute3 = new UIMenuItem(GetLabelText("PM_TINT3"), GetLabelText("PD_TINT3"));             // Window Maker Chute
            UIMenuItem chute4 = new UIMenuItem(GetLabelText("PM_TINT4"), GetLabelText("PD_TINT4"));             // Patriot Chute
            UIMenuItem chute5 = new UIMenuItem(GetLabelText("PM_TINT5"), GetLabelText("PD_TINT5"));             // Blue Chute
            UIMenuItem chute6 = new UIMenuItem(GetLabelText("PM_TINT6"), GetLabelText("PD_TINT6"));             // Black Chute
            UIMenuItem chute7 = new UIMenuItem(GetLabelText("PM_TINT7"), GetLabelText("PD_TINT7"));             // Hornet Chute
            UIMenuItem chute8 = new UIMenuItem(GetLabelText("PS_CAN_0"), "Air Force parachute.");               // Air Force Chute
            UIMenuItem chute9 = new UIMenuItem(GetLabelText("PM_TINT0"), "Desert parachute.");                  // Desert Chute
            UIMenuItem chute10 = new UIMenuItem("Shadow Chute", "Shadow parachute.");                           // Shadow Chute
            UIMenuItem chute11 = new UIMenuItem(GetLabelText("UNLOCK_NAME_PSRWD"), "High altitude parachute."); // High Altitude Chute
            UIMenuItem chute12 = new UIMenuItem("Airborne Chute", "Airborne parachute.");                       // Airborne Chute
            UIMenuItem chute13 = new UIMenuItem("Sunrise Chute", "Sunrise parachute.");                         // Sunrise Chute
            UIMenuItem rchute = new UIMenuItem("No Style", "Default parachute.");
            UIMenuItem rchute0 = new UIMenuItem(GetLabelText("PM_TINT0"), GetLabelText("PD_TINT0"));             // Rainbow Chute
            UIMenuItem rchute1 = new UIMenuItem(GetLabelText("PM_TINT1"), GetLabelText("PD_TINT1"));             // Red Chute
            UIMenuItem rchute2 = new UIMenuItem(GetLabelText("PM_TINT2"), GetLabelText("PD_TINT2"));             // Seaside Stripes Chute
            UIMenuItem rchute3 = new UIMenuItem(GetLabelText("PM_TINT3"), GetLabelText("PD_TINT3"));             // Window Maker Chute
            UIMenuItem rchute4 = new UIMenuItem(GetLabelText("PM_TINT4"), GetLabelText("PD_TINT4"));             // Patriot Chute
            UIMenuItem rchute5 = new UIMenuItem(GetLabelText("PM_TINT5"), GetLabelText("PD_TINT5"));             // Blue Chute
            UIMenuItem rchute6 = new UIMenuItem(GetLabelText("PM_TINT6"), GetLabelText("PD_TINT6"));             // Black Chute
            UIMenuItem rchute7 = new UIMenuItem(GetLabelText("PM_TINT7"), GetLabelText("PD_TINT7"));             // Hornet Chute
            UIMenuItem rchute8 = new UIMenuItem(GetLabelText("PS_CAN_0"), "Air Force parachute.");               // Air Force Chute
            UIMenuItem rchute9 = new UIMenuItem(GetLabelText("PM_TINT0"), "Desert parachute.");                  // Desert Chute
            UIMenuItem rchute10 = new UIMenuItem("Shadow Chute", "Shadow parachute.");                           // Shadow Chute
            UIMenuItem rchute11 = new UIMenuItem(GetLabelText("UNLOCK_NAME_PSRWD"), "High altitude parachute."); // High Altitude Chute
            UIMenuItem rchute12 = new UIMenuItem("Airborne Chute", "Airborne parachute.");                       // Airborne Chute
            UIMenuItem rchute13 = new UIMenuItem("Sunrise Chute", "Sunrise parachute.");                         // Sunrise Chute

            primaryChute.AddItem(chute);
            primaryChute.AddItem(chute0);
            primaryChute.AddItem(chute1);
            primaryChute.AddItem(chute2);
            primaryChute.AddItem(chute3);
            primaryChute.AddItem(chute4);
            primaryChute.AddItem(chute5);
            primaryChute.AddItem(chute6);
            primaryChute.AddItem(chute7);
            primaryChute.AddItem(chute8);
            primaryChute.AddItem(chute9);
            primaryChute.AddItem(chute10);
            primaryChute.AddItem(chute11);
            primaryChute.AddItem(chute12);
            primaryChute.AddItem(chute13);

            secondaryChute.AddItem(rchute);
            secondaryChute.AddItem(rchute0);
            secondaryChute.AddItem(rchute1);
            secondaryChute.AddItem(rchute2);
            secondaryChute.AddItem(rchute3);
            secondaryChute.AddItem(rchute4);
            secondaryChute.AddItem(rchute5);
            secondaryChute.AddItem(rchute6);
            secondaryChute.AddItem(rchute7);
            secondaryChute.AddItem(rchute8);
            secondaryChute.AddItem(rchute9);
            secondaryChute.AddItem(rchute10);
            secondaryChute.AddItem(rchute11);
            secondaryChute.AddItem(rchute12);
            secondaryChute.AddItem(rchute13);
            #endregion

            #region handle events
            primaryChute.OnItemSelect += (sender, item, index) =>
            {
                SetPedParachuteTintIndex(Game.PlayerPed.Handle, index - 1);
                Subtitle.Custom($"Primary parachute style selected: ~r~{item.Text}~s~.");
            };

            secondaryChute.OnItemSelect += (sender, item, index) =>
            {
                SetPlayerReserveParachuteTintIndex(Game.Player.Handle, index - 1);
                Subtitle.Custom($"Reserve parachute style selected: ~r~{item.Text}~s~.");
            };
            #endregion

            #region create more buttons
            UIMenuItem primaryChuteBtn = new UIMenuItem("Primary Parachute Style", "Select a primary parachute.");
            UIMenuItem secondaryChuteBtn = new UIMenuItem("Reserve Parachute Style", "Select a reserve parachute.");

            parachuteMenu.AddItem(primaryChuteBtn);
            primaryChuteBtn.SetRightLabel("→→→");
            parachuteMenu.AddItem(secondaryChuteBtn);
            secondaryChuteBtn.SetRightLabel("→→→");

            parachuteMenu.BindMenuToItem(primaryChute, primaryChuteBtn);
            parachuteMenu.BindMenuToItem(secondaryChute, secondaryChuteBtn);

            UIMenuCheckboxItem autoEquipParachute = new UIMenuCheckboxItem("Auto Equip Parachute", AutoEquipChute, "Automatically equip a parachute whenever you enter a plane/helicopter.");
            parachuteMenu.AddItem(autoEquipParachute);

            UIMenuItem togglePrimary = new UIMenuItem("Get / Remove Primary Parachute", "Equip a primary parachute.");
            UIMenuItem toggleSecondary = new UIMenuItem("Get Reserve Parachute", "Equip a reserve parachute, you need to get a primary parachute first before equipping a reserve parachute.");

            parachuteMenu.AddItem(togglePrimary);
            parachuteMenu.AddItem(toggleSecondary);
            #endregion

            #region handle parachute menu events
            parachuteMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == togglePrimary)
                {
                    if (HasPedGotWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, false))
                    {
                        RemoveWeaponFromPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute);
                        Notify.Success("Primary parachute ~r~removed~s~.", true);
                    }
                    else
                    {
                        GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, false);
                        Notify.Success("Primary parachute ~g~equippped~s~.", true);
                    }
                }
                else if (item == toggleSecondary)
                {
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                    Notify.Success("Reserve parachute ~g~equippped~s~.", true);
                }
            };

            parachuteMenu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (item == autoEquipParachute)
                {
                    AutoEquipChute = _checked;
                }
            };
            #endregion

            #region parachute smoke trail colors
            List<dynamic> smokeColor = new List<dynamic>()
            {
                "White",
                "Yellow",
                "Red",
                "Green",
                "Blue",
                "Dark Gray",
            };

            UIMenuListItem smokeColors = new UIMenuListItem("Smoke Trail Color", smokeColor, 0, "Select a parachute smoke trail color.");
            parachuteMenu.AddItem(smokeColors);
            parachuteMenu.OnListChange += (sender, item, index) =>
            {
                if (item == smokeColors)
                {
                    SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, false);
                    if (index == 0)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 255, 255);
                    }
                    else if (index == 1)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 255, 0);
                    }
                    else if (index == 2)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 255, 0, 0);
                    }
                    else if (index == 3)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 0, 255, 0);
                    }
                    else if (index == 4)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 0, 0, 255);
                    }
                    else if (index == 5)
                    {
                        SetPlayerParachuteSmokeTrailColor(Game.Player.Handle, 1, 1, 1);
                    }

                    SetPlayerCanLeaveParachuteSmokeTrail(Game.Player.Handle, true);
                }
            };
            #endregion

            #region misc parachute menu setup
            menu.AddItem(parachuteBtn);
            parachuteBtn.SetRightLabel("→→→");
            menu.BindMenuToItem(parachuteMenu, parachuteBtn);

            parachuteMenu.RefreshIndex();
            parachuteMenu.UpdateScaleform();

            primaryChute.RefreshIndex();
            primaryChute.UpdateScaleform();

            secondaryChute.RefreshIndex();
            secondaryChute.UpdateScaleform();

            MainMenu.Mp.Add(addonWeaponsMenu);
            MainMenu.Mp.Add(parachuteMenu);
            MainMenu.Mp.Add(primaryChute);
            MainMenu.Mp.Add(secondaryChute);
            #endregion
            #endregion

            #region Create Weapon Category Submenus
            UIMenuItem spacer = GetSpacerMenuItem("↓ Weapon Categories ↓");
            menu.AddItem(spacer);

            UIMenu handGuns = new UIMenu("Weapons", "Handguns", RightAlignMenus());
            UIMenuItem handGunsBtn = new UIMenuItem("Handguns");

            UIMenu rifles = new UIMenu("Weapons", "Assault Rifles", RightAlignMenus());
            UIMenuItem riflesBtn = new UIMenuItem("Assault Rifles");

            UIMenu shotguns = new UIMenu("Weapons", "Shotguns", RightAlignMenus());
            UIMenuItem shotgunsBtn = new UIMenuItem("Shotguns");

            UIMenu smgs = new UIMenu("Weapons", "Sub-/Light Machine Guns", RightAlignMenus());
            UIMenuItem smgsBtn = new UIMenuItem("Sub-/Light Machine Guns");

            UIMenu throwables = new UIMenu("Weapons", "Throwables", RightAlignMenus());
            UIMenuItem throwablesBtn = new UIMenuItem("Throwables");

            UIMenu melee = new UIMenu("Weapons", "Melee", RightAlignMenus());
            UIMenuItem meleeBtn = new UIMenuItem("Melee");

            UIMenu heavy = new UIMenu("Weapons", "Heavy Weapons", RightAlignMenus());
            UIMenuItem heavyBtn = new UIMenuItem("Heavy Weapons");

            UIMenu snipers = new UIMenu("Weapons", "Sniper Rifles", RightAlignMenus());
            UIMenuItem snipersBtn = new UIMenuItem("Sniper Rifles");

            MainMenu.Mp.Add(handGuns);
            MainMenu.Mp.Add(rifles);
            MainMenu.Mp.Add(shotguns);
            MainMenu.Mp.Add(smgs);
            MainMenu.Mp.Add(throwables);
            MainMenu.Mp.Add(melee);
            MainMenu.Mp.Add(heavy);
            MainMenu.Mp.Add(snipers);
            #endregion

            #region Setup weapon category buttons and submenus.
            handGunsBtn.SetRightLabel("→→→");
            menu.AddItem(handGunsBtn);
            menu.BindMenuToItem(handGuns, handGunsBtn);

            riflesBtn.SetRightLabel("→→→");
            menu.AddItem(riflesBtn);
            menu.BindMenuToItem(rifles, riflesBtn);

            shotgunsBtn.SetRightLabel("→→→");
            menu.AddItem(shotgunsBtn);
            menu.BindMenuToItem(shotguns, shotgunsBtn);

            smgsBtn.SetRightLabel("→→→");
            menu.AddItem(smgsBtn);
            menu.BindMenuToItem(smgs, smgsBtn);

            throwablesBtn.SetRightLabel("→→→");
            menu.AddItem(throwablesBtn);
            menu.BindMenuToItem(throwables, throwablesBtn);

            meleeBtn.SetRightLabel("→→→");
            menu.AddItem(meleeBtn);
            menu.BindMenuToItem(melee, meleeBtn);

            heavyBtn.SetRightLabel("→→→");
            menu.AddItem(heavyBtn);
            menu.BindMenuToItem(heavy, heavyBtn);

            snipersBtn.SetRightLabel("→→→");
            menu.AddItem(snipersBtn);
            menu.BindMenuToItem(snipers, snipersBtn);
            #endregion

            #region Loop through all weapons, create menus for them and add all menu items and handle events.
            foreach (ValidWeapon weapon in vw.WeaponList)
            {
                uint cat = (uint)GetWeapontypeGroup(weapon.Hash);
                if (weapon.Name != null && (IsAllowed(weapon.Perm) || IsAllowed(Permission.WPGetAll)))
                {
                    #region Create menu for this weapon and add buttons
                    UIMenu weaponMenu = new UIMenu("Weapon Options", weapon.Name, RightAlignMenus());
                    UIMenuItem weaponItem = new UIMenuItem(weapon.Name, $"Open the options for ~y~{weapon.Name.ToString()}~s~.");
                    weaponItem.SetRightLabel("→→→");
                    weaponItem.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);

                    MainMenu.Mp.Add(weaponMenu);

                    weaponInfo.Add(weaponMenu, weapon);

                    UIMenuItem getOrRemoveWeapon = new UIMenuItem("Equip/Remove Weapon", "Add or remove this weapon to/form your inventory.");
                    getOrRemoveWeapon.SetLeftBadge(UIMenuItem.BadgeStyle.Gun);
                    weaponMenu.AddItem(getOrRemoveWeapon);
                    if (!IsAllowed(Permission.WPSpawn))
                    {
                        getOrRemoveWeapon.Enabled = false;
                        getOrRemoveWeapon.Description = "This option has been disabled by the server owner.";
                        getOrRemoveWeapon.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    }

                    UIMenuItem fillAmmo = new UIMenuItem("Re-fill Ammo", "Get max ammo for this weapon.");
                    fillAmmo.SetLeftBadge(UIMenuItem.BadgeStyle.Ammo);
                    weaponMenu.AddItem(fillAmmo);

                    List<dynamic> tints = new List<dynamic>();
                    if (weapon.Name.Contains(" Mk II"))
                    {
                        foreach (var tint in ValidWeapons.WeaponTintsMkII)
                        {
                            tints.Add(tint.Key);
                        }
                    }
                    else
                    {
                        foreach (var tint in ValidWeapons.WeaponTints)
                        {
                            tints.Add(tint.Key);
                        }
                    }

                    UIMenuListItem weaponTints = new UIMenuListItem("Tints", tints, 0, "Select a tint for your weapon.");
                    weaponMenu.AddItem(weaponTints);
                    #endregion

                    #region Handle weapon specific list changes
                    weaponMenu.OnListChange += (sender, item, index) =>
                    {
                        if (item == weaponTints)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, weaponInfo[sender].Hash, false))
                            {
                                SetPedWeaponTintIndex(Game.PlayerPed.Handle, weaponInfo[sender].Hash, index);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first!");
                            }
                        }
                    };
                    #endregion

                    #region Handle weapon specific button presses
                    weaponMenu.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == getOrRemoveWeapon)
                        {
                            var info = weaponInfo[sender];
                            uint hash = info.Hash;
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, hash, false))
                            {
                                RemoveWeaponFromPed(Game.PlayerPed.Handle, hash);
                                Subtitle.Custom("Weapon removed.");
                            }
                            else
                            {
                                var ammo = 255;
                                GetMaxAmmo(Game.PlayerPed.Handle, hash, ref ammo);
                                GiveWeaponToPed(Game.PlayerPed.Handle, hash, ammo, false, true);
                                Subtitle.Custom("Weapon added.");
                            }
                        }
                        else if (item == fillAmmo)
                        {
                            if (HasPedGotWeapon(Game.PlayerPed.Handle, weaponInfo[sender].Hash, false))
                            {
                                var ammo = 900;
                                GetMaxAmmo(Game.PlayerPed.Handle, weaponInfo[sender].Hash, ref ammo);
                                SetAmmoInClip(Game.PlayerPed.Handle, weaponInfo[sender].Hash, ammo);
                            }
                            else
                            {
                                Notify.Error("You need to get the weapon first before re-filling ammo!");
                            }
                        }
                    };
                    #endregion

                    #region load components
                    if (weapon.Components != null)
                    {
                        if (weapon.Components.Count > 0)
                        {
                            foreach (var comp in weapon.Components)
                            {
                                UIMenuItem compItem = new UIMenuItem(comp.Key, "Click to equip or remove this component.");
                                weaponComponents.Add(compItem, comp.Key);
                                weaponMenu.AddItem(compItem);

                                #region Handle component button presses
                                weaponMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    if (item == compItem)
                                    {
                                        var Weapon = weaponInfo[sender];
                                        var componentHash = Weapon.Components[weaponComponents[item]];
                                        if (HasPedGotWeapon(Game.PlayerPed.Handle, Weapon.Hash, false))
                                        {
                                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, Weapon.Hash, componentHash))
                                            {
                                                RemoveWeaponComponentFromPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);
                                                Subtitle.Custom("Component removed.");
                                            }
                                            else
                                            {
                                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, Weapon.Hash, componentHash);
                                                Subtitle.Custom("Component equiped.");
                                            }
                                        }
                                        else
                                        {
                                            Notify.Error("You need to get the weapon first before you can modify it.");
                                        }
                                    }
                                };
                                #endregion
                            }
                        }
                    }
                    #endregion

                    // refresh and add to menu.
                    weaponMenu.RefreshIndex();
                    weaponMenu.UpdateScaleform();

                    if (cat == 970310034) // 970310034 rifles
                    {
                        rifles.AddItem(weaponItem);
                        rifles.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 416676503 || cat == 690389602) // 416676503 hand guns // 690389602 stun gun
                    {
                        handGuns.AddItem(weaponItem);
                        handGuns.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 860033945) // 860033945 shotguns
                    {
                        shotguns.AddItem(weaponItem);
                        shotguns.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 3337201093 || cat == 1159398588) // 3337201093 sub machine guns // 1159398588 light machine guns
                    {
                        smgs.AddItem(weaponItem);
                        smgs.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 1548507267 || cat == 4257178988 || cat == 1595662460) // 1548507267 throwables // 4257178988 fire extinghuiser // jerry can
                    {
                        throwables.AddItem(weaponItem);
                        throwables.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 3566412244 || cat == 2685387236) // 3566412244 melee weapons // 2685387236 knuckle duster
                    {
                        melee.AddItem(weaponItem);
                        melee.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 2725924767) // 2725924767 heavy weapons
                    {
                        heavy.AddItem(weaponItem);
                        heavy.BindMenuToItem(weaponMenu, weaponItem);
                    }
                    else if (cat == 3082541095) // 3082541095 sniper rifles
                    {
                        snipers.AddItem(weaponItem);
                        snipers.BindMenuToItem(weaponMenu, weaponItem);
                    }
                }
            }
            #endregion

            #region Disable submenus if no weapons in that category are allowed.
            if (handGuns.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(handGunsBtn);
                handGunsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                handGunsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                handGunsBtn.Enabled = false;
            }
            if (rifles.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(riflesBtn);
                riflesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                riflesBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                riflesBtn.Enabled = false;
            }
            if (shotguns.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(shotgunsBtn);
                shotgunsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                shotgunsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                shotgunsBtn.Enabled = false;
            }
            if (smgs.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(smgsBtn);
                smgsBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                smgsBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                smgsBtn.Enabled = false;
            }
            if (throwables.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(throwablesBtn);
                throwablesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                throwablesBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                throwablesBtn.Enabled = false;
            }
            if (melee.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(meleeBtn);
                meleeBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                meleeBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                meleeBtn.Enabled = false;
            }
            if (heavy.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(heavyBtn);
                heavyBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                heavyBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                heavyBtn.Enabled = false;
            }
            if (snipers.MenuItems.Count == 0)
            {
                menu.ReleaseMenuFromItem(snipersBtn);
                snipersBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                snipersBtn.Description = "The server owner removed the permissions for all weapons in this category.";
                snipersBtn.Enabled = false;
            }
            #endregion

            #region Handle button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                Ped ped = new Ped(Game.PlayerPed.Handle);
                if (item == getAllWeapons)
                {
                    foreach (var weapon in ValidWeapons.Weapons)
                    {
                        var ammo = 255;
                        GetMaxAmmo(Game.PlayerPed.Handle, weapon.Value, ref ammo);
                        ped.Weapons.Give((WeaponHash)weapon.Value, ammo, weapon.Key == "Unarmed", true);
                    }
                    ped.Weapons.Give(WeaponHash.Unarmed, 0, true, true);
                }
                else if (item == removeAllWeapons)
                {
                    ped.Weapons.RemoveAll();
                }
                else if (item == setAmmo)
                {
                    SetAllWeaponsAmmo();
                }
                else if (item == refillMaxAmmo)
                {
                    foreach (var wp in ValidWeapons.Weapons)
                    {
                        if (ped.Weapons.HasWeapon((WeaponHash)wp.Value))
                        {
                            int maxammo = 200;
                            GetMaxAmmo(ped.Handle, wp.Value, ref maxammo);
                            SetPedAmmo(ped.Handle, wp.Value, maxammo);
                        }
                    }
                }
                else if (item == spawnByName)
                {
                    SpawnCustomWeapon();
                }
            };
            #endregion

            #region Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (item == noReload)
                {
                    NoReload = _checked;
                    Subtitle.Custom($"No reload is now {(_checked ? "enabled" : "disabled")}.");
                }
                else if (item == unlimitedAmmo)
                {
                    UnlimitedAmmo = _checked;
                    Subtitle.Custom($"Unlimited ammo is now {(_checked ? "enabled" : "disabled")}.");
                }
            };
            #endregion
        }
        #endregion

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}