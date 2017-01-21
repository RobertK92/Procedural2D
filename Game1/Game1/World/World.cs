using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LibNoise;
using FarseerPhysics.Collision.Shapes;
using Game1.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace Game1
{
    [Flags]
    public enum ChunkNode 
    {
        Empty = 0,
        Occupied = 1,
        Invalid = 2
    }

    public enum AdjacentTile
    {
        Above,
        Below,
        Right,
        Left
    }
    
    public class World : BaseObject
    {
        private static bool _isLoading;
        public static bool IsLoading { get { return _isLoading; } }

        private int _tileWidth;
        public int TileWidth { get { return _tileWidth; } }

        private int _tileHeight;
        public int TileHeight { get { return _tileHeight; } }
        
        public int RenderRadiusInPixels { get { return TileWidth * Config.ChunkWidth * Config.RenderRadiusInChunks; } }

        private int _totalChunksX;
        public int TotalChunksX { get { return _totalChunksX; } }

        private int _totalChunksY;
        public int TotalChunksY { get { return _totalChunksY; } }
        
        private bool _generatingInitial;
        public bool Generating { get { return _generatingInitial; } }

        private int _seed;
        public int Seed { get { return _seed; } }
        
        private Player _player;
        public Player Player { get { return _player; } }
        
        private Random _random;
        public Random Random { get { return _random; } }

        private Perlin _perlin;
        public Perlin Perlin { get { return _perlin; } }

        private Progress<InitializeWorldProgressArgs> _initProgress;
        public Progress<InitializeWorldProgressArgs> InitProgress { get { return _initProgress; } }

        private WorldConfig _config;
        public WorldConfig Config { get { return _config; } }

        public event Action OnInitialGenerationComplete = delegate { };
        public event Action OnContinuousGenerationComplete = delegate { };
        
        private Dictionary<TileId, Tile> _tiles = new Dictionary<TileId, Tile>();
        private TileDataGenerator _tdg;

        private Dictionary<TileId, Color[,]> _usedTiles = new Dictionary<TileId, Color[,]>();
        private List<Point> _chunkCorners = new List<Point>();
        private ChunkNode[,] _chunkGrid;
        private Color[,] _chunkColors;

        private Region[] _regions;
        private List<WorldObjectDef> _worldObjects = new List<WorldObjectDef>();
        private List<DrawableObject> _outFaders = new List<DrawableObject>();
        private List<DrawableObject> _inFaders = new List<DrawableObject>();

        private int _initialGenerationRoutineCount;
        
        private Vector2 _generatedCenter;
        private Chunk _playerChunk;
        private Timer _timer;
        private int _generationTime;
        private int _chunksGenerated;
        private bool _preGenerationInitRunning;

        private Task _initTask;
        private int _widthInTiles;
        private int _heightInTiles;

        private const float MaxLoadingBarWidth = 800;
        private const float LoadingBarHeight = 15;

        private AnimatedSprite _loadingBackground;
        private Sprite _loadingBar;
        private NineSlicedSprite _loadingBarStroke;
        private TextField _loadingText;

        public World(int seed, int widthInTiles, int heightInTiles, int tileWidth, int tileHeight, Player player)
        {
            _seed = seed;
            _player = player;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _widthInTiles = widthInTiles;
            _heightInTiles = heightInTiles;
            OnInitialGenerationComplete += OnInitialGenerationCompleteHandler;
            OnContinuousGenerationComplete += OnContinuousGenerationCompleteHandler;
            _initProgress = new Progress<InitializeWorldProgressArgs>();
            InitProgress.ProgressChanged += OnProgressChanged;
            Reload();
        }

        private void OnProgressChanged(object sender, InitializeWorldProgressArgs e)
        {
            _loadingBar.Scale = new Vector2(MaxLoadingBarWidth * ((float)e.Percentage / 100), LoadingBarHeight-1);
            _loadingText.Text = string.Format("Generating {0}... {1} %", e.RegionId, e.Percentage);
        }

        public void Reload()
        {
            if (IsLoading)
                throw new Exception("Another world is being generated already");

            CreateLoadingScreen();
            
            DestroyWorld();

            XmlSerializer serializer = new XmlSerializer(typeof(WorldConfig));
            using (FileStream stream = new FileStream(string.Format("Content/{0}.xml", Strings.Content.WorldConfigXML), FileMode.Open))
            {
                _config = (WorldConfig)serializer.Deserialize(stream);
            }
            
            if (Config == null)
                throw new Exception("Failed to deserialize world config file");

            Name = "World";

            _random = new Random(_seed);
            _perlin = new Perlin();
            _perlin.Seed = Seed;
            
            _totalChunksX = _widthInTiles / Config.ChunkWidth;
            _totalChunksY = _heightInTiles / Config.ChunkHeight;

            _chunkGrid = new ChunkNode[_totalChunksX, _totalChunksY];
            _chunkColors = new Color[Config.ChunkWidth * TileWidth, Config.ChunkHeight * TileHeight];

            /* Create regions */
            int regionCount = Enum.GetNames(typeof(RegionId)).Length;
            float regionPercentage = 1.0f / regionCount;
            float percentage = 0.0f;
            _regions = new Region[regionCount];

            int chunksInHeightPerRegion = TotalChunksY / regionCount;
            int chunksInHeightCount = 0;
            for (int i = 0; i < regionCount; i++, percentage += regionPercentage)
            {
                if (i == (regionCount - 1))
                    chunksInHeightPerRegion = TotalChunksY - chunksInHeightCount;
                _regions[i] = new Region(this, (RegionId)i, TotalChunksX, chunksInHeightPerRegion, chunksInHeightCount, percentage);
                chunksInHeightCount += chunksInHeightPerRegion;
            }

            /* Create TDG */
            _tdg = new TileDataGenerator(this);

            /* Create tiles */
            _tiles = TilesLoader.LoadTiles();

            
            _initTask = Task.Run(() =>
            {
                InitializeWorld(_initProgress);
            });

            Coroutines.Run(WaitForInitialization());
        }

        private void CreateLoadingScreen()
        {
            _loadingBackground = new AnimatedSprite(Strings.Content.Textures.WhiteTexture1PNG);
            _loadingBackground.Color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            _loadingBackground.DrawingSpace = DrawingSpace.Screen;
            _loadingBackground.Scale = new Vector2(
                MGTK.Instance.Graphics.PreferredBackBufferWidth / _loadingBackground.Texture.Width,
                MGTK.Instance.Graphics.PreferredBackBufferHeight / _loadingBackground.Texture.Height);
            _loadingBackground.DrawOrder = int.MaxValue - 2;

            _loadingText = new TextField("0 %", Strings.Content.Fonts.UIFontBigSPRITEFONT);
            _loadingText.SamplerState = SamplerState.LinearClamp;
            _loadingText.DrawingSpace = DrawingSpace.Screen;
            _loadingText.DrawOrder = int.MaxValue - 1;

            _loadingBar = new Sprite(Strings.Content.Textures.WhiteTexture1PNG);
            _loadingBar.DrawingSpace = DrawingSpace.Screen;
            _loadingBar.Scale = new Vector2(1, LoadingBarHeight-1);
            _loadingBar.Position = new Vector2((MGTK.Instance.Graphics.PreferredBackBufferWidth - MaxLoadingBarWidth) / 2, 
                MGTK.Instance.Graphics.PreferredBackBufferHeight / 1.3f);
            _loadingBar.DrawOrder = int.MaxValue - 1;

            NineSliceData nineSliceData = new NineSliceData(1, 1, 1, 1);
            _loadingBarStroke = new NineSlicedSprite(Strings.Content.NineSlicePNG, nineSliceData);
            _loadingBarStroke.DrawingSpace = DrawingSpace.Screen;
            _loadingBarStroke.Position = _loadingBar.Position;
            _loadingBarStroke.DrawOrder = int.MaxValue;
            _loadingBarStroke.Scale = new Vector2(MaxLoadingBarWidth, LoadingBarHeight) / 4;

            _loadingText.Position += new Vector2(MGTK.Instance.Graphics.PreferredBackBufferWidth / 2, (MGTK.Instance.Graphics.PreferredBackBufferHeight / 1.4f));
        }

        private void DestroyLoadingScreen()
        {
            _loadingBarStroke.Destroy();
            _loadingBar.Destroy();
            _loadingBackground.Destroy();
            _loadingText.Destroy();
        }

        public void DestroyWorld()
        {
            Coroutines.StopAll();
            if (_regions != null)
            {
                for (int i = 0; i < _regions.Length; i++)
                {
                    _regions[i].Destroy();
                }
            }

            if (_worldObjects != null)
            {
                for (int i = 0; i < _worldObjects.Count; i++)
                {
                    _worldObjects[i].Sprite.DestroyFromWorld();
                }
            }

            _regions = null;
            _worldObjects.Clear();
            _outFaders.Clear();
            _inFaders.Clear();
            _initialGenerationRoutineCount = 0;
            _generatedCenter = Player.Position;
            _generationTime = 0;
            _chunksGenerated = 0;
            _usedTiles.Clear();
            _chunkCorners.Clear();
            _chunkGrid = null;
            _chunkColors = null;
            
        }

        private void InitializeWorld(IProgress<InitializeWorldProgressArgs> progress)
        {
            _isLoading = true;
            _preGenerationInitRunning = true;
            _generationTime = 0;
            _timer = new Timer(UpdateTimer, null, 0, 1000);
            
            /* Create chunk data using perlin noise (not the actual sprites) */
            float loadingPercentage = 0;
            RegionId lastId = _regions[0].Id;
            foreach (Region region in _regions)
            {
                string regionName = region.Id.ToString();
                for (int y = region.offsetY; y < region.offsetY + region.ChunksLengthY; y++)
                {
                    for (int x = 0; x < region.ChunksLengthX; x++)
                    {
                        loadingPercentage += ((1.0f / (_regions.Length * region.ChunksLengthY * region.ChunksLengthX)) * 100);
                        progress.Report(new InitializeWorldProgressArgs(region.Id, (int)loadingPercentage));
                        _tdg.GenerateTileDataForChunk(region, x, y);
                    }
                }
                lastId = region.Id;
            }

            progress.Report(new InitializeWorldProgressArgs(lastId, 100));
            _preGenerationInitRunning = false;
        }

        private IEnumerator WaitForInitialization()
        {
            while(_preGenerationInitRunning)
            {
                yield return null; 
            }

            int startX = (int)_player.Position.X / TileWidth / Config.ChunkWidth;
            int startY = (int)_player.Position.Y / TileHeight / Config.ChunkHeight;

            startX = startX > _totalChunksX ? _totalChunksX : startX;
            startY = startY > _totalChunksY ? _totalChunksY : startY;

            Coroutines.Run(GenerateInitial(startX, startY));
        }

        private IEnumerator GenerateInitial(int startX, int startY)
        {
            _preGenerationInitRunning = false;
            _generatingInitial = true;
            while (true)
            {
                if (!IsFillable(startX, startY) || (!IsNearPlayer(startX, startY) && !Config.ShowWholeWorld))
                    yield break;

                _initialGenerationRoutineCount++;

                int left = startX - 1;
                int right = startX + 1;
                int up = startY + 1;
                int down = startY - 1;

                CreateChunk(startX, startY);
                CreateChunk(startX, down);
                CreateChunk(right, startY);
                CreateChunk(startX, up);
                CreateChunk(left, startY);

                yield return null;

                int halfWidth = _totalChunksX / 2;
                int halfHeight = _totalChunksY / 2;

                Coroutines.Run(GenerateInitial(left, up), "Initial");
                Coroutines.Run(GenerateInitial(right, up), "Initial");
                Coroutines.Run(GenerateInitial(left, down), "Initial");
                Coroutines.Run(GenerateInitial(right, down), "Initial");

                _initialGenerationRoutineCount--;
            }
        }
        
        public Region GetRegion(RegionId id)
        {
            return _regions.FirstOrDefault(x => x.Id == id);
        }

        private void UpdateTimer(object obj)
        {
            _generationTime++;
        }

        private void OnContinuousGenerationCompleteHandler()
        {
            StoreCornerChunks();
        }

        private void OnInitialGenerationCompleteHandler()
        {
            _isLoading = false;
            DestroyLoadingScreen();
            Console.WriteLine(string.Format("World generated in {0} seconds.", _generationTime));
            Coroutines.MaxRoutinesConcurrent = int.MaxValue;
            Coroutines.Run(UpdatePlayerChunk(), "UpdatePlayerChunk");
            if (!Config.ShowWholeWorld)
                Coroutines.Run(GenerateContinuous(), "Continuous");
        }

        private IEnumerator UpdatePlayerChunk()
        {
            float t = 0.0f;
            while(true)
            {
                yield return null;
                t += Time.DeltaTime;
                if (t > Config.UpdatePlayerChunkEveryXSeconds)
                {
                    Point chunkPos = GetChunkPosition(_player.Position);
                    _playerChunk = GetRegion(chunkPos.Y).GetChunk(chunkPos);
                    t = 0.0f;
                }
            }
        }

        public bool FlipCoin(float chancePercentage)
        {
            return chancePercentage > (_random.NextDouble() * 100.0f);
        }

        private bool IsFillable(int chunkX, int chunkY)
        {
            bool inBounds = (chunkX < TotalChunksX && chunkY < TotalChunksY && chunkX >= 0 && chunkY >= 0);
            if (!inBounds)
                return false;
            ChunkNode node = _chunkGrid[chunkX, chunkY];
            return node == ChunkNode.Empty;
        }

        private bool IsNearPlayer(int chunkX, int chunkY)
        {
            return (Vector2.Distance(_player.Position, GetWorldPosition(chunkX, chunkY)) <= RenderRadiusInPixels);
        }

        public Vector2 GetWorldPosition(int chunkX, int chunkY)
        {
            return new Vector2(chunkX * Config.ChunkWidth * TileWidth, chunkY * Config.ChunkHeight * TileHeight);
        }

        private Point GetChunkPosition(Vector2 worldPosition)
        {
            float x = worldPosition.X / TileWidth / Config.ChunkWidth;
            float y = worldPosition.Y / TileHeight / Config.ChunkHeight;
            return new Point((int)x, (int)y);
        }

        private Region GetRegion(int chunkY)
        {
            for (int i = 0; i < _regions.Length; i++)
            {
                Region region = _regions[i];
                if((region.offsetY + region.ChunksLengthY) > chunkY)
                {
                    return region;
                }
            }
            return null;
        }

        private void SortWorldObjects()
        {
            _worldObjects = _worldObjects.OrderByDescending(x => x.WorldPosition.Y).ToList();
        }

        private void CreateWorldObjects()
        {
            for (int i = 0; i < _worldObjects.Count; i++)
            {
                WorldObjectDef obj = _worldObjects[i];
                obj.Sprite.Visible = true;
                obj.Sprite.SourceRect = obj.SourceRect;
                obj.Sprite.Scale = obj.Scale;
                obj.Sprite.Rotation = obj.Rotation;
                obj.Sprite.Position = obj.WorldPosition;
                obj.Sprite.DrawOrder = (int)obj.DrawOrder;
                obj.Sprite.Tag = obj.Tag;

                if (obj.WocData != null)
                {
                    obj.Sprite.EnablePhysics(obj.WocData.BodyType, obj.WocData.Shape, obj.WocData.Offset);
                    obj.Sprite.PhysicsBody.IsSensor = obj.WocData.IsSensor;
                    obj.Sprite.PhysicsBody.CollisionCategories = (Category)obj.WocData.Layer;
                    obj.Sprite.PhysicsBody.CollidesWith = (Category)obj.WocData.CollidesWith;
                    if (obj.Sprite.Tag == (byte)Tags.OpaqueSensor)
                    {
                        obj.Sprite.PhysicsBody.OnCollision += OnFaderContact;
                        obj.Sprite.PhysicsBody.OnSeparation += OnFaderSeperate;
                    }
                }

                _worldObjects[i] = obj;
            }
        }
        
        private bool OnFaderContact(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            BaseObject other = (BaseObject)fixtureB.UserData;
            if(other.Tag == (byte)Tags.Player)
            {
                WorldObject fader = (WorldObject)fixtureA.UserData;
                if (_inFaders.Contains(fader))
                    _inFaders.Remove(fader);
                if (!_outFaders.Contains(fader))
                    _outFaders.Add(fader);
            }
            return true;
        }

        private void OnFaderSeperate(Fixture fixtureA, Fixture fixtureB)
        {
            BaseObject other = (BaseObject)fixtureB.UserData;
            if (other.Tag == (byte)Tags.Player)
            {
                DrawableObject fader = (DrawableObject)fixtureA.UserData;
                if (_outFaders.Contains(fader))
                    _outFaders.Remove(fader);
                if (!_inFaders.Contains(fader))
                    _inFaders.Add(fader);
            }
        }

        /// <summary>
        /// Only makes sure the world object won't spawn again.
        /// This function will not destroy the actual object. (Use WorldObject.DestroyFromWorld() instead).
        /// </summary>
        /// <param name="wo"></param>
        public void DestroyWorldObject(WorldObject wo)
        {
            foreach(Region region in _regions)
            {
                foreach(Chunk chunk in region.chunks)
                {
                    for (int i = 0; i < chunk.Data.Objects.Count; i++)
                    {
                        if(chunk.Data.Objects[i].Guid == wo.Guid)
                        {
                            _worldObjects.Remove(chunk.Data.Objects[i]);
                            chunk.Data.Objects.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        private void CreateChunk(int chunkOffsetX, int chunkOffsetY)
        {
            if(!IsFillable(chunkOffsetX, chunkOffsetY))
                return;

            Region region = GetRegion(chunkOffsetY);
            Chunk chunk = region.GetChunk(new Point(chunkOffsetX, chunkOffsetY));

            /* Prepare world objects for creation */
            for (int i = 0; i < chunk.Data.Objects.Count; i++)
            {
                WorldObjectDef obj = chunk.Data.Objects[i];
                WorldObject sprite = new WorldObject(obj.Texture, obj.Guid, this);
                sprite.Visible = false;
                obj.Sprite = sprite;
                chunk.Data.Objects[i] = obj;
                _worldObjects.Add(chunk.Data.Objects[i]);

            }

            /* Create tiles */
            for (int y = 0; y < Config.ChunkHeight; y++)
            {
                for (int x = 0; x < Config.ChunkWidth; x++)
                {
                    TileId mask = chunk.Data.Tiles[x, y];
                    Color[,] newColors = new Color[TileWidth, TileHeight];
                    bool newColorsCreated = false;

                    foreach (TileId value in Enum.GetValues(mask.GetType()))
                    {
                        if (mask.HasFlag(value))
                        {
                            TileId id = value;
                            Tile tile = _tiles[id];
                            Texture2D tex = Content.Load<Texture2D>(tile.Texture);

                            if (_usedTiles.ContainsKey(mask))
                            {
                                Color[,] usedColors = _usedTiles[mask];
                                for (int cy = 0; cy < usedColors.GetLength(1); cy++)
                                {
                                    for (int cx = 0; cx < usedColors.GetLength(0); cx++)
                                    {
                                        _chunkColors[(x * TileWidth) + cx, (y * TileHeight) + cy] = usedColors[cx, cy];
                                    }
                                }
                            }
                            else
                            {
                                if (tile.SourceRect.Width > TileWidth || tile.SourceRect.Height > TileHeight)
                                    throw new Exception(string.Format("Tile '{0}' is too big too fit the tile. You can either make it smaller or create it as a WorldObject instead.", id.ToString()));

                                Color[] texColors = new Color[TileWidth * TileHeight];
                                tex.GetData(0, tile.SourceRect, texColors, 0, (tile.SourceRect.Width * tile.SourceRect.Height));

                                for (int ty = 0; ty < tile.SourceRect.Height; ty++)
                                {
                                    for (int tx = 0; tx < tile.SourceRect.Width; tx++)
                                    {
                                        Color c = texColors[ty * tile.SourceRect.Height + tx];
                                        if (c == Color.Magenta || c.A == 0)
                                            continue;

                                        int cx = (x * TileWidth) + tx;
                                        int cy = (y * TileHeight) + ty;
                                        Color current = _chunkColors[cx, cy];

                                        /* Blend semi-transparant color with the color underneath */
                                        if(current != Color.Transparent && c.A < 255)
                                        {
                                            float amount = (c.A / 255.0f);
                                            byte r = (byte)((c.R * amount) + current.R * (1 - amount));
                                            byte g = (byte)((c.G * amount) + current.G * (1 - amount));
                                            byte b = (byte)((c.B * amount) + current.B * (1 - amount));
                                            c = new Color(r, g, b, 255);
                                        }
                                        _chunkColors[cx, cy] = c;
                                        newColors[tx, ty] = c;
                                    }
                                }
                                newColorsCreated = true;
                            }
                        }
                    }
                    if(newColorsCreated)
                        _usedTiles.Add(mask, newColors);
                }
            }

            Texture2D texture = new Texture2D(GraphicsDevice, _chunkColors.GetLength(0), _chunkColors.GetLength(1), false, SurfaceFormat.Color);
            texture.SetData(_chunkColors.To1DArray());

            AnimatedSprite chunkSprite = new AnimatedSprite(texture);
            chunkSprite.Position = new Vector2(
                (texture.Width / 2) + (chunkOffsetX * TileWidth * Config.ChunkWidth),
                (texture.Height / 2) + (chunkOffsetY * TileHeight * Config.ChunkHeight));
            chunkSprite.DrawOrder = (int)DrawOrders.Tiles;
            chunk.Sprite = chunkSprite;
            
            _chunkGrid[chunkOffsetX, chunkOffsetY] = ChunkNode.Occupied;
            _chunksGenerated++;
        }
        
        private void DestroyChunk(int chunkX, int chunkY)
        {
            Region region = GetRegion(chunkY);
            Point chunkPoint = new Point(chunkX, chunkY);
            Chunk chunk = region.GetChunk(chunkPoint);
            
            if(chunk != null)
            {
                for (int i = 0; i < chunk.Data.Objects.Count; i++)
                {
                    WorldObjectDef obj = chunk.Data.Objects[i];
                    if(obj.Sprite != null)
                    {
                        _worldObjects.Remove(obj);
                        obj.Sprite.Destroy();
                        obj.Sprite = null;
                    }
                }

                _chunkGrid[chunkX, chunkY] = ChunkNode.Empty;
                _chunksGenerated--;

                if (chunk.Sprite != null)
                {
                    chunk.Sprite.Texture.Dispose();
                    chunk.Sprite.Destroy();
                    chunk.Sprite = null;
                }
            }
        }

        private IEnumerator GenerateContinuous()
        {
            Point chunkPoint = GetChunkPosition(_player.Position);
            while (true)
            {
                yield return null;
                if (Vector2.Distance(_player.Position, _generatedCenter) > ((float)RenderRadiusInPixels / 10))
                {
                    _generatedCenter = _player.Position;
                    Coroutines.Run(ProcessCorners(), "Corners");
                }
            }
        }

        private IEnumerator ProcessCorners()
        {
            for (int i = 0; i < _chunkCorners.Count; i++)
            {
                Point corner = _chunkCorners[i];

                yield return null;

                float d = Vector2.Distance(_player.Position, GetWorldPosition(corner.X, corner.Y));

                if (d <= RenderRadiusInPixels)
                {
                    CreateChunk(corner.X, corner.Y);
                }
                else
                {
                    ChunkNode node = GetNode(corner.X, corner.Y);
                    ChunkNode left = GetNode(corner.X - 1, corner.Y);
                    ChunkNode right = GetNode(corner.X + 1, corner.Y);
                    ChunkNode up = GetNode(corner.X, corner.Y + 1);
                    ChunkNode down = GetNode(corner.X, corner.Y - 1);

                    if (left == ChunkNode.Occupied)
                        DestroyChunk(corner.X - 1, corner.Y);
                    if (right == ChunkNode.Occupied)
                        DestroyChunk(corner.X + 1, corner.Y);
                    if (up == ChunkNode.Occupied)
                        DestroyChunk(corner.X, corner.Y + 1);
                    if (down == ChunkNode.Occupied)
                        DestroyChunk(corner.X, corner.Y - 1);
                }
            }

            SortWorldObjects();
            CreateWorldObjects();
            OnContinuousGenerationComplete.Invoke();
        }

        private ChunkNode GetNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _chunkGrid.GetLength(0) || y >= _chunkGrid.GetLength(1))
                return ChunkNode.Invalid;
            return _chunkGrid[x, y];
        }

        private void StoreCornerChunks()
        {
            Vector2 playerPos = _player.Position;
            _chunkCorners.Clear();

            Point playerChunk = GetChunkPosition(playerPos);

            int subX = Config.RenderRadiusInChunks + 4;
            int subY = Config.RenderRadiusInChunks + 4;

            int x0 = (playerChunk.X - subX) > 0 ? (playerChunk.X - subX) : 0;
            int y0 = (playerChunk.Y - subY) > 0 ? (playerChunk.Y - subY) : 0;

            for (int y = y0; y < y0 + (subY * 2); y++)
            {
                for (int x = x0; x < x0 + (subX * 2); x++)
                {
                    ChunkNode node = GetNode(x, y);
                    ChunkNode left = GetNode(x - 1, y);
                    ChunkNode right = GetNode(x + 1, y);
                    ChunkNode up = GetNode(x, y + 1);
                    ChunkNode down = GetNode(x, y - 1);
                    ChunkNode downLeft = GetNode(x + 1, y - 1);
                    ChunkNode downRight = GetNode(x - 1, y - 1);
                    ChunkNode upLeft = GetNode(x + 1, y + 1);
                    ChunkNode upRight = GetNode(x - 1, y + 1);

                    if (node == ChunkNode.Empty)
                    {
                        if (left == ChunkNode.Occupied || right == ChunkNode.Occupied ||
                            down == ChunkNode.Occupied || up == ChunkNode.Occupied ||
                            downLeft == ChunkNode.Occupied || downRight == ChunkNode.Occupied ||
                            upLeft == ChunkNode.Occupied || upRight == ChunkNode.Occupied)
                        {
                            _chunkCorners.Add(new Point(x, y));
                        }
                    }
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (_generatingInitial || _preGenerationInitRunning)
            {
                if (_initialGenerationRoutineCount == 0 && _generatingInitial)
                {
                    _timer.Dispose();
                    SortWorldObjects();
                    CreateWorldObjects();
                    OnInitialGenerationComplete.Invoke();
                    _generatingInitial = false;
                }
            }
            
            if (Config.WorldObjectFadingEnabled)
            {
                for (int i = 0; i < _inFaders.Count; i++)
                {
                    DrawableObject obj = _inFaders[i];
                    float delta = Time.DeltaTime * Config.WorldObjectFadeSpeed;
                    obj.Opacity = (obj.Opacity + delta) <= 1.0f ? (obj.Opacity + delta) : 1.0f;
                    if (obj.Opacity == 1.0f)
                    {
                        _inFaders.RemoveAt(i);
                    }
                }

                for (int i = 0; i < _outFaders.Count; i++)
                {
                    DrawableObject obj = _outFaders[i];
                    float delta = Time.DeltaTime * Config.WorldObjectFadeSpeed;
                    obj.Opacity = (obj.Opacity - delta) >= Config.WorldObjectFadedOpacity ? (obj.Opacity - delta) : Config.WorldObjectFadedOpacity;
                    if (obj.Opacity == Config.WorldObjectFadedOpacity)
                    {
                        _outFaders.RemoveAt(i);
                    }
                }
            }
/*

            if (Camera != null)
            {
                for (int i = 0; i < _chunks.Count; i++)
                {
                    Chunk chunk = _chunks[i];
                    Sprite sprite = chunk.Sprite;
                    int x = (int)sprite.Position.X;
                    int y = (int)sprite.Position.Y;
                    int width = (int)sprite.Size.X;
                    int height = (int)sprite.Size.Y;

                    Rectangle chunkRect = new Rectangle(x - (int)sprite.Origin.X, y - (int)sprite.Origin.Y, width, height);

                    if (!chunkRect.Intersects(Camera.VisibleArea))
                    {
                        sprite.Visible = false;
                    }
                    else
                    {
                        sprite.Visible = true;
                    }
                }
            }*/

            
        }

        protected override void DebugDraw(DebugDrawer drawer)
        {
            /*foreach(Point p in _chunkCorners)
            {
                drawer.DrawRect(GetWorldPosition(p.X, p.Y), new Vector2(ChunkWidth * TileWidth, ChunkHeight * TileHeight), Color.Red);   
            }*/
  
            if (!_generatingInitial)
            {
                
                foreach (Region region in _regions)
                {
                    drawer.DrawText(new Vector2(0, region.Height * TotalChunksY * Config.ChunkHeight * TileHeight), region.Id.ToString(), Color.Magenta, 20.0f);    
                    foreach (Chunk chunk in region.chunks)
                    {
                        if (chunk == null || !IsNearPlayer(chunk.Data.ChunkPosition.X, chunk.Data.ChunkPosition.Y))
                            continue;
                        drawer.DrawRect(new Vector2(
                            chunk.Data.ChunkPosition.X * Config.ChunkWidth * TileWidth,
                            chunk.Data.ChunkPosition.Y * Config.ChunkHeight * TileHeight), 
                            new Vector2(Config.ChunkWidth * TileWidth, Config.ChunkHeight * TileHeight), Color.Gray);
                    }
                }
            }
            /*for (int y = 0; y < TotalChunksY; y++)
            {
                for (int x = 0; x < TotalChunksX; x++)
                {
                    drawer.DrawText(GetWorldPosition(x, y), string.Format("x: {0}, y: {1}", x, y), Color.Black);
                }
            }*/
            drawer.DrawRect(new Rectangle(0, 0, Config.ChunkWidth * TileWidth * TotalChunksX, Config.ChunkHeight * TileHeight * TotalChunksY), Color.White);
        }
    }
}
