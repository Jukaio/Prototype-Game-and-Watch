using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberRenderer : MonoBehaviour
{
    private int previous_draw = -1;
    [SerializeField] int number = 0;
    [SerializeField] SpriteRenderer[] renderers;
    bool[][] numbers = new bool[][]
    {
        new bool[] // 0
        {
                 true,
            true,     true,
                 false,
            true,     true,
                 true
        },
        new bool[] // 1
        {
                 false,
            false,     true,
                 false,
            false,     true,
                 false
        },
        new bool[] // 2
        {
                 true,
            false,     true,
                 true,
            true,     false,
                 true
        },
        new bool[] // 3
        {
                 true,
            false,     true,
                 true,
            false,     true,
                 true
        },
        new bool[] // 4
        {
                 false,
            true,     true,
                 true,
            false,     true,
                 false
        },
        new bool[] // 5
        {
                 true,
            true,     false,
                 true,
            false,     true,
                 true
        },
        new bool[] // 6
        {
                 true,
            true,     false,
                 true,
            true,     true,
                 true
        },
        new bool[] // 7
        {
                 true,
            false,     true,
                 false,
            false,     true,
                 false
        },
        new bool[] // 8
        {
                 true,
            true,     true,
                 true,
            true,     true,
                 true
        },
        new bool[] // 9
        {
                 true,
            true,     true,
                 true,
            false,     true,
                 true
        },
        new bool[] // 10 = invisible
        {
                 false,
            false,     false,
                 false,
            false,     false,
                 false
        },
    };



    virtual public void set(int draw_number)
    {
        if (draw_number < numbers.Length && draw_number >= 0)
        {
            previous_draw = number;
            number = draw_number;
        }
    }

    public void refresh()
    {
        var temp = numbers[number];
        for(int i = 0; i < temp.Length; i++)
        {
            renderers[i].enabled = temp[i];
        }
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        if(number < numbers.Length && number >= 0)
        {
            refresh();
        }
    }
    public int get()
    {
        return number;
    }
}
