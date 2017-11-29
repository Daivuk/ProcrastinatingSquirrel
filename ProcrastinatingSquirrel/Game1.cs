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
using System.Threading;

namespace ProcrastinatingSquirrel
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		public static Game1 Instance;
		public GraphicsDeviceManager graphics;
		public enum eGAME_STATE
		{
			LOADING,
			LOADING_PROFILE,
			INTRO_SCREEN,
			MAIN_MENU,
			IN_GAME,
			IN_GAME_MENU,
			INVENTORY,
			STORE,
			CREDITS,
			DELETE_WORLD,
			DELETING_WORLD,
			CREATE_NEW_WORLD,
			WORLD_SELECT,
			GOAL_ACHIEVED_SCREEN,
			SELECTING_WORLD,
			SETTINGS
		}
		public static eGAME_STATE GameState = eGAME_STATE.INTRO_SCREEN;
		DateTime timeStart = DateTime.Now;
		SoundEffect s_sndMenuNavigate = null;
		CAnimFloat m_musicIgnoreSwitch = new CAnimFloat();
	//	MediaLibrary m_musics;

		Song m_ingame1;
		Song m_ingame2;

		public Game1()
		{
#if XBOX
			int[] affinity = new int[] { 1 };
			Thread.CurrentThread.SetProcessorAffinity(affinity);
#endif

			Instance = this;
			//this.Components.Add(new GamerServicesComponent(this));
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1280;
			graphics.PreferredBackBufferHeight = 720;
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
#if XBOX
			// No fuck it, no HD supports... ~30 FPS no thanks
		/*	if (graphics.GraphicsDevice.DisplayMode.Width == 1920 &&
				graphics.GraphicsDevice.DisplayMode.Height == 1080)
			{
				graphics.PreferredBackBufferWidth = 1920;
				graphics.PreferredBackBufferHeight = 1080;
				graphics.ApplyChanges();
				Globals.HDScale = 1080.0f / 720.0f;
			}*/
#endif
#if DEBUG
			//Guide.SimulateTrialMode = true;
#endif

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			new WorkerThread();

			// Create our frame data
			CFrameData.Instance = new CFrameData(graphics, Content);

			s_sndMenuNavigate = CFrameData.Instance.Content.Load<SoundEffect>("sounds/menuNavigate");

			new LoadingScreen();
			new StartScreen();
			new InGameMenu();
			new CreditScreen();
			new DeleteWorldScreen();
			new WorldSelectScreen();
			new GoalAchievedScreen();
		}

		int repeatCount = 0;

		void MediaPlayer_ActiveSongChanged(object sender, EventArgs e)
		{
			if (m_musicIgnoreSwitch.IsPlaying) return;
			m_musicIgnoreSwitch.StartAnim(1, 0, 5, 0, eAnimType.LINEAR);
			if (repeatCount % 15 == 0)
			{
				MediaPlayer.Volume = .5f;
				MediaPlayer.Play(m_ingame1);
			}
			else
			{
				MediaPlayer.Volume = .10f;
				if (m_ingame2 == null)
				{
					m_ingame2 = CFrameData.Instance.Content.Load<Song>("musics/ingame2");
				}
				MediaPlayer.Play(m_ingame2);
			}
			++repeatCount;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
		}

		public void LoadProfile()
		{
			new Profile();
			new SettingScreen();

			MediaPlayer.IsMuted = !Profile.Instance.MusicOn;
			SoundEffect.MasterVolume = Profile.Instance.SoundsOn ? 1 : 0;
		}

		public void CreateSave()
		{
			Profile.Instance.CreateNewSave();
		}

		public void SaveAndExit()
		{
			CSnowfield.Instance.Save();
			Profile.Instance.SetToDefaultWorld();
			this.Exit();
		}

		public void SaveAndContinue()
		{
			CSnowfield.Instance.Save();
			GameState = eGAME_STATE.IN_GAME;
		}

		public void GoBackToStartScreenNoSave()
		{
			// Delete world
			if (Store.Instance != null) Store.Instance.Dispose();
			if (Inventory.Instance != null) Inventory.Instance.Dispose();
			if (CSnowfield.Instance != null) CSnowfield.Instance.Dispose();
			IAnimatable.StopAllAnims();

			CFrameData.Instance.ResetStorage();

			new StartScreen();
			GameState = eGAME_STATE.INTRO_SCREEN;
		}

		bool newWorldCreated = false;
		public void SaveThenCreateNew()
		{
			CSnowfield.Instance.Save();

			// Delete world
			if (Store.Instance != null) Store.Instance.Dispose();
			if (Inventory.Instance != null) Inventory.Instance.Dispose();
			if (CSnowfield.Instance != null) CSnowfield.Instance.Dispose();
			IAnimatable.StopAllAnims();

			// Create new world
			Profile.Instance.CreateNewSave();

			newWorldCreated = true;
		}

		private void SelectWorld()
		{
			CSnowfield.Instance.Save();

			// Delete world
			if (Store.Instance != null) Store.Instance.Dispose();
			if (Inventory.Instance != null) Inventory.Instance.Dispose();
			if (CSnowfield.Instance != null) CSnowfield.Instance.Dispose();
			IAnimatable.StopAllAnims();

			Profile.Instance.CurrentSaveName = WorldSelectScreen.Instance.Selection;
			Profile.Instance.SetToDefaultWorld();

			worldSelected = true;
		}

		bool deleteCompleted = false;
		private void DeleteWorld()
		{
			// Delete world
			Profile.Instance.DeleteSave(worldToDelete);
			deleteCompleted = true;
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		bool musicPlaying = false;
		int time = 0;
		int fps = 0;
		int showFps = 0;
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Profile.Instance != null && graphics != null)
            {
                if (graphics.IsFullScreen != Profile.Instance.FullscreenOn)
                {
                    if (Profile.Instance.FullscreenOn)
                    {
                        graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    }
                    else
                    {
                        graphics.PreferredBackBufferWidth = 1280;
                        graphics.PreferredBackBufferHeight = 720;
                    }

                    if (CSnowfield.Instance != null)
                    {
                        CSnowfield.Instance.m_rtSens = new RenderTarget2D(graphics.GraphicsDevice, graphics.PreferredBackBufferWidth,
                            graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

                        CSnowfield.Instance.m_screenDim = new Vector2(
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferWidth,
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferHeight);

                        CSnowfield.Instance.textIconPos = new Vector2(
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferWidth * .5f,
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferHeight * .25f);
                    }

                    if (GoalAchievedScreen.Instance != null)
                    {
                        GoalAchievedScreen.Instance.m_textPos = new Vector2[]{
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

                    if (LoadingScreen.Instance != null)
                    {
                        LoadingScreen.Instance.m_textPos = new Vector2(
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferWidth / 2,
                            (float)CFrameData.Instance.Graphics.PreferredBackBufferHeight / 2);
                    }

                    graphics.ToggleFullScreen();
                }
            }

            //if (Guide.IsVisible) return;

            // Hey, always make sure we are currently signed in.
            // Or save and go back to start screen
            if (GameState != eGAME_STATE.INTRO_SCREEN)
			{
				if (!MakeSurePlayerIsSignedIn(CFrameData.Instance.InputMgr.PlayerIndex))
				{
					GoBackToStartScreenNoSave();
				}
			}

			if (time == 0) time = System.Environment.TickCount;
			if (System.Environment.TickCount - time >= 1000)
			{
				time += 1000;
				showFps = fps;
				fps = 0;
			}

			//if (Guide.IsVisible) return;
			// Update our modules
			CFrameData fd = CFrameData.Instance;
			fd.Update(gameTime);

			if (!MediaPlayer.GameHasControl) musicPlaying = false;
			if (MediaPlayer.GameHasControl && !musicPlaying && CSnowfield.Instance != null && Profile.Instance.MusicOn)
			{
				if (!CSnowfield.Instance.IsSquirrelHome)
				{
					musicPlaying = true;
					if (m_ingame1 == null) m_ingame1 = CFrameData.Instance.Content.Load<Song>("musics/ingame1");
					m_musicIgnoreSwitch.StartAnim(1, 0, 5, 0, eAnimType.LINEAR);
				//	MediaPlayer.IsRepeating = true;
					MediaPlayer.Volume = .5f;
					MediaPlayer.Play(m_ingame1);
					MediaPlayer.ActiveSongChanged += new EventHandler<EventArgs>(MediaPlayer_ActiveSongChanged);
				}
			}

			if (currentlyCheckingForPlayerIndex && GameState == eGAME_STATE.INTRO_SCREEN)
			{
				currentlyCheckingForPlayerIndex = false;
				if (MakeSurePlayerIsSignedIn(isCheckingForPlayerIndex))
				{
					CFrameData.Instance.InputMgr.PlayerIndex = isCheckingForPlayerIndex;
					GameState = eGAME_STATE.LOADING_PROFILE;
					LoadingScreen.Instance.StartLoading("Loading...");
					WorkerThread.Instance.AddWork(LoadProfile);
				}
			}

			// Update depending the state
			switch (GameState)
			{
				case eGAME_STATE.SETTINGS:
                    {
                        SettingScreen.Instance.Update();
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							s_sndMenuNavigate.Play();
							GameState = eGAME_STATE.IN_GAME_MENU;
						}
						break;
					}
				case eGAME_STATE.GOAL_ACHIEVED_SCREEN:
					{
						GoalAchievedScreen.Instance.Update();
						if ((fd.InputMgr.IsButtonFirstDown(Buttons.A) || fd.InputMgr.IsKeyFirstDown(Keys.Enter)) && GoalAchievedScreen.Instance.State == 0)
						{
							GoalAchievedScreen.Instance.NextState();
						}
						if (GoalAchievedScreen.Instance.State == 3)
						{
							GameState = eGAME_STATE.CREATE_NEW_WORLD;
							IAnimatable.UnpauseCategory("game");
							LoadingScreen.Instance.StartLoading("Loading");
							newWorldCreated = false;
							WorkerThread.Instance.AddWork(SaveThenCreateNew);
							musicPlaying = false;
						}
						break;
					}
				case eGAME_STATE.INTRO_SCREEN:
					{
						StartScreen.Instance.Update();
					/*	if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
						{
							if (MakeSurePlayerIsSignedInOrTry(PlayerIndex.One))
							{*/
								CFrameData.Instance.InputMgr.PlayerIndex = PlayerIndex.One;
								GameState = eGAME_STATE.LOADING_PROFILE;
								LoadingScreen.Instance.StartLoading("Loading...");
								WorkerThread.Instance.AddWork(LoadProfile);
					/*		}
						}
						else if (GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
						{
							if (MakeSurePlayerIsSignedInOrTry(PlayerIndex.Two))
							{
								CFrameData.Instance.InputMgr.PlayerIndex = PlayerIndex.Two;
								GameState = eGAME_STATE.LOADING_PROFILE;
								LoadingScreen.Instance.StartLoading("Loading...");
								WorkerThread.Instance.AddWork(LoadProfile);
							}
						}
						else if (GamePad.GetState(PlayerIndex.Three).Buttons.Start == ButtonState.Pressed)
						{
							if (MakeSurePlayerIsSignedInOrTry(PlayerIndex.Three))
							{
								CFrameData.Instance.InputMgr.PlayerIndex = PlayerIndex.Three;
								GameState = eGAME_STATE.LOADING_PROFILE;
								LoadingScreen.Instance.StartLoading("Loading...");
								WorkerThread.Instance.AddWork(LoadProfile);
							}
						}
						else if (GamePad.GetState(PlayerIndex.Four).Buttons.Start == ButtonState.Pressed)
						{
							if (MakeSurePlayerIsSignedInOrTry(PlayerIndex.Four))
							{
								CFrameData.Instance.InputMgr.PlayerIndex = PlayerIndex.Four;
								GameState = eGAME_STATE.LOADING_PROFILE;
								LoadingScreen.Instance.StartLoading("Loading...");
								WorkerThread.Instance.AddWork(LoadProfile);
							}
						}*/
						break;
					}
				case eGAME_STATE.LOADING:
					{
						break;
					}
				case eGAME_STATE.LOADING_PROFILE:
					{
						if (Profile.Instance != null)
						{
							if (Profile.Instance.IsLoaded)
							{
								if (Profile.Instance.CurrentSaveName == "")
								{
									Profile.Instance.IsLoaded = false;
									WorkerThread.Instance.AddWork(CreateSave);
								}
								else
								{
									// Load the game now
									if (Store.Instance != null) Store.Instance.Dispose();
									if (Inventory.Instance != null) Inventory.Instance.Dispose();
									if (CSnowfield.Instance != null) CSnowfield.Instance.Dispose();
									IAnimatable.StopAllAnims();
									new Store();
									new Inventory(); // Create our inventory instance
									new CSnowfield(); // Playfield
									GameState = eGAME_STATE.IN_GAME;
								}
							}
						}
						break;
					}
				case eGAME_STATE.IN_GAME:
					{
						CSnowfield.Instance.Update();
						if (Globals.TotalNutCollected >= Globals.NUT_GOAL) 
						{ 
							IAnimatable.PauseCategory("game");
							OnGoalAchieved();
							break; 
						}
						// If we press Yellow button we go into inventory
						if (fd.InputMgr.IsButtonFirstDown(Buttons.Y) || fd.InputMgr.IsKeyFirstDown(Keys.Tab))
						{
							IAnimatable.PauseCategory("game");
							GameState = eGAME_STATE.INVENTORY;
						}
						else if (fd.InputMgr.IsButtonFirstDown(Buttons.Start) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							IAnimatable.PauseCategory("game");
							GameState = eGAME_STATE.IN_GAME_MENU;
							InGameMenu.Instance.OnActivate();
						}
						break;
					}
				case eGAME_STATE.IN_GAME_MENU:
					{
						InGameMenu.Instance.Update();
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) ||
							fd.InputMgr.IsButtonFirstDown(Buttons.Start) ||
                            fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							s_sndMenuNavigate.Play();
							IAnimatable.UnpauseCategory("game");
							GameState = eGAME_STATE.IN_GAME;
						}
						//else if (fd.InputMgr.IsButtonFirstDown(Buttons.Y) && Guide.IsTrialMode)
						//{
						//	Guide.ShowMarketplace(fd.InputMgr.PlayerIndex);
						//}
						else if (fd.InputMgr.IsButtonFirstDown(Buttons.A) || fd.InputMgr.IsKeyFirstDown(Keys.Enter) || fd.InputMgr.IsKeyFirstDown(Keys.Space))
						{
							s_sndMenuNavigate.Play();
							switch (InGameMenu.Instance.CurrentChoiceId)
							{
								case 0:
									GameState = eGAME_STATE.SETTINGS;
									break;
								case 1:
									GameState = eGAME_STATE.CREATE_NEW_WORLD;
									IAnimatable.UnpauseCategory("game");
									LoadingScreen.Instance.StartLoading("Loading");
									newWorldCreated = false;
									WorkerThread.Instance.AddWork(SaveThenCreateNew);
									break;
								case 2:
									WorldSelectScreen.Instance.OnActivate();
									GameState = eGAME_STATE.WORLD_SELECT;
									break;
								/*	case 3:
										GameState = eGAME_STATE.DELETE_WORLD;
										break;*/
								case 3:
									GameState = eGAME_STATE.CREDITS;
									break;
								case 4:
									// Save the game
									GameState = eGAME_STATE.LOADING;
									LoadingScreen.Instance.StartLoading("Saving");
									WorkerThread.Instance.AddWork(SaveAndExit);
									break;
							}
						}
						break;
					}
				case eGAME_STATE.CREATE_NEW_WORLD:
					{
						if (newWorldCreated)
						{
							new Store();
							new Inventory(); // Create our inventory instance
							new CSnowfield(); // Playfield
							GameState = eGAME_STATE.IN_GAME;
						}
						break;
					}
				case eGAME_STATE.SELECTING_WORLD:
					{
						if (worldSelected)
						{
							new Store();
							new Inventory(); // Create our inventory instance
							new CSnowfield(); // Playfield
							GameState = eGAME_STATE.IN_GAME;
						}
						break;
					}
				case eGAME_STATE.CREDITS:
					{
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							s_sndMenuNavigate.Play(); 
							GameState = eGAME_STATE.IN_GAME_MENU;
						}
						break;
					}
				case eGAME_STATE.WORLD_SELECT:
					{
						WorldSelectScreen.Instance.Update();
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							s_sndMenuNavigate.Play();
							GameState = eGAME_STATE.IN_GAME_MENU;
						}
						else if ((fd.InputMgr.IsButtonFirstDown(Buttons.X) || fd.InputMgr.IsKeyFirstDown(Keys.Delete)) && 
							WorldSelectScreen.Instance.Selection != Profile.Instance.CurrentSaveName)
						{
							s_sndMenuNavigate.Play();
							GameState = eGAME_STATE.DELETE_WORLD;
							worldToDelete = WorldSelectScreen.Instance.Selection;
						}
						else if ((fd.InputMgr.IsButtonFirstDown(Buttons.A) || fd.InputMgr.IsKeyFirstDown(Keys.Enter)) &&
							WorldSelectScreen.Instance.Selection != Profile.Instance.CurrentSaveName)
						{
							s_sndMenuNavigate.Play();

							GameState = eGAME_STATE.SELECTING_WORLD;
							IAnimatable.UnpauseCategory("game");
							LoadingScreen.Instance.StartLoading("Loading");
							worldSelected = false;
							WorkerThread.Instance.AddWork(SelectWorld);
						}
						break;
					}
				case eGAME_STATE.DELETE_WORLD:
					{
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							s_sndMenuNavigate.Play();
							WorldSelectScreen.Instance.OnActivate();
							GameState = eGAME_STATE.WORLD_SELECT;
						}
						else if (fd.InputMgr.IsButtonFirstDown(Buttons.A) || fd.InputMgr.IsKeyFirstDown(Keys.Enter))
						{
							GameState = eGAME_STATE.DELETING_WORLD;
					/*		IAnimatable.UnpauseCategory("game");
							if (Store.Instance != null) Store.Instance.Dispose();
							if (Inventory.Instance != null) Inventory.Instance.Dispose();
							if (CSnowfield.Instance != null) CSnowfield.Instance.Dispose();
							IAnimatable.StopAllAnims();*/
							LoadingScreen.Instance.StartLoading("Deleting World");
							deleteCompleted = false;
							WorkerThread.Instance.AddWork(DeleteWorld);
						}
						break;
					}
				case eGAME_STATE.INVENTORY:
					{
						Inventory.Instance.Update();
						// If we press Red button we go out of inventory
						if (fd.InputMgr.IsButtonFirstDown(Buttons.B) || fd.InputMgr.IsKeyFirstDown(Keys.Escape))
						{
							IAnimatable.UnpauseCategory("game");
							GameState = eGAME_STATE.IN_GAME;
							s_sndMenuNavigate.Play();
						}
						break;
					}
				case eGAME_STATE.DELETING_WORLD:
					{
						if (deleteCompleted)
						{
							deleteCompleted = false;
							WorldSelectScreen.Instance.OnActivate();
							GameState = eGAME_STATE.WORLD_SELECT;
						}
						break;
					}
			}

		/*	if (fd.InputMgr.IsKeyFirstDown(Keys.Space))
			{
				IAnimatable.StopAllAnims();
				new Store();
				new Inventory();
				new CSnowfield();
			}*/

			// Allows the game to exit
