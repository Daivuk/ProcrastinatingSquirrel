using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;
using DK8;
using ProcrastinatingSquirrel.Entities;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;

// The snow field will be about 2km x 2km, and we spawn at the middle, 1km, 1km


namespace ProcrastinatingSquirrel
{
	class CSnowfield
	{
		public const int FIELD_SIZE = 1024;
		public const float TILE_SCALE = 128.0f;
		public const float INV_TILE_SCALE = 1.0f / TILE_SCALE;

		public const float COLLISION_EPSILON = .01f;

		public const float Z_GROUND = 1;
		public const float Z_SNOW = .9f;
		public const float Z_ENTITY_GROUND = .8f;
		public const float Z_ENTITY_TREE = .7f;
		public const float Z_EFFECT_WIND = .6f;
		public const float Z_ENTITY_AIR = .5f;

		static public Texture2D texSnow;
		static public Texture2D texSnowAlpha;
		static public Texture2D texGrass;
		static public Texture2D texIce;
		static public Texture2D texWater;
		static public Texture2D texSens;
		static public Texture2D texDig;
		static public Texture2D texCrack;
		static public Texture2D texHomeIcoBack;
		static public Texture2D texHomeIco;
		static public Texture2D texIsHomeIco;
		static public Texture2D texHomeInside;
		static public Texture2D texHomeInsideOver;
		static public Effect fxGround;
		static public Effect fxWater;
		static public Effect fxSens;
		static public Vector2 currentTopLeft;

		static SoundEffect s_sndItemPickup = CFrameData.Instance.Content.Load<SoundEffect>("sounds/itemPickup");
		static SoundEffect s_sndNutDeposit = CFrameData.Instance.Content.Load<SoundEffect>("sounds/nutDeposit");
		static SoundEffect s_sndSell = CFrameData.Instance.Content.Load<SoundEffect>("sounds/sell");
		static SoundEffect s_sndError = CFrameData.Instance.Content.Load<SoundEffect>("sounds/error");
		CSnowFlakeMgr m_snowFlakeMgr = new CSnowFlakeMgr();
		RenderTarget2D m_texSnowSplatter;
		RenderTarget2D m_texSnowSplatter2;
		public RenderTarget2D m_rtSens;
		Texture2D texHome;
		Vector2 m_homeSpritePos = new Vector2((float)FIELD_SIZE / 2 - 3.5f, (float)FIELD_SIZE / 2 - 3.5f);
		float m_homeSpriteScale = INV_TILE_SCALE * 2;
		List<Chunk> m_chunkPool = new List<Chunk>();
		Chunk[,] m_chunks;
		public void SetChunkAt(Chunk in_chunk, int in_x, int in_y)
		{
			m_chunks[in_x / Chunk.CHUNK_SIZE, in_y / Chunk.CHUNK_SIZE] = in_chunk;
		}
		float m_zoom = 1;
		static Vector2 m_homePos = new Vector2((float)FIELD_SIZE / 2, (float)FIELD_SIZE / 2);
		static Vector2 m_entranceSpawn = new Vector2((float)FIELD_SIZE / 2 + .0f, (float)FIELD_SIZE / 2 + 3.0f);
		static Vector2 m_exitSpawn = new Vector2(4, 6.5f);
		static Vector2 m_spawnPos = new Vector2(3 + .5f, 2 + .5f);
		Vector2 m_cameraPos = m_spawnPos;
		Vector2 m_cameraDst = m_spawnPos;
		static Rectangle m_homeExit = new Rectangle(3, 7, 2, 1);
		static Rectangle m_homeEntrance = new Rectangle(FIELD_SIZE / 2 - 1, FIELD_SIZE / 2 + 2, 1, 1);
		CAnimFloat m_teleportToInside = new CAnimFloat();
		CAnimFloat m_teleportToOutside = new CAnimFloat();
		CSquirrel m_squirrel = new CSquirrel();
		CAnimColor m_teleportFade = new CAnimColor(new Color(0, 0, 0, 0));
		CAnimVector2 m_depositNutsAnim = new CAnimVector2();
		int m_loadingTime = 0;
		public CSquirrel Squirrel
		{
			get { return m_squirrel; }
		}
		public bool IsSquirrelHome
		{
			get
			{
				return (m_squirrel.Position.X < 8 && m_squirrel.Position.Y < 8);
			}
		}
		CAnimStringBubble m_messageText = new CAnimStringBubble("game");
		CAnimColor m_messageFade = new CAnimColor("game");
		CAnimFloat m_waterAnim = new CAnimFloat("game");
		public class MsgIcon
		{
			public MsgIcon(IEntity in_entity)
			{
				entity = in_entity;
			}
			public IEntity entity;
			public Vector2 pos;
			public CAnimVector2 posAnim = new CAnimVector2("game");
			public CAnimColor color = new CAnimColor("game");
			public CAnimFloat scale = new CAnimFloat("game");
			public CAnimFloat sndTimer = new CAnimFloat("game");
		}
		List<MsgIcon> m_msgIcons = null;
		List<MsgIcon> m_depositIcons = null;
		public string WarningMessage
		{
			get
			{
				return m_messageText.Value;
			}
			set
			{
				if (m_messageText != null) m_messageText.KillAnims();
				m_messageText = new CAnimStringBubble("game", value);
				m_messageText.StartAnimFromCurrent(value, 1, 0, eAnimType.LINEAR);
				m_messageFade.StartAnim(new Color(255, 102, 0, 255), new Color(
					255, 102, 0, 0), 10, 0, eAnimType.EASE_IN);
				s_sndError.Play();
			}
		}
		public List<MsgIcon> MsgIcons
		{
			get
			{
				return m_msgIcons;
			}
			set
			{
				m_msgIcons = value;
				Vector2 leftPos = new Vector2(
					(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth * .5f -
						(float)(m_msgIcons.Count() - 1) * .5f * 56, 
					(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight * .25f + 32 + 24);
				float delay = 0;
				foreach (MsgIcon icon in m_msgIcons)
				{
					icon.pos = leftPos;
					leftPos.X += 56;
					delay += .20f;
					icon.scale.StartAnim(1, 1.5f, .15f, delay, eAnimType.EASE_OUT, eAnimFlag.PINGPONG);
					icon.sndTimer.StartAnim(1, 0, .1f, delay, eAnimType.LINEAR);
					icon.sndTimer.SetCallback(OnIconShowed, false);
					icon.color.StartAnim(Color.White, new Color(255, 255, 255, 0), 1.5f, delay, eAnimType.EASE_IN);
				}
			}
		}
		public List<MsgIcon> DepositIcons
		{
			get
			{
				return m_depositIcons;
			}
			set
			{
				m_depositIcons = value;
				float delay = 0;
				foreach (MsgIcon icon in m_depositIcons)
				{
					icon.posAnim.StartAnim(new Vector2(2.0f, 4.5f), new Vector2(1.5f, 5.5f), .5f, delay, eAnimType.EASE_IN);
					delay += .15f;
					icon.scale.Value = 1;
					icon.sndTimer.StartAnim(1, 0, .5f, delay, eAnimType.LINEAR);
					icon.sndTimer.UserData = icon.entity;
					if (icon == m_depositIcons.Last())
					{
						icon.sndTimer.SetCallback(OnSell, false);
					}
					else
					{
						icon.sndTimer.SetCallback(OnIconDeposit, false);
					}
					icon.color.Value = Color.White;
				}
			}
		}
		public string InfoMessage
		{
			get
			{
				return m_messageText.Value;
			}
			set
			{
				if (m_messageText != null) m_messageText.KillAnims();
				m_messageText = new CAnimStringBubble("game", value);
				m_messageText.StartAnimFromCurrent(value, .5f, 0, eAnimType.LINEAR);
				m_messageFade.StartAnim(Globals.TextColor, new Color(
					Globals.TextColor.R, Globals.TextColor.G, Globals.TextColor.B, (byte)0), 2, 0, eAnimType.EASE_IN);
			}
		}
		static public CSnowfield Instance = null;
		float m_snowAmount = 1.25f;
		CAnimFloat m_sensAnim = new CAnimFloat("game");
		public Vector2 HomePos { get { return m_homePos; } }

		public void OnIconShowed(IAnimatable in_anim)
		{
			s_sndItemPickup.Play(1, 0, 0);
		}

		public void OnIconDeposit(IAnimatable in_anim)
		{
			if (dontPlayDepositSnds) s_sndNutDeposit.Play(1, 0, 0);
			int value = (in_anim.UserData as IEntity).Value;
			Inventory.Instance.KuiCash = Inventory.Instance.KuiCash + value;
			Globals.TotalNutCollected += value;
		}

		public void OnSell(IAnimatable in_anim)
		{
			s_sndSell.Play(1, 0, 0);
			int value = (in_anim.UserData as IEntity).Value;
			Inventory.Instance.KuiCash = Inventory.Instance.KuiCash + value;
			Globals.TotalNutCollected += value;
		}

        List<Chunk> preLoadingChunks = new List<Chunk>();
        int timeStart;
        int totalLoadingTimeStart = System.Environment.TickCount;

        public CSnowfield()
        {
            totalLoadingTimeStart = System.Environment.TickCount;

            Instance = this;
            CFrameData fd = CFrameData.Instance;

            timeStart = System.Environment.TickCount;

            // Load some resources
            texSnow = fd.Content.Load<Texture2D>("textures/snow");
            texSnowAlpha = fd.Content.Load<Texture2D>("textures/snowAlpha");
            texGrass = fd.Content.Load<Texture2D>("textures/grass");
            texIce = fd.Content.Load<Texture2D>("textures/ice");
            texWater = fd.Content.Load<Texture2D>("textures/water");
            texSens = fd.Content.Load<Texture2D>("textures/Sens");
            texDig = fd.Content.Load<Texture2D>("textures/SnowDig");
            texCrack = fd.Content.Load<Texture2D>("textures/crack");
            texHomeIcoBack = fd.Content.Load<Texture2D>("textures/homeBack");
            texHomeIco = fd.Content.Load<Texture2D>("textures/homeHouse");
            texIsHomeIco = fd.Content.Load<Texture2D>("textures/isHome");
            texHome = fd.Content.Load<Texture2D>("textures/home");
            texHomeInside = fd.Content.Load<Texture2D>("textures/homeInside");
            texHomeInsideOver = fd.Content.Load<Texture2D>("textures/homeInsideOver");
            fxGround = fd.Content.Load<Effect>("effects/Ground");
            fxWater = fd.Content.Load<Effect>("effects/Water");
            fxSens = fd.Content.Load<Effect>("effects/Sens");
            m_rtSens = new RenderTarget2D(fd.Graphics.GraphicsDevice, fd.Graphics.PreferredBackBufferWidth,
                fd.Graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            int ResourcesTime = System.Environment.TickCount - timeStart;

            m_waterAnim.StartAnim(0, 1, 10, 0, eAnimType.LINEAR, eAnimFlag.LOOP);
            m_sensAnim.StartAnim(0, 1, 8, 0, eAnimType.LINEAR, eAnimFlag.LOOP);
            m_depositNutsAnim.StartAnim(new Vector2(.5f, 5.5f), new Vector2(.25f, 5.5f), .5f, 0, eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);

            fd.Randomize();
            m_seed = fd.Random.Next();

            // Load snowfield and inventory and squirrel
            List<Store.StoreItem> storeItems = Store.Instance.StoreItems;
            foreach (Store.StoreItem storeItem in storeItems)
            {
                storeItem.currentItem = storeItem.items.First();
            }
            Globals.TotalNutCollected = 0;
            try
            {
                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                // Check here
                string filename = "Snowfield_" + Profile.Instance.CurrentSaveName + ".sav";
                if (container.FileExists(filename))
                {
                    int version = 0;
                    // Load snowfield info, like seed.
                    BinaryReader fic_in = new BinaryReader(container.OpenFile(filename, FileMode.Open, FileAccess.Read));
                    version = fic_in.ReadInt32();
                    switch (version)
                    {
                        case 1:
                            m_seed = fic_in.ReadInt32();
                            Globals.TotalNutCollected = fic_in.ReadInt32();
                            if (Globals.TotalNutCollected >= Globals.NUT_GOAL) Globals.TotalNutCollected = 0;
                            break;
                        case 2:
                            m_seed = fic_in.ReadInt32();
                            Globals.TotalNutCollected = fic_in.ReadInt32();
                            if (Globals.TotalNutCollected >= Globals.NUT_GOAL) Globals.TotalNutCollected = 0;
                            break;
                        default:
                            m_seed = version;
                            break;
                    }
                    Inventory.Instance.Load(fic_in);
                    if (version >= 2) m_squirrel.Load(fic_in); // Probably nothing to load here

                    fic_in.Close();
                }

                container.Dispose();
            }
            catch
            {
                CFrameData.Instance.WhoTheHellRemoveStorageDevice();
            }

            fd.SetRandomSeed(m_seed);
            Globals.Random = new RandAndNoise(m_seed);
            m_squirrel.Position = m_spawnPos;


            // Instanciate the tiles
            timeStart = System.Environment.TickCount;
            int i;
            m_chunks = new Chunk[FIELD_SIZE / Chunk.CHUNK_SIZE, FIELD_SIZE / Chunk.CHUNK_SIZE];
            for (i = 0; i < Chunk.CHUNK_POOL_SIZE; ++i)
            {
                m_chunkPool.Add(new Chunk());
            }

            // Create the home chunk, and always keep it loaded
            //List<Chunk> preLoadingChunks = new List<Chunk>();
            Chunk chunk = GetFreeChunk();
            chunk.AlwaysKeep = true;
            chunk.Load(0, 0);
            preLoadingChunks.Add(chunk);

            // Load the 4 chunks at the middle (Home pos) so when we go out the house it's already there
            chunk = GetFreeChunk();
            chunk.Load(
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE - 1) * Chunk.CHUNK_SIZE,
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE - 1) * Chunk.CHUNK_SIZE);
            preLoadingChunks.Add(chunk);
            chunk = GetFreeChunk();
            chunk.Load(
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE,
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE - 1) * Chunk.CHUNK_SIZE);
            preLoadingChunks.Add(chunk);
            chunk = GetFreeChunk();
            chunk.Load(
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE - 1) * Chunk.CHUNK_SIZE,
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE);
            preLoadingChunks.Add(chunk);
            chunk = GetFreeChunk();
            chunk.Load(
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE,
                ((FIELD_SIZE / 2) / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE);
            preLoadingChunks.Add(chunk);

            return;
        }

