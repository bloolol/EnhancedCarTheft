// Decompiled with JetBrains decompiler
// Type: EnhancedCarTheft
// Assembly: Enhanced Car Theft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B6C25BF0-B559-4433-A091-24E5A853B31D
// Assembly location: C:\Users\clope\OneDrive\Desktop\Enhanced Car Theft.dll

using GTA;
using GTA.Native;
using GTA.UI;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Windows.Forms;

public class EnhancedCarTheft : Script
{
    private ScriptSettings config;
    private Keys menuKey;
    public static bool usingKeyboard = true;
    private readonly ObjectPool pool = new ObjectPool();
    private NativeMenu mainMenu = new NativeMenu("Enhanced Car Theft", "v2.00");
    private NativeCheckboxItem enableLockpicking = new NativeCheckboxItem("Enable LockPicking", true);
    private NativeCheckboxItem enableHotwiring = new NativeCheckboxItem("Enable Hotwiring", true);
    private NativeMenu settingsMenu = new NativeMenu("Settings", "Settings", "Configure more settings to your liking");
    private NativeSubmenuItem settings;
    private NativeSeparatorItem lockPick = new NativeSeparatorItem("Lockpicking");
    private NativeSliderItem lockpickSpeed = new NativeSliderItem("Lockpicking speed", "Adjust how quickly pick moves up", 10, 3);
    private NativeSliderItem lockpickDifficulty = new NativeSliderItem("Lockpicking difficulty", "Adjust how much leeway for pin to be considered in the correct position", 10, 2);
    private NativeCheckboxItem reportLockpickingAttempt = new NativeCheckboxItem("NPCs can report Lockpicking attempts?", "Enable/Disable NPCs report attempted vehicle breakin", true);
    private NativeSeparatorItem hotWire = new NativeSeparatorItem("Hotwiring");
    private NativeSliderItem hotwireSpeed = new NativeSliderItem("Hotwire speed", "Adjust how quickly pick rotates", 10, 2);
    private NativeSliderItem hotwireDifficulty = new NativeSliderItem("Hotwire difficulty", "Adjust how much leeway for pin to be considered in the correct position", 10, 2);
    private NativeSeparatorItem misc = new NativeSeparatorItem("Misc Settings");
    private NativeCheckboxItem enableVehicleWantedMechanics = new NativeCheckboxItem("Enable vehicle wanted mechanics", "Enable/Disable vehicle wanted mechanics, i.e, if car is heavily damaged you can be wanted if seen by police", true);
    private NativeCheckboxItem enableVehicleMidAirControl = new NativeCheckboxItem("Disable vehicle mid air control?", "Enable/Disable vehicle mid air control", true);
    private NativeCheckboxItem enableVehicleBrakeLights = new NativeCheckboxItem("Enable vehicle brake lights", "Enable/Disable vehicle brake lights when stopped", true);
    private NativeCheckboxItem enableHints = new NativeCheckboxItem("Enable help text", "Enable/Disable help text to guide player with controls", true);
    private Ped player;
    private Vehicle currentVehicle;
    private int lastVehicleHandle;
    private bool vehicleKnownToPolice = false;
    private bool areCopsSearching;
    public static bool lockpickingIsEnabled = true;
    public static bool hotwiringIsEnabled = true;
    public static bool midAirControlLogic;
    public static bool brakeLightsEnabled;
    public static bool hintsEnabled = true;
    public static bool vehicleWantedMechanics = true;
    public static bool canBeCaughtLockpicking = true;
    public static Keys moveLockPickKey = Keys.S;
    public static Keys abortPicking = Keys.Space;
    public static Keys pickPinKey = Keys.Q;
    public static Keys moveHotwirePickRight = Keys.W;
    public static Keys moveHotwirePickLeft = Keys.S;
    public static float lockpickMoveSpeed = 3f;
    public static float hotwireMoveSpeed = 2f;
    public static float lockpickDiff = 2f;
    public static float hotwireDiff = 2f;

    private void LoadConfig()
    {
        this.config = ScriptSettings.Load("scripts\\EnhancedCarTheft.ini");
        this.menuKey = this.config.GetValue<Keys>("SETTINGS", "MenuKey", Keys.L);
    }

    public EnhancedCarTheft()
    {
        this.Tick += new EventHandler(this.OnTick);
        this.KeyDown += new KeyEventHandler(this.OnKeyDown);
        this.enableLockpicking.Activated += new EventHandler(this.OnSettingsChanged);
        this.enableHotwiring.Activated += new EventHandler(this.OnSettingsChanged);
        this.lockpickSpeed.Activated += new EventHandler(this.OnSettingsChanged);
        this.lockpickDifficulty.Activated += new EventHandler(this.OnSettingsChanged);
        this.reportLockpickingAttempt.Activated += new EventHandler(this.OnSettingsChanged);
        this.hotwireSpeed.Activated += new EventHandler(this.OnSettingsChanged);
        this.hotwireDifficulty.Activated += new EventHandler(this.OnSettingsChanged);
        this.enableVehicleWantedMechanics.Activated += new EventHandler(this.OnSettingsChanged);
        this.enableVehicleMidAirControl.Activated += new EventHandler(this.OnSettingsChanged);
        this.enableVehicleBrakeLights.Activated += new EventHandler(this.OnSettingsChanged);
        this.enableHints.Activated += new EventHandler(this.OnSettingsChanged);
        this.Initialise();
        this.LoadConfig();
    }

