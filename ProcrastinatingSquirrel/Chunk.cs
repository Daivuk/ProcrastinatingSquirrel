using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Storage;
using System.IO.IsolatedStorage;
using DK8;
using System.Threading;
using System.IO;
using ProcrastinatingSquirrel.Entities;

namespace ProcrastinatingSquirrel
{
	class Chunk
	{
		//------------------------------------------------------------------------------------
		// Consts
		//------------------------------------------------------------------------------------
		static public int CHUNK_SIZE = 16;
		static public int CHUNK_POOL_SIZE = 18;
		static int TILES_COUNT = CHUNK_SIZE * CHUNK_SIZE;

		//------------------------------------------------------------------------------------
		// Statics / Shared
		//------------------------------------------------------------------------------------
		static List<Vector2> river1Path = null;
		static List<Vector2> river2Path = null;

		//------------------------------------------------------------------------------------
		// Privates
		//------------------------------------------------------------------------------------
		Rectangle m_rect = Rectangle.Empty;
		CTile[] m_tiles = null;
		bool m_isLoaded = false;
		bool m_needToBeSaved = false;
		bool m_isBeingLoaded = false;
		bool m_alwaysKeep = false;
		Vector2 m_tmpV2 = Vector2.Zero;
		Vector2 x0, x1, x2, x3, p;
		bool m_requiresFinalizing = false;
		bool m_needToUpdateSplatter = false;
		string m_filename;

		//------------------------------------------------------------------------------------
		// Accessors
		//------------------------------------------------------------------------------------
		public bool NeedToUpdateSplatter
		{
			get { return m_needToUpdateSplatter; }
			set { m_needToUpdateSplatter = value; }
		}
		public bool RequiresFinalizing
		{
			get { return m_requiresFinalizing; }
		}
		public bool AlwaysKeep
		{
			get { return m_alwaysKeep; }
			set { m_alwaysKeep = value; }
		}
		public bool IsLoaded
		{
			get { return m_isLoaded; }
		}
		public bool IsBeingLoaded
		{
			get { return m_isBeingLoaded; }
		}
		public bool NeedToBeSaved
		{
			get { return m_needToBeSaved; }
			set { m_needToBeSaved = true; }
		}
		public int X
		{
			get { return m_rect.X; }
		}
		public int Y
		{
			get { return m_rect.Y; }
		}

		//------------------------------------------------------------------------------------
		// Functions
		//------------------------------------------------------------------------------------
		public Chunk()
		{
			int i;

			m_rect.X = -1;
			m_rect.Y = -1;
			m_tiles = new CTile[TILES_COUNT];
			for (i = 0; i < TILES_COUNT; ++i)
			{
				m_tiles[i] = new CTile(Vector2.Zero);
			}
			m_rect.Width = CHUNK_SIZE;
			m_rect.Height = CHUNK_SIZE;

			// Create river paths if not already done
			if (river1Path == null)
			{
				river1Path = new List<Vector2>();
				int pCount = 50;
				float angle = 0;
				Vector2 pt;
				for (i = 0; i < pCount; ++i)
				{
					pt = new Vector2(
						(float)CSnowfield.FIELD_SIZE * .5f + 200 * (float)Math.Cos((double)MathHelper.ToRadians(angle)),
						(float)CSnowfield.FIELD_SIZE * .5f + 200 * (float)Math.Sin((double)MathHelper.ToRadians(angle)));
					pt.X += (float)(CFrameData.Instance.Random.NextDouble() * 2 - 1) * 20;
					pt.Y += (float)(CFrameData.Instance.Random.NextDouble() * 2 - 1) * 20;
					river1Path.Add(pt);
					angle += (360.0f / (float)pCount);
				}
			}
			if (river2Path == null)
			{
				river2Path = new List<Vector2>();
				int pCount = 100;
				float angle = 0;
				Vector2 pt;
				for (i = 0; i < pCount; ++i)
				{
					pt = new Vector2(
						(float)CSnowfield.FIELD_SIZE * .5f + 490 * (float)Math.Cos((double)MathHelper.ToRadians(angle)),
						(float)CSnowfield.FIELD_SIZE * .5f + 490 * (float)Math.Sin((double)MathHelper.ToRadians(angle)));
					pt.X += (float)(CFrameData.Instance.Random.NextDouble() * 2 - 1) * 20;
					pt.Y += (float)(CFrameData.Instance.Random.NextDouble() * 2 - 1) * 20;
					river2Path.Add(pt);
					angle += (360.0f / (float)pCount);
				}
			}
		}

