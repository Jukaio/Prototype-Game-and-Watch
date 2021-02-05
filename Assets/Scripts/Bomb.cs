using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    enum State
    {
        Ticking,
        Exploding
    }


    [SerializeField] private SpriteRenderer left_wig;
    [SerializeField] private SpriteRenderer right_wig;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer[] explosions;

    [SerializeField] private AudioClip explosion_sound;
    [SerializeField] private AudioClip tick_sound;
    [SerializeField] private AudioSource audio_source;
    public float interval = 0.2f;
    public float timer;
    private State current;
    private bool is_paused;

    public void pause()
    {
        is_paused = true;
    }
    public void play()
    {
        is_paused = false;
    }

    public IEnumerator explode_routine(float blink_time, float blink_interval, float explosion_timer, float delay)
    {
        current = State.Exploding;
        timer = blink_time;
        float interval_timer = blink_interval;
        left_wig.enabled = false;
        right_wig.enabled = false;
        body.enabled = false;

        while (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            interval_timer -= Time.fixedDeltaTime;

            if (interval_timer < 0)
            {
                audio_source.Play();
                right_wig.enabled = !right_wig.enabled;
                body.enabled = !body.enabled;
                interval_timer = blink_interval;
            }
            yield return new WaitForFixedUpdate();
        }
        audio_source.clip = explosion_sound;
        timer = explosion_timer;
        left_wig.enabled = false;
        right_wig.enabled = false;
        body.enabled = false;
        int to = explosions.Length;
        int index = 0;
        float change_interval = explosion_timer / to;
        interval_timer = change_interval;
        audio_source.Play();
        for(int i = 0; i < to; i++)
        {
            explosions[i].enabled = i == index;
        }

        while(timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            interval_timer -= Time.fixedDeltaTime;

            if(interval_timer < 0)
            {
                index++;
                for (int i = 0; i < to; i++)
                {
                    explosions[i].enabled = i == index;
                    interval_timer = change_interval;
                }
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < to; i++)
        {
            explosions[i].enabled = false;
        }
        yield return new WaitForSecondsRealtime(delay);
        left_wig.enabled = true;
        right_wig.enabled = false;
        body.enabled = true;
        audio_source.clip = tick_sound;
        current = State.Ticking;
    }
    private void swap_wig()
    {
        var left = left_wig.enabled;
        var right = right_wig.enabled;
        
        left_wig.enabled = right;
        right_wig.enabled = left;
        audio_source.Play();
    }
    public void wiggle()
    {
        var left = left_wig.enabled;
        var right = right_wig.enabled;

        left_wig.enabled = right;
        right_wig.enabled = left;
    }
    public void set_interval(float interval)
    {
        
        this.interval = interval;
        if (timer > interval)
            timer = interval;
    }

    private void FixedUpdate()
    {
        if (current == State.Ticking && !is_paused)
        {
            timer -= Time.fixedDeltaTime;
            if (timer < 0.0f)
            {
                swap_wig();
                timer = interval;
            }
        }
    }

    private void Start()
    {
        audio_source = GetComponent<AudioSource>();
        audio_source.clip = tick_sound;
        timer = interval;
        left_wig.enabled = true;
        right_wig.enabled = false;
        body.enabled = true;
        for(int i = 0; i < explosions.Length; i++)
        {
            explosions[i].enabled = false;
        }
        current = State.Ticking;

        /*
            var go = new GameObject("Body");
            go.transform.parent = gameObject.transform;
            var rend = go.AddComponent<SpriteRenderer>();
            rend.sprite = body;
            renderer.Add(go.AddComponent<SpriteRenderer>());

            go = new GameObject("Wig");
            go.transform.parent = gameObject.transform;
            rend = go.AddComponent<SpriteRenderer>();
            rend.sprite = wig;
            renderer.Add(go.AddComponent<SpriteRenderer>());

            foreach(var ex in explosion)
            {
                go = new GameObject("Explosion");
                go.transform.parent = gameObject.transform;
                rend = go.AddComponent<SpriteRenderer>();
                rend.sprite = ex;
                renderer.Add(go.AddComponent<SpriteRenderer>());
            }
            */
    }
}
