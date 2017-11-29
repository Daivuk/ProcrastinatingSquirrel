using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel.Entities
{
    class SpeedUpgrade : IEntity
    {
        private float m_speedUpgrade = 1.0f;
        public float SpeedUpgradeValue
        {
            get { return m_speedUpgrade; }
        }

        int m_cost = 0;
        public override int Cost
        {
            get { return m_cost; }
        }
        string m_name;
        Texture2D m_texture;

        public override string Name
        {
            get { return m_name; }
        }

        public SpeedUpgrade(string in_name, float in_speedMultiplier, int in_cost, string in_texture, int in_level)
        {
            Level = in_level;
            m_speedUpgrade = in_speedMultiplier;
            m_name = in_name;
            m_cost = in_cost;
            m_texture = CFrameData.Instance.Content.Load<Texture2D>(in_texture);
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
                    new Vector2(m_texture.Width / 2, m_texture.Height / 2),
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
