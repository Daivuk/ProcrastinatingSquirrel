using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DK8;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProcrastinatingSquirrel
{
	class LoadingScreen
	{
		public static LoadingScreen Instance;

		CAnimStringBubble m_txtLoading = null;
		public Vector2 m_textPos;

		public LoadingScreen()
		{
			Instance = this;
			m_textPos = new Vector2(
				(float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
				(float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2);
		}

		public void Update()
		{
		}

		public void Render()
		{
			if (m_txtLoading == null) return;

			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			SquirrelHelper.DrawString(m_txtLoading, m_textPos, Globals.TextColor,
				SquirrelHelper.eTEXT_ALIGN.MIDDLE, SquirrelHelper.eTEXT_ALIGN.CENTER);
			sb.End();
		}

		public void StartLoading()
		{
			StartLoading("Loading");
		}
		public void StartLoading(string in_text)
		{
			m_txtLoading = new CAnimStringBubble("load", in_text);
			m_txtLoading.StartAnimFromCurrent(in_text, 0.001f, 0, DK8.eAnimType.LINEAR);
		}
	}
}
