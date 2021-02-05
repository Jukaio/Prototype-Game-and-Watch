using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MultiDigitPrinter
{
    public interface ICallback
    {
        abstract public void on_timer_zero(Clock timer);
    }
    private List<ICallback> callbacks = new List<ICallback>();
    [SerializeField] float countdown = 0.0f;
    private bool paused = false;

    public void pause()
    {
        paused = true;
    }
    public void play()
    {
        paused = false;
    }

    public void restart_countdown(int number) 
    {
        countdown = number;
    }
    public float get_countdown()
    {
        return countdown;
    }
    public void register(ICallback callback)
    {
        callbacks.Add(callback);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (countdown > 0 && !paused)
        {
            set((int)countdown);
            countdown -= Time.fixedDeltaTime;
            if (countdown <= 0.0f)
            {
                foreach (var callback in callbacks)
                {
                    callback.on_timer_zero(this);
                }
            }
        }
    }
}