using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller : MonoBehaviour
{
    public enum Button
    {
        Up,
        Down,
        Left,
        Right,
        Action,
        Count
    }
    public interface ICallback
    {
        abstract public void on_press(Button what);
    }

    private bool[] buttons = new bool[(int)Button.Count];
    private List<ICallback> callbacks = new List<ICallback>();
    
    public void register(ICallback callback)
    {
        callbacks.Add(callback);
    }
    public void Update()
    {
        buttons[(int)Button.Up] = Input.GetButtonDown("Up");
        buttons[(int)Button.Down] = Input.GetButtonDown("Down");
        buttons[(int)Button.Left] = Input.GetButtonDown("Left");
        buttons[(int)Button.Right] = Input.GetButtonDown("Right");
        buttons[(int)Button.Action] = Input.GetButtonDown("Action");

        for(int i = 0; i < buttons.Length; i++)
        {
            if(buttons[i])
            {
                foreach(var callback in callbacks)
                {
                    callback.on_press((Button)i);
                }
            }
        }
        
    }
}