		public CTile GetTileAt(int x, int y)
		{
			x -= m_rect.X;
			y -= m_rect.Y;
			if (x < 0 || y < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE) return null;
			return m_tiles[y * CHUNK_SIZE + x];
		}

		public bool CheckIfOnDiskAndLoad()
		{
            try
            {
                //// Check first if this chunk is on disk.
                //StorageDevice device = CFrameData.Instance.StorageDevice;
                //IAsyncResult result = device.BeginOpenContainer("Chunks_" + Profile.Instance.CurrentSaveName, null, null);
                //result.AsyncWaitHandle.WaitOne();
                //StorageContainer container = device.EndOpenContainer(result);
                //result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                // Check here
                m_filename = "c" + X.ToString() + "_" + Y.ToString() + "_" + Profile.Instance.CurrentSaveName + ".sav";
                if (!container.FileExists(m_filename))
                {
                    container.Dispose();
                    return false;
                }

                // Load chunk!
                BinaryReader fic_in = new BinaryReader(container.OpenFile(m_filename, FileMode.Open, FileAccess.Read));
                CTile tile;
                int x, y;
                for (int i = 0; i < TILES_COUNT; ++i)
                {
                    tile = m_tiles[i];
                    tile.Reset();
                    x = i % CHUNK_SIZE + m_rect.X;
                    y = i / CHUNK_SIZE + m_rect.Y;
                    m_tmpV2.X = (float)(x);
                    m_tmpV2.Y = (float)(y);
                    tile.Position = m_tmpV2;
                    tile.Load(fic_in);
                }
                fic_in.Close();

                container.Dispose();
            }
            catch
            {
                CFrameData.Instance.WhoTheHellRemoveStorageDevice();
            }

            return true;
            //return false;
		}

		void Save()
		{
			try
			{
                //StorageDevice device = CFrameData.Instance.StorageDevice;
                //IAsyncResult result = device.BeginOpenContainer("Chunks_" + Profile.Instance.CurrentSaveName, null, null);
                //result.AsyncWaitHandle.WaitOne();
                //StorageContainer container = device.EndOpenContainer(result);
                //result.AsyncWaitHandle.Close();

                IsolatedStorageFile container = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

                // Save chunk!
                BinaryWriter fic_out = new BinaryWriter(container.OpenFile(m_filename, FileMode.Create, FileAccess.Write));
				CTile tile;
				for (int i = 0; i < TILES_COUNT; ++i)
				{
					tile = m_tiles[i];
					tile.Save(fic_out);
				}
				fic_out.Close();

				container.Dispose();
			}
			catch
			{
				CFrameData.Instance.WhoTheHellRemoveStorageDevice();
			}

			m_needToBeSaved = false;
		}

		public void LoadThread()
		{
			if (m_needToBeSaved)
			{
				// Save previous one
				Save();
			}


			if (CheckIfOnDiskAndLoad())
			{
				refreshj = m_rect.Y - 1;
				m_isLoaded = true;
				m_needToBeSaved = false;
				m_isBeingLoaded = false;
				m_requiresFinalizing = true;
				return;
			}

			// Generate it
			GenerateChunk();
			refreshj = m_rect.Y - 1;
			m_isLoaded = true;
			m_needToBeSaved = true;
			m_isBeingLoaded = false;
			m_requiresFinalizing = true;
		}

