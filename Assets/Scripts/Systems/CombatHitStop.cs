using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

internal sealed class CombatHitStop
{
    private struct RestoreScaleBeforeGameplay { }
    private struct SetScaleAfterGameplay { }

    private double endTime;
    private bool needsFirstFrame;
    private bool installed;
    private bool ownsScale;
    private float previousScale;

    public void Prepare()
    {
        if (!installed)
            Install();
    }

    public void Request(float duration)
    {
        if (duration <= 0f || Time.timeScale <= 0f || PauseManager.IsMenuPaused)
            return;

        if (!installed && !Install())
            return;

        needsFirstFrame = true;
        endTime = Math.Max(endTime, Time.realtimeSinceStartupAsDouble + duration);
    }

    public void Cancel()
    {
        endTime = 0;
        needsFirstFrame = false;
        RestoreScale();
    }

    public void Uninstall()
    {
        Cancel();
        if (!installed)
            return;

        PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
        loop.subSystemList = Array.FindAll(loop.subSystemList, system =>
            system.type != typeof(RestoreScaleBeforeGameplay) && system.type != typeof(SetScaleAfterGameplay));
        PlayerLoop.SetPlayerLoop(loop);
        installed = false;
    }

    private bool Install()
    {
        PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopSystem[] groups = loop.subSystemList;
        if (groups == null)
            return false;

        int timeGroup = Array.FindIndex(groups, system => system.type == typeof(TimeUpdate));
        if (timeGroup < 0)
            return false;

        int restoreIndex = timeGroup + 1;
        var updated = new PlayerLoopSystem[groups.Length + 2];
        Array.Copy(groups, 0, updated, 0, restoreIndex);
        updated[restoreIndex] = new PlayerLoopSystem
        {
            type = typeof(RestoreScaleBeforeGameplay),
            updateDelegate = RestoreScale
        };
        Array.Copy(groups, restoreIndex, updated, restoreIndex + 1, groups.Length - restoreIndex);
        updated[updated.Length - 1] = new PlayerLoopSystem
        {
            type = typeof(SetScaleAfterGameplay),
            updateDelegate = SetScale
        };
        loop.subSystemList = updated;
        PlayerLoop.SetPlayerLoop(loop);
        installed = true;
        return true;
    }

    // Unity samples scaled deltas in TimeUpdate. Limit ownership of timeScale to that
    // sampling window so input, game code and pause owners always see the actual scale.
    private void SetScale()
    {
        if (Time.timeScale <= 0f || PauseManager.IsMenuPaused)
        {
            Cancel();
            return;
        }

        if (!needsFirstFrame && Time.realtimeSinceStartupAsDouble >= endTime)
            return;

        needsFirstFrame = false;
        previousScale = Time.timeScale;
        ownsScale = true;
        Time.timeScale = 0f;
    }

    private void RestoreScale()
    {
        if (!ownsScale)
            return;

        ownsScale = false;
        if (Time.timeScale == 0f && !PauseManager.IsMenuPaused)
            Time.timeScale = previousScale;
    }
}
