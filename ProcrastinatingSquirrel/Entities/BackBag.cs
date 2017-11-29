using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel.Entities
{
	class BackBag : IEntity
	{
		private int m_capacity = 10;
		public int Capacity
		{
			get { return m_capacity; }
		}

		int m_cost = 0;
		public override int Cost
		{
			get { return m_cost; }
		}
		string m_name;
		Texture2D m_texture;
		Vector2 m_origin;

		public override string Name
		{
			get { return m_name; }
		}

		public BackBag(string in_name, int in_capacity, int in_cost, string in_texture, int in_level)
		{
			Level = in_level;
			m_capacity = in_capacity;
			m_name = in_name;
			m_cost = in_cost;
			m_texture = CFrameData.Instance.Content.Load<Texture2D>(in_texture);
			m_origin = new Vector2(m_texture.Width / 2, m_texture.Height / 2);
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
				scale = preferedSize.Value / (float)m_texture.Width;
			}

			if (centered)
			{
				sb.Draw(m_texture, screenPos, null, color, 0,
					m_origin,
					scale, SpriteEffects.None, 0);
			}
			else
			{
				sb.Draw(m_texture, screenPos, null, color
					, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			}
		}
	}
}
