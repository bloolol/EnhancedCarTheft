// Decompiled with JetBrains decompiler
// Type: Hotwiring
// Assembly: Enhanced Car Theft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B6C25BF0-B559-4433-A091-24E5A853B31D
// Assembly location: C:\Users\clope\OneDrive\Desktop\Enhanced Car Theft.dll

using GTA;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class Hotwiring : Script
{
    private Ped player;
    private Vehicle vehicle;
    private Vehicle currentVehicle;
    private CustomSprite hotwirePick;
    private CustomSprite hotwireBackground;
    private CustomSprite hotwireIgnition;
    private CustomSprite hotwireProgress;
    private bool hotwiring = false;
    private bool ignitionLocked = true;
    private float progressY = 63f;
    public int[] hotwireStages = new int[4];
    private int hotwireStageIndex = 0;
    private int lastCleanupTime = 0;
    private HashSet<Vehicle> hotwireVehicles = new HashSet<Vehicle>();

    public Hotwiring()
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
        string str1 = Hotwiring.Sprite("HotwirePick.png");
        string str2 = Hotwiring.Sprite("HotwireIgnition.png");
        string str3 = Hotwiring.Sprite("HotwireBackground.png");
        string str4 = Hotwiring.Sprite("HotwireProgressBar.png");
        if (File.Exists(str1) && this.hotwirePick == null)
            this.hotwirePick = new CustomSprite(str1, new SizeF(250f, 250f), new PointF(250f, 400f), Color.White);
        if (File.Exists(str2) && this.hotwireIgnition == null)
            this.hotwireIgnition = new CustomSprite(str2, new SizeF(250f, 250f), new PointF(250f, 400f), Color.White);
        if (File.Exists(str3) && this.hotwireBackground == null)
            this.hotwireBackground = new CustomSprite(str3, new SizeF(250f, 250f), new PointF(250f, 400f), Color.White);
        if (!File.Exists(str4) || this.hotwireProgress != null)
            return;
        this.hotwireProgress = new CustomSprite(str4, new SizeF(250f, 250f), new PointF(250f, 400f), Color.FromArgb(128, 0, 200, 0));
    }

    private void OnTick(object sender, EventArgs e)
    {
        this.player = Game.Player.Character;
        if (EnhancedCarTheft.hotwiringIsEnabled)
        {
            this.HotwireLogic();
            if (!EnhancedCarTheft.usingKeyboard && this.hotwiring)
            {
                if (Game.IsControlPressed(GTA.Control.ScriptRT) || Game.IsControlPressed(GTA.Control.ScriptLeftAxisY))
                    this.hotwirePick.Rotation += EnhancedCarTheft.hotwireMoveSpeed;
                if (Game.IsControlPressed(GTA.Control.ScriptLT) || Game.IsControlPressed(GTA.Control.MoveUpOnly))
                    this.hotwirePick.Rotation -= EnhancedCarTheft.hotwireMoveSpeed;
                if (Game.IsControlJustPressed(GTA.Control.Sprint))
                    this.HotwireAction();
            }
            if (this.hotwiring)
            {
                this.Hotwire();
                if (this.player.IsDead || !this.player.IsInVehicle())
                    this.AbortHotwiring();
                Vehicle currentVehicle = this.currentVehicle;
                if ((currentVehicle != null ? (currentVehicle.Exists() ? 1 : 0) : 0) != 0 && this.currentVehicle.IsEngineRunning)
                    this.AbortHotwiring();
                if (this.player.IsExitingVehicle)
                    this.AbortHotwiring();
            }
            if (EnhancedCarTheft.hintsEnabled && this.hotwiring)
            {
                if (!EnhancedCarTheft.usingKeyboard)
                    GTA.UI.Screen.ShowHelpTextThisFrame("Use ~INPUT_SCRIPT_LEFT_AXIS_Y~ or ~INPUT_SCRIPT_LT~ and ~INPUT_SCRIPT_RT~ to rotate pick.~n~Once the progress bar is highest, press ~INPUT_SPRINT~ to pick pin into place.~n~Once green, use ~INPUT_SCRIPT_LEFT_AXIS_Y~ to rotate and align ignition to START.");
                else
                    GTA.UI.Screen.ShowHelpTextThisFrame(string.Format("Use ~h~{0}~h~ and ~h~{1}~h~ to rotate pick.~n~Once the progress bar is highest, press ~h~{2}~h~ to pick pin into place.~n~Once green, use ~h~{3}~h~ to rotate and align ignition to START.", (object)EnhancedCarTheft.moveHotwirePickLeft, (object)EnhancedCarTheft.moveHotwirePickRight, (object)EnhancedCarTheft.pickPinKey, (object)EnhancedCarTheft.moveHotwirePickRight));
            }
        }
        if (Game.GameTime - this.lastCleanupTime < 5000)
            return;
        this.lastCleanupTime = Game.GameTime;
        this.hotwireVehicles.RemoveWhere((Predicate<Vehicle>)(veh => !veh.Exists() || veh.IsDead || veh.IsOnFire));
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!this.hotwiring || !Game.IsKeyPressed(EnhancedCarTheft.pickPinKey))
            return;
        this.HotwireAction();
    }

    private void HotwireLogic()
    {
        if (!EnhancedCarTheft.hotwiringIsEnabled)
            return;
        if ((Entity)this.player.VehicleTryingToEnter != (Entity)null)
            this.vehicle = this.player.VehicleTryingToEnter;
        if ((Entity)this.vehicle != (Entity)null && this.vehicle.Exists() && !this.vehicle.IsEngineRunning && !this.vehicle.IsBicycle && this.vehicle.NeedsToBeHotwired)
        {
            this.ActivateHotwiring();
            this.vehicle.NeedsToBeHotwired = false;
            this.vehicle.IsUndriveable = true;
            this.hotwireVehicles.Add(this.vehicle);
        }
        if (this.player.IsInVehicle())
        {
            if ((Entity)this.player.CurrentVehicle != (Entity)null)
                this.currentVehicle = this.player.CurrentVehicle;
            if ((Entity)this.currentVehicle != (Entity)null && this.currentVehicle.Exists())
            {
                if (this.hotwireVehicles.Contains(this.currentVehicle) && this.player.SeatIndex == VehicleSeat.Driver && !this.player.IsExitingVehicle)
                    this.hotwiring = true;
                if (this.currentVehicle.IsEngineRunning)
                    this.hotwireVehicles.Remove(this.currentVehicle);
            }
        }
    }

    private void ActivateHotwiring()
    {
        for (int index = 0; index < this.hotwireStages.Length - 1; ++index)
        {
            this.hotwireStages[index] = RandomHelper.random.Next(-91, 140);
            this.hotwireStageIndex = 0;
        }
    }

    private void Hotwire()
    {
        this.hotwireProgress.Draw(new SizeF(0.0f, this.progressY));
        this.hotwireBackground.Draw();
        this.hotwireIgnition.Draw();
        this.hotwirePick.Draw();
        if (Game.IsKeyPressed(EnhancedCarTheft.moveHotwirePickRight))
            this.hotwirePick.Rotation += EnhancedCarTheft.hotwireMoveSpeed;
        if (Game.IsKeyPressed(EnhancedCarTheft.moveHotwirePickLeft))
            this.hotwirePick.Rotation -= EnhancedCarTheft.hotwireMoveSpeed;
        if (this.hotwireStageIndex == 0)
            this.progressY = (float)(42.0 + (double)Math.Abs(this.hotwirePick.Rotation - (float)this.hotwireStages[this.hotwireStageIndex]) / 11.0);
        if (this.hotwireStageIndex == 1)
            this.progressY = (float)(21.0 + (double)Math.Abs(this.hotwirePick.Rotation - (float)this.hotwireStages[this.hotwireStageIndex]) / 11.0);
        if (this.hotwireStageIndex == 2)
            this.progressY = Math.Abs(this.hotwirePick.Rotation - (float)this.hotwireStages[this.hotwireStageIndex]) / 11f;
        if (this.hotwireStageIndex > 2)
        {
            this.progressY = 0.0f;
            this.ignitionLocked = false;
        }
        if (this.ignitionLocked)
        {
            if ((double)this.hotwirePick.Rotation <= -95.0)
                this.hotwirePick.Rotation = -95f;
            if ((double)this.hotwirePick.Rotation >= 140.0)
                this.hotwirePick.Rotation = 140f;
            this.hotwireProgress.Color = Color.FromArgb(128, (int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue);
        }
        else if (!this.ignitionLocked)
        {
            if ((double)this.hotwirePick.Rotation <= -95.0)
                this.hotwirePick.Rotation = -95f;
            if ((double)this.hotwirePick.Rotation >= 250.0)
            {
                this.currentVehicle.NeedsToBeHotwired = true;
                this.currentVehicle.IsUndriveable = false;
                this.hotwireVehicles.Remove(this.currentVehicle);
                this.AbortHotwiring();
                Script.Wait(4000);
            }
            this.hotwireProgress.Color = Color.FromArgb(128, 0, (int)byte.MaxValue, 0);
            this.hotwireProgress.Rotation = this.hotwireIgnition.Rotation;
        }
        if ((double)this.hotwirePick.Rotation <= (double)this.hotwireIgnition.Rotation - 90.0)
            this.hotwireIgnition.Rotation -= 2f;
        if ((double)this.hotwirePick.Rotation < (double)this.hotwireIgnition.Rotation + 135.0)
            return;
        this.hotwireIgnition.Rotation += 2f;
    }

    private void HotwireAction()
    {
        if (this.hotwireStageIndex >= 3)
            return;
        if ((double)this.hotwireStages[this.hotwireStageIndex] - (double)EnhancedCarTheft.hotwireDiff <= (double)this.hotwirePick.Rotation && (double)this.hotwirePick.Rotation <= (double)this.hotwireStages[this.hotwireStageIndex] + (double)EnhancedCarTheft.hotwireDiff)
        {
            ++this.hotwireStageIndex;
        }
        else
        {
            --this.hotwireStageIndex;
            if (this.hotwireStageIndex < 0)
                this.hotwireStageIndex = 0;
        }
    }

    private void AbortHotwiring()
    {
        this.hotwiring = false;
        this.progressY = 63f;
        this.hotwirePick.Rotation = 0.0f;
        this.hotwireIgnition.Rotation = 0.0f;
        this.hotwireProgress.Rotation = 0.0f;
        this.ignitionLocked = true;
    }
}
