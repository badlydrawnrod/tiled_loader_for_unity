using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Tiled;

public class LevelLoader : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public GameObject walker;

    private TiledMap map;
    private List<Texture2D> loadedTextures;

    public void LoadLevel(int levelNumber)
    {
        LoadLevel("level" + levelNumber);
    }

    private void LoadLevel(string levelFilename)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(levelFilename);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(textAsset.text);
        map = new TiledMap(doc.DocumentElement);
        LoadTextures();
        BuildLayers();
        BuildObjectGroups();
    }

    private void LoadTextures()
    {
        loadedTextures = new List<Texture2D>();
        foreach (var tileset in map.Tilesets)
        {
            // Lop off the ".png" then load the texture.
            string textureFilename = tileset.Image.Source.Substring(0, tileset.Image.Source.Length - 4);
            Texture2D texture = Resources.Load<Texture2D>(textureFilename); // TODO: What if we can't load it?
            loadedTextures.Add(texture);
        }
    }

    private void BuildLayers()
    {
        foreach (var layer in map.Layers)
        {
            BuildLayer(layer);
        }
    }

    private void BuildLayer(TiledLayer layer)
    {
        // Create a game object to hold this layer, and parent it to this transform.
        GameObject go = new GameObject(layer.Name);
        go.transform.SetParent(transform, false);

        // Add this layer's tiles.
        for (var row = 0; row < layer.Height; row++)
        {
            for (var col = 0; col < layer.Width; col++)
            {
                int gid = layer.Data[col + row * layer.Width];
                if (gid != 0)
                {
                    // Create the tile and parent it to the layer's transform.
                    GameObject tile = AddTile(layer, row, col, gid);
                    tile.transform.SetParent(go.transform, false);
                }
            }
        }
    }

    private GameObject AddTile(TiledLayer layer, int row, int col, int gid)
    {
        // Determine which tileset it is.
        int tilesetIndex = map.TilesetIndex(gid);
        TiledTileset tileset = map.Tilesets[tilesetIndex];

        // Calculate the texture region within the tile's texture.
        int tileIndex = gid - tileset.FirstGid;
        int u = (tileIndex % layer.Width) * map.TileWidth;
        int v = (tileIndex / layer.Height) * map.TileHeight;

        // Calculate the world coordinates.
        int x = (col - layer.Width / 2) * map.TileWidth;
        int y = -(row - layer.Height / 2) * map.TileHeight;

        // Add the tile to the world.
        Texture2D texture = loadedTextures[tilesetIndex];
        GameObject tile = AddTile(texture, new Rect(u, v, map.TileWidth, map.TileHeight), x, y);

        // Handle any properties associated with this tile.
        TiledTile tiledTile;
        if (map.TilesByGid.TryGetValue(gid, out tiledTile))
        {
            string type = tiledTile.GetPropertyByName("type");
            if (type == "block")
            {
                BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
                collider.usedByEffector = true;
                PlatformEffector2D effector = tile.AddComponent<PlatformEffector2D>();
                string oneWay = tiledTile.GetPropertyByName("one_way");
                effector.useOneWay = (oneWay != null && oneWay == "true");
            }
        }
        return tile;
    }

    private GameObject AddTile(Texture2D texture, Rect regionRect, float x, float y)
    {
        GameObject go = new GameObject("tile", typeof(SpriteRenderer));
        go.transform.position = new Vector2(x, y);
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(texture, regionRect, new Vector2(0.0f, 1.0f), 1); // Pivot point at top left of tile.
        return go;
    }

    private void BuildObjectGroups()
    {
        foreach (var group in map.ObjectGroups)
        {
            BuildObjectGroup(group);
        }
    }

    private void BuildObjectGroup(TiledObjectGroup group)
    {
        foreach (var obj in group.Objects)
        {
            string type = obj.Type;
            GameObject mob = null;
            switch (type)
            {
                case "player 1":
                    mob = player1;
                    break;
                case "player 2":
                    mob = player2;
                    break;
                case "walker":
                    mob = walker;
                    break;
            }
            if (mob != null)
            {
                float halfWidth = (map.Width * map.TileWidth) / 2;
                float halfHeight = (map.Height * map.TileHeight) / 2;
                var sprite = Instantiate(mob, new Vector3(obj.X - halfWidth + map.TileWidth / 2, -obj.Y + halfHeight - map.TileHeight / 2, 0), Quaternion.identity) as GameObject;
                sprite.transform.SetParent(transform, false);
            }
        }
    }
}
