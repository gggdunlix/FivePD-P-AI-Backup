using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Converters;
using MenuAPI;
using System.Linq;


namespace BackupMenu
{
    public class BackupMenu
    {

        public class Plugin : FivePD.API.Plugin
        {
            Menu menu = new MenuAPI.Menu("Backup Menu");

            public static JToken AIBackupConf = JObject.Parse(API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json"))["BackupMenuPlugin"]["AIBackup"];
            public static JToken Code1Conf = AIBackupConf["items"]["code1"];

            public Vehicle CopCar, ReqCar;
            public Ped CopPed, DeliveryOfficer;
            public bool isBackupUp;

            Blip CopPedBlip;
            
            MenuItem CancelBackupButtonCode1 = new MenuItem(((string)AIBackupConf["items"]["code1"]["cancelTitle"]), (string)AIBackupConf["items"]["code1"]["cancelDescription"]);
            MenuItem CancelBackupButtonCode2 = new MenuItem(((string)AIBackupConf["items"]["code2"]["cancelTitle"]), (string)AIBackupConf["items"]["code2"]["cancelDescription"]);
            MenuItem Code1AI = new MenuItem(Code1Conf["text"].ToString(), Code1Conf["description"].ToString());

            RelationshipGroup Backups = new RelationshipGroup(5555555);
            internal Plugin()
            {
                Events.OnDutyStatusChange += OnDutyStatusChange;

                Debug.Write("\n^2[^4Backup Menu^2] AI Backup Menu by GGGDunlix loading...\n");

                OnDutyStatusChange(true);
                OnDutyStatusChange(false);

                StartMenu();

                RegisterMenuCommand();

                Debug.Write("\n^2[^4Backup Menu^2] AI Backup Menu by GGGDunlix loaded. Press F10 to open, or change the keybind in your FiveM settings.\n");
            }

            private async Task OnDutyStatusChange(bool onDuty)
            {
                if (onDuty)
                {
                    var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                    var configJson = JObject.Parse(configFile);
                    JToken menuConfig = configJson["BackupMenuPlugin"];
                    bool allowWhenOnDuty = ((bool)menuConfig["allowWhenOnDuty"]);
                    if (allowWhenOnDuty)
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
                                menu.Visible = true;
                                Debug.Write("\n^2[^4Backup Menu^2] Opened menu\n");
                            }
                            showing = menu.Visible;
                        }), false /*This command is also not restricted, anyone can use it.*/ );
                    }
                    else
                    {
                        API.RegisterCommand("toggleMenuVisibility", new Action<bool>((showing) =>
                        {


                            Debug.Write("\n^2[^4Backup Menu^2] Cannot open Backup menu when On Duty.\n");
                        }), false /*This command is also not restricted, anyone can use it.*/ );
                        menu.CloseMenu();
                    }
                }
                else
                {
                    var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                    var configJson = JObject.Parse(configFile);
                    JToken menuConfig = configJson["BackupMenuPlugin"];
                    bool allowWhenOffDuty = ((bool)menuConfig["allowWhenOffDuty"]);
                    if (allowWhenOffDuty)
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
                                menu.Visible = true;
                                Debug.Write("\n^2[^4Backup Menu^2] Opened menu\n");
                            }
                            showing = menu.Visible;
                        }), false /*This command is also not restricted, anyone can use it.*/ );
                    }
                    else
                    {
                        API.RegisterCommand("toggleMenuVisibility", new Action<bool>((showing) =>
                        {


                            Debug.Write("\n^2[^4Backup Menu^2] Cannot open Backup menu when Off Duty.\n");
                        }), false /*This command is also not restricted, anyone can use it.*/ );
                        menu.CloseMenu();
                    }
                }
            }

            public async Task RegisterMenuCommand()
            {

                API.RegisterKeyMapping("toggleMenuVisibility", "Open Backup Menu", "keyboard", "F10");
            }






            public async Task StartMenu()
            {
                menu.ClearMenuItems();
                var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                var configJson = JObject.Parse(configFile);
                JToken menuConfig = configJson["BackupMenuPlugin"];

                string DisabledMessage = menuConfig["disabledMessage"].ToString();

                // ABOUT
                MenuItem about = new MenuItem("~c~About");
                about.Description = "This plugin was made by ~y~GGGDunlix~s~ for ~b~FivePD~s~. Several people have helped in the making of it, including ~y~Grandpa Rex~s~, ~y~Goose10X~s~, and ~y~Natixco~s~.";
                about.LeftIcon = MenuItem.Icon.INV_QUESTIONMARK;
                // PLAYER BACKUP
                JToken playerBackupConfig = menuConfig["playerBackup"];
                bool playerBackupEnabled = ((bool)playerBackupConfig["enabled"]);
                Menu playerBackup = new Menu(playerBackupConfig["title"].ToString(), playerBackupConfig["description"].ToString());
                MenuItem playerBackupOpen = new MenuItem(playerBackupConfig["title"].ToString(), playerBackupConfig["description"].ToString());

                if (playerBackupEnabled == true)
                {
                    playerBackupOpen.ParentMenu = menu;
                    MenuController.BindMenuItem(menu, playerBackup, playerBackupOpen);

                    JToken playerBackupItems = playerBackupConfig["items"];

                    playerBackupOpen.LeftIcon = MenuItem.Icon.INV_PERSON;

                    //Code1:
                    JToken Code1 = playerBackupItems["code1"];
                    if (((bool)Code1["enabled"]))
                    {
                        MenuItem Code1Item = new MenuItem(Code1["text"].ToString());

                        playerBackup.OnItemSelect += (_menu, _item, _index) =>
                        {
                            if (_item == Code1Item)
                            {
                                FivePD.API.Utilities.RequestBackup(Utilities.Backups.Code1);
                            }
                        };

                        playerBackup.AddMenuItem(Code1Item);

                    }

                    //Code2:
                    JToken Code2 = playerBackupItems["code2"];
                    if (((bool)Code2["enabled"]))
                    {
                        MenuItem Code2Item = new MenuItem(Code2["text"].ToString());

                        playerBackup.OnItemSelect += (_menu, _item, _index) =>
                        {
                            if (_item == Code2Item)
                            {
                                FivePD.API.Utilities.RequestBackup(Utilities.Backups.Code2);
                            }
                        };

                        playerBackup.AddMenuItem(Code2Item);

                    }

                    //Code3:
                    JToken Code3 = playerBackupItems["code3"];
                    if (((bool)Code3["enabled"]))
                    {
                        MenuItem Code3Item = new MenuItem(Code3["text"].ToString());

                        playerBackup.OnItemSelect += (_menu, _item, _index) =>
                        {
                            if (_item == Code3Item)
                            {
                                FivePD.API.Utilities.RequestBackup(Utilities.Backups.Code3);
                            }
                        };

                        playerBackup.AddMenuItem(Code3Item);

                    }

                    //Code99:
                    JToken Code99 = playerBackupItems["code99"];
                    if (((bool)Code99["enabled"]))
                    {
                        MenuItem Code99Item = new MenuItem(Code99["text"].ToString());

                        playerBackup.OnItemSelect += (_menu, _item, _index) =>
                        {
                            if (_item == Code99Item)
                            {
                                Utilities.RequestBackup(Utilities.Backups.Code99);
                            }
                        };

                        playerBackup.AddMenuItem(Code99Item);

                    }


                    //Other Agencies:
                    //JToken OtherAgencies = playerBackupItems["multiAgencyBackup"];
                    //if (((bool)OtherAgencies["enabled"]))
                    //{
                    //    MenuItem multiAgencyBackupOpen = new MenuItem(OtherAgencies["text"].ToString(), OtherAgencies["description"].ToString());
                    //    Menu MultiAgencyBackup = new Menu(OtherAgencies["text"].ToString(), OtherAgencies["description"].ToString());
                    //
                    //    MenuController.AddSubmenu(playerBackup, MultiAgencyBackup);
                    //    
                    //                  Utilities.RequestBackup(Utilities.Backups)
                    //   playerBackup.AddMenuItem(multiAgencyBackupOpen);
                    //
                    //}


                }
                else
                {
                    playerBackupOpen.Description = DisabledMessage;
                    playerBackupOpen.Enabled = false;
                    playerBackupOpen.LeftIcon = MenuItem.Icon.LOCK;
                    playerBackup.Visible = false;

                }
                //AI BACKUP
                var AIBackupConfig = menuConfig["AIBackup"];
                bool IsAIEnabled = ((bool)AIBackupConfig["enabled"]);

                if (IsAIEnabled)
                {
                    Menu AIBackup = new Menu(AIBackupConfig["title"].ToString(), AIBackupConfig["description"].ToString());
                    MenuItem AIBackupOpen = new MenuItem(AIBackupConfig["title"].ToString(), AIBackupConfig["description"].ToString());


                    MenuController.AddSubmenu(menu, AIBackup);
                    MenuController.BindMenuItem(menu, AIBackup, AIBackupOpen);

                    AIBackupOpen.LeftIcon = MenuItem.Icon.BRAND_PED;
                    JToken AIBackupItems = AIBackupConfig["items"];

                    menu.AddMenuItem(AIBackupOpen);

                    //Code1
                    JToken Code1ConfigAI = AIBackupItems["code1"];

                    if (((bool)Code1ConfigAI["enabled"]))
                    {
                        Code1AI = new MenuItem(Code1ConfigAI["text"].ToString(), Code1ConfigAI["description"].ToString());
                        AIBackup.AddMenuItem(Code1AI);
                        AIBackup.OnItemSelect += async (_menu, _item, _index) =>
                        {
                            if (_item == Code1AI)
                            {
                                AIBackup.RemoveMenuItem(Code1AI);
                                
                                var Vehicles = Code1ConfigAI["vehicle"];
                                List<JToken> VehicleList = new List<JToken>();

                                foreach (string vehicle in Vehicles)
                                {
                                    VehicleList.Add(vehicle);
                                }
                                var random = new Random();
                                int vindex = random.Next(0, VehicleList.Count);



                                string chosenVehicle = VehicleList.ElementAt(vindex).ToString();
                                await RequestAICode1(Game.PlayerPed, Game.PlayerPed, chosenVehicle, PedHash.Cop01SMY);
                                
                                Debug.Write("\n^2[^4Backup Menu^2] Requesting Code 1 Backup.\n");
                                AIBackup.AddMenuItem(CancelBackupButtonCode1);
                            } else if (_item == CancelBackupButtonCode1)
                            {
                                AIBackup.RemoveMenuItem(CancelBackupButtonCode1);

                                CancelBackup();

                                AIBackup.AddMenuItem(Code1AI);
                            }
                        };
                    }

                    

                }

                //Request Vehicle
                var ReqVehSettings = menuConfig["requestVehicle"];

                bool isReqVehEnabled = ((bool)ReqVehSettings["enabled"]);
                if (isReqVehEnabled)
                {
                    if (ReqVehSettings["mode"].ToString() == "list")
                    {
                        //LIST MODE:

                        var VehiclesJson = API.LoadResourceFile(API.GetCurrentResourceName(), "/config/vehicles.json");
                        var Vehicles = JObject.Parse(VehiclesJson);
                        JToken PoliceVehicles = Vehicles["police"];
                        //*Potential JTOKEN problem here ^

                        List<JToken> VehicleList = new List<JToken>();
                        foreach (JToken jtoken in PoliceVehicles)
                        {
                            PlayerData PData = Utilities.GetPlayerData();
                            int Department = PData.DepartmentID;
                            if (jtoken["availableForDepartments"] != null)
                            {
                                List<int> allowedDepts = new List<int>();
                                foreach (int dept in jtoken["availableForDepartments"])
                                {
                                    allowedDepts.Add(dept);
                                }

                                if (allowedDepts.Contains(Department))
                                {
                                    VehicleList.Add(jtoken);
                                }

                            }
                            else if (jtoken["availableForDepartments"] == null)
                            {
                                VehicleList.Add(jtoken);
                            }
                        }
                        List<string> VehicleNameList = new List<string>();
                        foreach (JToken jtoken in VehicleList)
                        {
                            VehicleNameList.Add(jtoken["name"].ToString());
                        }
                        MenuListItem ReqVehList = new MenuListItem(ReqVehSettings["title"].ToString(), VehicleNameList, VehicleNameList.Count, ReqVehSettings["description"].ToString());
                        ReqVehList.LeftIcon = MenuItem.Icon.CAR;
                        menu.AddMenuItem(ReqVehList);

                        menu.OnListItemSelect += async (_menu, _listItem, _listIndex, _itemIndex) =>
                        {
                            // Code in here would get executed whenever a list item is pressed.
                            if (_menu == menu)
                            {
                                if (_listItem == ReqVehList)
                                {
                                    JToken vehicle = VehicleList.ElementAt(_listIndex);

                                    string ReqVehicleModel = ((string)vehicle["vehicle"]);
                                    Debug.Write("\n^2[^4Backup Menu^2] JSON model: " + ReqVehicleModel + "\n");
                                    float SpawnDistance = ((float)ReqVehSettings["spawnDistance"]);
                                    float stopDistance = (float)ReqVehSettings["stopDistance"];
                                    float driveSpeed = (float)ReqVehSettings["driveSpeed"];

                                    Vector3 SpawnLoc = World.GetNextPositionOnStreet(Vector3Extension.Around(Game.PlayerPed.Position, SpawnDistance));
                                    Debug.Write("\n^2[^4Backup Menu^2] Spawn Loc: " + SpawnLoc + "\n");

                                    uint vehicleHash = (uint)GetHashKey(ReqVehicleModel);
                                    RequestModel(vehicleHash);
                                    Debug.Write("\n^2[^4Backup Menu^2] Requested vehicle model\n");
                                    int maxretries = 0;
                                    while (!HasModelLoaded(vehicleHash) && maxretries < 300)
                                    {
                                        await BaseScript.Delay(100);
                                        maxretries++;
                                    }

                                    if (HasModelLoaded(vehicleHash))
                                    {
                                        Debug.Write("\n^2[^4Backup Menu^2] Model loaded\n");
                                        Vehicle ReqCar = new Vehicle(CreateVehicle(vehicleHash, SpawnLoc.X, SpawnLoc.Y, SpawnLoc.Z, 0, true, false));
                                        SetVehicleOnGroundProperly(ReqCar.Handle);
                                        SetVehicleHasBeenOwnedByPlayer(ReqCar.Handle, true);
                                        SetEntityAsMissionEntity(ReqCar.Handle, true, true);
                                        Debug.Write("\n^2[^4Backup Menu^2] Vehicle Spawned\n");
                                        Debug.Write("\n^2[^4Backup Menu^2] Spawning ped\n");
                                        DeliveryOfficer = await Utilities.SpawnPed(PedHash.UndercoverCopCutscene, SpawnLoc, 0);
                                        Debug.Write("\n^2[^4Backup Menu^2] Setting seat ped\n");
                                        SetPedIntoVehicle(DeliveryOfficer.Handle, ReqCar.Handle, ((int)VehicleSeat.Driver));


                                        Debug.Write("\n^2[^4Backup Menu^2] doing extra stuff\n");
                                        await BaseScript.Delay(100);
                                        CitizenFX.Core.Vehicle pvehicle = ReqCar;
                                        string vehiclename = ReqCar.DisplayName.ToLower();
                                        var json = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/vehicles.json"));
                                        JToken policeVehicles = json["police"];
                                        JToken playerVehicleConfig = policeVehicles.FirstOrDefault(_vehicle => _vehicle["vehicle"].ToString().ToLower() == vehiclename);
                                        JToken extras = playerVehicleConfig["extras"];

                                        if (extras != null)
                                        {
                                            pvehicle.ToggleExtra(1, false);
                                            pvehicle.ToggleExtra(2, false);
                                            pvehicle.ToggleExtra(3, false);
                                            pvehicle.ToggleExtra(4, false);
                                            pvehicle.ToggleExtra(5, false);
                                            pvehicle.ToggleExtra(6, false);
                                            pvehicle.ToggleExtra(7, false);
                                            pvehicle.ToggleExtra(8, false);
                                            pvehicle.ToggleExtra(9, false);
                                            pvehicle.ToggleExtra(10, false);
                                            pvehicle.ToggleExtra(11, false);
                                            pvehicle.ToggleExtra(12, false);
                                            foreach (int extra in extras)
                                            {
                                                pvehicle.ToggleExtra(extra, true);
                                                Debug.Write("\nExtra " + extra + " activated.");
                                            }
                                        }
                                        else
                                        {
                                            Debug.Write("\nNo extras set!");
                                        }

                                        JToken livery = playerVehicleConfig["livery"];
                                        if (livery != null)
                                        {
                                            SetVehicleLivery(pvehicle.Handle, ((int)livery));
                                        }
                                        else
                                        {
                                            Debug.Write("\nNo livery set!");
                                        }

                                        PlayerData playerData = Utilities.GetPlayerData();
                                        string callsign = playerData.Callsign;
                                        int networkId = (ReqCar).NetworkId;
                                        await BaseScript.Delay(100);
                                        SetVehicleNumberPlateText((Entity.FromNetworkId(networkId)).Handle, callsign);

                                        Blip ReqCarBlip = ReqCar.AttachBlip();
                                        ReqCarBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        ReqCarBlip.Name = ((string)ReqVehSettings["blipName"]);



                                        DeliveryOfficer.Weapons.Give(WeaponHash.SNSPistolMk2, 30, false, false);

                                        DeliveryOfficer.Task.DriveTo(ReqCar, Game.PlayerPed.Position, stopDistance, driveSpeed, ((int)DrivingStyle.SometimesOvertakeTraffic));
                                        Tick += DeliverCar; 



                                    }

                                    else
                                    {
                                        CitizenFX.Core.UI.Screen.ShowNotification("Vehicle could not be loaded in time. Try again!");
                                    }



                                }
                            }
                        };
                    }
                    else if (ReqVehSettings["mode"].ToString() == "submenu")
                    {
                        //SUBMENU  MODE:
                        var VehiclesJson = API.LoadResourceFile(API.GetCurrentResourceName(), "/config/vehicles.json");
                        var Vehicles = JObject.Parse(VehiclesJson);
                        JToken PoliceVehicles = Vehicles["police"];
                        //*Potential JTOKEN problem here ^

                        List<JToken> VehicleList = new List<JToken>();
                        foreach (JToken jtoken in PoliceVehicles)
                        {
                            PlayerData PData = Utilities.GetPlayerData();
                            int Department = PData.DepartmentID;
                            if (jtoken["availableForDepartments"] != null)
                            {
                                List<int> allowedDepts = new List<int>();
                                foreach (int dept in jtoken["availableForDepartments"])
                                {
                                    allowedDepts.Add(dept);
                                }

                                if (allowedDepts.Contains(Department))
                                {
                                    VehicleList.Add(jtoken);
                                }

                            }
                            else if (jtoken["availableForDepartments"] == null)
                            {
                                VehicleList.Add(jtoken);
                            }
                        }
                        List<string> VehicleNameList = new List<string>();
                        foreach (JToken jtoken in VehicleList)
                        {
                            VehicleNameList.Add(jtoken["name"].ToString());
                        }
                        MenuItem ReqVehSubOpen = new MenuItem(ReqVehSettings["title"].ToString(), ReqVehSettings["description"].ToString());
                        ReqVehSubOpen.LeftIcon = MenuItem.Icon.CAR;
                        menu.AddMenuItem(ReqVehSubOpen);

                        Menu ReqVehSubMenu = new Menu(((string)ReqVehSettings["title"]));
                        MenuController.BindMenuItem(menu, ReqVehSubMenu, ReqVehSubOpen);

                        foreach (string vehName in VehicleNameList)
                        {
                            MenuItem vehicleSpawn = new MenuItem(vehName);
                            ReqVehSubMenu.AddMenuItem(vehicleSpawn);

                        }
                        ReqVehSubMenu.OnItemSelect += async (_menu, _item, _index) =>
                        {
                            if (_menu == ReqVehSubMenu)
                            {

                                JToken vehicle = VehicleList.ElementAt(_index);

                                string ReqVehicleModel = ((string)vehicle["vehicle"]);
                                Debug.Write("\n^2[^4Backup Menu^2] JSON model: " + ReqVehicleModel + "\n");
                                float SpawnDistance = ((float)ReqVehSettings["spawnDistance"]);
                                float stopDistance = (float)ReqVehSettings["stopDistance"];
                                float driveSpeed = (float)ReqVehSettings["driveSpeed"];

                                Vector3 SpawnLoc = World.GetNextPositionOnStreet(Vector3Extension.Around(Game.PlayerPed.Position, SpawnDistance));
                                Debug.Write("\n^2[^4Backup Menu^2] Spawn Loc: " + SpawnLoc + "\n");

                                uint vehicleHash = (uint)GetHashKey(ReqVehicleModel);
                                RequestModel(vehicleHash);
                                Debug.Write("\n^2[^4Backup Menu^2] Requested vehicle model\n");
                                int maxretries = 0;
                                while (!HasModelLoaded(vehicleHash) && maxretries < 300)
                                {
                                    await BaseScript.Delay(100);
                                    maxretries++;
                                }

                                if (HasModelLoaded(vehicleHash))
                                {
                                    Debug.Write("\n^2[^4Backup Menu^2] Model loaded\n");
                                    Vehicle ReqCar = new Vehicle(CreateVehicle(vehicleHash, SpawnLoc.X, SpawnLoc.Y, SpawnLoc.Z, 0, true, false));
                                    SetVehicleOnGroundProperly(ReqCar.Handle);
                                    SetVehicleHasBeenOwnedByPlayer(ReqCar.Handle, true);
                                    SetEntityAsMissionEntity(ReqCar.Handle, true, true);
                                    Debug.Write("\n^2[^4Backup Menu^2] Vehicle Spawned\n");
                                    Debug.Write("\n^2[^4Backup Menu^2] Spawning ped\n");
                                    DeliveryOfficer = await Utilities.SpawnPed(PedHash.UndercoverCopCutscene, SpawnLoc, 0);
                                    Debug.Write("\n^2[^4Backup Menu^2] Setting seat ped\n");
                                    SetPedIntoVehicle(DeliveryOfficer.Handle, ReqCar.Handle, ((int)VehicleSeat.Driver));


                                    Debug.Write("\n^2[^4Backup Menu^2] doing extra stuff\n");
                                    await BaseScript.Delay(100);
                                    CitizenFX.Core.Vehicle pvehicle = ReqCar;
                                    string vehiclename = ReqCar.DisplayName.ToLower();
                                    var json = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/vehicles.json"));
                                    JToken policeVehicles = json["police"];
                                    JToken playerVehicleConfig = policeVehicles.FirstOrDefault(_vehicle => _vehicle["vehicle"].ToString().ToLower() == vehiclename);
                                    JToken extras = playerVehicleConfig["extras"];

                                    if (extras != null)
                                    {
                                        pvehicle.ToggleExtra(1, false);
                                        pvehicle.ToggleExtra(2, false);
                                        pvehicle.ToggleExtra(3, false);
                                        pvehicle.ToggleExtra(4, false);
                                        pvehicle.ToggleExtra(5, false);
                                        pvehicle.ToggleExtra(6, false);
                                        pvehicle.ToggleExtra(7, false);
                                        pvehicle.ToggleExtra(8, false);
                                        pvehicle.ToggleExtra(9, false);
                                        pvehicle.ToggleExtra(10, false);
                                        pvehicle.ToggleExtra(11, false);
                                        pvehicle.ToggleExtra(12, false);
                                        foreach (int extra in extras)
                                        {
                                            pvehicle.ToggleExtra(extra, true);
                                            Debug.Write("\nExtra " + extra + " activated.");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Write("\nNo extras set!");
                                    }

                                    JToken livery = playerVehicleConfig["livery"];
                                    if (livery != null)
                                    {
                                        SetVehicleLivery(pvehicle.Handle, ((int)livery));
                                    }
                                    else
                                    {
                                        Debug.Write("\nNo livery set!");
                                    }

                                    PlayerData playerData = Utilities.GetPlayerData();
                                    string callsign = playerData.Callsign;
                                    int networkId = (ReqCar).NetworkId;
                                    await BaseScript.Delay(100);
                                    SetVehicleNumberPlateText((Entity.FromNetworkId(networkId)).Handle, callsign);

                                    Blip ReqCarBlip = ReqCar.AttachBlip();
                                    ReqCarBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    ReqCarBlip.Name = ((string)ReqVehSettings["blipName"]);



                                    DeliveryOfficer.Weapons.Give(WeaponHash.SNSPistolMk2, 30, false, false);

                                    DeliveryOfficer.Task.DriveTo(ReqCar, Game.PlayerPed.Position, stopDistance, driveSpeed, ((int)DrivingStyle.SometimesOvertakeTraffic));
                                    Tick += DeliverCar;

                                    DeliveryOfficer.RelationshipGroup = Backups;
                                    Game.PlayerPed.RelationshipGroup = Backups;


                                }

                                else
                                {
                                    CitizenFX.Core.UI.Screen.ShowNotification("Vehicle could not be loaded in time. Try again!");
                                }




                            }
                        };

                        }
                    else
                    {
                        Debug.Write("\n^2[^4Backup Menu^2] ~r~ERROR: \"mode\" for \"RequestVehicle\" must be either \"list\" or \"submenu\"\n");
                        MenuItem ReqVehError = new MenuItem("~r~Invalid Config!", "~r~\"mode\" must be set to \"list\" or \"submenu\"");

                        menu.AddMenuItem(ReqVehError);
                    }
                }


                //MENU SETTINGS
                menu.MenuTitle = menuConfig["menuTitle"].ToString();
                menu.MenuSubtitle = "by GGGDunlix";



                MenuController.AddMenu(menu);
                MenuController.AddSubmenu(menu, playerBackup);


                menu.AddMenuItem(playerBackupOpen);
                menu.AddMenuItem(about);




            }

            public async Task StopMenu()
            {
                menu.CloseMenu();
            }

            public async Task ValidateJson()
            {
                var ConfigFile = API.LoadResourceFile(API.GetCurrentResourceName(), "/config/backup_menu.json");
                JObject Config = JObject.Parse(ConfigFile);

            }












            //RESPONSES:
            public async Task CancelBackup()
            {
                await Delay(1500);
                CopPed.MarkAsNoLongerNeeded();
                CopCar.MarkAsNoLongerNeeded();
                CopCar.IsSirenActive = false;
                CopPedBlip.Delete();
                CopPedBlip.SyncDelete();

                CopPed.AttachedBlip.Delete();
                Tick -= ContinueCode1AI;
                isBackupUp = false;
                CopPed.Task.WanderAround();


            }

            public async Task ContinueCode1AI()
            {
                isBackupUp = true;
                var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                var configJson = JObject.Parse(configFile);
                JToken menuConfig = configJson["BackupMenuPlugin"];
                JToken AIBackupConfig = menuConfig["AIBackup"];
                JToken Code1 = AIBackupConfig["items"]["code1"];

                float susDistance = ((float)Code1["suspectDistance"]);
                float speed = ((float)Code1["speed"]);
                if (CopPed.Exists())
                {
                    if (CopPed.IsAlive || !CopPed.IsDead)
                    {
                        if (CopPed.IsInRangeOf(Game.PlayerPed.Position, susDistance + 10))
                        {
                            CopPed.AlwaysKeepTask = false;
                            CopPed.BlockPermanentEvents = false;
                            if (CopPed.IsInVehicle())
                            {
                                CopPed.Task.LeaveVehicle();
                            }
                            else if (!CopPed.IsInVehicle())
                            {
                                CopPed.Task.FollowToOffsetFromEntity(Game.PlayerPed, new Vector3(0, 0, 0), -1, susDistance);
                            }

                        }
                        else
                        {
                            CopPed.AlwaysKeepTask = true;
                            CopPed.BlockPermanentEvents = true;
                            if (CopPed.IsInVehicle())
                            {
                                CopPed.Task.DriveTo(CopCar, Game.PlayerPed.Position, susDistance, speed);
                            }
                            else
                            {
                                CopPed.Task.EnterVehicle(CopCar, VehicleSeat.Driver, -1, 1);
                            }
                        }
                    }
                    else if (!CopPed.IsAlive || !CopCar.IsAlive || CopPed.IsDead || CopCar.IsDead)
                    {
                        Tick -= ContinueCode1AI;
                        await CancelBackup();
                    }
                }
                
            }
            public async Task RequestAICode1(Ped friend, Ped suspect, string vehicle, PedHash copPed)
            {
                
                var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                var configJson = JObject.Parse(configFile);
                JToken menuConfig = configJson["BackupMenuPlugin"];
                JToken AIBackupConfig = menuConfig["AIBackup"];
                JToken Code1 = AIBackupConfig["items"]["code1"];
                Vector3 location = friend.Position;
                Vector3 offsetLoc = World.GetNextPositionOnStreet(Vector3Extension.Around(location, ((float)Code1["spawnDistance"])));
                uint vehicleHash = (uint)GetHashKey(vehicle);
                CopPed = await Utilities.SpawnPed(copPed, offsetLoc, 0);


                RequestModel(vehicleHash);
                Debug.Write("\n^2[^4Backup Menu^2] Requested vehicle model\n");
                int maxretries = 0;
                while (!HasModelLoaded(vehicleHash) && maxretries < 300)
                {
                    await BaseScript.Delay(100);
                    maxretries++;
                }

                if (HasModelLoaded(vehicleHash))
                {
                    Debug.Write("\n^2[^4Backup Menu^2] Model loaded\n");
                    CopCar = new Vehicle(CreateVehicle(vehicleHash, offsetLoc.X, offsetLoc.Y, offsetLoc.Z, 0, true, false));
                    SetVehicleOnGroundProperly(CopCar.Handle);
                    SetVehicleHasBeenOwnedByPlayer(CopCar.Handle, true);
                    SetEntityAsMissionEntity(CopCar.Handle, true, true);
                    SetPedIntoVehicle(CopPed.Handle, CopCar.Handle, ((int)VehicleSeat.Driver));


                   








                } 
                else
                {
                    CitizenFX.Core.UI.Screen.ShowNotification("Vehicle could not be loaded in time. Try again!");
                }

                Debug.Write("\n^2[^4Backup Menu^2] Vehicle Stuff finished, blip time!\n");
                CopPedBlip = CopPed.AttachBlip();
                CopPedBlip.Sprite = BlipSprite.PoliceOfficer;
                CopPedBlip.Scale = 0.5f;
               


                CopPed.SetIntoVehicle(CopCar, VehicleSeat.Driver);

                float susDistance = ((float)Code1["suspectDistance"]);
                float speed = ((float)Code1["speed"]);

                CopPed.Weapons.Give(WeaponHash.Pistol, 9999, false, true);
                Debug.Write("\n^2[^4Backup Menu^2] Starting Tasks\n");
                CopPed.Task.DriveTo(CopCar, location, susDistance, speed);

                CopPed.RelationshipGroup = Backups;
                Game.PlayerPed.RelationshipGroup = Backups;
                Debug.Write("\n^2[^4Backup Menu^2] Starting Ticks\n");
                Tick += ContinueCode1AI;







            }

            //Vehicle Spawner
            public async Task DeliverCar()
            {
                var configFile = API.LoadResourceFile(API.GetCurrentResourceName(), "config/backup_menu.json");
                var configJson = JObject.Parse(configFile);
                JToken menuConfig = configJson["BackupMenuPlugin"];
                var ReqVehSettings = menuConfig["requestVehicle"];
                float SpawnDistance = ((float)ReqVehSettings["spawnDistance"]);
                float stopDistance = (float)ReqVehSettings["stopDistance"];
                float driveSpeed = (float)ReqVehSettings["driveSpeed"];


                Vector3 SpawnLoc = World.GetNextPositionOnStreet(Vector3Extension.Around(Game.PlayerPed.Position, SpawnDistance));
                if (DeliveryOfficer.IsInRangeOf(Game.PlayerPed.Position, stopDistance + 3))
                {
                    if (DeliveryOfficer.IsInVehicle())
                    {
                        DeliveryOfficer.Task.LeaveVehicle();
                    }
                    else
                    {
                        DeliveryOfficer.Task.WanderAround();
                        DeliveryOfficer.MarkAsNoLongerNeeded();
                        Tick -= DeliverCar;
                    }
                }
                else
                {
                    DeliveryOfficer.Task.DriveTo(ReqCar, Game.PlayerPed.Position, stopDistance, driveSpeed, ((int)DrivingStyle.SometimesOvertakeTraffic));
                }


            }
            public static async void SpawnVehicle(string vehicle, Vector3 location)
            {
                var hash = (uint)API.GetHashKey(vehicle);

                API.RequestModel(hash);
                while (!API.HasModelLoaded(hash))
                {
                    API.RequestModel(hash);
                    await Delay(0);
                }

                Vehicle spawn = new Vehicle(API.CreateVehicle(hash, location.X, location.Y, location.Z + 1f, 0, true, true))
                {
                    NeedsToBeHotwired = false,
                    PreviouslyOwnedByPlayer = true,
                    IsPersistent = true,
                    IsStolen = false,
                    IsWanted = false
                };

                spawn.IsEngineRunning = true;
                spawn.PlaceOnGround();
            }
        }
    }
}
