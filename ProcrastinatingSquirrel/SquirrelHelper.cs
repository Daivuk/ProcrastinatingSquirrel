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
	class CAnimStringBubble: IAnim<string>
	{
		List<CAnimFloat> m_charactersAnim;
		public float CharacterScale(int characterIndex) { return m_charactersAnim[characterIndex].Value; }

		public CAnimStringBubble(string category, string value) : base(value) 
		{
			Category = category;
			m_charactersAnim = new List<CAnimFloat>(m_value.Length);
			int i;
			for (i = 0; i < m_value.Length; ++i)
			{
				m_charactersAnim.Add(new CAnimFloat(Category));
			}
		}
		public CAnimStringBubble(string category)
			: base("")
		{
			Category = category;
			m_charactersAnim = new List<CAnimFloat>(m_value.Length);
			int i;
			for (i = 0; i < m_value.Length; ++i)
			{
				m_charactersAnim.Add(new CAnimFloat(Category));
			}
		}
		public void KillAnims()
		{
			foreach (CAnimFloat animFloat in m_charactersAnim)
			{
				animFloat.Stop();
			}
			Stop();
		}

		protected override void Lerp(float percent)
		{
			int i;

			if ((AnimFlags & eAnimFlag.LOOP) == 0)
			{
				int previousPos = m_value.Length;
				m_value = m_to.Substring(0, (int)((float)m_to.Length * percent));
				for (i = previousPos; i < m_value.Length; ++i)
				{
					if (!m_charactersAnim[i].IsPlaying)
					{
						// Play that characters animation
						m_charactersAnim[i].StartAnim(
							1, 3.0f, .15f, 0, eAnimType.EASE_OUT, eAnimFlag.PINGPONG);
					}
				}
			}
			for (i = 0; i < m_value.Length; ++i)
			{
				if (!m_charactersAnim[i].IsPlaying)
				{
					m_charactersAnim[i].StartAnim(
						1 - (float)CFrameData.Instance.Random.NextDouble() * .2f,
						1 + (float)CFrameData.Instance.Random.NextDouble() * .2f,
						.5f + (float)CFrameData.Instance.Random.NextDouble() * .5f, 0, 
						eAnimType.EASE_BOTH, eAnimFlag.PINGPONG | eAnimFlag.LOOP);
				}
			}

			// Change the state, because we arrived at max, we want all the characters to wabble a little bit
			if (percent == 1 && (AnimFlags & eAnimFlag.LOOP) == 0)
			{
				StartAnim(To, To, 1, 0, eAnimType.LINEAR, eAnimFlag.LOOP);
				SetIgnoreDoneAnim();
			}
		}
	}

	class SquirrelHelper
	{
		static Color color_highlight = new Color(1, 1, 1, .25f);
		static Color color_shadow = new Color(0, 0, 0, .25f);
		static Vector2 highlightOffset = new Vector2(-1, -1);
		static Vector2 shadowOffset = new Vector2(2, 2);

		public enum eTEXT_ALIGN
		{
			LEFT,
			MIDDLE,
			RIGHT,
			TOP,
			CENTER,
			BOTTOM
		}

		static void AlignPosition(string text, ref Vector2 position, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
			AlignPosition(text, ref position, hAlign, vAlign, 1);
		}
		static void AlignPosition(string text, ref Vector2 position, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign, float in_scale)
		{
			in_scale *= Globals.HDScale;
			Vector2 stringSize = CFrameData.Instance.CommonResources.Font_AgentOrange.MeasureString(text) * in_scale;
			switch (hAlign)
			{
				case eTEXT_ALIGN.MIDDLE:
					position.X -= stringSize.X * .5f;
					break;
				case eTEXT_ALIGN.RIGHT:
					position.X -= stringSize.X;
					break;
			}
			switch (vAlign)
			{
				case eTEXT_ALIGN.CENTER:
					position.Y -= stringSize.Y * .5f;
					break;
				case eTEXT_ALIGN.BOTTOM:
					position.Y -= stringSize.Y;
					break;
			}
		}


		//
		//--- Print fonts with a highlight and a shadow
		//
		static public void DrawString(string text, Vector2 position, Color color, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
			DrawString(text, position, color, hAlign, vAlign, 1);
		}
		static public void DrawString(string text, Vector2 position, Color color, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign, float in_scale)
		{
			in_scale *= Globals.HDScale;
			AlignPosition(text, ref position, hAlign, vAlign, in_scale);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + highlightOffset * in_scale, new Color(
					color_highlight.R, color_highlight.G, color_highlight.B,
					(int)((float)color_highlight.A * ((float)color.A / 255.0f))),
					0, Vector2.Zero, in_scale, SpriteEffects.None, 0);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + shadowOffset * in_scale, new Color(
					color_shadow.R, color_shadow.G, color_shadow.B,
					(int)((float)color_shadow.A * ((float)color.A / 255.0f))),
					0, Vector2.Zero, in_scale, SpriteEffects.None, 0);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position, color,
					0, Vector2.Zero, in_scale, SpriteEffects.None, 0);
		}

		static public void DrawString(CAnimString text, Vector2 position, Color color, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
			AlignPosition(text.To, ref position, hAlign, vAlign);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text.Value, position + highlightOffset, new Color(
					color_highlight.R, color_highlight.G, color_highlight.B,
					(int)((float)color_highlight.A * ((float)color.A / 255.0f))),
					0, Vector2.Zero, Globals.HDScale, SpriteEffects.None, 0);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text.Value, position + shadowOffset, new Color(
					color_shadow.R, color_shadow.G, color_shadow.B,
					(int)((float)color_shadow.A * ((float)color.A / 255.0f))),
					0, Vector2.Zero, Globals.HDScale, SpriteEffects.None, 0);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text.Value, position, color,
					0, Vector2.Zero, Globals.HDScale, SpriteEffects.None, 0);
		}

		static public void DrawString(CAnimStringBubble text, Vector2 position, Color color, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
		//	if (!text.IsPlaying) text.StartAnim(text.Value, text.Value, 1, 0, eAnimType.LINEAR, eAnimFlag.LOOP);
		/*	if (!text.IsPlaying)
			{
				DrawString(text.Value, position, color, hAlign, vAlign);
				return;
			} */

			AlignPosition(text.To, ref position, hAlign, vAlign);

			Vector2 curPos = new Vector2();
			Vector2 stringSize = 
				CFrameData.Instance.CommonResources.Font_AgentOrange.MeasureString(text.To);

			// We will have to draw character per character
			int i;
			for (i = 0; i < text.To.Length; ++i)
			{
				Vector2 curSize = CFrameData.Instance.CommonResources.Font_AgentOrange.MeasureString(text.To.Substring(0, i));
				curPos.X = position.X + curSize.X;
				curPos.Y = position.Y + stringSize.Y * .5f;
				DrawString(text.To.Substring(i, 1), curPos, color, 0, new Vector2(0, stringSize.Y * .5f), 
					text.CharacterScale(i),
					SpriteEffects.None, 0, eTEXT_ALIGN.LEFT, eTEXT_ALIGN.TOP);
			}
		}

		static public void DrawString(string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
			AlignPosition(text, ref position, hAlign, vAlign);
			scale *= Globals.HDScale;
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + highlightOffset, color_highlight, rotation, origin, scale, effects, layerDepth);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + shadowOffset, color_shadow, rotation, origin, scale, effects, layerDepth);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position, color, rotation, origin, scale, effects, layerDepth);
		}

		static public void DrawString(string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, eTEXT_ALIGN hAlign, eTEXT_ALIGN vAlign)
		{
			AlignPosition(text, ref position, hAlign, vAlign);
			scale *= Globals.HDScale;
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + highlightOffset, color_highlight, rotation, origin, scale, effects, layerDepth);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position + shadowOffset, color_shadow, rotation, origin, scale, effects, layerDepth);
			CFrameData.Instance.SpriteBatch.DrawString(
				CFrameData.Instance.CommonResources.Font_AgentOrange,
				text, position, color, rotation, origin, scale, effects, layerDepth);
		}

		// Helper function to generate noise, diamond, not perfect
		static float SmoothInterpolate(float h1, float h2, float h3, float h4, float px, float py)
		{
			if (px <= .5f)
			{
				px = px * px * 2;
			}
			else
			{
				px = 1 - ((1 - px) * (1 - px)) * 2;
			}
			if (py <= .5f)
			{
				py = py * py * 2;
			}
			else
			{
				py = 1 - ((1 - py) * (1 - py)) * 2;
			}
			return
				h1 * (1 - px) * (1 - py) +
				h2 * (1 - px) * (py) +
				h3 * (px) * (py) +
				h4 * (px) * (1 - py);
		}

		static public void Fill(float[,] out_array, int width, int height, float value)
		{
			int x, y;
			for (y = 0; y < width; ++y)
			{
				for (x = 0; x < height; ++x)
				{
					out_array[x, y] = value;
				}
			}
		}

		static public void RadialGradient(float[,] out_array, int width, int height, int in_x, int in_y, float from, float to, float value)
		{
			int x, y;
			for (y = 0; y < width; ++y)
			{
				for (x = 0; x < height; ++x)
				{
					float disSqr = (float)((x - in_x) * (x - in_x) + (y - in_y) * (y - in_y));
					if (disSqr < to * to)
					{
						if (disSqr <= from * from)
						{
							out_array[x, y] = value;
						}
						else
						{
							float dis = (float)Math.Sqrt((double)disSqr);
							float percent = (dis - from) / (to - from);
							out_array[x, y] = MathHelper.Lerp(value, out_array[x, y], percent);
						}
					}
				}
			}
		}

		static public void AddHeight(float[,] out_array, float[,] in_array, int width, int height)
		{
			int x, y;
			for (y = 0; y < width; ++y)
			{
				for (x = 0; x < height; ++x)
				{
					out_array[x, y] += in_array[x, y];
				}
			}
		}

		static public float[,] GenerateNoise(int size, float amplitude, int width, int height)
		{
			float[,] array = new float[width + 1, height + 1];
			return GenerateNoise(array, size, amplitude, width, height);
		}
