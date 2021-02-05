using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen : MonoBehaviour, Clock.ICallback, Controller.ICallback
{
    public enum Outcome
    {
        None,
        Lose,
        Win
    }
    public enum VerticalAllignment
    {
        Center,
        Left,
        Right
    }
    public enum HorizontalAllignment
    {
        Center,
        Top,
        Bottom
    }
    public enum State
    {
        Intro,
        Play
    }
    [System.Serializable]
    public struct Padding
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }

    
    [System.Serializable]
    public class Grid
    {
        public class Tile
        {
            public Tile(GameObject prototype, Vector3 at)
            {
                if (prototype != null)
                {
                    context = Instantiate(prototype, at, Quaternion.identity);
                    content = context.GetComponent<CellContent>();
                }
            }
            public void set_number(int number)
            {
                content.set_number(number);
            }
            public void set_player(bool to)
            {
                content.set_player(to);
            }
            public void add(Tile neighbour)
            {
                content.add(neighbour.content);
            }
            public void count_bombs()
            {
                content.count_bombs();
            }
            public int get_bomb_count()
            {
                return content.get_bomb_count();
            }
            public void set_bomb(bool to)
            {
                content.set_bomb(to);
            }
            public bool has_bomb()
            {
                return content.contains_bomb();
            }
            public void reposition(Vector2 at)
            {
                context.transform.position = new Vector3(at.x, at.y, 0);
            }
            public void set_number_active(bool to)
            {
                content.set_number_visibility(to);
            }
            public void set_neighbour_numbers_active(bool to)
            {
                content.set_neighbours_active(to);
            }
            private GameObject context = null;
            private CellContent content = null;
        }



        public Grid(Vector2 position, Vector2 full_size, GameObject[] prototype, float cell_size, int free_space_amount)
        {
            assign_data(position, full_size, cell_size, free_space_amount);
            reposition(Vector2.zero);
            create_internal_tiles(prototype);
            add_neighbours_to_tiles();
            place_bombs();
            count_bombs();

        }
        private static Vector2Int calculate_counts(Vector2 full_size, float cell_size)
        {
            Vector2 temp = new Vector2(full_size.x, full_size.y);
            Vector2Int count = new Vector2Int((int)(temp.x / cell_size), (int)(temp.y / cell_size));
            if (count.x % 2 == 0) count.x--;
            if (count.y % 2 == 0) count.y--;
            return count;
        }
        private void reposition(Vector2 at)
        {
            var half_count = (new Vector2(count.x * cell_size, count.y * cell_size) * 0.5f);
            this.origin_position = at - half_count;
            this.max_position = at + half_count;
            this.center = at;

            bounds = new Rect(origin_position + half_cell, max_position - half_cell);
        }

        private void refresh_tile_positions()
        {
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    Vector2 at = grid_to_world(new Vector2Int(x, y), true);
                    data[x][y].reposition(at);
                }
            }
        }

        private void assign_data(Vector2 position, Vector2 full_size, float cell_size, int free_space_amount)
        {
            this.cell_size = cell_size;
            this.free_space_amount = free_space_amount;
            this.half_cell = (Vector2.one * (cell_size / 2.0f));
            this.count = calculate_counts(full_size, cell_size);
            this.start = Vector2Int.zero;
            this.goal = get_counts() - Vector2Int.one;
            reposition(position);
        }
        private void create_internal_tiles(GameObject[] prototype)
        {
            data = new Tile[count.x][];
            int index = 0;
            for (int x = 0; x < data.Length; x++)
            {
                data[x] = new Tile[count.y];
                for (int y = 0; y < data[x].Length; y++)
                {
                    Vector2 at = grid_to_world(new Vector2Int(x, y), true);
                    data[x][y] = new Tile(prototype[(++index % prototype.Length)], at);
                }
            }
        }
        private void add_neighbours_to_tiles()
        {
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    var use = data[x][y];
                    if (x > 0)  /* filler */    { use.add(data[x - 1][y]); };
                    if (y > 0)  /* filler */    { use.add(data[x][y - 1]); };
                    if (x < data.Length - 1)    { use.add(data[x + 1][y]); };
                    if (y < data[x].Length - 1) { use.add(data[x][y + 1]); };

                    //if (x > 0 && y > 0)  /* filler */                  { use.add(data[x - 1][y - 1]); };
                    //if (x < data.Length - 1 && y > 0)  /* filler */    { use.add(data[x + 1][y - 1]); };
                    //if (x < data.Length - 1 && y < data[x].Length - 1) { use.add(data[x + 1][y + 1]); };
                    //if (x > 0 && y < data[x].Length - 1)               { use.add(data[x - 1][y + 1]); };
                }
            }
        }
        private void set_bomb(int x, int y, bool to)
        {
            data[x][y].set_bomb(to);
        }
        public void place_bombs()
        {
            var map = MazeMaker.create(get_counts().x, get_counts().y, free_space_amount);
            map[start.x, start.y] = false;
            map[goal.x, goal.y] = false;

            // Just bruteforce 100% solveable levels! 
            while (!AStar.solvable(map, start, goal))
            {
                map = MazeMaker.create(get_counts().x, get_counts().y, free_space_amount);
                map[start.x, start.y] = false;
                map[goal.x, goal.y] = false;
            }
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    set_bomb(x, y, map[x, y]);
                }
            }
        }

        public void count_bombs()
        {
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    var use = data[x][y];
                    use.count_bombs();
                }
            }
        }
        public void draw_rect(Vector2 pos, Vector2 size, Color color)
        {
            Vector2 half = size / 2.0f;
            Vector2 top_left = new Vector2(-half.x, half.y) + pos;
            Vector2 top_right = new Vector2(half.x, half.y) + pos;
            Vector2 bottom_left = new Vector2(-half.x, -half.y) + pos;
            Vector2 bottom_right = new Vector2(half.x, -half.y) + pos;

            Debug.DrawLine(top_left, top_right, color);
            Debug.DrawLine(top_right, bottom_right, color);
            Debug.DrawLine(bottom_right, bottom_left, color);
            Debug.DrawLine(bottom_left, top_left, color);
        }
        public void draw_cross(Vector2 pos, Vector2 size, Color color)
        {
            Vector2 half = size / 2.0f;
            Vector2 top_left = new Vector2(-half.x, half.y) + pos;
            Vector2 top_right = new Vector2(half.x, half.y) + pos;
            Vector2 bottom_left = new Vector2(-half.x, -half.y) + pos;
            Vector2 bottom_right = new Vector2(half.x, -half.y) + pos;

            Debug.DrawLine(top_left, bottom_right, color);
            Debug.DrawLine(top_right, bottom_left, color);
        }
        public Vector2Int world_to_grid(Vector2 at)
        {
            Vector2 index = at;
            index -= origin_position;
            index /= cell_size;

            return new Vector2Int((int)index.x, (int)index.y);
        }
        public Vector2 grid_to_world(Vector2Int at, bool at_center)
        {
            Vector2 pos = at;
            pos *= cell_size;
            pos += origin_position;
            if (at_center) pos += half_cell;

            return new Vector2(pos.x, pos.y);
        }
        public void set_separators(GameObject proto_center, GameObject proto_edge, GameObject proto_corner)
        {
            // Center
            for (int x = 1; x < count.x; x++)
            {
                for (int y = 1; y < count.y; y++)
                {
                    var at = grid_to_world(new Vector2Int(x, y), false);
                    Instantiate(proto_center).transform.position = at;
                }
            }
            { // Edges
                for (int x = 1; x < count.x; x++)
                {
                    var at = grid_to_world(new Vector2Int(x, 0), false);
                    var go = Instantiate(proto_edge);
                    go.transform.position = at;
                    go.transform.Rotate(Vector3.forward, 180.0f);

                }
                for (int x = 1; x < count.x; x++)
                {
                    var at = grid_to_world(new Vector2Int(x, count.y), false);
                    var go = Instantiate(proto_edge);
                    go.transform.position = at;
                }
                for (int y = 1; y < count.y; y++)
                {
                    var at = grid_to_world(new Vector2Int(0, y), false);
                    var go = Instantiate(proto_edge);
                    go.transform.position = at;
                    go.transform.Rotate(Vector3.forward, 90.0f);
                }
                for (int y = 1; y < count.x; y++)
                {
                    var at = grid_to_world(new Vector2Int(count.x, y), false);
                    var go = Instantiate(proto_edge);
                    go.transform.position = at;
                    go.transform.Rotate(Vector3.forward, -90.0f);
                }
            }

            { // Corners
                var at = grid_to_world(new Vector2Int(0, 0), false);
                var go = Instantiate(proto_corner);
                go.transform.position = at;
                go.transform.Rotate(Vector3.forward, 180.0f);

                at = grid_to_world(new Vector2Int(0, count.y), false);
                go = Instantiate(proto_corner);
                go.transform.position = at;
                go.transform.Rotate(Vector3.forward, 90.0f);

                at = grid_to_world(new Vector2Int(count.x, 0), false);
                go = Instantiate(proto_corner);
                go.transform.position = at;
                go.transform.Rotate(Vector3.forward, 270.0f);

                at = grid_to_world(new Vector2Int(count.x, count.y), false);
                go = Instantiate(proto_corner);
                go.transform.position = at;
                go.transform.Rotate(Vector3.forward, 0.0f);
            }
        }
        public void debug_draw()
        {
            for(int x = 0; x < count.x; x++)
            {
                for(int y = 0; y < count.y; y++)
                {
                    draw_rect(grid_to_world(new Vector2Int(x, y), true), 
                              new Vector2(cell_size, cell_size), 
                              Color.red);
                }
            }

            draw_rect(origin_position, new Vector2(0.5f, 0.5f), Color.yellow);
            draw_rect(bounds.position, new Vector2(0.5f, 0.5f), Color.cyan);
            draw_rect(max_position, new Vector2(0.5f, 0.5f), Color.yellow);
            draw_rect(bounds.size, new Vector2(0.5f, 0.5f), Color.cyan);
            draw_rect(center, new Vector2(0.5f, 0.5f), Color.green);
        }
        public void debug_draw_bombs()
        {
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    if (data[x][y].has_bomb())
                    {
                        var at = grid_to_world(new Vector2Int(x, y), true);
                        var size = new Vector2(cell_size, cell_size);
                        draw_rect(at, size, Color.blue);
                        draw_cross(at, size, Color.blue);
                        
                    }
                }
            }
        }
        public bool has_bomb(Vector2Int at)
        {
            return data[at.x][at.y].has_bomb();
        }
        public void allign(VerticalAllignment allignment, Vector2 camera_position, Vector2 half_view_size, Padding padding)
        {
            Vector2 at = camera_position;
            at.y = center.y;
            switch (allignment)
            {
                case VerticalAllignment.Center:
                    reposition(at);
                    break;
                case VerticalAllignment.Left:
                    at.x += padding.left;
                    at.x -= half_view_size.x;
                    at.x += (new Vector2(count.x * 0.5f, count.y * 0.5f) * cell_size).x;
                    reposition(at);
                    break;
                case VerticalAllignment.Right:
                    at.x -= padding.right;
                    at.x += half_view_size.x;
                    at.x -= (new Vector2(count.x * 0.5f, count.y * 0.5f) * cell_size).x;
                    reposition(at);
                    break;
            }
            refresh_tile_positions();
        }
        public void allign(HorizontalAllignment allignment, Vector2 camera_position, Vector2 half_view_size, Padding padding)
        {
            Vector2 at = camera_position;
            at.x = center.x;
            switch (allignment)
            {
                case HorizontalAllignment.Center:
                    reposition(at);
                    break;
                case HorizontalAllignment.Bottom:
                    at.y += padding.bottom;
                    at.y -= half_view_size.y;
                    at.y += (new Vector2(count.x * 0.5f, count.y * 0.5f) * cell_size).y;
                    reposition(at);
                    break;
                case HorizontalAllignment.Top:
                    at.y -= padding.top;
                    at.y += half_view_size.y;
                    at.y -= (new Vector2(count.x * 0.5f, count.y * 0.5f) * cell_size).y;
                    reposition(at);
                    break;
            }
            refresh_tile_positions();
        }
        
        public void flush_player()
        {
            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    var index = new Vector2Int(x, y);
                    data[index.x][index.y].set_player(false);
                }
            }
        }

        public void set_player(Vector2 at, bool to)
        {
            var index = world_to_grid(at);
            data[index.x][index.y].set_player(to);
        }
        public Rect get_bounds()
        {
            return bounds;
        }
        public Vector2Int get_counts()
        {
            return count;
        }
        public Vector2 get_center()
        {
            return center;
        }

        public Vector2Int get_start()
        {
            return start;
        }
        public Vector2Int get_goal()
        {
            return goal;
        }
        public int get_bomb_count(Vector2Int at)
        {
            return data[at.x][at.y].get_bomb_count();
        }
        public void set_bomb_count(Vector2Int at, int to)
        {
            data[at.x][at.y].set_number(to);
        }
        public void set_number_active(Vector2Int at, bool to)
        {
            data[at.x][at.y].set_number_active(to);
        }
        public void set_cell_and_neighbours_active(Vector2Int at, bool to)
        {
            set_number_active(at, to);
            data[at.x][at.y].set_neighbour_numbers_active(to);
        }
        Vector2Int start;
        Vector2Int goal;
        int free_space_amount;
        Rect bounds;
        Vector2 origin_position;
        Vector2Int count;
        float cell_size;
        Vector2 max_position;
        Vector2 half_cell;
        Vector2 center;
        Tile[][] data = null;
    }
    [SerializeField] private bool is_hard_mode;
    [SerializeField] private bool was_hard_mode;
    [SerializeField] private GameObject[] cell_prototypes = null;
    [SerializeField] private Camera view = null;
    [SerializeField] private Grid grid;
    [SerializeField] private int free_space_amount;
    [SerializeField] private VerticalAllignment vertical_allignment;
    [SerializeField] private HorizontalAllignment horizontal_allignment;
    [SerializeField] private Padding paddings;
    [SerializeField] private Vector2 deadzone;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject center_separator;
    [SerializeField] private GameObject edge_separator;
    [SerializeField] private GameObject corner_separator;
    [SerializeField] private float cell_size;
    [SerializeField] private MultiDigitPrinter score_printer;
    [SerializeField] private Clock clock_printer;
    [SerializeField] private AudioClip activated_bomb;
    [SerializeField] private AudioClip respawn;
    [SerializeField] private AudioClip win;
    [SerializeField] private AudioClip reset_stuff_sound;
    [SerializeField] private AudioClip select_time_sound;
    [SerializeField] private AudioClip game_start_sounds;
    [SerializeField] private AudioClip select_difficulty_sound;
    [SerializeField] private SpriteRenderer game_a;
    [SerializeField] private SpriteRenderer game_b;
    [SerializeField] private Bomb bomb;
    [SerializeField] private int play_time;
    private List<Vector2Int> instance_one = new List<Vector2Int>();
    private List<Vector2Int> instance_two = new List<Vector2Int>();
    private bool is_instance_one = true;
    public float intro_interval = 0.4f;
    private float intro_timer = 0.0f;
    private AudioSource audio_source;
    private AudioSource reset_stuff_audio;
    private State state;
    private int score;
    private Vector2 gameplay_screen_size;
    private Vector2 full_size;
    private Vector2 half_size;
    
    public void set_hardmode(bool hard_mode)
    {
        was_hard_mode = is_hard_mode;
        if(!hard_mode)
        {
            game_a.enabled = true;
            game_b.enabled = false;
        }
        else
        {
            game_a.enabled = false;
            game_b.enabled = true;
        }
        is_hard_mode = hard_mode;
    }

    public float get_cell_size()
    {
        return cell_size;
    }
    public Vector2 clamp(Vector2 that)
    {
        Rect bounds = grid.get_bounds();
        that.x = Mathf.Clamp(that.x, bounds.position.x, bounds.size.x);
        that.y = Mathf.Clamp(that.y, bounds.position.y, bounds.size.y);
        return that;
    }
    public void set_all_numbers_active(bool to)
    {
        for (int x = 0; x < grid.get_counts().x; ++x)
        {
            for (int y = 0; y < grid.get_counts().y; ++y)
            {
                grid.set_number_active(new Vector2Int(x, y), to);
            }
        }
    }
    public void reset_game()
    {
        grid.set_player(player.transform.position, false);
        grid.place_bombs();
        grid.flush_player();
        grid.count_bombs();
        Vector2 spawn_location = grid.grid_to_world(grid.get_start(), true);
        player.transform.position = spawn_location;
        grid.set_player(spawn_location, true);
        var count = grid.get_bomb_count(grid.get_start());
        bomb.set_interval(1.0f - (Mathf.Pow(count + 1, 2) / 10));
        if (!is_hard_mode)
        {
            set_all_numbers_active(true);
        }
        else if (is_hard_mode)
            field_of_view(spawn_location);
    }
    public void game_over()
    {
        StartCoroutine(game_over_routine());
        StartCoroutine(bomb.explode_routine(0.7f, 0.1f, 0.8f, 2.5f));
        audio_source.clip = activated_bomb;
        audio_source.Play();
    }
    public void win_level()
    {
        reset_game();
        score_printer.set(++score);
        audio_source.clip = win;
        audio_source.Play();
    }
    private void field_of_view(Vector2 at)
    {
        for (int x = 0; x < grid.get_counts().x; ++x)
        {
            for (int y = 0; y < grid.get_counts().y; ++y)
            {
                grid.set_number_active(new Vector2Int(x, y), false);
            }
        }
        grid.set_cell_and_neighbours_active(grid.world_to_grid(at), true);
    }
    public Outcome refresh_player_position_and_check_death(Vector2 current_pos, Vector2 previous_pos)
    {
        grid.set_player(previous_pos, false);
        grid.set_player(current_pos, true);

        if(is_hard_mode)
            field_of_view(current_pos);
        if(is_hard_mode != was_hard_mode)
        {
            if(!is_hard_mode)
                set_all_numbers_active(true);
        }

        var player_is_at = grid.world_to_grid(current_pos);
        var count = grid.get_bomb_count(player_is_at);
        bomb.set_interval(1.2f - (Mathf.Pow(count + 1, 2) / 15));

        was_hard_mode = is_hard_mode;
        if (grid.has_bomb(player_is_at))
            return Outcome.Lose;
        else if (player_is_at == grid.get_goal())
            return Outcome.Win;
        return Outcome.None;
    }

    private void calculate_screen_size(Camera camera)
    {
        full_size.y = camera.orthographicSize * 2f;
        full_size.x = full_size.y * camera.aspect;

        gameplay_screen_size = full_size - (deadzone * 2.0f);

        half_size = full_size / 2f;
    }
    private void setup_environment()
    {
        audio_source = GetComponent<AudioSource>();
        reset_stuff_audio = gameObject.AddComponent<AudioSource>();
        reset_stuff_audio.playOnAwake = false;
        reset_stuff_audio.clip = reset_stuff_sound;

        clock_printer.register(this);
        score = 0;
        view.transform.Translate(new Vector3(0, 0, -10));
        calculate_screen_size(view);

        float scale = cell_size * 0.5f;
        foreach(var cell_prototype in cell_prototypes)
        {
            var content = cell_prototype.GetComponent<CellContent>();
            content.resize(cell_size * 0.5f);
        }
        scale /= 2.0f;

        edge_separator.transform.localScale = new Vector3(scale, scale, 1);
        center_separator.transform.localScale = new Vector3(scale, scale, 1);
        corner_separator.transform.localScale = new Vector3(scale, scale, 1);
        clock_printer.restart_countdown(play_time);
        grid = new Grid(Vector2.zero, gameplay_screen_size, cell_prototypes, cell_size, free_space_amount);
        grid.allign(vertical_allignment, view.transform.position, half_size, paddings);
        grid.allign(horizontal_allignment, view.transform.position, half_size, paddings);
        grid.set_separators(center_separator, edge_separator, corner_separator);
        Vector2 spawn_location = grid.grid_to_world(grid.get_start(), true);
        player.transform.position = spawn_location;
        player.GetComponent<PlayerMovement>().set_spawn(spawn_location);
        player.GetComponent<Controller>().register(this);

    }
    private void enter_intro()
    {
        clock_printer.pause();
        player.GetComponent<PlayerMovement>().set_authority_block(true);
        bomb.pause();
        state = State.Intro;
    }

    private void enter_play()
    {
        player.GetComponent<PlayerMovement>().set_authority_block(false);
        bomb.play();
        reset_game();
        clock_printer.play();
        clock_printer.set(play_time);
        score_printer.set(0);
        state = State.Play;
    }

    void Awake()
    {
        was_hard_mode = is_hard_mode;
        setup_environment();
        enter_intro();

        bool swap = true;
        for(int x = 0; x < grid.get_counts().x; ++x)
        {
            for (int y = 0; y < grid.get_counts().y; ++y)
            {
                grid.set_bomb_count(new Vector2Int(x, y), 8);
                grid.set_player(grid.grid_to_world(new Vector2Int(x, y), true), true);
                if (swap) instance_one.Add(new Vector2Int(x, y));
                else      instance_two.Add(new Vector2Int(x, y));
                swap = !swap;
            }
        }
        set_hardmode(false);
        intro_timer = intro_interval * 5.0f;


    }
    private void play_intro()
    {
        clock_printer.set(play_time);
        score_printer.set(888);
        if (intro_timer < 0)
        {
            foreach (var at in instance_one)
            {
                grid.set_number_active(at, !is_instance_one);
                grid.set_player(grid.grid_to_world(at, true), is_instance_one);
            }
            foreach (var at in instance_two)
            {
                grid.set_number_active(at, is_instance_one);
                grid.set_player(grid.grid_to_world(at, true), !is_instance_one);
            }
            is_instance_one = !is_instance_one;
            intro_timer = intro_interval;
            bomb.wiggle();
        }
        intro_timer -= Time.fixedDeltaTime;
    }
    private void FixedUpdate()
    {
        if(state == State.Intro)
        {
            play_intro();
        }

        //grid.count_bombs();
        //grid.debug_draw();
        grid.debug_draw_bombs();
        
    }

    public void on_timer_zero(Clock timer)
    {
        game_over();
    }

    public interface GameAndWatchAnim
    {
        public abstract void update();
        public abstract void exit();
    }
    public class ClockAddAnimation : GameAndWatchAnim
    {
        float current;
        float target;

        float fraction;
        Clock context;
        AudioSource audio;
        int previous;
        public ClockAddAnimation(Clock context, float target, float time, AudioSource audio_souce)
        {
            this.target = target;
            this.context = context;
            this.current = context.get_countdown();
            float difference = target - current;
            this.fraction = difference > 0 ? difference / time : 0;
            this.audio = audio_souce;
            previous = (int)current;
        }
        public void update()
        {
            if (current < target)
            {
                previous = (int)current;
                current += (fraction * Time.fixedDeltaTime);
                if (current > target)
                    context.set((int)target);
                else
                    context.set((int)current);

                if (previous != (int)current)
                {
                    audio.Play();
                }
            }
        }
        public void exit()
        {
            context.restart_countdown((int)target);
        }
    }

    public class ScoreSubtractAnimation : GameAndWatchAnim
    {
        float current;
        float fraction;
        MultiDigitPrinter context;

       public ScoreSubtractAnimation(MultiDigitPrinter context, int from, float time)
        {
            this.context = context;
            this.current = from;
            this.fraction = from > 0 ? from / time : 0;
        }
        public void update()
        {
            if (current > 0)
            {
                current -= (fraction * Time.fixedDeltaTime);
                context.set((int)current);
            }
        }
        public void exit()
        {
            current = 0;
            context.set((int)current);
        }
    }
    public class LoseAnimation : GameAndWatchAnim
    {
        float timer;
        bool active;
        Grid context;
        Vector2 position;
        ClockAddAnimation clock;
        ScoreSubtractAnimation score;
        List<GameAndWatchAnim> children;

        public void add(GameAndWatchAnim that)
        {
            children.Add(that);
        }

        public LoseAnimation(Grid context, Vector2 position, float time)
        {
            this.children = new List<GameAndWatchAnim>();
            this.position = position;
            this.context = context;

            this.timer = time;
            this.active = true;
        }
        public bool is_playing()
        {
            return timer > 0;
        }
        public void update()
        {
            foreach(var child in children)
            {
                child.update();
            }

            context.set_player(position, active);

            active = !active;
            timer -= Time.fixedDeltaTime;
        }
        public void exit()
        {
            foreach (var child in children)
            {
                child.exit();
                context.set_player(position, true);
            }
        }
    }
    void prepare_game_over_routine()
    {
        clock_printer.pause();
        player.GetComponent<PlayerMovement>().set_authority_block(true);
    }
    IEnumerator exit_game_over_routine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        player.GetComponent<PlayerMovement>().set_authority_block(false);
        score = score_printer.get();
        clock_printer.play();
    }
    IEnumerator game_over_animation_first(float full_time)
    {
        var lose_anim = new LoseAnimation(grid, player.transform.position, full_time);
        while (lose_anim.is_playing())
        {
            lose_anim.update();
            yield return new WaitForFixedUpdate();
        }
        lose_anim.exit();
    }
    IEnumerator game_over_animation(float full_time, float sub_time, float delay, AudioSource audio)
    {
        var lose_anim = new LoseAnimation(grid, player.transform.position, full_time);
        lose_anim.add(new ClockAddAnimation(clock_printer, play_time, sub_time, audio));
        lose_anim.add(new ScoreSubtractAnimation(score_printer, score, sub_time));
        while (lose_anim.is_playing())
        {
            lose_anim.update();
            yield return new WaitForFixedUpdate();
        }
        lose_anim.exit();
        yield return new WaitForSecondsRealtime(delay);
    }
    IEnumerator game_over_routine()
    {
        prepare_game_over_routine();
        yield return game_over_animation_first(1.0f);
        yield return game_over_animation(2.0f, 1.0f, 1.0f, reset_stuff_audio);
        reset_game();
        audio_source.clip = respawn;
        audio_source.Play();
        //reset_stuff_audio.Play(); // Replace with specific sound
        yield return exit_game_over_routine(0.5f);
        yield break;
    }

    public void on_press(Controller.Button what)
    {
        if(state == State.Intro)
        {
            if (what == Controller.Button.Action)
            {
                audio_source.clip = game_start_sounds;
                audio_source.Play();
                enter_play();
            }
            else if (what == Controller.Button.Up ||
                     what == Controller.Button.Down)
            {
                audio_source.clip = select_difficulty_sound;
                audio_source.Play();
                set_hardmode(!is_hard_mode);
            }
            else if (what == Controller.Button.Left)
            {
                audio_source.clip = select_time_sound;
                audio_source.Play();
                play_time -= 30;
                play_time = Mathf.Clamp(play_time - 30, 60, 990);
                clock_printer.restart_countdown(play_time);

            }
            else if (what == Controller.Button.Right)
            {
                audio_source.clip = select_time_sound;
                audio_source.Play();
                play_time = Mathf.Clamp(play_time + 30, 60, 990);
                clock_printer.restart_countdown(play_time);
            }
        }


    }
}

