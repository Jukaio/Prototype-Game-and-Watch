using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, Controller.ICallback
{
    [SerializeField]private Screen game;
    [SerializeField] private AudioClip blip;
    private AudioSource audio_source; 

    private Vector2 previous_position = new Vector2();
    private bool block_input = false;
    private Vector2 spawn;
    private bool authority_block = false;

    public void set_authority_block(bool to)
    {
        authority_block = to;
    }

    public void set_spawn(Vector2 at)
    {
        spawn = at;
    }

    public void on_press(Controller.Button what)
    {
        if (block_input || authority_block)
            return;
        previous_position = transform.position;

        float step = game.get_cell_size();
        Vector3 next_position = previous_position;
        switch (what)
        {
            case Controller.Button.Up:
                next_position += (Vector3.up * step);    
                break;
            case Controller.Button.Down:
                next_position += (Vector3.down * step);
                break;
            case Controller.Button.Left:
                next_position += (Vector3.left * step);
                break;
            case Controller.Button.Right:
                next_position += (Vector3.right * step);
                break;
        }
        next_position = game.clamp(next_position);
        transform.position = next_position;
        block_input = true;

        var outcome = game.refresh_player_position_and_check_death(transform.position, previous_position);
        switch (outcome)
        {
            case Screen.Outcome.None:
                if(previous_position != (Vector2)transform.position)
                {
                    audio_source.Play();
                }
                return;
            case Screen.Outcome.Lose:
                game.game_over();
                break;
            case Screen.Outcome.Win:
                game.win_level();
                break;
        }

    }

    void Start()
    {
        GetComponent<Controller>().register(this);
        audio_source = GetComponent<AudioSource>();
        audio_source.clip = blip;
        //game.refresh_player_position_and_check_death(transform.position, transform.position);
    }

    void FixedUpdate()
    {
        block_input = false;
    }
}
