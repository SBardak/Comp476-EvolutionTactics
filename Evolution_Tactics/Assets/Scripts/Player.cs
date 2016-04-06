using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    public virtual void StartTurn()
    {
    }

    public Color TeamColor;

    public void PositionCharacter(Character character)
    {
        PositionCharacter(character, null);
    }

    public void PositionCharacter(List<Character> characters)
    {
        Tile prev = null;
        foreach (var c in characters)
            prev = PositionCharacter(c, prev);
    }

    Tile PositionCharacter(Character character, Tile t)
    {
        int x = 0, z = 0;
        if (t == null)
        {
            do
            {
                x = Random.Range(0, TileGenerator.Instance.mapWidth - 1);
                z = Random.Range(0, TileGenerator.Instance.mapHeight - 1);
                t = TileGenerator.Instance.Tiles[x, z];
            } while (t.IsOccupied);
        }
        else
        {
            x = (int)t.transform.position.x;
            z = (int)t.transform.position.z;

            int xtra = 1, ztra = 1;
            int count = 2;
            int xx, zz;
            do
            {
                xtra = Random.Range(0, count / 2 + 1);
                ztra = Random.Range(0, count / 2 + 1);
                xx = x + ((Random.Range(0, 2) < 1) ? -1 : 1) * xtra;
                zz = z + ((Random.Range(0, 2) < 1) ? -1 : 1) * ztra;

                xx = Mathf.Clamp(xx, 0, TileGenerator.Instance.mapWidth - 1);
                zz = Mathf.Clamp(zz, 0, TileGenerator.Instance.mapHeight - 1);

                t = TileGenerator.Instance.Tiles[xx, zz];
                ++count;
            } while (t.IsOccupied);
        }
        character.SetCurrentTile(t);
        character.transform.position = new Vector3(t.transform.position.x, t.transform.position.y, t.transform.position.z);

        return t;
    }
}
