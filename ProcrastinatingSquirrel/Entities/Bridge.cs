using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel.Entities
{
	class Bridge : IEntity
	{
		int m_cost = 0;
		public override int Cost
		{
			get { return m_cost; }
		}
		static Texture2D m_textureIcon = CFrameData.Instance.Content.Load<Texture2D>("textures/bridgeIcon");
		static public Texture2D Texture = CFrameData.Instance.Content.Load<Texture2D>("textures/bridge");

		static Vector2 m_origin = new Vector2(m_textureIcon.Width / 2, m_textureIcon.Height / 2);

		public override string Name
		{
			get { return "Bridge"; }
		}

		public Bridge()
		{
		}

		public Bridge(int in_cost)
		{
			m_cost = in_cost;
		}

		public override void Render()
		{
			CFrameData.Instance.SpriteBatch.Draw(Texture, Position, null, Color.White, 0, m_origin,
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
				scale = preferedSize.Value / (float)Texture.Width;
			}

			if (centered)
			{
				sb.Draw(m_textureIcon, screenPos, null, color, 0,
					m_origin,
					scale, SpriteEffects.None, 0);
			}
			else
			{
				sb.Draw(m_textureIcon, screenPos, null, color
					, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			}
		}
	}
}
