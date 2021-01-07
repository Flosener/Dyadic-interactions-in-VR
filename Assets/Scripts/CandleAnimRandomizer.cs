using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandleAnimRandomizer : MonoBehaviour
{
    private int _state;
    private List<string> _stateNames;
    private Animator _anim;

    private void Start()
    {
        _state = Random.Range(0,3);
        _stateNames = new List<string>() {"flying_candle_1", "flying_candle_2", "flying_candle_3"};
        _anim = GetComponent<Animator>();
    }

    // Play random animation for the candles in the EntranceHall.
    void Update()
    {
        _anim.Play(_stateNames[_state]);
    }
}
