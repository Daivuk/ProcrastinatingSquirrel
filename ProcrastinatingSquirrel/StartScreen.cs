using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DK8;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel
{
	class StartScreen
	{
		public static StartScreen Instance;

		bool m_firstFrame = true;
		CAnimStringBubble m_txtPressStart = new CAnimStringBubble("start", "Press Start");
		Vector2 m_textPos;

		public StartScreen()
		{
			if (Instance != null)
			{
				if (m_txtPressStart != null) m_txtPressStart = new CAnimStringBubble("start", "Press Start");
			}
			Instance = this;
			m_textPos = new Vector2(
				(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
				(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2);
		}

		public void Update()
		{
			if (m_firstFrame)
			{
				m_firstFrame = false;
				m_txtPressStart.StartAnimFromCurrent("Press Start", .5f, .10f, DK8.eAnimType.LINEAR);
			}
		}

		public void Render()
		{
			if (m_firstFrame) return;

			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			SquirrelHelper.DrawString(m_txtPressStart, m_textPos, Globals.TextColor, 
				SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
			sb.End();
		}
	}
}
