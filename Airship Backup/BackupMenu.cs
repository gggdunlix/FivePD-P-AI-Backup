using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MenuAPI;

namespace BackupMenu
{
    public class BackupMenu
    {
        public class Plugin : FivePD.API.Plugin
        {
            Menu menu = new MenuAPI.Menu("Backup Menu");
            internal Plugin()
            {


                Debug.Write("\n^2[^4Backup Menu^2] AI Backup Menu by GGGDunlix loading...\n");


                Tick += RegisterMenuCommand;

            }
            public async Task RegisterMenuCommand()
            {
                API.RegisterCommand("toggleMenuVisibility", new Action<bool>((showing) =>
                {

                    //NOTE TO SELF : Contributers to remember:
                    //Goose10X for Heli and Spotlight
                    //Rex for Duty Menu & JSON stuff
                    //Natixco for JSON Stuff & eventhandler if used
                    //
                    if (menu.Visible)
                    {
                        menu.Visible = false;
                        Debug.Write("\n^2[^4Backup Menu^2] Closed menu\n");
                    }
                    else
                    {
                        StartMenu();
                        menu.Visible = true;
                        Debug.Write("\n^2[^4Backup Menu^2] Opened menu\n");
                    }
                    showing = menu.Visible;
                }), false /*This command is also not restricted, anyone can use it.*/ );
                API.RegisterKeyMapping("toggleMenuVisibility", "Open Backup Menu", "keyboard", "F10");
            }






            public async Task StartMenu()
            {
                menu.ClearMenuItems();
                var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                var configJson = JObject.Parse(configFile);
                JToken menuConfig = configJson["BackupMenuPlugin"];

                // ABOUT
                MenuItem about = new MenuItem("~c~About");
                about.Description = "This plugin was made by ~y~GGGDunlix~s~ for ~b~FivePD~s~. Several people have helped in the making of it, including ~y~Grandpa Rex~s~, ~y~Goose10X~s~, and ~y~Natixco~s~.";
                about.LeftIcon = MenuItem.Icon.INFO;
                // PLAYER BACKUP
                JToken playerBackupConfig = menuConfig["playerBackup"];
                bool playerBackupEnabled = ((bool)playerBackupConfig["enabled"]);
                MenuItem playerBackup = new MenuItem(playerBackupConfig["title"].ToString(), playerBackupConfig["description"].ToString());
                if (playerBackupEnabled == true)
                {
                    playerBackup.Enabled = true;
                } else
                {
                    playerBackup.Enabled = false;
                }

                //MENU SETTINGS
                menu.MenuTitle = menuConfig["menuTitle"].ToString();
                menu.MenuSubtitle = "by GGGDunlix";



                MenuController.AddMenu(menu);
                menu.AddMenuItem(about);



            }



        }
    }
}
