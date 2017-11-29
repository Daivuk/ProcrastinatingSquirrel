using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;
using System.Threading;
using ProcrastinatingSquirrel;


namespace DK8
{
	class SDirectionnalLight0
	{
		public bool IsEnabled = false;
		public Vector3 DiffuseColor = Vector3.One;
		public Vector3 AmbientColor = Vector3.Zero;
		public Vector3 Direction = Vector3.Forward;
	}

	class CFrameData
	{
		CInputMgr m_inputMgr;
		public CInputMgr InputMgr
		{
			get {return m_inputMgr;}
		}

		GameTime m_gameTime;
		public float GetDeltaSecond()
		{
			return (float)m_gameTime.ElapsedGameTime.TotalSeconds;
		}

		GraphicsDeviceManager m_graphics;
		public GraphicsDeviceManager Graphics
		{
			get { return m_graphics; }
		}

		SpriteBatch m_spriteBatch;
		public SpriteBatch SpriteBatch
		{
			get { return m_spriteBatch; }
		}

		CCommonResources m_commonResources;
		public CCommonResources CommonResources
		{
			get { return m_commonResources; }
		}

		ContentManager m_content;
		public ContentManager Content
		{
			get { return m_content; }
		}

		Matrix m_viewMatrix;
		Matrix m_projectionMatrix;
		Vector3 m_camPos;
		public Matrix ViewMatrix
		{
			get { return m_viewMatrix; }
		}
		public Matrix ProjectionMatrix
		{
			get { return m_projectionMatrix; }
		}
		public Vector3 CameraPosition
		{
			get { return m_camPos; }
		}

#if !XBOX
		CCursor m_cursor = null;
		public CCursor Cursor { get { return m_cursor; } }
#endif

		Random m_random = new Random();
		public Random Random { get { return m_random; } set { m_random = value; } }
		public void SetRandomSeed(int seed) { m_random = new Random(seed); }
		public void Randomize() { m_random = new Random(); }

		Vector2 m_screenCenter = Vector2.Zero;
		public Vector2 ScreenCenter { get { return m_screenCenter; } }

		Vector2 m_screenSize = Vector2.Zero;
		public Vector2 ScreenSize { get { return m_screenSize; } }

		public SDirectionnalLight0 DirectionnalLight0 = new SDirectionnalLight0();

		static public CFrameData Instance = null;

		public CFrameData(GraphicsDeviceManager graphics, ContentManager content)
		{
			m_content = content;
			m_graphics = graphics;
			m_inputMgr = new CInputMgr();
			m_spriteBatch = new SpriteBatch(m_graphics.GraphicsDevice);
			m_commonResources = new CCommonResources(m_content, this);
			m_screenCenter.X = (float)m_graphics.GraphicsDevice.Viewport.Width * .5f;
			m_screenCenter.Y = (float)m_graphics.GraphicsDevice.Viewport.Height * .5f;
			m_screenSize.X = (float)m_graphics.GraphicsDevice.Viewport.Width;
			m_screenSize.Y = (float)m_graphics.GraphicsDevice.Viewport.Height;
		}

		public void Update(GameTime gameTime)
		{
			m_gameTime = gameTime;

			// Update anims
			IAnimatable.UpdateAnims();

			// Update inputs
			m_inputMgr.Update(gameTime);
		}

		// Setup a camera view
		public void SetCameraView(
			Vector3 position,
			Vector3 lookAt,
			Vector3 up,
			float fov,
			float near,
			float far)
		{
			m_graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			float aspectRatio =
				(float)m_graphics.GraphicsDevice.Viewport.Width /
				(float)m_graphics.GraphicsDevice.Viewport.Height;
			m_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(fov),
				aspectRatio, near, far);
			m_viewMatrix = Matrix.CreateLookAt(position, lookAt, up);
			m_camPos = position;
		}

		public void ResetStorage()
		{
			//m_storageDevice = null;
		}

		public void WhoTheHellRemoveStorageDevice()
		{
            // Pop error, and say that the game will continue without the save capabilities
     /*       List<string> MBOPTIONS = new List<string>();
			MBOPTIONS.Add("Exit Game");
			while (Guide.IsVisible) Thread.Sleep(100);
			IAsyncResult result = Guide.BeginShowMessageBox(
				"Storage Device Full, Corrupted or Removed",
				"An error occured while saving your game. The current Storage device might be:\nFull, Corrupted or has been Removed intentionnaly.\n\nYour only option now is to exit the game. Sorry =)",
				MBOPTIONS, 0, MessageBoxIcon.Alert, null, null);
			result.AsyncWaitHandle.WaitOne();
		    Game1.Instance.Exit();

			WorkerThread.Instance.Stop();*/
		}

		IAsyncResult BeginShowSelector(PlayerIndex in_playerIndex)
		{
	/*		try
			{
				IAsyncResult result = StorageDevice.BeginShowSelector(in_playerIndex, null, null);
				return result;
			}
			catch*/
			{
				return null;
			}
		}
        /*
		StorageDevice m_storageDevice = null;
		public StorageDevice StorageDevice
		{
			get
			{
				if (m_storageDevice != null) return m_storageDevice;
				while (true)
				{
					while (Guide.IsVisible) Thread.Sleep(100);
					IAsyncResult result;
					while ((result = BeginShowSelector(InputMgr.PlayerIndex)) == null) Thread.Sleep(100);
				//	IAsyncResult result = StorageDevice.BeginShowSelector(InputMgr.PlayerIndex, null, null);
					result.AsyncWaitHandle.WaitOne();
					StorageDevice device = StorageDevice.EndShowSelector(result);
					if (device != null && device.IsConnected)
					{
						m_storageDevice = device;
						return m_storageDevice;
					}
					else
					{
						// Pop error, and say that the game will continue without the save capabilities
						List<string> MBOPTIONS = new List<string>();
						MBOPTIONS.Add("Select Device");
						MBOPTIONS.Add("Exit Game");
						while (Guide.IsVisible) Thread.Sleep(100);
						result = Guide.BeginShowMessageBox(
							"Selecting a Device", 
							"No device has been chosen. You have to select a device to continue playing. This game can not run without save capabilities.", 
							MBOPTIONS, 0, MessageBoxIcon.Alert, null, null);
						result.AsyncWaitHandle.WaitOne();
						int? option = Guide.EndShowMessageBox(result);
						if (option != null)
						{
							if (option == 1)
							{
								Profile.Instance.IsExitingGame = true;
								Game1.Instance.Exit();
							}
						}
					}
				}
			}
		}*/
	}
}
