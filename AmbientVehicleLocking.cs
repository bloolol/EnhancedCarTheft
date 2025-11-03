// Decompiled with JetBrains decompiler
// Type: AmbientVehicleLocking
// Assembly: Enhanced Car Theft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B6C25BF0-B559-4433-A091-24E5A853B31D
// Assembly location: C:\Users\clope\OneDrive\Desktop\Enhanced Car Theft.dll

using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

public class AmbientVehicleLocking : Script
{
    private Ped player;
    private Vehicle currentVehicle;
    private HashSet<Vehicle> lockedVehicles = new HashSet<Vehicle>();
    private int lastCleanupTime = 0;
    private Vehicle lastStolenVehicle = (Vehicle)null;

    public AmbientVehicleLocking()
    {
        this.Tick += new EventHandler(this.OnTick);
        this.Interval = 100;
    }

    private void OnTick(object sender, EventArgs e)
    {
        this.player = Game.Player.Character;
        if ((Entity)this.player.CurrentVehicle != (Entity)null)
        {
            this.currentVehicle = this.player.CurrentVehicle;
            if (!this.currentVehicle.IsStolen && (Entity)this.currentVehicle != (Entity)this.lastStolenVehicle)
            {
                this.currentVehicle.IsStolen = true;
                this.currentVehicle.LockStatus = VehicleLockStatus.None;
                this.lastStolenVehicle = this.currentVehicle;
            }
        }
        if (EnhancedCarTheft.lockpickingIsEnabled)
            this.LockNearbyAmbientVehicles();
        if (Game.GameTime - this.lastCleanupTime < 5000)
            return;
        this.lastCleanupTime = Game.GameTime;
        this.CleanupLockedVehicles();
    }
    public static bool IsCopCar(Vehicle vehicle)
    {
        if (vehicle == null || !vehicle.Exists()) return false;

        return 
               vehicle.Model.ToString().ToLower().Contains("police") ||
               vehicle.Model.ToString().ToLower().Contains("sheriff") ||
               vehicle.Model.ToString().ToLower().Contains("fbi") ||
               vehicle.Model.ToString().ToLower().Contains("pranger") ||
               vehicle.Model.ToString().ToLower().Contains("riot");
    }

    public static bool IsDoorOpen(Vehicle veh)
    {
        Ped player = Game.Player.Character;
        if (veh != null &&
               veh.Exists() &&
               !veh.IsDead &&
               veh.IsDriveable &&

               !veh.Model.IsBike &&
               !veh.Model.IsBoat &&
               !veh.Model.IsQuadBike &&
               !veh.Model.IsTrain)
        {
            // Get the seat the player is trying to enter
            VehicleSeat targetSeat = player.SeatIndex;

            // Map seat to door index
            VehicleDoorIndex targetDoor = VehicleDoorIndex.FrontLeftDoor;
            switch (targetSeat)
            {
                case VehicleSeat.RightFront:
                    targetDoor = VehicleDoorIndex.FrontRightDoor;
                    break;
                case VehicleSeat.LeftRear:
                    targetDoor = VehicleDoorIndex.BackLeftDoor;
                    break;
                case VehicleSeat.RightRear:
                    targetDoor = VehicleDoorIndex.BackRightDoor;
                    break;
            }

            // Skip if the target door is open
            if (veh.Doors[targetDoor]?.IsOpen == true)
                return true;

        }
        return false;


    }
    private void LockNearbyAmbientVehicles()
    {
        foreach (Vehicle nearbyVehicle in World.GetNearbyVehicles(this.player.Position, 15f))
        {
            if (nearbyVehicle.Exists() && !((Entity)nearbyVehicle == (Entity)this.player.CurrentVehicle) && !IsDoorOpen(nearbyVehicle)  && !nearbyVehicle.IsPersistent && !nearbyVehicle.IsTrain && !nearbyVehicle.IsPlane && !this.lockedVehicles.Contains(nearbyVehicle) && !nearbyVehicle.PreviouslyOwnedByPlayer && !nearbyVehicle.IsStolen && (Entity)nearbyVehicle.Driver == (Entity)null)
            {
                nearbyVehicle.LockStatus = VehicleHelper.VehicleIsValidType(nearbyVehicle) ? (RandomHelper.random.Next(100) <= 97 ? VehicleLockStatus.CannotEnter : VehicleLockStatus.None) : VehicleLockStatus.CanBeBrokenInto;
                nearbyVehicle.NeedsToBeHotwired = true;
                this.lockedVehicles.Add(nearbyVehicle);
            }
        }
    }

    private void CleanupLockedVehicles()
    {
        this.lockedVehicles.RemoveWhere((Predicate<Vehicle>)(veh => !veh.Exists() || veh.IsDead || veh.IsOnFire));
        if (EnhancedCarTheft.lockpickingIsEnabled)
            return;
        foreach (Vehicle vehicle in this.lockedVehicles.ToList<Vehicle>())
        {
            vehicle.LockStatus = VehicleLockStatus.CanBeBrokenInto;
            this.lockedVehicles.Remove(vehicle);
        }
    }
}
