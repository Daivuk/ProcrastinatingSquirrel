using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;
using ProcrastinatingSquirrel.Entities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace ProcrastinatingSquirrel
{
	class WorldSelectScreen
	{
		public static WorldSelectScreen Instance = null;
		CAnimFloat m_cursorAnim = new CAnimFloat("igm", 1);
		Texture2D texInvCursor;
		Vector2 m_cursorOriginL = new Vector2(32, 32);
		Vector2 m_cursorOriginR = new Vector2(0, 32);
		Vector2 m_cursorScale = new Vector2(10, 1);
		Color m_unselectedColor = new Color(Globals.TextColor.ToVector4() * .85f);
		Rectangle m_srcRectLeft = new Rectangle(0, 0, 32, 64);
		Rectangle m_srcRectRight = new Rectangle(32, 0, 32, 64);
		static SoundEffect s_sndMenuNavigate = CFrameData.Instance.Content.Load<SoundEffect>("sounds/menuNavigate");
		List<string> m_saves = null;
		List<string> m_menuChoices = null;
		int m_currentChoiceId = 0;
		static Color s_cantSelectColor = new Color(Globals.TextColor.ToVector3() * .25f);
		public int CurrentChoiceId
		{
			get { return m_currentChoiceId; }
		}

		public WorldSelectScreen()
		{
			Instance = this;
			texInvCursor = CFrameData.Instance.Content.Load<Texture2D>("textures/invCursor");
		}

		public void Update()
		{
			CFrameData fd = CFrameData.Instance;
			if (m_menuChoices.Count() == 1) return;

			if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickDown) ||
				fd.InputMgr.IsButtonFirstDown(Buttons.DPadDown) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.S) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Down))
			{
				++m_currentChoiceId;
				if (m_currentChoiceId >= m_menuChoices.Count()) m_currentChoiceId = 0;
				s_sndMenuNavigate.Play();
				if (m_saves[m_currentChoiceId] == Profile.Instance.CurrentSaveName)
				{
					++m_currentChoiceId;
					if (m_currentChoiceId >= m_menuChoices.Count()) m_currentChoiceId = 0;
				}
			}
			else if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickUp) ||
				fd.InputMgr.IsButtonFirstDown(Buttons.DPadUp) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.W) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Up))
			{
				--m_currentChoiceId;
				if (m_currentChoiceId < 0) m_currentChoiceId = m_menuChoices.Count() - 1;
				s_sndMenuNavigate.Play();
				if (m_saves[m_currentChoiceId] == Profile.Instance.CurrentSaveName)
				{
					--m_currentChoiceId;
					if (m_currentChoiceId < 0) m_currentChoiceId = m_menuChoices.Count() - 1;
				}
			}
		}

		public string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public void OnActivate()
		{
			m_cursorAnim.StartAnim(0, 16, .5f, 0, eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
			m_currentChoiceId = 0;
			m_menuChoices = Profile.Instance.Saves.ToList();
			m_saves = Profile.Instance.Saves.ToList();
			for (int i = 0; i < m_menuChoices.Count(); ++i)
			{
				string name = m_menuChoices[i];
				name = ReplaceFirst(name, "_", "/");
				name = ReplaceFirst(name, "_", "/");
				name = ReplaceFirst(name, "_", " ");
				name = ReplaceFirst(name, "_", ":");
				name = ReplaceFirst(name, "_", ":");
				name = ReplaceFirst(name, "_", " ");
				m_menuChoices[i] = name;
			}
			m_saves.Reverse();
			m_menuChoices.Reverse();
			if (m_saves[m_currentChoiceId] == Profile.Instance.CurrentSaveName)
			{
				++m_currentChoiceId;
				if (m_currentChoiceId >= m_saves.Count()) m_currentChoiceId = 0;
			}
		}

		Vector2 m_tmpV2 = Vector2.Zero;
		Vector2 m_tmpV2_2 = Vector2.Zero;
		public void Render()
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;
			float padding = 16;
			Rectangle safeFrame = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			// Fade out the back
			sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, .75f));

			// Text
			if (m_menuChoices.Count() == 1)
			{
				m_tmpV2.X = (float)fd.Graphics.PreferredBackBufferWidth / 2;
				m_tmpV2.Y = (float)fd.Graphics.PreferredBackBufferHeight / 2;

					SquirrelHelper.DrawString("You do not currently have other saves", m_tmpV2,
						Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER, 1);
			}
			else
			{
				m_tmpV2.X = (float)fd.Graphics.PreferredBackBufferWidth / 2;
				m_tmpV2.Y = (float)fd.Graphics.PreferredBackBufferHeight / 2/* -
					(float)m_menuChoices.Count()  * .5f * 64*/;
				m_tmpV2.Y -= (float)m_currentChoiceId * 64;
				int cur = 0;
				for (int i = 0; i < m_saves.Count(); ++i)
				{
					string choice = m_menuChoices[i];
					if (m_saves[i] == Profile.Instance.CurrentSaveName)
					{
						SquirrelHelper.DrawString(choice, m_tmpV2,
							s_cantSelectColor,
							SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER,
							(cur == m_currentChoiceId) ? 1 : .90f);
					}
					else
					{
						SquirrelHelper.DrawString(choice, m_tmpV2,
							(cur == m_currentChoiceId) ? Globals.TextColor : m_unselectedColor,
							SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER,
							(cur == m_currentChoiceId) ? 1 : .90f);
					}
					m_tmpV2.Y += 64;
					++cur;
				}

				// Cursor
				m_tmpV2.X = (float)fd.Graphics.PreferredBackBufferWidth / 2;
				m_tmpV2.Y = (float)fd.Graphics.PreferredBackBufferHeight / 2/* -
					(float)m_menuChoices.Count() * .5f * 64*/;
				m_tmpV2.Y -= (float)m_currentChoiceId * 64;
				m_tmpV2.Y += (float)m_currentChoiceId * 64;
				m_tmpV2_2 = fd.CommonResources.Font_AgentOrange.MeasureString(m_menuChoices[m_currentChoiceId]);

				m_tmpV2.X -= m_tmpV2_2.X * .5f - m_cursorAnim.Value;
				sb.Draw(texInvCursor, m_tmpV2, m_srcRectLeft, Globals.IconColor,
					0, m_cursorOriginL, 1, SpriteEffects.None, 0);

				m_tmpV2.X += m_tmpV2_2.X - m_cursorAnim.Value * 2;
				sb.Draw(texInvCursor, m_tmpV2, m_srcRectRight, Globals.IconColor,
					0, m_cursorOriginR, 1, SpriteEffects.None, 0);

				sb.Draw(fd.CommonResources.Tex_Buttons,
					new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32),
                    fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
				SquirrelHelper.DrawString("Select",
					new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding),
					Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

				sb.Draw(fd.CommonResources.Tex_Buttons,
					new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32 - 48),
					fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnX : fd.CommonResources.rectBtnDel, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
				SquirrelHelper.DrawString("Delete Selected",
					new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding - 48),
					Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
			}

			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnB : fd.CommonResources.rectBtnEsc, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Back",
				new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

			sb.End();
		}

		public string Selection { get { return m_saves[m_currentChoiceId]; } }
	}
}
