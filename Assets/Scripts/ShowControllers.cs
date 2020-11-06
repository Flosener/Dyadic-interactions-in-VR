using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ShowControllers : MonoBehaviour
{
    public bool showWand = false;
    
    void Update()
    {
        foreach (var hand in Player.instance.hands)
        {
            if (showWand)
            {
                // TO-DO: switch controllerPrefab and mesh etc. with wand prefab.
                hand.ShowController();
                // hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
            }
            else
            {
                hand.HideController();
                hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
            }
        }
    }
}
