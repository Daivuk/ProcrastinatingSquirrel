using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DK8;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel
{
	class GoalAchievedScreen
	{
		public static GoalAchievedScreen Instance;

		CAnimColor m_fadeOut = new CAnimColor(Color.Black);
		CAnimStringBubble[] m_txtLoading = null;
		public Vector2[] m_textPos;
		public int State = 0;

		public GoalAchievedScreen()
		{
			Instance = this;
			m_textPos = new Vector2[]{
				new Vector2(
					(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
					(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2 - 64),
				new Vector2(
					(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
					(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2),
				new Vector2(
					(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
					(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2 + 64),
			};
		}

		public void OnActivate()
		{
			State = 0;

			string text1 = "Goal Achieved!";
			string text2 = "You can now pass the winter";
			string text3 = "without worrying about food";
			m_txtLoading = new CAnimStringBubble[3];

			m_txtLoading[0] = new CAnimStringBubble("ui", text1);
			m_txtLoading[0].StartAnimFromCurrent(text1, 1, 1, DK8.eAnimType.LINEAR);

			m_txtLoading[1] = new CAnimStringBubble("ui", text2);
			m_txtLoading[1].StartAnimFromCurrent(text2, 1, 2, DK8.eAnimType.LINEAR);

			m_txtLoading[2] = new CAnimStringBubble("ui", text3);
			m_txtLoading[2].StartAnimFromCurrent(text3, 1, 3, DK8.eAnimType.LINEAR);

			m_fadeOut.StartAnim(Color.Transparent, Color.Black, 1, 0, eAnimType.LINEAR);
		}

		public void Update()
		{
		}

		public void Render()
		{
			if (m_txtLoading == null) return;

			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;
			float padding = 16;
			Rectangle safeFrame = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;

			if (State == 0 || State == 1)
			{
				sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

				if (State == 0)
				{
					sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, m_fadeOut.Value);

					SquirrelHelper.DrawString(m_txtLoading[0], m_textPos[0], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
					SquirrelHelper.DrawString(m_txtLoading[1], m_textPos[1], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
					SquirrelHelper.DrawString(m_txtLoading[2], m_textPos[2], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);

					sb.Draw(fd.CommonResources.Tex_Buttons,
						new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32),
                        fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter, 
                        Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
					SquirrelHelper.DrawString("Yay!",
						new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding),
						Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
				}
				else if (State == 1)
				{
					SquirrelHelper.DrawString(m_txtLoading[0], m_textPos[0], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
					SquirrelHelper.DrawString(m_txtLoading[1], m_textPos[1], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
					SquirrelHelper.DrawString(m_txtLoading[2], m_textPos[2], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);

					sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, m_fadeOut.Value);
				}
				sb.End();
			}
			else if (State == 2)
			{
				sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
					SquirrelHelper.DrawString(m_txtLoading[0], m_textPos[0], Globals.TextColor,
						SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);

					sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, m_fadeOut.Value);
				sb.End();
			}
		}

		internal void NextState()
		{
			NextState(null);
		}
		internal void NextState(IAnimatable in_anim)
		{
			if (State == 0)
			{
				State = 1;
				m_fadeOut.StartAnim(Color.Transparent, Color.Black, 1, 0, eAnimType.LINEAR);
				m_fadeOut.SetCallback(NextState, false);
			}
			else if (State == 1)
			{
				State = 2;
				m_fadeOut.Value = Color.Transparent;
				m_fadeOut.StartAnim(Color.Transparent, Color.Black, 1.5f, 6, eAnimType.LINEAR);
				m_fadeOut.SetCallback(NextState, false);

				string text1 = "One year later...";
				m_txtLoading[0] = new CAnimStringBubble("ui", text1);
				m_txtLoading[0].StartAnimFromCurrent(text1, 3, 1, DK8.eAnimType.EASE_OUT);
			}
			else if (State == 2)
			{
				State = 3;
			}
		}
	}
}
