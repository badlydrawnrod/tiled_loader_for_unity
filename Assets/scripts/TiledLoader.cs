using System;
using System.Collections.Generic;
using System.Xml;

namespace Tiled
{
    /// <summary>
    /// Represents a TMX map. See: https://github.com/bjorn/tiled/wiki/TMX-Map-Format.
    /// </summary>
    public class TiledMap
    {
        /// <summary>
        /// The TMX format version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// The orientation. Can be "orthogonal", "isometric" or "staggered".
        /// </summary>
        public string Orientation { get; private set; }

        /// <summary>
        /// The map width in tiles.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The map height in tiles.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The width of a tile.
        /// </summary>
        public int TileWidth { get; private set; }

        /// <summary>
        /// The height of a tile.
        /// </summary>
        public int TileHeight { get; private set; }

        /// <summary>
        /// The background colour of a map.
        /// </summary>
        public string BackgroundColor { get; private set; }

        /// <summary>
        /// The order in which tiles are rendered. Can be "right-down", "right-up", "left-down" or "left-up".
        /// </summary>
        public string RenderOrder { get; private set; }

        /// <summary>
        /// The tilesets used by this map.
        /// </summary>
        public List<TiledTileset> Tilesets { get; private set; }

        /// <summary>
        /// This map's layers.
        /// </summary>
        public List<TiledLayer> Layers { get; private set; }

        /// <summary>
        /// This map's object groups.
        /// </summary>
        public List<TiledObjectGroup> ObjectGroups { get; private set; }

        /// <summary>
        /// All tiles that have properties, indexed by global tile id.
        /// </summary>
        public Dictionary<int, TiledTile> TilesByGid { get; private set; }

        public TiledMap(XmlElement e)
        {
            ParseMap(e);
        }

        private void ParseMap(XmlElement e)
        {
            LoadAttributes(e);
            LoadTilesets(e);
            LoadLayers(e);
            LoadObjectGroups(e);
        }

        private void LoadAttributes(XmlElement e)
        {
            Version = e.GetAttribute("version");
            Orientation = e.GetAttribute("orientation");
            Width = int.Parse(e.GetAttribute("width"));
            Height = int.Parse(e.GetAttribute("height"));
            TileWidth = int.Parse(e.GetAttribute("tilewidth"));
            TileHeight = int.Parse(e.GetAttribute("tileheight"));
            BackgroundColor = e.GetAttribute("backgroundColor");
            RenderOrder = e.GetAttribute("renderorder");
        }

        private void LoadTilesets(XmlElement e)
        {
            TilesByGid = new Dictionary<int, TiledTile>();
            var tilesets = e.SelectNodes("tileset");
            Tilesets = new List<TiledTileset>();
            foreach (var tileset in tilesets)
            {
                var tmxTileset = new TiledTileset(tileset as XmlElement);
                Tilesets.Add(tmxTileset);
                foreach (var tile in tmxTileset.Tiles)
                {
                    TilesByGid[tile.Id + tmxTileset.FirstGid] = tile;
                }
            }
        }

        private void LoadLayers(XmlElement e)
        {
            var layers = e.SelectNodes("layer");
            Layers = new List<TiledLayer>();
            foreach (var layer in layers)
            {
                Layers.Add(new TiledLayer(layer as XmlElement));
            }
        }

        private void LoadObjectGroups(XmlElement e)
        {
            var objectGroups = e.SelectNodes("objectgroup");
            ObjectGroups = new List<TiledObjectGroup>();
            foreach (var objectGroup in objectGroups)
            {
                ObjectGroups.Add(new TiledObjectGroup(objectGroup as XmlElement));
            }
        }

        public int TilesetIndex(int gid)
        {
            for (var i = Tilesets.Count - 1; i >= 0; i--)
            {
                if (gid >= Tilesets[i].FirstGid)
                {
                    return i;
                }
            }
            return -1;
        }

    }

    /// <summary>
    /// Represents a tileset in a TMX map.
    /// </summary>
    public class TiledTileset
    {
        /// <summary>
        /// The global tile id of the lowest numbered tile in this tileset.
        /// </summary>
        public int FirstGid { get; private set; }

