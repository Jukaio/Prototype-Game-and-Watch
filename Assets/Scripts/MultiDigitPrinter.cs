using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDigitPrinter : MonoBehaviour
{
    [SerializeField] GameObject number_prototype;
    [SerializeField] int digit_count;
    [SerializeField] float margins = 0.5f;
    NumberRenderer[] numbers;
    private int internal_number = 0;

    void Awake()
    {
        numbers = new NumberRenderer[digit_count];
        for (int i = 0; i < digit_count; i++)
        {
            var temp = Instantiate(number_prototype, transform);
            numbers[i] = temp.GetComponent<NumberRenderer>();
            numbers[i].transform.localPosition = (Vector3.right * margins * i);
        }
        set(internal_number);
    }
    public int get()
    {
        return internal_number;
    }
    public void set(int number)
    {
        this.internal_number = number;

        const float base_number = 10.0f;
        var next = number;
        for (int i = 0; i < digit_count; i++)
        {
            int power = digit_count - i - 1;
            var divider = (int)Mathf.Pow(base_number, power);
            var use = next / divider;
            numbers[i].set((int)use);
            next = next % divider;
        }
    }
}
