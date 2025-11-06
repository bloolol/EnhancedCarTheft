// Decompiled with JetBrains decompiler
// Type: Lockpicking
// Assembly: Enhanced Car Theft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B6C25BF0-B559-4433-A091-24E5A853B31D
// Assembly location: C:\Users\clope\OneDrive\Desktop\Enhanced Car Theft.dll

using GTA;
using GTA.Native;
using GTA.UI;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class Lockpicking : Script
{
    private Ped player;
    private Vehicle vehicle;
    private CustomSprite pick;
    private CustomSprite[] pins = new CustomSprite[6];
    private CustomSprite background;
    private float yPosition = 0.0f;
    private bool lockpicking = false;
    private int[] pinY = new int[6];
    private int[] pinPositions = new int[6];
    private int currentPinIndex = 0;
    private int z = 0;
    private float tenSecondTick = 0.0f;

    public Lockpicking()
    {
        this.Tick += new EventHandler(this.OnTick);
        this.KeyDown += new KeyEventHandler(this.OnKeyDown);
        this.Initialise();
    }

    private void Initialise() => this.CreateSprites();

    private static string Sprite(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LockpickHotwire", fileName);
    }

    private void CreateSprites()
    {
        string str1 = Lockpicking.Sprite("LockPickPick.png");
        string str2 = Lockpicking.Sprite("LockPickPin.png");
        string str3 = Lockpicking.Sprite("LockPickBackground.png");
        if (File.Exists(str2))
        {
            for (int index = 0; index < this.pins.Length; ++index)
            {
                float x = (float)(325 - index * 15);
                this.pins[index] = new CustomSprite(str2, new SizeF(250f, 250f), new PointF(x, 520f), Color.White);
                this.pinPositions[index] = -1;
            }
        }
        if (File.Exists(str1) && this.pick == null)
            this.pick = new CustomSprite(str1, new SizeF(250f, 250f), new PointF(355f, 506f), Color.White);
        if (!File.Exists(str3) || this.background != null)
            return;
        this.background = new CustomSprite(str3, new SizeF(250f, 250f), new PointF(250f, 520f), Color.FromArgb(128, (int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue));
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!EnhancedCarTheft.lockpickingIsEnabled)
            return;
        this.player = Game.Player.Character;
        if (!EnhancedCarTheft.usingKeyboard)
        {
            if (this.player.IsTryingToEnterALockedVehicle)
            {
                if ((Entity)this.player.VehicleTryingToEnter != (Entity)null)
                    this.vehicle = this.player.VehicleTryingToEnter;
                if ((Entity)this.vehicle != (Entity)null)
                {
                    if (Game.IsControlJustPressed(GTA.Control.MeleeAttackLight) && this.vehicle.Exists() && this.vehicle.LockStatus == VehicleLockStatus.Locked)
                        this.vehicle.LockStatus = VehicleLockStatus.CanBeBrokenIntoPersist;
                    if (Game.IsControlJustPressed(GTA.Control.Jump) && this.vehicle.Exists() && this.vehicle.LockStatus == VehicleLockStatus.Locked)
                    {
                        this.lockpicking = true;
                        for (int index = 0; index < this.pinY.Length; ++index)
                        {
                            this.pinY[index] = RandomHelper.random.Next(-20, -5);
                            this.pinPositions[index] = -1;
                        }
                        this.player.Task.PlayAnimation("amb@prop_human_parking_meter@male@enter", "enter");
                        Script.Wait(1000);
                        this.player.Task.PlayAnimation("amb@prop_human_parking_meter@male@base", "base", 8f, -1f, -1, AnimationFlags.Loop, 1f);
                    }
                }
            }
            if (this.lockpicking)
            {
                if (Game.IsControlJustPressed(GTA.Control.Sprint))
                    this.LockPickAction();
                if (Game.IsControlPressed(GTA.Control.ScriptLeftAxisY))
                {
                    this.pick.Rotation += EnhancedCarTheft.lockpickMoveSpeed * 0.1f;
                    this.yPosition -= EnhancedCarTheft.lockpickMoveSpeed * 0.15f;
                }
            }
        }
        if (EnhancedCarTheft.hintsEnabled && (Entity)this.vehicle != (Entity)null)
        {
            if (!EnhancedCarTheft.usingKeyboard)
            {
                if (this.vehicle.LockStatus == VehicleLockStatus.Locked && !this.lockpicking && Game.IsControlJustPressed(GTA.Control.Enter))
                    GTA.UI.Screen.ShowHelpText("While attempting to enter, press ~INPUT_JUMP~ to lockpick or ~INPUT_MELEE_ATTACK_LIGHT~ to break in.", 3000);
                if (this.lockpicking)
                    GTA.UI.Screen.ShowHelpTextThisFrame("Use ~INPUT_SCRIPT_LEFT_AXIS_Y~ to move the pin into the highlighted position. Once highlighted, press ~INPUT_SPRINT~ to pick pin into place.");
            }
            else
            {
                if (this.vehicle.LockStatus == VehicleLockStatus.Locked && !this.lockpicking && Game.IsControlJustPressed(GTA.Control.Enter))
                    GTA.UI.Screen.ShowHelpText("While attempting to enter, press ~INPUT_ENTER~ to lockpick or ~INPUT_MELEE_ATTACK_LIGHT~ to break in.", 3000);
                if (this.lockpicking)
                    GTA.UI.Screen.ShowHelpTextThisFrame(string.Format("Use ~h~{0}~h~ to move the pin into the highlighted position. Once highlighted, press ~h~{1}~h~ to pick pin into place.", (object)EnhancedCarTheft.moveLockPickKey, (object)EnhancedCarTheft.pickPinKey));
            }
        }
        if (this.lockpicking)
        {
            this.LockPick();
            this.tenSecondTick += Game.LastFrameTime;
            if ((double)this.tenSecondTick >= 10.0)
            {
                this.tenSecondTick = 0.0f;
                Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, (InputArgument)(Enum)EventType.ShockingNonViolentWeaponAimedAt, (InputArgument)(Entity)this.player, (InputArgument)1f);
                this.CaughtLockPicking();
            }
            if (this.player.IsRagdoll || this.player.IsInAir)
                this.AbortLockPicking();
            if (!EnhancedCarTheft.usingKeyboard && Game.IsControlJustPressed(GTA.Control.MeleeAttackLight))
                this.AbortLockPicking();
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (EnhancedCarTheft.lockpickingIsEnabled && this.player.IsTryingToEnterALockedVehicle)
        {
            if ((Entity)this.player.VehicleTryingToEnter != (Entity)null)
                this.vehicle = this.player.VehicleTryingToEnter;
            if ((Entity)this.vehicle != (Entity)null && !AmbientVehicleLocking.IsDoorOpen(vehicle))
            {
                if (e.KeyCode == Keys.R && this.vehicle.Exists() && this.vehicle.LockStatus == VehicleLockStatus.Locked)
                    this.vehicle.LockStatus = VehicleLockStatus.CanBeBrokenIntoPersist;
                if (e.KeyCode == Keys.F && this.vehicle.Exists() && this.vehicle.LockStatus == VehicleLockStatus.Locked)
                {
                    this.lockpicking = true;
                    for (int index = 0; index < this.pinY.Length; ++index)
                    {
                        this.pinY[index] = RandomHelper.random.Next(-20, -5);
                        this.pinPositions[index] = -1;
                    }
                    this.player.Task.PlayAnimation("amb@prop_human_parking_meter@male@enter", "enter");
                    Script.Wait(1000);
                    this.player.Task.PlayAnimation("amb@prop_human_parking_meter@male@base", "base", 8f, -1f, -1, AnimationFlags.Loop, 1f);
                }
            }
        }
        if (this.lockpicking && Game.IsKeyPressed(EnhancedCarTheft.pickPinKey))
            this.LockPickAction();
        if (!this.lockpicking || !Game.IsKeyPressed(EnhancedCarTheft.abortPicking))
            return;
        this.AbortLockPicking();
    }

    private void AbortLockPicking()
    {
        this.lockpicking = false;
        this.currentPinIndex = 0;
        this.z = 0;
        for (int index = 0; index < this.pins.Length; ++index)
            this.pins[index].Color = Color.White;
        this.player.Task.PlayAnimation("amb@prop_human_parking_meter@male@exit", "exit");
    }

    private void LockPick()
    {
        this.background.Draw();
        this.pick.Draw(new SizeF((float)this.z, 0.0f));
        this.pick.Rotation -= 0.1f;
        this.yPosition += 0.15f;
        for (int index = 0; index < this.pins.Length; ++index)
        {
            if (index < this.currentPinIndex)
                this.pins[index].Draw(new SizeF(0.0f, (float)this.pinPositions[index]));
            else if (index == this.currentPinIndex)
                this.pins[index].Draw(new SizeF(0.0f, this.yPosition));
            else
                this.pins[index].Draw();
        }
        this.pins[this.currentPinIndex].Color = (double)this.pinY[this.currentPinIndex] - (double)EnhancedCarTheft.lockpickDiff > (double)this.yPosition || (double)this.yPosition > (double)this.pinY[this.currentPinIndex] + (double)EnhancedCarTheft.lockpickDiff ? Color.White : Color.FromArgb((int)byte.MaxValue, 0, 200, 0);
        if (Game.IsKeyPressed(EnhancedCarTheft.moveLockPickKey))
        {
            this.pick.Rotation += EnhancedCarTheft.lockpickMoveSpeed * 0.1f;
            this.yPosition -= EnhancedCarTheft.lockpickMoveSpeed * 0.15f;
        }
        if ((double)this.yPosition <= -30.0)
            this.yPosition = -30f;
        if ((double)this.yPosition >= 0.0)
            this.yPosition = 0.0f;
        if ((double)this.pick.Rotation >= 20.0)
            this.pick.Rotation = 20f;
        if ((double)this.pick.Rotation > 0.0)
            return;
        this.pick.Rotation = 0.0f;
    }

    private void CaughtLockPicking()
    {
        if (!EnhancedCarTheft.canBeCaughtLockpicking)
            return;
        foreach (Ped nearbyPed in World.GetNearbyPeds(this.player, 20f))
        {
            if ((Entity)nearbyPed != (Entity)null && nearbyPed.IsAlive && nearbyPed.IsHuman && !nearbyPed.IsPlayer && nearbyPed.HasClearLineOfSightToInFront((Entity)this.player) && RandomHelper.random.Next(0, 100) >= 80)
                Game.Player.Wanted.ReportCrime(CrimeType.StealVehicle);
        }
    }

    private void LockPickAction()
    {
        if (this.pins[this.currentPinIndex].Color == Color.FromArgb((int)byte.MaxValue, 0, 200, 0))
        {
            this.pinPositions[this.currentPinIndex] = (int)this.yPosition;
            ++this.currentPinIndex;
            this.z -= 15;
            this.pick.Rotation = 0.0f;
            this.yPosition = 0.0f;
            if (this.currentPinIndex >= this.pins.Length)
            {
                this.AbortLockPicking();
                this.vehicle.LockStatus = VehicleLockStatus.None;
            }
        }
        else
        {
            --this.currentPinIndex;
            this.z += 15;
            this.pick.Rotation = 0.0f;
            this.yPosition = 0.0f;
            if (this.currentPinIndex < 0)
                this.currentPinIndex = 0;
        }
        if (this.z <= -75)
            this.z = -75;
        if (this.z <= 0)
            return;
        this.AbortLockPicking();
        if ((Entity)this.vehicle != (Entity)null)
        {
            this.vehicle.IsAlarmSet = true;
            this.vehicle.StartAlarm();
            Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, (InputArgument)(Enum)EventType.ShockingPropertyDamage, (InputArgument)(Entity)this.player, (InputArgument)1f);
            this.CaughtLockPicking();
        }
    }
}