		public bool Load(int x, int y)
		{
			if (IsBeingLoaded) return false;

			if (m_rect.X != -1 && m_rect.Y != -1)
			{
				CSnowfield.Instance.SetChunkAt(null, m_rect.X, m_rect.Y);
			}

			m_rect.X = x;
			m_rect.Y = y;
			CSnowfield.Instance.SetChunkAt(this, m_rect.X, m_rect.Y);
			m_isLoaded = false;
			m_isBeingLoaded = true;
			m_needToUpdateSplatter = false;

			WorkerThread.Instance.AddWork(LoadThread);
			return true;
		}

		int Density(int nbPer, int squareUnit)
		{
			return (int)(((float)(CHUNK_SIZE * CHUNK_SIZE) / (float)(squareUnit * squareUnit)) * (float)nbPer);
		}

		public bool HasDirtAt(int in_x, int in_y)
		{
			// In the middle of the map, we have a small open area
			m_tmpV2.X = (float)(in_x);
			m_tmpV2.Y = (float)(in_y);
			if (Vector2.DistanceSquared(m_tmpV2, CSnowfield.Instance.HomePos) <= 5.8f * 5.8f) return true;
			return (Globals.Random.Noise((double)in_x / 32.0, (double)in_y / 32.0, 0.0) > .95);
		}

		public void GenerateChunk()
		{
			int x, y;
			int count;
			int i, j;
			double noise;
			double noise2;
			float t, invT;
			CTile tile;//, tile2;
			Random random = new Random((int)(Globals.Random.Noise((double)m_rect.X, (double)m_rect.Y) * 100000 + Globals.Random.Seed));
			CFrameData.Instance.Random = random;

			// Generate features first, like clarieres and such
			for (i = 0; i < TILES_COUNT; ++i)
			{
				x = i % CHUNK_SIZE + m_rect.X;
				y = i / CHUNK_SIZE + m_rect.Y;
				tile = m_tiles[i];
				tile.Reset();
				m_tmpV2.X = (float)(x);
				m_tmpV2.Y = (float)(y);
				tile.Position = m_tmpV2;

				tile.Type = eTILE_TYPE.TILE_SNOW;

				// Clairieres
				if (HasDirtAt(x, y))
				{
					tile.Type = eTILE_TYPE.TILE_GRASS;
				}
			}

			// If it's the home tile, generate things differently
			if (m_rect.X == 0 && m_rect.Y == 0)
			{
				for (i = 0; i < TILES_COUNT; ++i)
				{
					tile = m_tiles[i];
					tile.IsDug = true;
					tile.IsPassable = false;
				}
				// Make inside home tiles passable
				for (j = 1; j <= 6; ++j)
				{
					for (i = 1; i <= 6; ++i)
					{
						GetTileAt(i, j).IsPassable = true;
					}
				}
				GetTileAt(3, 7).IsPassable = true;
				GetTileAt(4, 7).IsPassable = true;
				GetTileAt(2, 1).IsPassable = false;
				GetTileAt(1, 3).IsPassable = false;
				GetTileAt(2, 3).IsPassable = false;
				GetTileAt(1, 5).IsPassable = false;
				return;
			}

			// River 1
			count = river1Path.Count();
			for (i = 1; i < count + 1; ++i)
			{
				x0 = river1Path[(i) % count];
				x3 = river1Path[(i + 1) % count];
				x1 = x0 + Vector2.Normalize((x0 - river1Path[(i - 1) % count]) + (x3 - x0)) * 5;
				x2 = x3 + Vector2.Normalize((x3 - river1Path[(i + 2) % count]) + (x0 - x3)) * 5;

				// Do some bezier tricks
				for (t = 0; t <= 1; t += .01f)
				{
					invT = 1 - t;
					p = x0 * (invT * invT * invT) + x1 * 3 * t * (invT * invT) + x2 * 3 * (t * t) * invT + x3 * (t * t * t);

					x = (int)p.X;
					y = (int)p.Y;
					if (x < m_rect.X - 3 || y < m_rect.Y - 3 || x > m_rect.X + CHUNK_SIZE + 2 || y > m_rect.Y + CHUNK_SIZE + 2) continue;

					PlaceWater(x, y);
				}
            }

            // River 2
            count = river2Path.Count();
			for (i = 1; i < count + 1; ++i)
			{
				x0 = river2Path[(i) % count];
				x1 = river2Path[(i) % count] + Vector2.Normalize((river2Path[(i) % count] - river2Path[(i - 1) % count])) * 5;
				x2 = river2Path[(i + 1) % count] + Vector2.Normalize((river2Path[(i + 1) % count] - river2Path[(i + 2) % count])) * 5;
				x3 = river2Path[(i + 1) % count];

				// Do some bezier tricks
				for (t = 0; t <= 1; t += .01f)
				{
					invT = 1 - t;
					p = x0 * (invT * invT * invT) + x1 * 3 * t * (invT * invT) + x2 * 3 * (t * t) * invT + x3 * (t * t * t);

					x = (int)p.X;
					y = (int)p.Y;
					if (x < m_rect.X - 4 || y < m_rect.Y - 4 || x > m_rect.X + CHUNK_SIZE + 4 || y > m_rect.Y + CHUNK_SIZE + 4) continue;

					PlaceWater(x - 1, y - 1);
					PlaceWater(x, y - 1);
					PlaceWater(x + 1, y - 1);
					PlaceWater(x + 2, y - 1);

					PlaceWater(x - 1, y);
					PlaceWater(x, y);
					PlaceWater(x + 1, y);
					PlaceWater(x + 2, y);

					PlaceWater(x - 1, y + 1);
					PlaceWater(x, y + 1);
					PlaceWater(x + 1, y + 1);
					PlaceWater(x + 2, y + 1);

					PlaceWater(x - 1, y + 2);
					PlaceWater(x, y + 2);
					PlaceWater(x + 1, y + 2);
					PlaceWater(x + 2, y + 2);
				}
            }

            // Add ice
            for (i = 0; i < TILES_COUNT; ++i)
			{
				x = i % CHUNK_SIZE + m_rect.X;
				y = i / CHUNK_SIZE + m_rect.Y;
				tile = m_tiles[i];
				noise = Globals.Random.Noise((double)x / 64.0, (double)y / 64.0, 10.0);
				noise = RadialGradient(noise, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 128, 256, -1);
				if (noise > .5)
				{
					/*	tile2 = GetTileAt(x - 1, y - 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x, y - 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x + 1, y - 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x - 1, y);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x + 1, y);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x - 1, y + 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x, y + 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;
						tile2 = GetTileAt(x + 1, y + 1);
						if (tile2 == null) continue;
						if (tile2.Type != eTILE_TYPE.TILE_SNOW && tile2.Type != eTILE_TYPE.TILE_ICE) continue;*/
					if (!tile.IsDug && tile.Type == eTILE_TYPE.TILE_SNOW)
						tile.Type = eTILE_TYPE.TILE_ICE;
				}
            }

            // Dense forest
            count = Density(10, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 1.0);
				if (noise > .75 && noise <= 1 &&
					tile.Type == eTILE_TYPE.TILE_SNOW)
				{
                    tile.Entity = new CTree();
                    tile.IsPassable = false;
                }
            }