        bool loaded = false;

        public void CSnowfield2()
        { 
			while (true)
			{
				int loadedCount = 0;
				foreach (Chunk preLoadingChunk in preLoadingChunks)
				{
					if (!preLoadingChunk.IsLoaded) break;
					++loadedCount;
				}
				if (loadedCount == preLoadingChunks.Count()) break;
                //System.Diagnostics.Debug.WriteLine("Loaded count: " + loadedCount);
                System.Threading.Thread.Sleep(10);
                return; // We keep loading
            }
            loaded = true;

            foreach (Chunk preLoadingChunk in preLoadingChunks)
			{
				preLoadingChunk.FinalizeLoad();
			}
			int InitTilesTime = System.Environment.TickCount - timeStart;
			timeStart = System.Environment.TickCount;

            // Create the snow splatter texture
            CFrameData fd = CFrameData.Instance;
            m_texSnowSplatter = new RenderTarget2D(fd.Graphics.GraphicsDevice, FIELD_SIZE, FIELD_SIZE, false, SurfaceFormat.Color, DepthFormat.None);
			fd.Graphics.GraphicsDevice.SetRenderTarget(m_texSnowSplatter);
			fd.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
			fd.SpriteBatch.Draw(fd.CommonResources.Tex_White, new Rectangle(0, 0, FIELD_SIZE, FIELD_SIZE), Color.Black);
			fd.SpriteBatch.End();
			fd.Graphics.GraphicsDevice.SetRenderTarget(null);
			m_texSnowSplatter2 = new RenderTarget2D(fd.Graphics.GraphicsDevice, FIELD_SIZE, FIELD_SIZE, false, SurfaceFormat.Color, DepthFormat.None);
			fd.Graphics.GraphicsDevice.SetRenderTarget(m_texSnowSplatter2);
			fd.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
			fd.SpriteBatch.Draw(fd.CommonResources.Tex_White, new Rectangle(0, 0, FIELD_SIZE, FIELD_SIZE), Color.Black);
			fd.SpriteBatch.End();
			fd.Graphics.GraphicsDevice.SetRenderTarget(null);

			// Make home tiles unpassable
			GetTileAt(FIELD_SIZE / 2 - 1, FIELD_SIZE / 2 - 1).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2, FIELD_SIZE / 2 - 1).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2 + 1, FIELD_SIZE / 2 - 1).IsPassable = false;

