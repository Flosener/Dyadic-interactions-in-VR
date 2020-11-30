using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandleAnimRandomizer : MonoBehaviour
{
    private int state;
    private List<string> stateNames;
    private Animator anim;

    private void Start()
    {
        state = Random.Range(0,3);
        stateNames = new List<string>() {"flying_candle_1", "flying_candle_2", "flying_candle_3"};
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.Play(stateNames[state]);
    }
}