/*
		static public void GenerateNoiseWorker(object data)
		{
			CSnowfield.SLoadThreadData workData = data as CSnowfield.SLoadThreadData;

			float h1, h2, h3, h4;
			int x, y;
			int i, j;
			for (j = workData.startJ; j < workData.endJ; ++j)
			{
				for (i = 0; i < FIELD_SIZE; ++i)
				{
					m_tiles[i, j] = new CTile(new Vector2((float)i, (float)j));
				}
			}
		}*/

		static public float[,] GenerateNoise(float[,] array, int size, float amplitude, int width, int height)
		{
			float h1, h2, h3, h4;
			int x, y;
			int i, j;
			CFrameData fd = CFrameData.Instance;

			// First create the height points
			for (y = 0; y < height + 1; y += size)
			{
				for (x = 0; x < width + 1; x += size)
				{
					array[x, y] += (float)(fd.Random.NextDouble() * 2 - 1) * amplitude;
				}
			}

		//	CSnowfield.Instance.WorkingThreads(GenerateNoiseWorker, 

			// Now smooth the values between
			for (y = 0; y < height; y += size)
			{
				for (x = 0; x < width; x += size)
				{
					h1 = array[x, y];
					h2 = array[x, y + size];
					h3 = array[x + size, y + size];
					h4 = array[x + size, y];
					for (j = y; j < y + size; ++j)
					{
						for (i = x; i < x + size; ++i)
						{
							if (i == x && j == y) continue;
							float px = (float)(i - x) / (float)size;
							float py = (float)(j - y) / (float)size;
							if (px <= .5f)
							{
								px = px * px * 2;
							}
							else
							{
								px = 1 - ((1 - px) * (1 - px)) * 2;
							}
							if (py <= .5f)
							{
								py = py * py * 2;
							}
							else
							{
								py = 1 - ((1 - py) * (1 - py)) * 2;
							}
							array[i, j] = 
								h1 * (1 - px) * (1 - py) +
								h2 * (1 - px) * (py) +
								h3 * (px) * (py) +
								h4 * (px) * (1 - py);

						}
					}
				}
			}

			return array;
		}
	}
}