        /// <summary>
        /// The TSX file (if any) that contains this tileset.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// This tileset's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The maximum width of the tiles in this tileset.
        /// </summary>
        public int TileWidth { get; private set; }

        /// <summary>
        /// The maximum height of the tiles in this tileset.
        /// </summary>
        public int TileHeight { get; private set; }

        /// <summary>
        /// The spacing in pixels between tiles in this tileset's image.
        /// </summary>
        public int Spacing { get; private set; }

        /// <summary>
        /// The margin in pixels around tiles in this tileset's image.
        /// </summary>
        public int Margin { get; private set; }

        /// <summary>
        /// This tileset's image.
        /// </summary>
        public TiledImage Image { get; private set; }

        /// <summary>
        /// The tiles in this tileset that have properties.
        /// </summary>
        public List<TiledTile> Tiles { get; private set; }

        public TiledTileset(XmlElement e)
        {
            ParseTileset(e);
        }

        private void ParseTileset(XmlElement e)
        {
            FirstGid = int.Parse(e.GetAttribute("firstgid"));
            Source = e.GetAttribute("source");
            Name = e.GetAttribute("name");
            TileWidth = int.Parse(e.GetAttribute("tilewidth"));
            TileHeight = int.Parse(e.GetAttribute("tileheight"));

            Spacing = e.HasAttribute("spacing")
                ? int.Parse(e.GetAttribute("spacing"))
                : 0;

            Margin = e.HasAttribute("margin")
                ? int.Parse(e.GetAttribute("margin"))
                : 0;

            if (e["image"] != null)
            {
                Image = new TiledImage(e["image"]);
            }

            Tiles = new List<TiledTile>();
            var tiles = e.SelectNodes("tile");
            foreach (var tile in tiles)
            {
                Tiles.Add(new TiledTile(tile as XmlElement));
            }
        }
    }

    /// <summary>
    /// Represents a tile (with properties) in a TMX map.
    /// </summary>
    public class TiledTile
    {
        /// <summary>
        /// This tile's id, local to its tileset.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The properties associated with this tile.
        /// </summary>
        public List<TiledProperty> Properties { get; private set; }

        public TiledTile(XmlElement e)
        {
            ParseTile(e);
        }

        private void ParseTile(XmlElement e)
        {
            Id = int.Parse(e.GetAttribute("id"));
            
            Properties = new List<TiledProperty>();
            if (e["properties"] != null)
            {
                var ep = e["properties"];
                var properties = ep.SelectNodes("property");
                foreach (var property in properties)
                {
                    Properties.Add(new TiledProperty(property as XmlElement));
                }
            }
        }

        public string GetPropertyByName(string name)
        {
            TiledProperty property = Properties.Find(e => e.Name == name);
            return property != null ? property.Value : null;
        }
    }

    /// <summary>
    /// Represents a single tile property in a TMX map.
    /// </summary>
    public class TiledProperty
    {
        /// <summary>
        /// This property's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This property's value.
        /// </summary>
        public string Value { get; private set; }

        public TiledProperty(XmlElement e)
        {
            ParseProperty(e);
        }

        private void ParseProperty(XmlElement e)
        {
            Name = e.GetAttribute("name");
            Value = e.GetAttribute("value");
        }
    }

    /// <summary>
    /// Represents a tileset image in a TMX map.
    /// </summary>
    public class TiledImage
    {
        /// <summary>
        /// This image's source, ie, its filename.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// This image's width in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// This image's height in pixels.
        /// </summary>
        public int Height { get; private set; }

        public TiledImage(XmlElement e)
        {
            ParseImage(e);
        }

        private void ParseImage(XmlElement e)
        {
            Source = e.GetAttribute("source");
            Width = int.Parse(e.GetAttribute("width"));
            Height = int.Parse(e.GetAttribute("height"));
        }
    }

    /// <summary>
    /// Represents a layer within a TMX map.
    /// </summary>
    public class TiledLayer
    {
        /// <summary>
        /// This layer's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This layer's width in tiles. Always the same as the map's width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// This layer's height in tiles. Always the same as the map's height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The opacity of the layer from 0.0 to 1.0.
        /// </summary>
        public float Opacity { get; private set; }

        /// <summary>
        /// The layer data, ie, global tile ids. Note that this implementation only handles CSV format.
        /// </summary>
        public int[] Data { get; private set; }

