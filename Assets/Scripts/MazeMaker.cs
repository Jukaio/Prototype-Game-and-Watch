using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MazeMaker
{
    private enum Direction
    {
        Horizontal,
        Vertical
    }
    static public bool[,] data;
    static RectInt rect;
    static int depth_x = 2;
    static int depth_y = 2;

    static public bool[,] create(int w, int h, int depth)
    {

        depth_x = Mathf.Clamp(depth, 2, w);
        depth_y = Mathf.Clamp(depth, 2, h);
        rect = new RectInt(0, 0, w, h);
        data = new bool[w, h];
        divide_grid(0, 0, w, h, choose_direction(w, h));

        bool double_row_x = ((w - 2) % 2) == 0;
        bool double_row_y = ((h - 2) % 2) == 0;

        //set_border(w, h, double_row_x, double_row_y);
        //free_path(spawn, goal);
        return data;
    }
    static private Direction random_direction()
    {
        Random.InitState(Random.Range(0, int.MaxValue));
        return (Direction)Random.Range(0, 1);
    }

    static private int RandomNumber(int min, int max)
    {
        Random.InitState(Random.Range(0, int.MaxValue));
        return Random.Range(min, max);
    }

    static private Direction choose_direction(int width, int height)
    {
        if (width < height) return Direction.Horizontal;
        else if (width > height) return Direction.Vertical;
        else return (Direction)RandomNumber(0, 1);
    }

    static void divide_grid(int x, int y, int w, int h, Direction orientation)
    {
        if (w < depth_x || h < depth_y)
            return;

        // Which direction
        int dx = (orientation == Direction.Horizontal) ? 1 : 0;
        int dy = (orientation == Direction.Vertical) ? 1 : 0;

        // Random numbers (whole line)
        int rx = x + (dy * RandomNumber(0, w)); // (0 - 11)
        int ry = y + (dx * RandomNumber(0, h)); 

        // Only apply to the chosen direction
        // Add an odd/even offset
        // Make even
        rx += (dy * (rx % 2));
        ry += (dx * (ry % 2));

        // Random Point 
        // chosen direction with random length
        int px = rx + (dx * RandomNumber(0, w));
        int py = ry + (dy * RandomNumber(0, h));

        // TODO: Point at UNEVEN location on EVEN ROW

        // If direction is not 0 
        // And if coordinate is not even already
        if (dx != 0 && (px % 2) == 0)
        {
            if (px <= rect.position.x) px += (1 + (rect.position.x % 1));
            else if (px >= rect.size.x) px -= (1 + (rect.size.x % 1));
            else px += (int)random_direction(); // else just randomise an increase or decrease
        }
        else if (dy != 0 && (py % 2) == 0)
        {
            if (py <= rect.position.y) py += (1 + (rect.position.y % 1));
            else if (py >= rect.size.y) py -= (1 + (rect.size.y % 1));
            else py += (int)random_direction();
        }

        int length = dx * w + dy * h;

        // Draw line
        for (int i = 0; i < length; i++)
        {
            int ix = rx + (dx * i);
            int iy = ry + (dy * i);
            if (data[ix, iy]) Debug.Log("Already true");
            data[ix, iy] = !(px == ix && py == iy); // !(px == ix && py == iy) for maze
        }
        // Lower side
        /*
           nx, ny = x, y
           w, h = horizontal ? [width, wy-y+1] : [wx-x+1, height]
           divide(grid, nx, ny, w, h, choose_orientation(w, h) 
        */
        int nx = x; // 0
        int ny = y; // 0
        int width = (dx * w) + (dy * (rx - x )); // width + direction * dot
        int height = (dy * h) + (dx * (ry - y ));
        divide_grid(nx, ny, width, height, choose_direction(width, height));

        // Upper side
        /*
           nx, ny = horizontal ? [x, wy+1] : [wx+1, y]
           w, h = horizontal ? [width, y+height-wy-1] : [x+width-wx-1, height]
           divide(grid, nx, ny, w, h, choose_orientation(w, h))
        */
        nx = (dx * x) + (dy * (rx + 1));
        ny = (dy * y) + (dx * (ry + 1));
        width = (dx * w) + (dy * (x + w - rx - 1));
        height = (dy * h) + (dx * (y + h - ry - 1));
        divide_grid(nx, ny, width, height, choose_direction(width, height));
    }
}