			GetTileAt(FIELD_SIZE / 2 - 1, FIELD_SIZE / 2).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2, FIELD_SIZE / 2).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2 + 1, FIELD_SIZE / 2).IsPassable = false;

			GetTileAt(FIELD_SIZE / 2 - 1, FIELD_SIZE / 2 + 1).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2, FIELD_SIZE / 2 + 1).IsPassable = false;
			GetTileAt(FIELD_SIZE / 2 + 1, FIELD_SIZE / 2 + 1).IsPassable = false;

			// Place store items into the house
	//		List<Store.StoreItem> storeItems = Store.Instance.StoreItems;
			Point itemPos = new Point(5, 2);
            List<Store.StoreItem> storeItems = Store.Instance.StoreItems;
            foreach (Store.StoreItem storeItem in storeItems)
			{
			//	storeItem.currentItem = storeItem.items.First();
				CTile tile = GetTileAt(itemPos.X, itemPos.Y);
				tile.Entity = new CStoreItem(storeItem);
				tile.Entity.Position += new Vector2(.5f, -.5f);
				itemPos.Y += 1;
			}

			Globals.WindAnims[0].StartAnim(.25f, -1, 3.0f, 0,
				eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
			Globals.WindAnims[1].StartAnim(.25f, -1, 3.0f, 1.5f,
				eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
			Globals.WindAnims[2].StartAnim(.25f, -1, 3.0f, 3.0f,
				eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
			Globals.WindAnims[3].StartAnim(.25f, -1, 3.0f, 4.5f,
				eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);


            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            m_loadingTime = System.Environment.TickCount - totalLoadingTimeStart;
		}

		private Chunk GetFreeChunk()
		{
			Chunk winner = null;
			float winnerDis = 0;
			float disToSquirrel = 0;
			Vector2 chunkPos = Vector2.Zero;
			Vector2 squirrelPos = m_squirrel.Position;
			if (IsSquirrelHome)
			{
				squirrelPos = HomePos;
			}
			foreach (Chunk chunk in m_chunkPool)
			{
				if (chunk.IsBeingLoaded || chunk.AlwaysKeep) continue;
				if (!chunk.IsLoaded) return chunk;
				chunkPos.X = (float)chunk.X + (float)(Chunk.CHUNK_SIZE / 2);
				chunkPos.Y = (float)chunk.Y + (float)(Chunk.CHUNK_SIZE / 2);
				disToSquirrel = Vector2.DistanceSquared(chunkPos, squirrelPos);
				if (disToSquirrel > winnerDis)
				{
					winnerDis = disToSquirrel;
					winner = chunk;
				}
			}
			return winner;
		}


		public bool IsDugAt(Vector2 digPos)
		{
			int x = (int)digPos.X;
			int y = (int)digPos.Y;

			if (x < 0) return true;
			if (y < 0) return true;
			if (x >= FIELD_SIZE) return true;
			if (y >= FIELD_SIZE) return true;

			CTile tile = GetTileAt(x, y);
			if (tile == null) return false;

			return tile.IsDug;
		}


		public bool CanDigAt(Vector2 digPos, IEntity digger)
		{
			int x = (int)digPos.X;
			int y = (int)digPos.Y;
			int diggerX = (int)digger.Position.X;
			int diggerY = (int)digger.Position.Y;

			if (x < 0) return false;
			if (y < 0) return false;
			if (x >= FIELD_SIZE) return false;
			if (y >= FIELD_SIZE) return false;

			CTile tile = GetTileAt(x, y);
			if (tile == null) return false;

			if (tile.IsDug) return false;

			if (tile.Entity != null)
			{
				if (!tile.Entity.CanDig) return false;
			}

			CTile tileBlockedX = GetTileAt(diggerX, y);
			CTile tileBlockedY = GetTileAt(x, diggerY);

			if (tileBlockedX == null || tileBlockedY == null) return false;

			return !(!tileBlockedX.IsDug && !tileBlockedY.IsDug);
		}


		public void DigAt(Vector2 digPos, IEntity digger)
		{
			int x = (int)digPos.X;
			int y = (int)digPos.Y;

			if (x < 0) return;
			if (y < 0) return;
			if (x >= FIELD_SIZE) return;
			if (y >= FIELD_SIZE) return;

			CTile tile = GetTileAt(x, y);

			if (tile.IsDug) return;

			tile.Dig(digger);

			if (tile.IsDug)
			{
				for (int j = y - 1; j <= y + 1; ++j)
				{
					for (int i = x - 1; i <= x + 1; ++i)
					{
						RefreshDig(i, j);
					}
				}

				// Trigger serounding entities
				tile = GetTileAt(x - 1, y);
				if (tile != null) tile.Trigger(digger, x, y);
				tile = GetTileAt(x + 1, y);
				if (tile != null) tile.Trigger(digger, x, y);
				tile = GetTileAt(x, y - 1);
				if (tile != null) tile.Trigger(digger, x, y);
				tile = GetTileAt(x, y + 1);
				if (tile != null) tile.Trigger(digger, x, y);
			}
		}


		bool IsNeighborDug(int x, int y)
		{
			if (x < 0) return false;
			if (y < 0) return false;
			if (x >= FIELD_SIZE) return false;
			if (y >= FIELD_SIZE) return false;
			CTile tile = GetTileAt(x, y);
			if (tile == null) return false;
			return tile.IsDug;
		}

		struct SCollisionSegment
		{
			public Vector2 p1;
			public Vector2 p2;
			public Vector2 normal;
		}


		const int MAX_COLLISIONS_SEGMENTS = 20;
		SCollisionSegment[] m_collisionSegments = new SCollisionSegment[MAX_COLLISIONS_SEGMENTS];
		int m_nbCollisionSegments = 0;
		private int m_seed = 100;


		void AddCollisionSegment(Vector2 p1, Vector2 p2, float radius)
		{
			if (m_nbCollisionSegments >= MAX_COLLISIONS_SEGMENTS) return;

			Vector2 u = p2 - p1;
			u.Normalize();

			m_collisionSegments[m_nbCollisionSegments].p1 = p1 - u * radius * .3f;
			m_collisionSegments[m_nbCollisionSegments].p2 = p2 + u * radius * .3f;
			Vector3 normal = Vector3.Cross(
				new Vector3(p2 - p1, 0), Vector3.UnitZ);
			normal.Normalize();
			m_collisionSegments[m_nbCollisionSegments].normal = new Vector2(normal.X, normal.Y);


			m_nbCollisionSegments++;
		}


		void RenderCollisionSegments()
		{
			for (int i = 0; i < m_nbCollisionSegments; ++i)
			{
				RenderLine(
					m_collisionSegments[i].p1,
					m_collisionSegments[i].p2,
					Color.BlueViolet);
				RenderLine(
					(m_collisionSegments[i].p1 + m_collisionSegments[i].p2) * .5f,
					(m_collisionSegments[i].p1 + m_collisionSegments[i].p2) * .5f + 
						m_collisionSegments[i].normal * .35f,
					Color.Magenta);
			}
		}


		void RenderLine(Vector2 p1, Vector2 p2, Color color)
		{
			float angle = -(float)Math.Atan2((double)(p2.X - p1.X), (double)(p2.Y - p1.Y));
			float len = (p2 - p1).Length();

			CFrameData.Instance.SpriteBatch.Draw(
				CFrameData.Instance.CommonResources.Tex_White,
				p1, null, color, angle, Vector2.Zero, new Vector2(.035f, len), SpriteEffects.None, 0);
		}

		public bool IsTilePassable(int x, int y)
		{
			CTile tile = GetTileAt(x, y);
			if (tile == null) return false;
			return tile.IsPassable;
		}


		public bool DoCollisions(ref Vector2 p1, ref Vector2 p2, float radius)
		{
			// Only check the 8 tiles around us
			int x = (int)p2.X;
			int y = (int)p2.Y;
			if (x < 1) x = 1;
			if (y < 1) y = 1;
			if (x > FIELD_SIZE - 2) x = FIELD_SIZE - 2;
			if (y > FIELD_SIZE - 2) y = FIELD_SIZE - 2;

			bool result = false;
			m_nbCollisionSegments = 0;
			Vector2 myTilePos = new Vector2((float)x, (float)y);

			// First, setup our collision segments
			if (!IsTilePassable(x - 1, y))
			{
				AddCollisionSegment(myTilePos, myTilePos + Vector2.UnitY, radius);
			}
			if (!IsTilePassable(x + 1, y))
			{
				AddCollisionSegment(myTilePos + Vector2.UnitY + Vector2.UnitX, myTilePos + Vector2.UnitX, radius);
			}
			if (!IsTilePassable(x, y - 1))
			{
				AddCollisionSegment(myTilePos + Vector2.UnitX, myTilePos, radius);
			}
			if (!IsTilePassable(x, y + 1))
			{
				AddCollisionSegment(myTilePos + Vector2.UnitY, myTilePos + Vector2.UnitX + Vector2.UnitY, radius);
			}
			if (!IsTilePassable(x - 1, y - 1))
			{
				if (IsTilePassable(x, y - 1))
				{
					AddCollisionSegment(myTilePos - Vector2.UnitY, myTilePos, radius);
				}
				if (IsTilePassable(x - 1, y))
				{
					AddCollisionSegment(myTilePos, myTilePos - Vector2.UnitX, radius);
				}
			}
			if (!IsTilePassable(x - 1, y + 1))
			{
				if (IsTilePassable(x, y + 1))
				{
					AddCollisionSegment(myTilePos + Vector2.UnitY, myTilePos + Vector2.UnitY * 2, radius);
				}
				if (IsTilePassable(x - 1, y))
				{
					AddCollisionSegment(myTilePos - Vector2.UnitX + Vector2.UnitY, myTilePos + Vector2.UnitY, radius);
				}
			}
			if (!IsTilePassable(x + 1, y - 1))
			{
				if (IsTilePassable(x, y - 1))
				{
					AddCollisionSegment(myTilePos + Vector2.UnitX, myTilePos - Vector2.UnitY + Vector2.UnitX, radius);
				}
				if (IsTilePassable(x + 1, y))
				{
					AddCollisionSegment(myTilePos + Vector2.UnitX * 2, myTilePos + Vector2.UnitX, radius);
				}
			}
			if (!IsTilePassable(x + 1, y + 1))
			{
				if (IsTilePassable(x, y + 1))
				{
					AddCollisionSegment(myTilePos + Vector2.UnitX + Vector2.UnitY * 2, myTilePos + Vector2.UnitX + Vector2.UnitY, radius);
				}
				if (IsTilePassable(x + 1, y))
				{
					AddCollisionSegment(myTilePos + Vector2.UnitX + Vector2.UnitY, myTilePos + Vector2.UnitX * 2 + Vector2.UnitY, radius);
				}
			}

			// Perform collision on those segments!
			Vector2 rayP1;
			Vector2 rayP2;
			float d1;
			float d2;
			float d;
			for (int i = 0; i < m_nbCollisionSegments; ++i)
			{
				rayP1 = p1 - m_collisionSegments[i].normal * radius;
				rayP2 = p2 - m_collisionSegments[i].normal * radius;

				d = -Vector2.Dot(m_collisionSegments[i].p1, m_collisionSegments[i].normal);
				d1 = Vector2.Dot(rayP1, m_collisionSegments[i].normal) + d;
				d2 = Vector2.Dot(rayP2, m_collisionSegments[i].normal) + d;

				if (d2 <= 0 && d1 >= -COLLISION_EPSILON)
				{
					rayP2 = rayP2 + m_collisionSegments[i].normal * -d2;

					// Are we in the bounds?
					if (PointOnSegment(rayP2, m_collisionSegments[i].p1, m_collisionSegments[i].p2))
					{
						// We have a collision! I repeat: we have a collision!
						result = true;
						p2 = rayP2 + m_collisionSegments[i].normal * radius;
						continue;
					}
				}
			}

			if (!result)
			{
				Vector2 point;
				Vector2 u = p2 - p1;
				u.Normalize();

				// We need to test against the points now. Circle to points collision
				for (int i = 0; i < m_nbCollisionSegments; ++i)
				{
					if (Vector2.Dot(m_collisionSegments[i].normal, u) >= 0) continue;
					for (int j = 0; j < 2; ++j)
					{
						if (j == 0) point = m_collisionSegments[i].p1;
						else point = m_collisionSegments[i].p2;

						if ((point - p2).LengthSquared() < radius * radius)
						{
							u = p2 - point;
							u.Normalize();
							p2 = point + u * radius;
							result = true;
							return true;
						}
					}
				}
			}

			return result;
		}


		bool PointOnSegment(Vector2 p, Vector2 p1, Vector2 p2)
		{
			if (p1.X < p2.X)
			{
				if (p.X >= p1.X && p.X <= p2.X) return true;
			}
			else if (p1.X > p2.X)
			{
				if (p.X <= p1.X && p.X >= p2.X) return true;
			}
			if (p1.Y < p2.Y)
			{
				if (p.Y >= p1.Y && p.Y <= p2.Y) return true;
			}
			else if (p1.Y > p2.Y)
			{
				if (p.Y <= p1.Y && p.Y >= p2.Y) return true;
			}

			return false;
		}


		static Rectangle[] s_digSrcRects = new Rectangle[]{
			new Rectangle(64, 832, 128, 128),
			new Rectangle(0, 0, 128, 128),new Rectangle(128, 0, 128, 128),new Rectangle(256, 0, 128, 128),
			new Rectangle(384, 0, 128, 128),new Rectangle(320, 832, 128, 128),new Rectangle(640, 0, 128, 128),
			new Rectangle(768, 0, 128, 128),new Rectangle(896, 0, 128, 128),new Rectangle(0, 128, 128, 128),
			new Rectangle(128, 128, 128, 128),new Rectangle(256, 128, 128, 128),new Rectangle(384, 128, 128, 128),
			new Rectangle(512, 128, 128, 128),new Rectangle(640, 128, 128, 128),new Rectangle(768, 128, 128, 128),
			new Rectangle(896, 128, 128, 128),new Rectangle(0, 256, 128, 128),new Rectangle(128, 256, 128, 128),
			new Rectangle(256, 256, 128, 128),new Rectangle(384, 256, 128, 128),new Rectangle(512, 256, 128, 128),
			new Rectangle(640, 256, 128, 128),new Rectangle(768, 256, 128, 128),new Rectangle(896, 256, 128, 128),
			new Rectangle(0, 384, 128, 128),new Rectangle(128, 384, 128, 128),new Rectangle(256, 384, 128, 128),
			new Rectangle(384, 384, 128, 128),new Rectangle(512, 384, 128, 128),new Rectangle(640, 384, 128, 128),
			new Rectangle(768, 384, 128, 128),new Rectangle(896, 384, 128, 128),new Rectangle(0, 512, 128, 128),
			new Rectangle(128, 512, 128, 128),new Rectangle(256, 512, 128, 128),new Rectangle(384, 512, 128, 128),
			new Rectangle(512, 512, 128, 128),new Rectangle(640, 512, 128, 128),new Rectangle(768, 512, 128, 128),
			new Rectangle(896, 512, 128, 128),new Rectangle(0, 640, 128, 128),new Rectangle(128, 640, 128, 128),
			new Rectangle(256, 640, 128, 128),new Rectangle(384, 640, 128, 128),new Rectangle(512, 640, 128, 128),
			new Rectangle(640, 640, 128, 128),new Rectangle(768, 640, 128, 128),
		};

		static Rectangle inflated = Rectangle.Empty;
		static bool[] nbs = new bool[8];


		public void RefreshDig(int x, int y)
		{
			CTile tile = GetTileAt(x, y);
			if (tile == null) return;
			if (!tile.IsDug)
			{
				tile.DigSrcRect = s_digSrcRects[0];
			}
			else
			{
				nbs[0] = IsNeighborDug(x - 1, y - 1);
				nbs[1] = IsNeighborDug(x, y - 1);
				nbs[2] = IsNeighborDug(x + 1, y - 1);
				nbs[3] = IsNeighborDug(x - 1, y);
				nbs[4] = IsNeighborDug(x + 1, y);
				nbs[5] = IsNeighborDug(x - 1, y + 1);
				nbs[6] = IsNeighborDug(x, y + 1); 
				nbs[7] = IsNeighborDug(x + 1, y + 1);

				if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == true /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[1];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[2];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == true && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[3];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == true && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[4];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[5];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[6];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[7];
				}
				else if (/*nbs[0] == true && */nbs[1] == true && nbs[2] == true &&
					nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[8];
				}


				else if (/*nbs[0] == false &&*/ nbs[1] == true &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == true /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[9];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[10];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == true && nbs[4] == false &&
					nbs[5] == true && nbs[6] == true/* && nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[11];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[12];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
						 nbs[3] == true && nbs[4] == false &&
					nbs[5] == false && nbs[6] == true/* && nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[13];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[14];
				}
				else if (nbs[0] == false && nbs[1] == true &&/* nbs[2] == true &&*/
					nbs[3] == true && nbs[4] == false &&
					nbs[5] == true && nbs[6] == true/* && nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[15];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[16];
				}


				else if (/*nbs[0] == false &&*/ nbs[1] == true &&/* nbs[2] == false &&*/
						 nbs[3] == false && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[17];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == true && nbs[2] == true &&
						 nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[18];
				}
				else if (nbs[0] == true && nbs[1] == true && /*nbs[2] == true &&*/
						 nbs[3] == true && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[19];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == true && nbs[2] == false &&
						 nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[20];
				}
				else if (nbs[0] == false && nbs[1] == true && /*nbs[2] == true &&*/
						 nbs[3] == true && nbs[4] == false &&
					/*nbs[5] == false &&*/ nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[21];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[22];
				}
				else if (nbs[0] == true && nbs[1] == true &&/* nbs[2] == true &&*/
					nbs[3] == true && nbs[4] == false &&
					nbs[5] == false && nbs[6] == true/* && nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[23];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false/* && nbs[2] == false*/ &&
					nbs[3] == false && nbs[4] == false &&
					/*nbs[5] == false && */nbs[6] == false /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[24];
				}


				else if (/*nbs[0] == false &&*/ nbs[1] == true && nbs[2] == false &&
					nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == false && */nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[25];
				}
				else if (nbs[0] == false && nbs[1] == true && /*nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == false &&
					nbs[5] == false && nbs[6] == true /*&& nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[26];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					/*nbs[5] == false &&*/ nbs[6] == false/* && nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[27];
				}
				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[28];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[29];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[30];
				}
				else if (/*nbs[0] == false && */nbs[1] == true && nbs[2] == false &&
					nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == true &&*/ nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[31];
				}
				else if (nbs[0] == true && nbs[1] == true &&/* nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == false &&
					nbs[5] == true && nbs[6] == true/* && nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[32];
				}


				else if (/*nbs[0] == true &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[33];
				}
				else if (/*nbs[0] == true &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[34];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					/*nbs[5] == true && */nbs[6] == false/* && nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[35];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					/*nbs[5] == true && */nbs[6] == false/* && nbs[7] == false*/)
				{
					tile.DigSrcRect = s_digSrcRects[36];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[37];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[38];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[39];
				}
				else if (/*nbs[0] == false && */nbs[1] == true && nbs[2] == true &&
					nbs[3] == false && nbs[4] == true &&
					/*nbs[5] == true && */nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[40];
				}


				else if (/*nbs[0] == false &&*/ nbs[1] == false &&/* nbs[2] == false &&*/
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[41];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					/*nbs[5] == true && */nbs[6] == false /*&& nbs[7] == true*/)
				{
					tile.DigSrcRect = s_digSrcRects[42];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[43];
				}
				else if (nbs[0] == true && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[44];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == true &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[45];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == false && nbs[6] == true && nbs[7] == true)
				{
					tile.DigSrcRect = s_digSrcRects[46];
				}
				else if (nbs[0] == false && nbs[1] == true && nbs[2] == false &&
					nbs[3] == true && nbs[4] == true &&
					nbs[5] == true && nbs[6] == true && nbs[7] == false)
				{
					tile.DigSrcRect = s_digSrcRects[47];
				}
				inflated = tile.DigSrcRect;
				inflated.Inflate(-2, -2);
				tile.DigSrcRect = inflated;
			}
		}


		public void AddNut(CTile tile, int x, int y)
		{
			if (!(tile.Type == eTILE_TYPE.TILE_SNOW ||
				tile.Type == eTILE_TYPE.TILE_ICE)) return;

			// Depending on the distance from home, calculate the chances to have a great nut
			float disFromHome = Vector2.Distance(new Vector2((float)x, (float)y), m_homePos);
			float maxDis = (float)FIELD_SIZE * .5f - /*epsilon*/32;
			float percent = disFromHome / maxDis;

			int centerValue = (int)MathHelper.Lerp(1, CNut.maxValue, percent);
			int value = CFrameData.Instance.Random.Next(100);

			// Nuts distribution
			// 3%  centerValue - 2
			// 17% centerValue - 1
			// 60% centerValue
			// 17% centerValue + 1
			// 3%  centerValue + 2
			if (value < 60) value = centerValue;
			else if (value < 60 + 17) value = centerValue - 1;
			else if (value < 60 + 17 + 3) value = centerValue - 2;
			else if (value < 60 + 17 + 3 + 17) value = centerValue + 1;
			else value = centerValue + 2;

			if (tile.Type == eTILE_TYPE.TILE_ICE)
			{
				// Better chances to get higher rated nuts in ICE
				if (CFrameData.Instance.Random.Next(100) < 30)
				{
					value++;
				}
			}
			// Even higher chances to get higher rated nuts in snow storms

			if (value < 1) value = 1;
			if (value > CNut.maxValue) value = CNut.maxValue;

			tile.Entity = new CNut(value);
			tile.IsPassable = true;
		}


		int Density(int nbPer, int squareUnit)
		{
			return (int)(((float)(FIELD_SIZE * FIELD_SIZE) / (float)(squareUnit * squareUnit)) * (float)nbPer);
		}

		bool dontPlayDepositSnds = false;
		public void OnTeleportOutside(IAnimatable in_anim)
		{
			m_cameraPos = m_cameraDst = m_squirrel.Position = m_entranceSpawn;

			// Finish all nuts deposing animation, so we don't lose moneyz
			if (m_depositIcons != null)
			{
				dontPlayDepositSnds = true;
				foreach (MsgIcon icon in m_depositIcons)
				{
					if (icon.sndTimer.IsPlaying)
					{
						icon.posAnim.Stop();
						icon.sndTimer.Stop(true);
					}
				}
				dontPlayDepositSnds = false;
			}

			// Always save when exiting the hut
			Game1.GameState = Game1.eGAME_STATE.LOADING;
			LoadingScreen.Instance.StartLoading("Saving");
			WorkerThread.Instance.AddWork(Game1.Instance.SaveAndContinue);
		}
		public void OnTeleportInside(IAnimatable in_anim)
		{
			m_cameraPos = m_cameraDst = m_squirrel.Position = m_exitSpawn;
			m_cameraPos.Y = 4;
			m_messageFade.Stop();
			m_messageText.Stop();
		}

		Point m_squirrelPosi = Point.Zero;
		public void Update()
		{
            if (!loaded)
            {
                CSnowfield2();
                if (!loaded) return;
            }
			CFrameData fd = CFrameData.Instance;
			Vector2 oldCamPos = m_cameraPos;
			Rectangle updateRect = Rectangle.Empty;
			int i, j;
			updateRect.X = (int)(m_cameraPos.X - ((float)fd.Graphics.PreferredBackBufferWidth * .5f / (TILE_SCALE * m_zoom))) - 1;
			updateRect.Y = (int)(m_cameraPos.Y - ((float)fd.Graphics.PreferredBackBufferHeight * .5f / (TILE_SCALE * m_zoom))) - 1;
			updateRect.Width = ((int)(m_cameraPos.X + ((float)fd.Graphics.PreferredBackBufferWidth * .5f / (TILE_SCALE * m_zoom))) + 2) - updateRect.X;
			updateRect.Height = ((int)(m_cameraPos.Y + ((float)fd.Graphics.PreferredBackBufferHeight * .5f / (TILE_SCALE * m_zoom))) + 2) - updateRect.Y;
		//	updateRect.Width = (int)((float)(fd.Graphics.PreferredBackBufferWidth) / (TILE_SCALE * m_zoom)) + 20;
		//	updateRect.Height = (int)((float)(fd.Graphics.PreferredBackBufferHeight) / (TILE_SCALE * m_zoom)) + 20;

			if (updateRect.Left < 0) updateRect.X = 0;
			if (updateRect.Top < 0) updateRect.Y = 0;
			if (updateRect.Right >= FIELD_SIZE) updateRect.Width = (FIELD_SIZE - updateRect.Left);
			if (updateRect.Bottom >= FIELD_SIZE) updateRect.Height = (FIELD_SIZE - updateRect.Top);

			// Check if we don't need to finalize a Chunk in the pool
			foreach (Chunk chunk in m_chunkPool)
			{
				if (chunk.IsLoaded && chunk.RequiresFinalizing)
				{
					chunk.FinalizeLoad();
					break; // Once per frame is enough
				}
			}

			if (!m_teleportFade.IsPlaying)
			{
				m_squirrel.Update();
				m_squirrelPosi.X = (int)(m_squirrel.Position.X);
				m_squirrelPosi.Y = (int)(m_squirrel.Position.Y);

				// Check if we don't get to a teleport (For the home)
				if (m_homeExit.Contains(m_squirrelPosi))
				{
					m_teleportToOutside.StartAnim(0, 1, .25f, 0, eAnimType.LINEAR);
					m_teleportToOutside.SetCallback(OnTeleportOutside, false);
					m_teleportFade.StartAnim(Color.Transparent, Color.Black, .25f, 0, eAnimType.LINEAR, eAnimFlag.PINGPONG);
				}
				else
				{
					m_squirrelPosi.X = (int)(m_squirrel.Position.X - .5f);
					m_squirrelPosi.Y = (int)(m_squirrel.Position.Y + .5f);
					if (m_homeEntrance.Contains(m_squirrelPosi))
					{
						m_teleportToInside.StartAnim(0, 1, .25f, 0, eAnimType.LINEAR);
						m_teleportToInside.SetCallback(OnTeleportInside, false);
						m_teleportFade.StartAnim(Color.Transparent, Color.Black, .25f, 0, eAnimType.LINEAR, eAnimFlag.PINGPONG);
					}
				}

				m_cameraDst = m_squirrel.Position;
			}

#if DEBUG
			m_zoom += CFrameData.Instance.InputMgr.GamePadState.ThumbSticks.Right.Y * CFrameData.Instance.GetDeltaSecond();
#endif

			if (m_zoom < .05f) m_zoom = .05f;
			if (m_zoom > .65f) m_zoom = .65f;

            // Animate camera
            if (!m_teleportFade.IsPlaying)
			{
				m_cameraPos = m_cameraPos + (m_cameraDst - m_cameraPos) * CFrameData.Instance.GetDeltaSecond() * 2;
			//	m_cameraPos.X += fd.InputMgr.GamePadState.ThumbSticks.Left.X * 50.0f * CFrameData.Instance.GetDeltaSecond();
			//	m_cameraPos.Y -= fd.InputMgr.GamePadState.ThumbSticks.Left.Y * 50.0f * CFrameData.Instance.GetDeltaSecond();
			}

			if (m_cameraPos.X < 0) m_cameraPos.X = 0;
			if (m_cameraPos.Y < 0) m_cameraPos.Y = 0;
			if (m_cameraPos.X > (float)FIELD_SIZE) m_cameraPos.X = (float)FIELD_SIZE;
			if (m_cameraPos.Y > (float)FIELD_SIZE) m_cameraPos.Y = (float)FIELD_SIZE;

			if (IsSquirrelHome && !m_teleportFade.IsPlaying)
			{
				m_cameraPos.Y = 4;// = new Vector2(m_cameraPos.X, 4);
				// If we get near the stash, let's sell all our stuff
				if (m_squirrel.Position.X < 2.5f &&
					m_squirrel.Position.Y > 4.5f && m_squirrel.Position.Y < 6.5f)
				{
					Inventory.Instance.SellAllItems();
				}
			}

			// Update entities that are in the view
			CTile tile;
			for (j = updateRect.Top; j < updateRect.Bottom; ++j)
			{
				for (i = updateRect.Left; i < updateRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					if (tile.Entity != null)
					{
						tile.Entity.Update();
					}
				}
			}

			// Always try to load the chunks around the player
			if (!IsSquirrelHome)
			{
				int chunkX = (int)m_squirrel.Position.X / Chunk.CHUNK_SIZE;
				int chunkY = (int)m_squirrel.Position.Y / Chunk.CHUNK_SIZE;
			//	int chunkX = (int)m_cameraPos.X / Chunk.CHUNK_SIZE;
			//	int chunkY = (int)m_cameraPos.Y / Chunk.CHUNK_SIZE;
				for (j = chunkY - 1; j <= chunkY + 1; ++j)
				{
					for (i = chunkX - 1; i <= chunkX + 1; ++i)
					{
						Chunk chunk = GetChunkAt(i * Chunk.CHUNK_SIZE, j * Chunk.CHUNK_SIZE);
						if (chunk == null)
						{
							if (i * Chunk.CHUNK_SIZE >= 1024 || i * Chunk.CHUNK_SIZE < 0 ||
								j * Chunk.CHUNK_SIZE >= 1024 || j * Chunk.CHUNK_SIZE < 0) continue;
							chunk = GetFreeChunk();
							if (chunk != null)
							{
								chunk.Load(i * Chunk.CHUNK_SIZE, j * Chunk.CHUNK_SIZE);
								continue;
							}
							continue; // No more free chunks available? doh :|
						}
						if (chunk.IsLoaded || chunk.IsBeingLoaded) continue;
						chunk.Load(i * Chunk.CHUNK_SIZE, j * Chunk.CHUNK_SIZE);
					}
				}
			}

			m_snowFlakeMgr.Move(-((m_cameraPos - oldCamPos) * m_zoom * TILE_SCALE) * 1.25f);
			m_snowFlakeMgr.Update();
		}

		Rectangle m_fieldRect = new Rectangle(0, 0, FIELD_SIZE, FIELD_SIZE);
		public Vector2 m_screenDim = new Vector2(
			(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth, 
			(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight);
		public void Render()
		{
            if (!loaded) return;
			SpriteBatch sb = CFrameData.Instance.SpriteBatch;
			CFrameData fd = CFrameData.Instance;

			// Create the snow splatter texture
			foreach (Chunk chunk in m_chunkPool)
			{
				if (chunk.NeedToUpdateSplatter)
				{
					chunk.NeedToUpdateSplatter = false;
					fd.Graphics.GraphicsDevice.SetRenderTarget(m_texSnowSplatter);
					Rectangle dstRect = Rectangle.Empty;
					dstRect.Width = 3;
					dstRect.Height = 3;
					Rectangle dstRectWater = Rectangle.Empty;
					dstRectWater.Width = 1;
					dstRectWater.Height = 1;
					fd.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
					fd.SpriteBatch.Draw(m_texSnowSplatter2, m_fieldRect, Color.White);
					for (int j = chunk.Y; j < chunk.Y + Chunk.CHUNK_SIZE; ++j)
					{
						for (int i = chunk.X; i < chunk.X + Chunk.CHUNK_SIZE; ++i)
						{
							if (GetTileAt(i, j).Type == eTILE_TYPE.TILE_SNOW)
							{
								dstRect.X = i - 2;
								dstRect.Y = j - 2;
								fd.SpriteBatch.Draw(fd.CommonResources.Tex_White, dstRect, Color.Red);
							}
							else if (GetTileAt(i, j).Type == eTILE_TYPE.TILE_WATER)
							{
								dstRectWater.X = i - 1;
								dstRectWater.Y = j - 1;
								fd.SpriteBatch.Draw(fd.CommonResources.Tex_White, dstRectWater, Color.Lime);
							}
						}
					}
					fd.SpriteBatch.End();
					fd.Graphics.GraphicsDevice.SetRenderTarget(null);

					// Now copy it into our second texture, so we can recopy it later
					fd.Graphics.GraphicsDevice.SetRenderTarget(m_texSnowSplatter2);
					fd.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
					fd.SpriteBatch.Draw(m_texSnowSplatter, m_fieldRect, Color.White);
					fd.SpriteBatch.End();
					fd.Graphics.GraphicsDevice.SetRenderTarget(null);
					break; // One per frame
				}
			}

			Matrix camMat = Matrix.Identity;
			camMat *= Matrix.CreateScale(TILE_SCALE * m_zoom);
			camMat.Translation = new Vector3(
				-m_cameraPos.X * TILE_SCALE * m_zoom + (float)fd.Graphics.PreferredBackBufferWidth * .5f,
				-m_cameraPos.Y * TILE_SCALE * m_zoom + (float)fd.Graphics.PreferredBackBufferHeight * .5f,
				0);
			Rectangle renderRect = Rectangle.Empty;
			renderRect.X = (int)(m_cameraPos.X - ((float)fd.Graphics.PreferredBackBufferWidth * .5f / (TILE_SCALE * m_zoom))) - 1;
			renderRect.Y = (int)(m_cameraPos.Y - ((float)fd.Graphics.PreferredBackBufferHeight * .5f / (TILE_SCALE * m_zoom))) - 1;
			renderRect.Width = ((int)(m_cameraPos.X + ((float)fd.Graphics.PreferredBackBufferWidth * .5f / (TILE_SCALE * m_zoom))) + 2) - renderRect.X;
			renderRect.Height = ((int)(m_cameraPos.Y + ((float)fd.Graphics.PreferredBackBufferHeight * .5f / (TILE_SCALE * m_zoom))) + 2) - renderRect.Y;
		//	renderRect.Width = (int)((float)(fd.Graphics.PreferredBackBufferWidth) / (TILE_SCALE * m_zoom)) + 20;
		//	renderRect.Height = (int)((float)(fd.Graphics.PreferredBackBufferHeight) / (TILE_SCALE * m_zoom)) + 20;

			if (renderRect.Left < 0) renderRect.X = 0;
			if (renderRect.Top < 0) renderRect.Y = 0;
			if (renderRect.Right >= FIELD_SIZE) renderRect.Width = (FIELD_SIZE - renderRect.Left);
			if (renderRect.Bottom >= FIELD_SIZE) renderRect.Height = (FIELD_SIZE - renderRect.Top);


			// Sens blurry vision for nuts and some predators
			fd.Graphics.GraphicsDevice.SetRenderTarget(m_rtSens);
			CFrameData.Instance.Graphics.GraphicsDevice.Clear(Color.Transparent);
			sb.Begin(
				SpriteSortMode.Deferred, BlendState.NonPremultiplied,
				SamplerState.LinearWrap, DepthStencilState.None,
				RasterizerState.CullNone, null, camMat);
			CTile tile;
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					if (tile.Entity != null)
					{
						tile.Entity.RenderSens();
					}
				}
			}
			sb.End();
			fd.Graphics.GraphicsDevice.SetRenderTarget(null);
			

			fxGround.CurrentTechnique = fxGround.Techniques[0];
			fxGround.Parameters["SnowSplatter"].SetValue(m_texSnowSplatter);
			fxGround.Parameters["SnowTexture"].SetValue(texSnowAlpha);
			fxGround.Parameters["InvSplatterSize"].SetValue(1.0f / (float)FIELD_SIZE);
			fxGround.Parameters["snowAmount"].SetValue(m_snowAmount);
			currentTopLeft.X = (float)renderRect.Left / (float)FIELD_SIZE;
			currentTopLeft.Y = (float)renderRect.Top / (float)FIELD_SIZE;
			fxGround.Parameters["CurSplatterUV"].SetValue(currentTopLeft);
			sb.Begin(
				SpriteSortMode.Texture, BlendState.NonPremultiplied,
				SamplerState.LinearWrap, DepthStencilState.None,
				RasterizerState.CullNone, fxGround, camMat);
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					tile.Render();
				}
			}
			sb.End();

			fxWater.CurrentTechnique = fxWater.Techniques[0];
			fxWater.Parameters["SnowSplatter"].SetValue(m_texSnowSplatter);
			fxWater.Parameters["TexSens"].SetValue(texSens);
			fxWater.Parameters["InvSplatterSize"].SetValue(1.0f / (float)FIELD_SIZE);
			currentTopLeft.X = (float)renderRect.Left / (float)FIELD_SIZE;
			currentTopLeft.Y = (float)renderRect.Top / (float)FIELD_SIZE;
			fxWater.Parameters["CurSplatterUV"].SetValue(currentTopLeft);
			fxWater.Parameters["WaterAnim"].SetValue(m_waterAnim.Value);
			sb.Begin(
				SpriteSortMode.Deferred, BlendState.NonPremultiplied,
				SamplerState.LinearWrap, DepthStencilState.None,
				RasterizerState.CullNone, fxWater, camMat);
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					if (tile.Type != eTILE_TYPE.TILE_WATER) continue;
					tile.RenderWater();
				}
			}
			sb.End();

			sb.Begin(
				SpriteSortMode.Deferred, BlendState.NonPremultiplied,
				SamplerState.LinearWrap, DepthStencilState.None,
				RasterizerState.CullNone, null, camMat);
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					if (tile.Entity != null)
					{
						tile.Entity.RenderUnder();
					}
				}
			}

			// Dug snow
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					tile.RenderDig();
				}
			}
			sb.End();

			// Draw sens with the sens effect yea :D
			fxSens.CurrentTechnique = fxSens.Techniques[0];
			fxSens.Parameters["squirrelPosition"].SetValue(Squirrel.Position);
			fxSens.Parameters["sensTopLeftPosition"].SetValue(
					(m_cameraPos * (TILE_SCALE * m_zoom)) / m_screenDim);
			fxSens.Parameters["topLeftPosition"].SetValue(
					m_cameraPos - (new Vector2((float)fd.Graphics.PreferredBackBufferWidth * .5f,
					(float)fd.Graphics.PreferredBackBufferHeight * .5f) / (TILE_SCALE * m_zoom)));
			fxSens.Parameters["bottomRightPosition"].SetValue(
					m_cameraPos + (new Vector2((float)fd.Graphics.PreferredBackBufferWidth * .5f,
					(float)fd.Graphics.PreferredBackBufferHeight * .5f) / (TILE_SCALE * m_zoom)));
			fxSens.Parameters["sensRadius"].SetValue(m_squirrel.SensRadius);
			fxSens.Parameters["TexSens"].SetValue(m_rtSens);
			fxSens.Parameters["sensOffset"].SetValue(m_sensAnim.Value);
			sb.Begin(
				SpriteSortMode.Immediate, BlendState.NonPremultiplied,
				SamplerState.LinearWrap, DepthStencilState.None,
				RasterizerState.CullNone, fxSens);
			sb.Draw(texSens, fd.Graphics.GraphicsDevice.Viewport.Bounds, null, Color.White);
			sb.End();

			// Draw Home and squirrel
			sb.Begin(
				SpriteSortMode.Deferred, BlendState.NonPremultiplied, 
				SamplerState.LinearWrap, DepthStencilState.None, 
				RasterizerState.CullNone, null, camMat);

			// Inside the house, in a corner
			sb.Draw(fd.CommonResources.Tex_White, m_houseInsidePos, null, Color.Black, 0, Vector2.Zero, 32, SpriteEffects.None, 0);
			sb.Draw(texHomeInside, Vector2.Zero, null, Color.White, 0, Vector2.Zero, INV_TILE_SCALE, SpriteEffects.None, 0);

			// Render entities
			for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
			{
				for (int i = renderRect.Left; i < renderRect.Right; ++i)
				{
					tile = GetTileAt(i, j);
					if (tile == null) continue;
					if (tile.Entity != null)
					{
						tile.Entity.Render();
					}
				}
			}

			if (m_squirrel.Position.Y <= (float)FIELD_SIZE / 2 + .5f)
			{
				m_squirrel.Render();
			}

			sb.Draw(texHome, m_homeSpritePos, null, Color.White, 0, Vector2.Zero, m_homeSpriteScale, SpriteEffects.None, 0);

			// Draw squirrel
			if (m_squirrel.Position.Y > (float)FIELD_SIZE / 2 + .5f)
			{
				m_squirrel.Render();
			}

			sb.Draw(texHomeInsideOver, Vector2.Zero, null, Color.White, 0, Vector2.Zero, INV_TILE_SCALE, SpriteEffects.None, 0);

			// Draw collisions segments
			//RenderCollisionSegments();
			sb.End();

			// Draw the layered stuff like the trees
			Vector2 offset = Vector2.Zero;
			for (int layer = 0; layer <= 5; ++layer)
			{
				sb.Begin(
					SpriteSortMode.Deferred, BlendState.NonPremultiplied, 
					SamplerState.LinearWrap, DepthStencilState.None, 
					RasterizerState.CullNone, null, camMat);
					for (int j = renderRect.Top; j < renderRect.Bottom; ++j)
					{
						for (int i = renderRect.Left; i < renderRect.Right; ++i)
						{
							if (layer <= 1)
							{
								offset.X = 0;
								offset.Y = 0;
							}
							else
							{
								offset.X = (float)i - m_cameraPos.X;
								offset.Y = (float)j - m_cameraPos.Y;
								offset *= .035f * (float)(layer - 1) * m_zoom;
							}
							tile = GetTileAt(i, j);
							if (tile == null) continue;
							if (tile.Entity != null)
							{
								tile.Entity.RenderLayer(layer, ref offset);
							}
						}
					}
					offset.X = (float)m_squirrel.Position.X - m_cameraPos.X;
					offset.Y = (float)m_squirrel.Position.Y - m_cameraPos.Y;
					offset *= .035f * (float)(layer - 1) * m_zoom;
					m_squirrel.RenderLayer(layer, ref offset);
				sb.End();
			}

			// Draw the weather
			m_snowFlakeMgr.Render();

			//------------ Draw the hud -------------
			// Draw the home icon
			if (Vector2.DistanceSquared(m_cameraPos, m_homePos) >= 6 * 6 &&
				m_squirrel.Position.X > 8 && m_squirrel.Position.Y > 8)
			{
				Vector2 dir = m_homePos - m_cameraPos;
				dir.Normalize();
				float angle = (float)Math.Atan2((double)dir.Y, (double)dir.X);

				Vector2 icoPos = m_screenDim * .5f +
					dir * 2000.0f;

				// Snap to safe frame
				Rectangle icoEdges = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;
				icoEdges.Inflate(-48, -48);
				m_v3_1.X = (float)icoEdges.Left;
				m_v3_1.Y = (float)icoEdges.Top;
				m_v3_1.Z = -10;
				m_v3_2.X = (float)icoEdges.Right;
				m_v3_2.Y = (float)icoEdges.Bottom;
				m_v3_2.Z = 10;
				m_bb.Min = m_v3_1;
				m_bb.Max = m_v3_2;
				m_v3_1.X = icoPos.X;
				m_v3_1.Y = icoPos.Y;
				m_v3_1.Z = 0;
				m_v3_2.X = -dir.X;
				m_v3_2.Y = -dir.Y;
				m_v3_2.Z = 0;
				m_ray.Position = m_v3_1;
				m_ray.Direction = m_v3_2;
				float? rayIntersectDis = m_bb.Intersects(m_ray);
				icoPos -= dir * rayIntersectDis.Value;
				
				sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				sb.Draw(texHomeIcoBack, icoPos, null, Globals.IconColor, angle, m_homeIconOrigin, .75f, SpriteEffects.None, 0);
				sb.Draw(texHomeIco, icoPos, null, Globals.IconColor, 0, m_homeIconOrigin, .75f, SpriteEffects.None, 0);
				sb.End();
			}

			if (IsSquirrelHome && Inventory.Instance.HasItemsToSell)
			{
				sb.Begin(
					SpriteSortMode.Deferred, BlendState.NonPremultiplied,
					SamplerState.LinearWrap, DepthStencilState.None,
					RasterizerState.CullNone, null, camMat);
				SquirrelHelper.DrawString("Deposit nuts ->", 
					m_depositNutsAnim.Value, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.CENTER,
						INV_TILE_SCALE * 1.25f);
				sb.End();
			}

			// Show on screen messages
			if (m_messageFade.IsPlaying)
			{
				sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				SquirrelHelper.DrawString(m_messageText, textIconPos, m_messageFade.Value, SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);				
				sb.End();
			}

			// Show icons
			if (m_msgIcons != null)
			{
				sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
					foreach (MsgIcon icon in m_msgIcons)
					{
						if (icon.color.IsPlaying)
						{
							icon.entity.DrawInventoryItem(icon.pos, 64 * icon.scale.Value, icon.color.Value, true);
						}
					}
				sb.End();
			}
			if (m_depositIcons != null)
			{
				sb.Begin(
					SpriteSortMode.Deferred, BlendState.NonPremultiplied,
					SamplerState.LinearWrap, DepthStencilState.None,
					RasterizerState.CullNone, null, camMat);
					foreach (MsgIcon icon in m_depositIcons)
					{
						if (icon.posAnim.IsPlaying)
						{
							icon.entity.DrawInventoryItem(icon.posAnim.Value, icon.scale.Value, icon.color.Value, true);
						}
					}
				sb.End();
			}

			// Squirrel hud
			if (m_squirrel != null && Game1.GameState == Game1.eGAME_STATE.IN_GAME)
			{
				m_squirrel.RenderHud();
			}

			// Fade
			if (m_teleportFade.IsPlaying)
			{
				sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, m_teleportFade.Value);
				sb.End();
		    }

			//sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
   //         sb.Draw(m_texSnowSplatter, new Rectangle(0, 0, m_texSnowSplatter.Width, m_texSnowSplatter.Height), new Color(1, 1, 1, .2f));
			//sb.End();

