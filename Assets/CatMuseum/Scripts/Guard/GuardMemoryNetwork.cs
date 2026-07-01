using System.Collections.Generic;

public static class GuardMemoryNetwork
{
    private static readonly List<GuardController> guards = new List<GuardController>();

    public static void RegisterGuard(GuardController guard)
    {
        if (guard == null)
        {
            return;
        }

        if (!guards.Contains(guard))
        {
            guards.Add(guard);
        }
    }

    public static void UnregisterGuard(GuardController guard)
    {
        if (guard == null)
        {
            return;
        }

        guards.Remove(guard);
    }

    public static void BroadcastRecognition(GuardController sourceGuard, PlayerInteractor player)
    {
        if (player == null)
        {
            return;
        }

        foreach (GuardController guard in guards)
        {
            if (guard == null)
            {
                continue;
            }

            if (guard == sourceGuard)
            {
                continue;
            }

            guard.ReceiveRecognition(player);
        }
    }
}