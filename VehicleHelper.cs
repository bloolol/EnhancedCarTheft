// Decompiled with JetBrains decompiler
// Type: VehicleHelper
// Assembly: Enhanced Car Theft, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B6C25BF0-B559-4433-A091-24E5A853B31D
// Assembly location: C:\Users\clope\OneDrive\Desktop\Enhanced Car Theft.dll

using GTA;

public static class VehicleHelper
{
    public static bool VehicleIsValidType(Vehicle veh)
    {
        return veh.Exists() && veh.Model.Hash != -2130482718 && !veh.IsBicycle && !veh.IsBike && !veh.IsQuadBike && !veh.IsSubmarine && !veh.IsTrain && !veh.IsPlane && veh.IsAutomobile;
    }
}