#if DEBUG
			sb.Begin();
			sb.DrawString(fd.CommonResources.Font_System,
				"Load time: " + m_loadingTime.ToString(), debugText_loadTime_pos, Color.Yellow);
			sb.DrawString(fd.CommonResources.Font_System,
				"Anim count: " + IAnimatable.AnimCount, debugText_animCount_pos, Color.Yellow);
			sb.DrawString(fd.CommonResources.Font_System,
				"Home dis: " + Vector2.Distance(HomePos, m_squirrel.Position), debugText_homeDis_pos, Color.Yellow);
		//	sb.DrawString(fd.CommonResources.Font_System,
			//		"Allocs: " + System.GC.GetTotalMemory(true), debugText_allocs_pos, Color.Yellow);
			sb.End();
#endif
		}

		BoundingBox m_bb = new BoundingBox();
		Vector3 m_v3_1 = Vector3.Zero;
		Vector3 m_v3_2 = Vector3.Zero;
		Ray m_ray = new Ray();
		Vector2 m_homeIconOrigin = new Vector2(64, 64);
		public Vector2 textIconPos = new Vector2(
			(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth * .5f,
			(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight * .25f);

		static Vector2 debugText_loadTime_pos = new Vector2(10, 32);
		static Vector2 debugText_animCount_pos = new Vector2(10, 64);
		static Vector2 debugText_homeDis_pos = new Vector2(10, 96);
		static Vector2 debugText_allocs_pos = new Vector2(10, 128);
		private Vector2 m_houseInsidePos = new Vector2(-12, -12);

		internal CTile GetTileAt(int dest_x, int dest_y)
		{
			Chunk chunk = GetChunkAt(dest_x, dest_y);
			if (chunk == null) return null;
			if (chunk.IsBeingLoaded || !chunk.IsLoaded) return null;
			return chunk.GetTileAt(dest_x, dest_y);
		}

		public Chunk GetChunkAt(int dest_x, int dest_y)
		{
			if (dest_x < 0 || dest_y < 0 || dest_x >= FIELD_SIZE || dest_y >= FIELD_SIZE) return null;
			return m_chunks[dest_x / Chunk.CHUNK_SIZE, dest_y / Chunk.CHUNK_SIZE];
		}

		internal void MoveEntity(CTile in_from, CTile in_to)
		{
			in_to.Entity = in_from.Entity;
			in_from.Entity = null;
		}

		internal void KillAt(int dest_x, int dest_y)
		{
			// Kill players at this position
			if ((int)m_squirrel.Position.X == dest_x &&
				(int)m_squirrel.Position.Y == dest_y)
			{
				m_squirrel.Die();
			}
		}

		public Vector2 SpawnPos { get { return m_spawnPos; } }

		internal void Dispose()
		{
			m_snowFlakeMgr.Dispose();
			m_snowFlakeMgr = null;
			m_texSnowSplatter.Dispose();
			m_texSnowSplatter2.Dispose();
			m_rtSens.Dispose();
			foreach (Chunk chunk in m_chunkPool)
			{
				chunk.Dispose();
			}
			m_chunkPool = null;
			for (int j = 0; j < FIELD_SIZE / Chunk.CHUNK_SIZE; ++j)
			{
				for (int i = 0; i < FIELD_SIZE / Chunk.CHUNK_SIZE; ++i)
				{
					m_chunks[i, j] = null;
				}
			}
			m_chunks = null;
			m_teleportToInside.Stop();
			m_teleportToInside = null;
			m_teleportToOutside.Stop();
			m_teleportToOutside = null;
			m_squirrel.Dispose();
			m_squirrel = null;
			m_teleportFade.Stop();
			m_teleportFade = null;
			m_depositNutsAnim.Stop();
			m_depositNutsAnim = null;
			m_messageText.Stop();
			m_messageText = null;
			m_messageFade.Stop();
			m_messageFade = null;
			if (m_msgIcons != null)
			{
				foreach (MsgIcon icon in m_msgIcons)
				{
					icon.entity = null;
					icon.posAnim.Stop(); icon.posAnim = null;
					icon.color.Stop(); icon.color = null;
					icon.scale.Stop(); icon.scale = null;
					icon.sndTimer.Stop(); icon.sndTimer = null;
				}
			}
			m_msgIcons = null;
			if (m_depositIcons != null)
			{
				foreach (MsgIcon icon in m_depositIcons)
				{
					icon.entity = null;
					icon.posAnim.Stop(); icon.posAnim = null;
					icon.color.Stop(); icon.color = null;
					icon.scale.Stop(); icon.scale = null;
					icon.sndTimer.Stop(); icon.sndTimer = null;
				}
			}
			m_depositIcons = null;
			m_sensAnim.Stop();
			m_sensAnim = null;
			m_waterAnim.Stop();
			m_waterAnim = null;
			m_collisionSegments = null;

			Instance = null;
		}
		/*
		public void ForceResaveAll()
		{
			foreach (Chunk chunk in m_chunkPool)
			{
				if (chunk.IsLoaded)
				{
					chunk.WaitAndMarkForSave();
				}
			}

			Save();
		}*/

		internal void Save()
		{
			foreach (Chunk chunk in m_chunkPool)
			{
				if (chunk.NeedToBeSaved)
				{
					chunk.WaitAndSave();
				}
			}

			// Save snowfield stuff, like seed and inventory and squirrel
			try
			{
				//StorageDevice device = CFrameData.Instance.StorageDevice;
				//IAsyncResult result = device.BeginOpenContainer("Snowfield_" + Profile.Instance.CurrentSaveName, null, null);
				//result.AsyncWaitHandle.WaitOne();
				//StorageContainer container = device.EndOpenContainer(result);
				//result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                // Check here
                string filename = "Snowfield_" + Profile.Instance.CurrentSaveName + ".sav";
				BinaryWriter fic_out = new BinaryWriter(container.OpenFile(filename, FileMode.Create));
				fic_out.Write((int)2); // version
				fic_out.Write(m_seed);
				fic_out.Write(Globals.TotalNutCollected);
				Inventory.Instance.Save(fic_out);
				m_squirrel.Save(fic_out);
				fic_out.Close();

				container.Dispose();
			}
			catch
			{
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}
		}

		void OnDeathSpawnInHouse(IAnimatable in_anim)
		{
			m_cameraPos = m_cameraDst = m_squirrel.Position;
			m_cameraPos.Y = 4;
			m_teleportFade.StartAnim(Color.Black, Color.Transparent, .5f, 0, eAnimType.LINEAR);
			m_teleportFade.SetCallback(null, false);
		}

		internal void TriggerDeath()
		{
			m_teleportFade.StartAnim(Color.Transparent, Color.Black, .5f, 0, eAnimType.LINEAR);
			m_teleportFade.SetCallback(OnDeathSpawnInHouse, false);
		}
	}
}
