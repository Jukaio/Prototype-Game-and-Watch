using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellContent : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] NumberRenderer number_renderer;
    public bool has_bomb = false;

    List<CellContent> neighbours = new List<CellContent>();
    public void set_neighbours_active(bool to)
    {
        foreach(var neighbour in neighbours)
        {
            neighbour.set_number_visibility(to);
            neighbour.number_renderer.refresh();
        }
    }
    public void set_number_visibility(bool to)
    {
        number_renderer.gameObject.SetActive(to);
        number_renderer.refresh();
    }
    public void set_number(int number)
    {
        number_renderer.set(number);
    }
    public void set_bomb(bool to)
    {
        has_bomb = to;
    }
    public bool contains_bomb()
    {
        return has_bomb;
    }
    public void set_player(bool to)
    {
        player.SetActive(to);
    }
    public void add(CellContent neighbour)
    {
        neighbours.Add(neighbour);
    }
    public void resize(float factor)
    {
        transform.localScale = new Vector3(factor, factor, 1);
    }

    public void count_bombs()
    {
        int count = 0;
        foreach(var item in neighbours)
        {
            count += item.has_bomb ? 1 : 0;
        }
        count += this.has_bomb ? 1 : 0;
        number_renderer.set(count);
    }
    public int get_bomb_count()
    {
        return number_renderer.get();
    }

    void Awake()
    {
        player.SetActive(false);
    }

}