    private void Initialise()
    {
        this.pool.Add((IProcessable)this.mainMenu);
        this.pool.Add((IProcessable)this.settingsMenu);
        this.mainMenu.Add((NativeItem)this.enableLockpicking);
        this.mainMenu.Add((NativeItem)this.enableHotwiring);
        this.mainMenu.Add((NativeItem)this.misc);
        this.mainMenu.Add((NativeItem)this.reportLockpickingAttempt);
        this.mainMenu.Add((NativeItem)this.enableVehicleWantedMechanics);
        this.mainMenu.Add((NativeItem)this.enableVehicleMidAirControl);
        this.mainMenu.Add((NativeItem)this.enableVehicleBrakeLights);
        this.mainMenu.Add(this.settingsMenu);
        this.settings = new NativeSubmenuItem(this.settingsMenu, this.mainMenu);
        this.settingsMenu.Add((NativeItem)this.lockPick);
        this.settingsMenu.Add((NativeItem)this.lockpickSpeed);
        this.settingsMenu.Add((NativeItem)this.lockpickDifficulty);
        this.settingsMenu.Add((NativeItem)this.hotWire);
        this.settingsMenu.Add((NativeItem)this.hotwireSpeed);
        this.settingsMenu.Add((NativeItem)this.hotwireDifficulty);
        this.settingsMenu.Add((NativeItem)this.misc);
        this.settingsMenu.Add((NativeItem)this.enableHints);
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        EnhancedCarTheft.lockpickingIsEnabled = this.enableLockpicking.Checked;
        EnhancedCarTheft.hotwiringIsEnabled = this.enableHotwiring.Checked;
        EnhancedCarTheft.canBeCaughtLockpicking = this.reportLockpickingAttempt.Checked;
        EnhancedCarTheft.vehicleWantedMechanics = this.enableVehicleWantedMechanics.Checked;
        EnhancedCarTheft.midAirControlLogic = this.enableVehicleMidAirControl.Checked;
        EnhancedCarTheft.brakeLightsEnabled = this.enableVehicleBrakeLights.Checked;
        EnhancedCarTheft.lockpickMoveSpeed = (float)this.lockpickSpeed.Value;
        EnhancedCarTheft.lockpickDiff = (float)this.lockpickDifficulty.Value;
        EnhancedCarTheft.hotwireMoveSpeed = (float)this.hotwireSpeed.Value;
        EnhancedCarTheft.hotwireDiff = (float)this.hotwireDifficulty.Value * 8f;
        EnhancedCarTheft.hintsEnabled = this.enableHints.Checked;
        Notification.PostTicker("Settings Updated!", false);
    }

    private void OnTick(object sender, EventArgs e)
    {
        this.pool.Process();
        EnhancedCarTheft.usingKeyboard = Game.LastInputMethod != InputMethod.GamePad;
        this.player = Game.Player.Character;
        if (this.player.IsInVehicle())
        {
            this.currentVehicle = this.player.CurrentVehicle;
            if ((Entity)this.currentVehicle != (Entity)null && this.currentVehicle.Exists())
            {
                this.MidAirControl();
                this.BrakeLights();
            }
        }
        if (!EnhancedCarTheft.vehicleWantedMechanics)
            return;
        this.VehicleWantedLogic();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != this.menuKey)
            return;
        this.mainMenu.Visible = !this.mainMenu.Visible;
    }

    private void MidAirControl()
    {
        if (!EnhancedCarTheft.midAirControlLogic)
            return;
        if (!this.currentVehicle.IsOnAllWheels || this.currentVehicle.IsInAir)
        {
            if (VehicleHelper.VehicleIsValidType(this.currentVehicle))
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, (InputArgument)0, (InputArgument)59);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, (InputArgument)0, (InputArgument)60);
            }
        }
        else
        {
            Function.Call(Hash.ENABLE_CONTROL_ACTION, (InputArgument)0, (InputArgument)59);
            Function.Call(Hash.ENABLE_CONTROL_ACTION, (InputArgument)0, (InputArgument)60);
        }
    }

    private void BrakeLights()
    {
        if (!EnhancedCarTheft.brakeLightsEnabled || !this.currentVehicle.IsStopped)
            return;
        this.currentVehicle.AreBrakeLightsOn = true;
    }

    private void VehicleWantedLogic()
    {
        this.currentVehicle = this.player.CurrentVehicle;
        if (!this.player.IsInVehicle())
            return;
        Vehicle currentVehicle = this.currentVehicle;
        if (currentVehicle != null && currentVehicle.Exists())
        {
            bool flag = (double)this.currentVehicle.BodyHealth < 800.0;
            bool isAlarmSounding = this.currentVehicle.IsAlarmSounding;
            if (this.currentVehicle.Handle != this.lastVehicleHandle)
            {
                this.lastVehicleHandle = this.currentVehicle.Handle;
                this.vehicleKnownToPolice = false;
            }
            if (!this.areCopsSearching && Game.Player.Wanted.WantedLevel > 0)
                this.vehicleKnownToPolice = true;
            if (flag | isAlarmSounding)
                this.currentVehicle.IsWanted = true;
            else if (!isAlarmSounding && !this.vehicleKnownToPolice && this.areCopsSearching)
                this.currentVehicle.IsWanted = false;
        }
        if (!VehicleHelper.VehicleIsValidType(this.currentVehicle))
            this.currentVehicle.IsWanted = true;
    }
}
