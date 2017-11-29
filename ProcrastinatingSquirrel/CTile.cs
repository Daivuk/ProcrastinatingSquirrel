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

namespace ProcrastinatingSquirrel
{
	enum eTILE_TYPE
	{
		TILE_SNOW = 0,
		TILE_GRASS = 1,
		TILE_ICE = 2,
		TILE_WATER = 3
	}

	class CTile
	{
		internal void Save(System.IO.BinaryWriter fic_out)
		{
			// EEEE TTPD
			// E = Entity type, 0 if none
			// T = tile type
			// P = Is passable
			// D = is Dug

			byte flags = 0;
			flags |= m_isDug ? (byte)0x01 : (byte)0x00;
			flags |= m_isPassable ? (byte)0x02 : (byte)0x00;
			flags |= (byte)((int)m_type << 2);

			int entityType = 0;
			if (m_entity != null)
			{
				if (m_entity.GetType() == typeof(CNut))
				{
					entityType = 1;
				}
				else if (m_entity.GetType() == typeof(CTree))
				{
					entityType = 2;
				}
				else if (m_entity.GetType() == typeof(SnowPile))
				{
					entityType = 3;
				}
				else if (m_entity.GetType() == typeof(Bridge))
				{
					entityType = 4;
				}
			}
			flags |= (byte)((int)entityType << 4);

			fic_out.Write(flags);

			// Now save the entity
			if (m_entity != null)
			{
				m_entity.Save(fic_out);
			}
		}

		internal void Load(System.IO.BinaryReader fic_in)
		{
			// EEEE TTPD
			// E = Entity type, 0 if none
			// T = tile type
			// P = Is passable
			// D = is Dug

			byte flags = fic_in.ReadByte();
			Type = (eTILE_TYPE)((flags & (byte)12) >> 2);
			m_isDug = ((flags & 0x01) != 0);
			m_isPassable = ((flags & 0x02) != 0);
			int entityType = (int)(flags & (byte)240) >> 4;
			if (entityType == 0) m_entity = null;
			else
			{
				switch (entityType)
				{
					case 1: m_entity = new CNut(); break;
					case 2: m_entity = new CTree(); break;
					case 3: m_entity = new SnowPile(); break;
					case 4: m_entity = new Bridge(); break;
					default: return;
				}
				m_entity.Load(fic_in);
				Entity = m_entity;
			}
		}

		internal void Reset()
		{
			m_isDug = false;
			m_isPassable = true;
			m_entity = null;
		}

		IEntity m_entity = null;
		public IEntity Entity
		{
			get { return m_entity; }
			set 
			{ 
				m_entity = value;
				if (m_entity != null)
				{
					m_entity.Tile = this;
					m_entity.Position = m_position + Vector2.One * .5f;
				}
			}
		}
		int m_originalStrength = -1; // -1 means not touched yet
		int m_snowStrength = 3; // The force it takes to dig it. The squirrel starts with a force of 1. Means 2 hit to dig it at first
		Vector2 m_position;
		public Vector2 Position
		{
			get { return m_position; }
			set
			{
				m_position = value;

				// Snow harder and harder to dig the further we are
				int homeDis = (int)(Vector2.Distance(CSnowfield.Instance.HomePos, m_position) / 50);
				switch (homeDis)
				{
					case 0: m_snowStrength = 3; break; // 0-50
					case 1: m_snowStrength = 9; break; // 50-100
					case 2: m_snowStrength = 27; break; // 100-150
					case 3: m_snowStrength = 81; break; // 150-200
					case 4: m_snowStrength = 243; break; // 200-250
					case 5: m_snowStrength = 243 * 2; break; // 250-300
					case 6: m_snowStrength = 243 * 2; break; // 300-350
					case 7: m_snowStrength = 243 * 3; break; // 350-400
					case 8: m_snowStrength = 243 * 3; break; // 400-450
					default: m_snowStrength = 243 * 6; break; // 450-512
				}
				if (Type == eTILE_TYPE.TILE_ICE) m_snowStrength *= 3;
				m_originalStrength = -1;
			}
		}
		Rectangle m_textureSrcRect;
		Rectangle m_digSrcRect = new Rectangle(512, 0, 128, 128);
		public Rectangle DigSrcRect
		{
			get { return m_digSrcRect; }
			set { m_digSrcRect = value; }
		}
		Rectangle m_spriteDest;
		Texture2D m_texture;
		eTILE_TYPE m_type;
		bool m_isDug = false;
		public bool IsDug
		{
			get { return m_isDug; }
			set { m_isDug = value; }
		}
		bool m_isPassable = true;
		public bool IsPassable
		{
			get
			{
				return m_isDug && m_isPassable;
			}
			set
			{
				m_isPassable = value;
			}
		}
		public eTILE_TYPE Type
		{
			get { return m_type; }
			set
			{
				m_type = value;
				switch (m_type)
				{
					case eTILE_TYPE.TILE_SNOW:
						m_texture = CSnowfield.texGrass;
						break;
					case eTILE_TYPE.TILE_GRASS:
						m_texture = CSnowfield.texGrass;
						IsDug = true;
						m_isPassable = true;
						break;
					case eTILE_TYPE.TILE_ICE:
						m_texture = CSnowfield.texIce;
						Position = Position;
						break;
					case eTILE_TYPE.TILE_WATER:
						m_texture = CSnowfield.texGrass;
						IsDug = true;
						m_isPassable = false;
						break;
				}
				m_textureSrcRect.X = ((int)m_position.X * (int)CSnowfield.TILE_SCALE) % m_texture.Width;
				m_textureSrcRect.Y = ((int)m_position.Y * (int)CSnowfield.TILE_SCALE) % m_texture.Height;
				m_textureSrcRect.Width = (int)CSnowfield.TILE_SCALE;
				m_textureSrcRect.Height = (int)CSnowfield.TILE_SCALE;
				m_spriteDest.X = (int)m_position.X;
				m_spriteDest.Y = (int)m_position.Y;
				m_spriteDest.Width = 1;
				m_spriteDest.Height = 1;
			}
		}
		static Rectangle[] s_srcRectCrack = new Rectangle[] {
			new Rectangle(0, 0, 128, 128),
			new Rectangle(128, 0, 128, 128),
			new Rectangle(0, 128, 128, 128),
			new Rectangle(128, 128, 128, 128)};