        /// <summary>
        /// The visibility of the layer. True means visible and false means hidden.
        /// </summary>
        public bool Visible { get; private set; }

        public TiledLayer(XmlElement e)
        {
            ParseLayer(e);
        }

        private void ParseLayer(XmlElement e)
        {
            Name = e.GetAttribute("name");
            Width = int.Parse(e.GetAttribute("width"));
            Height = int.Parse(e.GetAttribute("height"));
            Opacity = e.HasAttribute("opacity")
                ? float.Parse(e.GetAttribute("opacity"))
                : 1.0f;
            Visible = !e.HasAttribute("visible") || int.Parse(e.GetAttribute("visible")) != 0;

            Data = ParseData(e["data"]);
        }

        private int[] ParseData(XmlElement e)
        {
            if (e.GetAttribute("encoding") == "csv")
            {
                return ParseCsvData(e.InnerText);
            }
            throw new Exception("Unsupported layer encoding - 'csv' only please");
        }

        private int[] ParseCsvData(string s)
        {
            int[] gids = new int[Width * Height];
            int i = 0;
            foreach (var index in s.Split(','))
            {
                var gid = int.Parse(index.Trim());
                gids[i] = gid;
                i++;
            }
            return gids;
        }
    }

    /// <summary>
    /// Represents an object group in a TMX map.
    /// </summary>
    public class TiledObjectGroup
    {
        /// <summary>
        /// This object group's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This object group's colour.
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// The opacity of this object group from 0.0 to 1.0.
        /// </summary>
        public float Opacity { get; private set; }

        /// <summary>
        /// The visibility of this object group. True means visible and false means hidden.
        /// </summary>
        public bool Visible { get; private set; }

        /// <summary>
        /// The objects in this object group.
        /// </summary>
        public List<TiledObject> Objects { get; private set; }

        public TiledObjectGroup(XmlElement e)
        {
            ParseObjectGroup(e);
        }

        private void ParseObjectGroup(XmlElement e)
        {
            Name = e.GetAttribute("name");
            Color = e.GetAttribute("color");
            Opacity = e.HasAttribute("opacity")
                ? float.Parse(e.GetAttribute("opacity"))
                : 1.0f;
            Visible = !e.HasAttribute("visible") || int.Parse(e.GetAttribute("visible")) != 0;

            var objects = e.SelectNodes("object");
            Objects = new List<TiledObject>();
            foreach (var obj in objects)
            {
                Objects.Add(new TiledObject(obj as XmlElement));
            }
        }
    }

    /// <summary>
    /// Represents an object in a TMX map.
    /// </summary>
    public class TiledObject
    {
        /// <summary>
        /// This object's unique id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// This object's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This object's type.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The x coordinate of this object, in pixels.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The y coordinate of this object, in pixels.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// The width of this object, in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of this object, in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The rotation of this object in degrees clockwise.
        /// </summary>
        public int Rotation { get; private set; }

        /// <summary>
        /// An optional reference to a global id.   
        /// </summary>
        public int Gid { get; private set; }

        /// <summary>
        /// The visibility of this object group. True means visible and false means hidden.
        /// </summary>
        public bool Visible { get; private set; }

        public TiledObject(XmlElement e)
        {
            ParseObject(e);
        }

        private void ParseObject(XmlElement e)
        {
            Id = e.HasAttribute("id")
                ? int.Parse(e.GetAttribute("id"))
                : -1;
            Name = e.GetAttribute("name");
            Type = e.GetAttribute("type");
            X = int.Parse(e.GetAttribute("x"));
            Y = int.Parse(e.GetAttribute("y"));
            Width = e.HasAttribute("width")
                ? int.Parse(e.GetAttribute("width"))
                : 0;
            Height = e.HasAttribute("height")
                ? int.Parse(e.GetAttribute("height"))
                : 0;
            Rotation = e.HasAttribute("rotation")
                ? int.Parse(e.GetAttribute("rotation"))
                : 0;
            Gid = e.HasAttribute("gid")
                ? int.Parse(e.GetAttribute("gid"))
                : -1;
            Visible = !e.HasAttribute("visible") || int.Parse(e.GetAttribute("visible")) != 0;
        }
    }
}