            // forest
            count = Density(3, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 1.0);
				if (noise > 0.0 && noise <= .5 && tile.Type == eTILE_TYPE.TILE_SNOW)
				{
                    tile.Entity = new CTree();
                    tile.IsPassable = false;
                }
            }

            // Sparse forest
            count = Density(1, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 1.0);
				if (noise > -.5 && noise <= 0.0 && tile.Type == eTILE_TYPE.TILE_SNOW)
				{
                    tile.Entity = new CTree();
                    tile.IsPassable = false;
                }
			}

			// Dense Snow Piles
			count = Density(10, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 2.0);
				noise = RadialGradient(noise, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 8, 150, -1);
				if (noise > .5 && noise <= 1 &&
					tile.Type == eTILE_TYPE.TILE_SNOW &&
					tile.Entity == null)
				{
                    tile.Entity = new SnowPile();
                }
			}

			count = Density(3, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 2.0);
				noise = RadialGradient(noise, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 8, 100, -1);
				if (noise > 0 && noise <= .5 &&
					tile.Type == eTILE_TYPE.TILE_SNOW &&
					tile.Entity == null)
				{
                    tile.Entity = new SnowPile();
                }
			}

			count = Density(1, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 32.0, (double)y / 32.0, 2.0);
				noise = RadialGradient(noise, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 8, 50, -1);
				if (noise > -.5 && noise <= 0 &&
					tile.Type == eTILE_TYPE.TILE_SNOW &&
					tile.Entity == null)
				{
                    tile.Entity = new SnowPile();
                }
			}

			// Nuts
			count = Density(12, 10);
			for (j = 0; j < count; ++j)
			{
				x = random.Next(CHUNK_SIZE) + m_rect.X;
				y = random.Next(CHUNK_SIZE) + m_rect.Y;
				tile = GetTileAt(x, y);
				noise = Globals.Random.Noise((double)x / 4.0, (double)y / 4.0, 3.0);
				noise2 = Globals.Random.Noise((double)x / 16.0, (double)y / 16.0, 4.0);
				noise = RadialGradient(noise, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 4, 12, 1);
				noise2 = RadialGradient(noise2, x, y, CSnowfield.FIELD_SIZE / 2, CSnowfield.FIELD_SIZE / 2, 4, 12, 1);
				if (tile.Type == eTILE_TYPE.TILE_ICE ||
					noise * noise2 > 0)
				{
                    CSnowfield.Instance.AddNut(GetTileAt(x, y), x, y);
                }
			}
		}

		private void PlaceWater(int x, int y)
		{
			// Dirt tiles
			CTile tile = GetTileAt(x - 1, y - 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x - 1, y);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x, y - 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x - 1, y + 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 1, y - 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x - 1, y + 2);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 2, y - 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x, y + 2);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 2, y);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 1, y + 2);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 2, y + 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;
			tile = GetTileAt(x + 1, y + 1);
			if (tile != null && tile.Type != eTILE_TYPE.TILE_WATER) tile.Type = eTILE_TYPE.TILE_GRASS;

			// Water tiles
			tile = GetTileAt(x, y);
			if (tile != null) tile.Type = eTILE_TYPE.TILE_WATER;
			tile = GetTileAt(x + 1, y);
			if (tile != null) tile.Type = eTILE_TYPE.TILE_WATER;
			tile = GetTileAt(x, y + 1);
			if (tile != null) tile.Type = eTILE_TYPE.TILE_WATER;
		}

		private double RadialGradient(double in_value, int x, int y, int cx, int cy, double in_from, double in_to, double in_newValue)
		{
			double diffX = (double)x - (double)cx;
			double diffY = (double)y - (double)cy;
			double dis = Math.Sqrt(diffX * diffX + diffY * diffY);
			double percent = 0;
			if (dis <= in_from) percent = 1;
			else if (dis >= in_to) percent = 0;
			else percent = (dis - in_to) / (in_from - in_to);
			return in_value * (1 - percent) + in_newValue * percent;
		}

		int refreshj = 0;

		public void FinalizeLoad()
		{
			m_requiresFinalizing = false;
			// Refresh dig
			int i, j = 0;
			for (; refreshj <= m_rect.Y + CHUNK_SIZE; ++refreshj)
			{
				for (i = m_rect.X - 1; i <= m_rect.X + CHUNK_SIZE; ++i)
				{
					CSnowfield.Instance.RefreshDig(i, refreshj);
				}
				++j;
				if (j == 1)
				{
					++refreshj;
					break;
				}
			}
			if (refreshj <= m_rect.Y + CHUNK_SIZE)
			{
				m_requiresFinalizing = true;
			}
			else m_needToUpdateSplatter = true;
		}

		internal void Dispose()
		{
			river1Path = null;
			river2Path = null;
			foreach (CTile tile in m_tiles)
			{
				tile.Dispose();
			}
			m_tiles = null;
		}

		internal void WaitAndSave()
		{
			while (IsBeingLoaded)
			{
				Thread.Sleep(100);
			}
			Save();
		}

		internal void WaitAndMarkForSave()
		{
			while (IsBeingLoaded)
			{
				Thread.Sleep(100);
			}
			m_needToBeSaved = true;
		}
	}
}
