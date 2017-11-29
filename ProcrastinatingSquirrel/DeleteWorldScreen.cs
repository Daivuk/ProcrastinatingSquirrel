using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DK8;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel
{
	class DeleteWorldScreen
	{
		static public DeleteWorldScreen Instance;

		string[] m_credits = new string[]{
			"Are you sure you want to",
			"Delete selected world?"
		};

		public DeleteWorldScreen()
		{
			Instance = this;
		}

		public void Update()
		{
		}

		Vector2 m_tmpV2 = Vector2.Zero;
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
			m_tmpV2.X = (float)fd.Graphics.PreferredBackBufferWidth / 2;
			m_tmpV2.Y = (float)fd.Graphics.PreferredBackBufferHeight / 2 -
				(float)m_credits.Count() * .5f * 64;
			int cur = 0;
			foreach (string choice in m_credits)
			{
				SquirrelHelper.DrawString(choice, m_tmpV2, Globals.TextColor,
					SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
				m_tmpV2.Y += 64;
				++cur;
			}
            
			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Delete",
				new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnB : fd.CommonResources.rectBtnEsc, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Cancel",
				new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
			sb.End();
		}
	}
}
