using MoreMountains.NiceVibrations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public static class VibrationsManager
{
    static bool hapticsEnabled = true;
    public static void OnLocalPlayerMadeShot_Vibrations(List<Rune> activeRuneModifiers)
    {
        if (!hapticsEnabled) return;
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
        if (!hapticsEnabled) return;
        try { MMVibrationManager.Haptic(HapticTypes.Warning); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerPickedUpRune_Vibrations()
    {
        if (!hapticsEnabled) return;
        try { MMVibrationManager.ContinuousHaptic(0.36f, 1f, 0.06f); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerJump_Vibrations()
    {
        if (!hapticsEnabled) return;
        try { MMVibrationManager.Haptic(HapticTypes.SoftImpact); } catch (Exception e) { Debug.Log(e); }
    }

    public static void OnLocalPlayerDies_Vibrations()
    {
        if (!hapticsEnabled) return;
        try { MMVibrationManager.Haptic(HapticTypes.Failure); } catch (Exception e) { Debug.Log(e); }
    }

    static IEnumerator DebuffOnPlayer_Vibrations;
    
    public static void OnLocalPlayerReceivedDebuff_Vibrations(MonoBehaviour localPlayerMonoBehaviour)
    {
        if (!hapticsEnabled) return;
        if (DebuffOnPlayer_Vibrations != null) return;

        DebuffOnPlayer_Vibrations = DebuffOnPlayer();
        localPlayerMonoBehaviour.StartCoroutine(DebuffOnPlayer_Vibrations);
    }
    public static void OnLocalPlayerDebuffEnded_Vibrations(MonoBehaviour localPlayerMonoBehaviour)
    {
        if (!hapticsEnabled) return;
        if (DebuffOnPlayer_Vibrations == null) return;

        localPlayerMonoBehaviour.StopCoroutine(DebuffOnPlayer_Vibrations);
        DebuffOnPlayer_Vibrations = null;
    }

    static IEnumerator DebuffOnPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            if(hapticsEnabled)
                MMVibrationManager.ContinuousHaptic(0.08f, 1f, 0.01f);
        }
    }

    public static void OnMatchStarting_Vibrations()
    {
        if (!hapticsEnabled) return;
        try { MMVibrationManager.Haptic(HapticTypes.SoftImpact); } catch (Exception e) { Debug.Log(e); }
    }
    public static void OnMatchStarted_Vibrations()
    {
        if (!hapticsEnabled) return;
        try { MMVibrationManager.Haptic(HapticTypes.Success); } catch (Exception e) { Debug.Log(e); }
    }
}
