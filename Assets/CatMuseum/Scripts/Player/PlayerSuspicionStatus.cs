using System.Collections.Generic;
using UnityEngine;

public class PlayerSuspicionStatus : MonoBehaviour
{
    private readonly HashSet<GuardController> chasingGuards = new HashSet<GuardController>();

    public bool IsBeingChased => chasingGuards.Count > 0;
    public int ChasingGuardCount => chasingGuards.Count;

    public void RegisterChaser(GuardController guard)
    {
        if (guard == null)
        {
            return;
        }

        chasingGuards.Add(guard);
    }

    public void UnregisterChaser(GuardController guard)
    {
        if (guard == null)
        {
            return;
        }

        chasingGuards.Remove(guard);
    }
}