		public CTile(Vector2 position)
		{
		/*	m_position = position;
			Type = eTILE_TYPE.TILE_SNOW; // Default...*/
		}


		public void Render()
		{
            if (m_texture == null) return;

            CFrameData.Instance.SpriteBatch.Draw(m_texture, m_spriteDest, m_textureSrcRect, new Color(
				(m_position.X / (float)CSnowfield.FIELD_SIZE - CSnowfield.currentTopLeft.X) * 4,
				(m_position.Y / (float)CSnowfield.FIELD_SIZE - CSnowfield.currentTopLeft.Y) * 4,
				(float)m_textureSrcRect.Left / (float)m_texture.Width, (float)m_textureSrcRect.Top / (float)m_texture.Height));
		}


		public void RenderWater()
		{
			if (m_type != eTILE_TYPE.TILE_WATER) return;
			CFrameData.Instance.SpriteBatch.Draw(CSnowfield.texWater, m_spriteDest, m_textureSrcRect, new Color(
				(m_position.X / (float)CSnowfield.FIELD_SIZE - CSnowfield.currentTopLeft.X) * 4,
				(m_position.Y / (float)CSnowfield.FIELD_SIZE - CSnowfield.currentTopLeft.Y) * 4,
				(float)m_textureSrcRect.Left / (float)m_texture.Width, (float)m_textureSrcRect.Top / (float)m_texture.Height));
		}


		public void RenderDig()
		{
			CFrameData.Instance.SpriteBatch.Draw(CSnowfield.texDig, m_spriteDest, m_digSrcRect, Color.White);

			// Render crack
			if (m_originalStrength > 0)
			{
				float percent = (float)(m_snowStrength) / (float)m_originalStrength;
				int crackRectId = 3 - (int)(percent * 4);

				CFrameData.Instance.SpriteBatch.Draw(CSnowfield.texCrack, m_spriteDest, s_srcRectCrack[crackRectId], Color.White);
			}
		}


		public void Update() { }


		void Collapse() // When the snow is removed
		{
			m_originalStrength = -1;
			m_snowStrength = 0;
			m_isDug = true;
			CSnowfield.Instance.GetChunkAt((int)m_position.X, (int)m_position.Y).NeedToBeSaved = true;
		}

		public void Trigger(IEntity digger, int in_x, int in_y)
		{
			if (m_entity != null)
			{
				m_entity.Trigger(digger, in_x, in_y);
				CSnowfield.Instance.GetChunkAt((int)m_position.X, (int)m_position.Y).NeedToBeSaved = true;
			}
		}


		public void Dig(IEntity digger)
		{
			if (m_snowStrength > 0)
			{
				if (m_entity != null)
				{
					if (!m_entity.CanDig) return;
				}
			//	if (m_entity == null)
				{
					if (m_originalStrength == -1) m_originalStrength = m_snowStrength;
					m_snowStrength -= digger.GetDiggingStrength();
					if (m_snowStrength <= 0)
					{
						if (m_entity != null)
						{
							digger.GiveItem(m_entity);
							m_entity = null;
						}
						Collapse();
					}
				}
			/*	else
				{
					if (m_entity.Dig(digger))
					{
						digger.GiveItem(m_entity);
						m_entity = null;
						Collapse();
					}
				}*/
			}
		}

		internal void Dispose()
		{
			if (m_entity != null) m_entity.Dispose();
			m_entity = null;
		}
	}
}
