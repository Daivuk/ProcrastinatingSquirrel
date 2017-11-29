using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ProcrastinatingSquirrel.Entities
{
	class SnowPile : IEntity
	{
		Texture2D m_texture;
		Texture2D m_fellTexture;
		Texture2D m_fellTextureU;
		private Vector2 m_origin;
		CAnimVector2 m_offsetAnim = new CAnimVector2("game");
		int dest_x;
		int dest_y;
		bool m_fell = false;
		static SoundEffect s_sndSnowFall = CFrameData.Instance.Content.Load<SoundEffect>("sounds/snowFall");

		internal override void Dispose()
		{
			m_offsetAnim.Stop();
			m_offsetAnim = null;
			base.Dispose();
		}

		public override bool CanDig
		{
			get { return false; }
		}

		public override void Save(System.IO.BinaryWriter fic_out)
		{
			byte flags = m_fell ? (byte)1 : (byte)0;
			fic_out.Write(flags);
		}

		public override void Load(System.IO.BinaryReader fic_in)
		{
			m_fell = (fic_in.ReadByte() == 1) ? true : false;
		}

		static Texture2D[] s_textures = new Texture2D[]
		{
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPile1"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPile2"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPile3"),
		};
		static Texture2D[] s_fellTextures = new Texture2D[]
		{
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileD1"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileD2"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileD3"),
		};
		static Texture2D[] s_fellTexturesU = new Texture2D[]
		{
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileU1"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileU2"),
			CFrameData.Instance.Content.Load<Texture2D>("textures/snowPileU3"),
		};
		static Vector2 s_origin = new Vector2((float)s_textures[0].Width * .5f, (float)s_textures[0].Height * .5f);

		public SnowPile()
		{
			CFrameData fd = CFrameData.Instance;
			int rnd = fd.Random.Next(3);
			m_texture = s_textures[rnd];
			m_fellTexture = s_fellTextures[rnd];
			m_fellTextureU = s_fellTexturesU[rnd];
		}

		public override void Update()
		{
		}

		public override void Render()
		{
			if (m_offsetAnim.IsPlaying)
			{
				CFrameData.Instance.SpriteBatch.Draw(m_texture, m_offsetAnim.Value, null, Color.White, 0, s_origin,
					CSnowfield.INV_TILE_SCALE, SpriteEffects.None, 0);
			}
			if (m_fell)
			{
				CFrameData.Instance.SpriteBatch.Draw(m_fellTextureU, Position, null, Color.White, 0, s_origin,
					CSnowfield.INV_TILE_SCALE * 1.5f, SpriteEffects.None, 0);
			}
		}

		public override void RenderUnder()
		{
			if (m_fell)
			{
				CFrameData.Instance.SpriteBatch.Draw(m_fellTexture, Position, null, Color.White, 0, s_origin,
					CSnowfield.INV_TILE_SCALE * 1.5f, SpriteEffects.None, 0);
			}
		}

		public override void RenderSens()
		{
			if (m_offsetAnim.IsPlaying || m_fell) return;
			CFrameData.Instance.SpriteBatch.Draw(m_texture, Position, null, Color.White, 0, s_origin,
				CSnowfield.INV_TILE_SCALE, SpriteEffects.None, 0);
		}

		public override void Trigger(IEntity digger, int in_x, int in_y)
		{
			if (m_fell || m_offsetAnim.IsPlaying) return;
			s_sndSnowFall.Play();
			CFrameData fd = CFrameData.Instance;
			dest_x = in_x;
			dest_y = in_y;
			m_offsetAnim.SetCallback(EndAnimCallback, false);
			m_offsetAnim.Value = Position;
			for (int i = 0; i < 8; ++i)
			{
				m_offsetAnim.QueueAnimFromCurrent(Position +
					new Vector2((float)fd.Random.NextDouble() * .065f, (float)fd.Random.NextDouble() * .065f),
					.065f, 0, eAnimType.EASE_BOTH);
			}
			CTile tile = CSnowfield.Instance.GetTileAt(dest_x, dest_y);
			if (tile != null)
			{
				m_offsetAnim.QueueAnimFromCurrent(tile.Position + Vector2.One * .5f,
					.3f, 0, eAnimType.EASE_IN);
			}
		}

		public void EndAnimCallback(IAnimatable anim)
		{
			// Fill destination with snow pile
			m_fell = true;
			CTile tile = CSnowfield.Instance.GetTileAt(dest_x, dest_y);
			if (tile == null) return; // This shouldn't happen.. we unloaded the chunk?
			CSnowfield.Instance.MoveEntity(Tile, tile);
			Tile.IsPassable = false;
			CSnowfield.Instance.KillAt(dest_x, dest_y);
		}
	}
}
