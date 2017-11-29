using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;


namespace DK8
{
	class CCursor
	{
		Texture2D m_cursorTexture = null;
		public Texture2D CursorTexture
		{
			get { return m_cursorTexture; }
			set { m_cursorTexture = value; }
		}

		public bool IsVisible { get; set; }

		public CCursor()
		{
			IsVisible = false;
		}

		public void Draw(CFrameData frameData)
		{
			if (IsVisible)
			{
				frameData.SpriteBatch.Begin();
				frameData.SpriteBatch.Draw(m_cursorTexture, frameData.InputMgr.MousePos, Color.Beige);
				frameData.SpriteBatch.End();
			}
		}
	}
}
