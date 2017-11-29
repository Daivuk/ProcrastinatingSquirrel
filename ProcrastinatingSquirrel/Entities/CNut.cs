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
	class CNut : IEntity
	{
		Texture2D m_texture;
		SpriteEffects m_flipped;
		int m_count;
		int m_value;
		int m_digStrength;
		public override int Value
		{
			get { return s_nutInfos[m_value].kuiValue; }
		}
		public override int Count
		{
			get { return m_count; }
		}

		static Rectangle s_srcRectShadow = new Rectangle(0, 0, 192, 192);
		static Rectangle s_srcRectBody = new Rectangle(192, 0, 192, 192);
		static Rectangle s_srcRectCap = new Rectangle(0, 192, 192, 192);
		static Rectangle s_srcRectExtra = new Rectangle(192, 192, 192, 192);
		static Vector2 s_origin = new Vector2(96, 96);
		static Vector2[][] s_countPos = new Vector2[][] {
			new Vector2[] {},
			new Vector2[] {new Vector2(0, 0)},
			new Vector2[] {new Vector2(-.15f, -.15f), new Vector2(.15f, .15f)},
			new Vector2[] {new Vector2(0, -.15f), new Vector2(-.15f, .15f), new Vector2(.15f, .15f)},
			new Vector2[] {},
			new Vector2[] {new Vector2(.15f, 0), new Vector2(-.15f, 0), new Vector2(-.25f, .25f), new Vector2(.25f, .25f), new Vector2(0, .30f)},
			};
		class CNutInfo
		{
			public Color[] colors;
			public String filename;
			public String name;
			public int kuiValue;
			public Texture2D texture;
			public CNutInfo(Color[] in_colors, String in_filename, String in_name, int in_kuiValue)
			{
				colors = in_colors;
				filename = in_filename;
				name = in_name;
				kuiValue = in_kuiValue;
				texture = CFrameData.Instance.Content.Load<Texture2D>(filename);
			}
		}
		static CNutInfo[] s_nutInfos = new CNutInfo[] {
			null, // First one is ignored
			new CNutInfo(new Color[] {new Color(160, 89, 43), new Color(80, 45, 23)}, "textures/OakNut", "Acorn", 1),
			new CNutInfo(new Color[] {new Color(140, 107, 72)}, "textures/KoreanPineNut", "Pine nut", 2),
			new CNutInfo(new Color[] {new Color(255, 198, 107)}, "textures/Cashew", "Cashew", 4),

			new CNutInfo(new Color[] {new Color(99, 179, 179)}, "textures/Cashew", "Frozen Cashew", 8),
			new CNutInfo(new Color[] {new Color(171, 170, 169), new Color(80, 45, 23)}, "textures/OakNut", "Snow nut", 10),
			new CNutInfo(new Color[] {new Color(240, 108, 108), new Color(80, 45, 23)}, "textures/OakNut", "Strawberry nut", 20),

			new CNutInfo(new Color[] {new Color(128, 128, 128)}, "textures/KoreanPineNut", "Soft pine nut", 40),
			new CNutInfo(new Color[] {new Color(27, 74, 37)}, "textures/KoreanPineNut", "Fresh Pine nut", 50),
			new CNutInfo(new Color[] {new Color(231, 178, 86)}, "textures/Almond", "Almond", 100),

			new CNutInfo(new Color[] {new Color(119, 33, 33), new Color(80, 45, 23)}, "textures/OakNut", "Cherry nut", 200),
			new CNutInfo(new Color[] {new Color(30, 28, 27)}, "textures/Cashew", "Black cashew", 250),
			new CNutInfo(new Color[] {new Color(150, 199, 111), new Color(232, 185, 141)}, "textures/Pistachio", "Pistachio", 500),

			new CNutInfo(new Color[] {new Color(119, 33, 33)}, "textures/Almond", "Wine almond", 1000),
			new CNutInfo(new Color[] {new Color(250, 200, 65), new Color(243, 115, 12)}, "textures/OakNut", "Golden oak nut", 1250),
			new CNutInfo(new Color[] {new Color(0, 255, 0), new Color(200, 150, 120)}, "textures/Pistachio", "Radioactive pistachio", 2500),
			};
		static public int maxValue = s_nutInfos.Count() - 1;

		public override string Name
		{
			get { return s_nutInfos[m_value].name; }
		}

		public override void Save(System.IO.BinaryWriter fic_out)
		{
			byte flags = (byte)m_value;
			flags |= (m_flipped == SpriteEffects.FlipHorizontally) ? (byte)0x10 : (byte)0x00;
			int count = 0;
			switch (m_count)
			{
				case 1: count = 0; break;
				case 2: count = 1; break;
				case 3: count = 2; break;
				case 5: count = 3; break;
			}
			flags |= (byte)(count << 5);
			fic_out.Write(flags);
		}

		public override void Load(System.IO.BinaryReader fic_in)
		{
			byte flags = fic_in.ReadByte();
			m_value = (int)(flags & 0x0f);
			m_flipped = ((flags & (byte)0x10) != 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			int count = (int)(flags & 96) >> 5;
			switch (count)
			{
				case 0: m_count = 1; break;
				case 1: m_count = 2; break;
				case 2: m_count = 3; break;
				case 3: m_count = 5; break;
			}

			m_digStrength = m_value;
			m_texture = s_nutInfos[m_value].texture;
		//	m_texture = CFrameData.Instance.Content.Load<Texture2D>(s_nutInfos[m_value].filename);
		}

		public CNut()
		{
		}

		public CNut(int in_value)
		{
			m_value = in_value;
			m_digStrength = in_value;
			m_texture = s_nutInfos[m_value].texture;// CFrameData.Instance.Content.Load<Texture2D>(s_nutInfos[m_value].filename);
			m_flipped = (CFrameData.Instance.Random.Next(2) == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			m_count = CFrameData.Instance.Random.Next(100);
			if (m_count < 65 + m_value) m_count = 1;		// 65% chances + 1% chance per value
			else if (m_count < 85 + m_value) m_count = 2;	// 20% chances - 1% chance per value
			else if (m_count < 95 + m_value) m_count = 3;	// 10% chances - 1% chance per value
			else m_count = 5;								// 5% chances - 1% chance per value
		}


		public override void Update() 
		{
		}

		public override bool IsSameInventoryType(IEntity in_entity)
		{
			if (in_entity.GetType() == typeof(CNut))
			{
				CNut otherNut = in_entity as CNut;
				if (otherNut.m_value == m_value)
				{
					return true;
				}
			}
			return false;
		}

		public override bool Dig(IEntity digger) 
		{
			m_digStrength -= digger.GetDiggingStrength();
			if (m_digStrength <= 0)
			{
				m_digStrength = 0;
				return true;
			}
			return false; 
		}

		void DrawNut(Vector2 m_offset)
		{
			float scale = .5f * CSnowfield.INV_TILE_SCALE;
			CFrameData.Instance.SpriteBatch.Draw(m_texture, Position + m_offset, s_srcRectShadow,
				Color.White, Angle, s_origin, scale, m_flipped, 0);
			CFrameData.Instance.SpriteBatch.Draw(m_texture, Position + m_offset, s_srcRectBody,
				s_nutInfos[m_value].colors[0], Angle, s_origin, scale, m_flipped, 0);
			if (s_nutInfos[m_value].colors.Count() >= 2)
			{
				CFrameData.Instance.SpriteBatch.Draw(m_texture, Position + m_offset, s_srcRectCap,
					s_nutInfos[m_value].colors[1], Angle, s_origin, scale, m_flipped, 0);
			}
			if (s_nutInfos[m_value].colors.Count() >= 3)
			{
				CFrameData.Instance.SpriteBatch.Draw(m_texture, Position + m_offset, s_srcRectExtra,
					s_nutInfos[m_value].colors[1], Angle, s_origin, scale, m_flipped, 0);
			}
		}

		public override void DrawInventoryItem(Vector2 screenPos, float? preferedSize, Color? in_color, bool centered)
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

			Color color = Color.White;
			if (in_color != null) color = in_color.Value;

			float scale = .5f;
			if (preferedSize != null)
			{
				scale = preferedSize.Value / ((float)m_texture.Width / 2);
			}
			Vector2 origin = Vector2.Zero;
			if (centered)
			{
				origin = new Vector2(m_texture.Width / 4, m_texture.Height / 4);
			}

			sb.Draw(m_texture, screenPos, s_srcRectShadow, color
				, 0, origin, scale, SpriteEffects.None, 0);
			sb.Draw(m_texture, screenPos, s_srcRectBody, new Color(s_nutInfos[m_value].colors[0].ToVector4() * color.ToVector4())
				, 0, origin, scale, SpriteEffects.None, 0);
			if (s_nutInfos[m_value].colors.Count() >= 2)
			{
				sb.Draw(m_texture, screenPos, s_srcRectCap, new Color(s_nutInfos[m_value].colors[1].ToVector4() * color.ToVector4())
					, 0, origin, scale, SpriteEffects.None, 0);
			}
			if (s_nutInfos[m_value].colors.Count() >= 3)
			{
				sb.Draw(m_texture, screenPos, s_srcRectExtra, new Color(s_nutInfos[m_value].colors[1].ToVector4() * color.ToVector4())
					, 0, origin, scale, SpriteEffects.None, 0);
			}
		}

		public override void RenderSens()
		{
			for (int i = 0; i < s_countPos[m_count].Count(); ++i)
			{
				DrawNut(s_countPos[m_count][i]);
			}
		}
	}
}
