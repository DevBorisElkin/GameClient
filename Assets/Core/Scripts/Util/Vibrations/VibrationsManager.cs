using MoreMountains.NiceVibrations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public static class VibrationsManager
{
    public static void OnLocalPlayerMadeShot_Vibrations(List<Rune> activeRuneModifiers)
    {
        try
        {
            if (activeRuneModifiers.Count > 0)
            {
                if (activeRuneModifiers.Contains(Rune.RedViolet)) MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
                else MMVibrationManager.Haptic(HapticTypes.MediumImpact);
            }
            else MMVibrationManager.Haptic(HapticTypes.LightImpact);
        }
        catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerGotHitByShot_Vibrations()
    {
        try { MMVibrationManager.Haptic(HapticTypes.Warning); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerPickedUpRune_Vibrations()
    {
        try { MMVibrationManager.ContinuousHaptic(0.36f, 1f, 0.06f); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerJump_Vibrations()
    {
        try { MMVibrationManager.Haptic(HapticTypes.SoftImpact); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerDies_Vibrations()
    {
        try { MMVibrationManager.Haptic(HapticTypes.Failure); } catch (Exception e) { Debug.Log(e); }
    }
}