#if DEBUG
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();
#endif
		}

		private void OnGoalAchieved()
		{
			// Show Goal Achieved Screen. Then transition to start the game again!
			MediaPlayer.Stop();
			repeatCount = 0;
			GameState = eGAME_STATE.GOAL_ACHIEVED_SCREEN;
			GoalAchievedScreen.Instance.OnActivate();
		}

		PlayerIndex isCheckingForPlayerIndex = PlayerIndex.One;
		bool currentlyCheckingForPlayerIndex = false;
	//	private bool worldLoaded = false;
		private bool worldSelected;
		private string worldToDelete;

		private bool MakeSurePlayerIsSignedInOrTry(PlayerIndex playerIndex)
		{
#if WINDOWS
		//	return true;
#endif
			//foreach (SignedInGamer signedInGamer in Gamer.SignedInGamers)
			//{
			//	if (signedInGamer.PlayerIndex == playerIndex)
			//	{
			//		return true;
			//	}
			//}

			//// Show the sign in dialogs...
			//Guide.ShowSignIn(1, false);
			//currentlyCheckingForPlayerIndex = true;
			//isCheckingForPlayerIndex = playerIndex;

			return true;
		}

		private bool MakeSurePlayerIsSignedIn(PlayerIndex playerIndex)
		{
#if WINDOWS
		//	return true;
#endif
			//foreach (SignedInGamer signedInGamer in Gamer.SignedInGamers)
			//{
			//	if (signedInGamer.PlayerIndex == playerIndex)
			//	{
			//		return true;
			//	}
			//}

			return true;
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			++fps;
			GraphicsDevice.Clear(Color.Black);

            // Render depending the state
            switch (GameState)
			{
				// Loading screens
				case eGAME_STATE.LOADING:
					{
						LoadingScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.LOADING_PROFILE:
					{
						LoadingScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.DELETING_WORLD:
					{
						LoadingScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.CREATE_NEW_WORLD:
					{
						LoadingScreen.Instance.Render();
						break;
					}


				case eGAME_STATE.GOAL_ACHIEVED_SCREEN:
					{
						if (GoalAchievedScreen.Instance.State == 0 ||
							GoalAchievedScreen.Instance.State == 3) CSnowfield.Instance.Render();
						GoalAchievedScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.INTRO_SCREEN:
					{
						StartScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.IN_GAME:
					{
						CSnowfield.Instance.Render();
						break;
					}
				case eGAME_STATE.IN_GAME_MENU:
					{
						CSnowfield.Instance.Render();
						InGameMenu.Instance.Render();
						break;
					}
				case eGAME_STATE.WORLD_SELECT:
					{
						CSnowfield.Instance.Render();
						WorldSelectScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.SETTINGS:
					{
						CSnowfield.Instance.Render();
						SettingScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.CREDITS:
					{
						CSnowfield.Instance.Render();
						CreditScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.DELETE_WORLD:
					{
						CSnowfield.Instance.Render();
						DeleteWorldScreen.Instance.Render();
						break;
					}
				case eGAME_STATE.INVENTORY:
					{
						CSnowfield.Instance.Render();
						Inventory.Instance.Render();
						break;
					}
			}

#if DEBUG
			CFrameData.Instance.SpriteBatch.Begin();
			string timeInfo = (new DateTime(DateTime.Now.Ticks - timeStart.Ticks)).ToLongTimeString();
			CFrameData.Instance.SpriteBatch.DrawString(CFrameData.Instance.CommonResources.Font_System,
				timeInfo, Vector2.Zero, Color.Yellow);
			CFrameData.Instance.SpriteBatch.DrawString(CFrameData.Instance.CommonResources.Font_System, 
				"fps: " + showFps, new Vector2(200, 16), Color.Yellow);
			CFrameData.Instance.SpriteBatch.End();
#endif

			base.Draw(gameTime);
		}
	}
}
