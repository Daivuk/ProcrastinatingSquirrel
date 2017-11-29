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
	class CCommonResources
	{
		public SpriteFont Font_AgentOrange;
		public SpriteFont Font_System;

		public BasicEffect Effect_Basic;

	/*	public VertexDeclaration Decl_PositionColor;
		public VertexDeclaration Decl_PositionTexture;
		public VertexDeclaration Decl_PositionColorTexture;
		public VertexDeclaration Decl_PositionNormalTexture;*/

		public Texture2D Tex_White = null;
		public Texture2D Tex_Buttons = null;

		public Rectangle rectBtnA = new Rectangle(0, 0, 64, 64);
		public Rectangle rectBtnB = new Rectangle(64, 0, 64, 64);
		public Rectangle rectBtnX = new Rectangle(128, 0, 64, 64);
		public Rectangle rectBtnY = new Rectangle(192, 0, 64, 64);
		public Rectangle rectBtnRT = new Rectangle(256, 0, 64, 64);
		public Rectangle rectBtnTab = new Rectangle(0, 64, 64, 64);
		public Rectangle rectBtnSpace = new Rectangle(64, 64, 128, 64);
		public Rectangle rectBtnEsc = new Rectangle(192, 64, 64, 64);
		public Rectangle rectBtnEnter = new Rectangle(256, 64, 128, 64);
		public Rectangle rectBtnDel = new Rectangle(384, 64, 64, 64);
		public Vector2 btnOrigin = new Vector2(32, 32);

		public CCommonResources(ContentManager content, CFrameData frameData)
		{
			// Textures
			Tex_White = content.Load<Texture2D>("textures\\white");
			Tex_Buttons = content.Load<Texture2D>("textures\\buttons");

			// Models

			// Vertex declarations
		/*	Decl_PositionColor = new VertexDeclaration(
				frameData.Graphics.GraphicsDevice,
				VertexPositionColor.VertexElements);
			Decl_PositionTexture = new VertexDeclaration(
				frameData.Graphics.GraphicsDevice,
				VertexPositionTexture.VertexElements);
			Decl_PositionColorTexture = new VertexDeclaration(
				frameData.Graphics.GraphicsDevice,
				VertexPositionColorTexture.VertexElements);
			Decl_PositionNormalTexture = new VertexDeclaration(
				frameData.Graphics.GraphicsDevice,
				VertexPositionNormalTexture.VertexElements);*/

			// Shaders
			Effect_Basic = new BasicEffect(frameData.Graphics.GraphicsDevice/*, null*/);

			// Font
			Font_System = content.Load<SpriteFont>("fonts\\System");
			Font_AgentOrange = content.Load<SpriteFont>("fonts\\AgentOrange");
		}
	}
}
