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

namespace ProcrastinatingSquirrel
{
	class CTree : IEntity
	{
		const float WIND_STRENGTH = .05f;

		internal override void Dispose()
		{
			m_windAnim = null;
			base.Dispose();
		}

		static Texture2D m_texTreeTrunk = CFrameData.Instance.Content.Load<Texture2D>("textures/treeTrunk");
		static Texture2D m_texTree = CFrameData.Instance.Content.Load<Texture2D>("textures/pineTree");
		static Rectangle[] m_rectTree = new Rectangle[]{
			new Rectangle(0, 0, 256, 256),
			new Rectangle(256, 0, 256, 256),
			new Rectangle(0, 256, 128, 128),
			new Rectangle(128, 256, 128, 128)
		};
		static Vector2[] m_origins = new Vector2[]{
			new Vector2(128, 128),
			new Vector2(128, 128),
			new Vector2(64, 64),
			new Vector2(64, 64)
		};
		static Rectangle m_rectShadow = new Rectangle(256, 256, 256, 256);
		float m_scale;
		CAnimFloat m_windAnim;

		public override string Name
		{
			get { return "Tree Trunk"; }
		}
		public override bool CanDig
		{
			get { return false; }
		}

		public CTree()
		{
			Angle = (float)CFrameData.Instance.Random.NextDouble() * MathHelper.TwoPi;
			m_scale = (float)CFrameData.Instance.Random.NextDouble() * .3f + .70f;
			m_scale *= 2;

			m_windAnim = Globals.WindAnims[CFrameData.Instance.Random.Next(Globals.WindAnims.Count())];
		}


		public override void Update() 
		{ 
		}


		public override void Render()
		{
			CFrameData.Instance.SpriteBatch.Draw(m_texTreeTrunk, Position, null, Color.White, Angle,
				new Vector2((float)m_texTreeTrunk.Width * .5f, (float)m_texTreeTrunk.Height * .5f), 
				CSnowfield.INV_TILE_SCALE, SpriteEffects.None, 0);
		}

		public override void DrawInventoryItem(Vector2 screenPos, float? preferedSize, Color? in_color, bool centered)
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;
			Color color = Color.White;
			if (in_color != null) color = in_color.Value;

			float scale = .75f;
			if (preferedSize != null)
			{
				scale = preferedSize.Value / (float)m_texTreeTrunk.Width;
			}

			if (centered)
			{
				sb.Draw(m_texTreeTrunk, screenPos, null, color, 0,
					new Vector2(m_texTreeTrunk.Width / 2, m_texTreeTrunk.Height / 2),
					scale, SpriteEffects.None, 0);
			}
			else
			{
				sb.Draw(m_texTreeTrunk, screenPos, null, color
					, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			}
		}

		public override void RenderLayer(int layer, ref Vector2 offset)
		{
			Vector2 squirrelPos = CSnowfield.Instance.Squirrel.Position;
			float opacity = (squirrelPos - Position).LengthSquared();
			if (opacity <= 4 * 4)
			{
				opacity /= (4 * 4);
				if (opacity < .15f) opacity = .15f;
			}
			Color col = new Color(1, 1, 1, opacity);
			if (layer == 0)
			{
				float scale = CSnowfield.INV_TILE_SCALE * m_scale;
				float windStrength = WIND_STRENGTH * m_scale;

				CFrameData.Instance.SpriteBatch.Draw(m_texTree,
					new Vector2(Position.X + offset.X + m_windAnim.Value * windStrength,
								Position.Y + offset.Y - m_windAnim.Value * windStrength * .25f),
					m_rectShadow, col, Angle,
					m_origins[0], scale, SpriteEffects.None, 0);
			}
			else if (layer <= 4)
			{
				float scale = CSnowfield.INV_TILE_SCALE * m_scale;

				float windStrength = WIND_STRENGTH * ((float)layer + 1) * m_scale;
				CFrameData.Instance.SpriteBatch.Draw(m_texTree,
					new Vector2(Position.X + offset.X + m_windAnim.Value * windStrength,
								Position.Y + offset.Y - m_windAnim.Value * windStrength * .25f),
					m_rectTree[layer - 1], col, Angle,
					m_origins[layer - 1], scale, SpriteEffects.None, 0);
			}
		}
	}
}

