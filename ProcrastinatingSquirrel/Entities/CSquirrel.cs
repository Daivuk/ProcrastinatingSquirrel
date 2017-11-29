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
using ProcrastinatingSquirrel.Entities;

namespace ProcrastinatingSquirrel
{
	class CSquirrel : IEntity
	{
		class CSquirrelAnimUserData
		{
			public float speedMultiplier;
			public CSquirrelAnimUserData(float in_speedMultiplier)
			{
				speedMultiplier = in_speedMultiplier;
			}
		}

		CAnimStringBubble m_storeText = new CAnimStringBubble("game", "Store");
		Vector2 m_origin = new Vector2(64, 64);
		float m_angle = 0;
		const int ANIM_STANDING = 0;
		const int ANIM_RUNNING = 1;
		const int ANIM_DIG = 2;
		CTile m_previousTile = null;
		float m_digDelay = 0;
		static float m_baseSensRadius = 1.7f;
		float m_radius = .35f;
		CAnimFloat m_showSensHelperTimer = new CAnimFloat();
		bool canDig = false;
		bool m_canFitBridge = false;
		bool m_outsideFirstTime = true;
		int m_bridgeCount = 0;
		Vector2[] m_bridgeFitPos = new Vector2[4];
		static SoundEffect s_sndplaceBridge = CFrameData.Instance.Content.Load<SoundEffect>("sounds/place");
		static SoundEffect[] s_sndSteps = new SoundEffect[] {
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step1"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step2"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step3"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step4"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step5"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step6"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step7"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/step8")
		};
		static SoundEffect[] s_sndDig = new SoundEffect[] {
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/dig1"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/dig2"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/dig3"),
			CFrameData.Instance.Content.Load<SoundEffect>("sounds/dig4")
		};
		CAnimFloat m_sensRadius = new CAnimFloat("game", m_baseSensRadius);
		public float SensRadius
		{
			get { return m_sensRadius.Value; }
		}
		public float MaxSensRadius
		{
			get { return m_baseSensRadius + Inventory.Instance.SensUpgradeValue; }
		}
		Vector2 m_digPosition;
		CAnimationSprite m_animSprite = new CAnimationSprite(new CAnimationInfo[] {
				new CAnimationInfo("textures/squirrelStanding", new CFrameInfo[] {
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(128, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(256, 0, 128, 128), null)
					}, 2.0f),
				new CAnimationInfo("textures/squirrelRunning", new CFrameInfo[] {
					new CFrameInfo(new Rectangle(0, 0, 128, 128), new CSquirrelAnimUserData(1.25f)),
					new CFrameInfo(new Rectangle(0, 0, 128, 128), new CSquirrelAnimUserData(1.25f)),
					new CFrameInfo(new Rectangle(128, 0, 128, 128), new CSquirrelAnimUserData(1.25f)),
					new CFrameInfo(new Rectangle(256, 0, 128, 128), new CSquirrelAnimUserData(.75f), s_sndSteps, .25f),
					new CFrameInfo(new Rectangle(384, 0, 128, 128), new CSquirrelAnimUserData(1))
					}, 13.0f),
				new CAnimationInfo("textures/squirrelRunning", new CFrameInfo[] {
					new CFrameInfo(new Rectangle(0, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(128, 0, 128, 128), null),
					new CFrameInfo(new Rectangle(256, 0, 128, 128), null),
					}, 10.0f),
			});
		CAnimationSprite m_animSensHelper = new CAnimationSprite(new CAnimationInfo[] {
			new CAnimationInfo("textures/sensHelper", new CFrameInfo[] {
				new CFrameInfo(new Rectangle(0, 0, 512, 320)),
				new CFrameInfo(new Rectangle(512, 0, 512, 320)),
				new CFrameInfo(new Rectangle(0, 320, 512, 320)),
				new CFrameInfo(new Rectangle(512, 320, 512, 320)),
				new CFrameInfo(new Rectangle(0, 640, 512, 320)),
				new CFrameInfo(new Rectangle(512, 640, 512, 320)),
			}, 4.0f),
		});
		static Color s_bridgePlacementColor = new Color(150, 255, 150, 150);
		Texture2D m_storyBubble = CFrameData.Instance.Content.Load<Texture2D>("textures/StoryBubble");


		public CSquirrel()
		{
			m_animSprite.PlayAnim(ANIM_STANDING, eAnimType.LINEAR, eAnimFlag.LOOP);
			m_animSensHelper.PlayAnim(0, eAnimType.LINEAR, eAnimFlag.LOOP);
		}


		public override void Update()
		{
			CFrameData fd = CFrameData.Instance;
			if (m_animSprite.IsPlaying) m_animSprite.Update();
			m_digPosition = Position;
			Vector2 direction = new Vector2(
				-(float)Math.Sin(m_angle),
				(float)Math.Cos(m_angle));
			m_digPosition += direction * .75f;
			canDig = CSnowfield.Instance.CanDigAt(m_digPosition, this);
			if (m_animSprite.CurrentAnimation == ANIM_DIG)
			{
				m_digDelay -= fd.GetDeltaSecond();
				if (m_digDelay <= 0)
				{
					m_digDelay = 1000;
					CSnowfield.Instance.DigAt(m_digPosition, this);
					s_sndDig[fd.Random.Next(s_sndDig.Length)].Play(1, -.1f, 0);
				}
				if (!m_animSprite.IsPlaying)
				{
					// We are done digging, switch back to standing (Idle)
					m_animSprite.PlayAnim(ANIM_STANDING, eAnimType.LINEAR, eAnimFlag.LOOP);
				}
			}
			else
			{
				Vector2 movingDirection = fd.InputMgr.GamePadState.ThumbSticks.Left;
				movingDirection.Y *= -1;
				
				// Check movement with DPad also
				Vector2 dpadMovement = Vector2.Zero;
				if (fd.InputMgr.IsButtomDown(Buttons.DPadRight) || fd.InputMgr.IsKeyDown(Keys.D) || fd.InputMgr.IsKeyDown(Keys.Right))
				{
					dpadMovement += Vector2.UnitX;
				}
				if (fd.InputMgr.IsButtomDown(Buttons.DPadLeft) || fd.InputMgr.IsKeyDown(Keys.A) || fd.InputMgr.IsKeyDown(Keys.Left))
				{
					dpadMovement -= Vector2.UnitX;
				}
				if (fd.InputMgr.IsButtomDown(Buttons.DPadDown) || fd.InputMgr.IsKeyDown(Keys.S) || fd.InputMgr.IsKeyDown(Keys.Down))
				{
					dpadMovement += Vector2.UnitY;
				}
				if (fd.InputMgr.IsButtomDown(Buttons.DPadUp) || fd.InputMgr.IsKeyDown(Keys.W) || fd.InputMgr.IsKeyDown(Keys.Up))
				{
					dpadMovement -= Vector2.UnitY;
				}
				if (dpadMovement.LengthSquared() > 0)
				{
					dpadMovement.Normalize();
					movingDirection = dpadMovement;
				}

				if (movingDirection.LengthSquared() > .25f)
				{
					if (m_animSprite.CurrentAnimation != ANIM_RUNNING)
					{
						m_animSprite.PlayAnim(ANIM_RUNNING, eAnimType.LINEAR, eAnimFlag.LOOP);
					}
					movingDirection.Normalize();
					Vector2 lastPosition = Position;
					Position += movingDirection * fd.GetDeltaSecond() *
						((CSquirrelAnimUserData)m_animSprite.UserData).speedMultiplier * 3.0f * Inventory.Instance.SpeedMultiplier;
					m_angle = (float)Math.Atan2(-(double)movingDirection.X, (double)movingDirection.Y);

					// Perform collisions
					Vector2 result = Position;
					if (CSnowfield.Instance.DoCollisions(ref lastPosition, ref result, m_radius))
					{
						Position = result;
					}
                    CTile currentTile = CSnowfield.Instance.GetTileAt((int)Position.X, (int)Position.Y);
                    if (currentTile != null)
                    {
                        if (!currentTile.IsPassable && m_previousTile != null)
                        {
                            Position = m_previousTile.Position + Vector2.One * .5f;
                            currentTile = m_previousTile;
                        }
                        if (currentTile != m_previousTile)
                        {
                            m_previousTile = currentTile;
                        }
                    }
                }
				else
				{
					if (m_animSprite.CurrentAnimation != ANIM_STANDING)
					{
						m_animSprite.PlayAnim(ANIM_STANDING, eAnimType.LINEAR, eAnimFlag.LOOP);
					}
				}

				// Digging
				if (fd.InputMgr.IsButtomDown(Buttons.X) ||
					fd.InputMgr.IsButtomDown(Buttons.RightTrigger) ||
					fd.InputMgr.IsButtomDown(Buttons.LeftTrigger) ||
                    fd.InputMgr.IsKeyDown(Keys.Space))
				{
					if (canDig)
					{
						m_digDelay = .15f;

						// Is there something to dig in front of us?
						m_animSprite.PlayAnim(ANIM_DIG, eAnimType.LINEAR, 0);
					}
				}
			}

			// If we are home, we update the sens to full
			if (CSnowfield.Instance.IsSquirrelHome)
			{
				m_showSensHelperTimer.Stop();
				float sensRadiusDest = MaxSensRadius;
				if (m_sensRadius.To != sensRadiusDest)
				{
					m_sensRadius.StartAnimFromCurrent(sensRadiusDest, .25f, 0, eAnimType.EASE_IN);
				}
			}
			else
			{
				if (m_outsideFirstTime)
				{
					m_outsideFirstTime = false;
					m_showSensHelperTimer.StartAnim(1, 0, 8, 0, eAnimType.LINEAR);
				}
				if (m_sensRadius.To != .5f)
				{
					m_sensRadius.StartAnimFromCurrent(.5f, m_sensRadius.Value * 80, 0, eAnimType.EASE_IN);
				}
			}

			// Check if the dig position is a river
			m_canFitBridge = false;
			if (Inventory.Instance.HasItem<Bridge>() &&
				Vector2.DistanceSquared(CSnowfield.Instance.HomePos, Position) < 90000) // We don't want to put bridges on the second river
			{
				CTile digTile = CSnowfield.Instance.GetTileAt((int)m_digPosition.X, (int)m_digPosition.Y);
				if (digTile != null)
				{
					if (digTile.Type == eTILE_TYPE.TILE_WATER &&
						digTile.Entity == null)
					{
						// yay! Check in the direction the kuikui is looking,
						// can we fit a bridge of 4 or less
						m_bridgeCount = 0;
						if (Math.Abs(direction.X) > Math.Abs(direction.Y)) direction.Y = 0;
						else direction.X = 0;
						direction.Normalize();
						int i = 0;
						for (i = 0; i < 5; ++i)
						{
							CTile tile = CSnowfield.Instance.GetTileAt(
								(int)(m_digPosition.X + direction.X * (float)i),
								(int)(m_digPosition.Y + direction.Y * (float)i));
							if (tile == null)
							{
								break;
							}
							if (tile.Type != eTILE_TYPE.TILE_WATER)
							{
								if (i > 0)
								{
									m_canFitBridge = true;
									break;
								}
							}
							if (!(tile.Type == eTILE_TYPE.TILE_WATER &&
								tile.Entity == null))
							{
								break;
							}
							else 
							if (i == 4)
							{
								break;
							}
							m_bridgeFitPos[i] = tile.Position;
							++m_bridgeCount;
						}
					/*	if (m_canFitBridge)
						{
							if (m_bridgeCount > Inventory.Instance.GetItemCount<Bridge>())
							{
								m_canFitBridge = false;
							}
						}*/
					}
				}
			}
			if (m_canFitBridge && (fd.InputMgr.IsButtonFirstDown(Buttons.A) || fd.InputMgr.IsKeyFirstDown(Keys.Enter)))
			{
				// Build a bridge here!!
				for (int i = 0; i < m_bridgeCount; ++i)
				{
					CTile tile = CSnowfield.Instance.GetTileAt((int)m_bridgeFitPos[i].X, (int)m_bridgeFitPos[i].Y);
					tile.Entity = new Bridge();
					tile.IsPassable = true;
                    var chunk = CSnowfield.Instance.GetChunkAt((int)m_bridgeFitPos[i].X, (int)m_bridgeFitPos[i].Y);
                    if (chunk != null) chunk.NeedToBeSaved = true;
                }
				s_sndplaceBridge.Play();
				Inventory.Instance.RemoveOneOf<Bridge>();
			}
		}

		static SoundEffect s_sndFoundItem = CFrameData.Instance.Content.Load<SoundEffect>("sounds/foundItem");
		static SoundEffect s_sndFoundBigItem = CFrameData.Instance.Content.Load<SoundEffect>("sounds/foundBigItem");

		public override void GiveItem(IEntity item)
		{
			Inventory.Instance.AddItem(item);
			if (item.Count == 5)
			{
				s_sndFoundBigItem.Play(1, 0, 0);
			}
			else
			{
				s_sndFoundItem.Play(.5f, 0, 0);
			}
		}

		public override int GetDiggingStrength() 
		{
			int diggingStrength = Inventory.Instance.DiggingStrength;
			if (diggingStrength == 0) diggingStrength = 1;
			return diggingStrength; 
		}

		public void Die()
		{
			// Lose all nuts, spawn in the middle again
			Inventory.Instance.LoseAllNuts();
			Position = CSnowfield.Instance.SpawnPos;
			m_previousTile = null;
			CSnowfield.Instance.TriggerDeath();
		}

		public override void Render()
		{
			CFrameData.Instance.SpriteBatch.Draw(m_animSprite.Texture, Position,
				m_animSprite.SrcRect, Color.White, m_angle,
				m_origin,
				CSnowfield.INV_TILE_SCALE, SpriteEffects.None, 0);
		}

		public override void RenderLayer(int layer, ref Vector2 offset)
		{
			if (layer != 5) return;

			// Draw bridge if we can build one
			if (m_canFitBridge)
			{
				for (int i = 0; i < m_bridgeCount; ++i)
				{
					CFrameData.Instance.SpriteBatch.Draw(Bridge.Texture, m_bridgeFitPos[i],
						null, s_bridgePlacementColor, 0,
						Vector2.Zero,
						CSnowfield.INV_TILE_SCALE, SpriteEffects.None, 0);
				}
			}

			// Sens helper
			if (m_sensRadius.Value == .5f)
			{
				if (!m_showSensHelperTimer.IsPlaying)
				{
					m_showSensHelperTimer.StartAnim(0, 25, 25, 0, eAnimType.LINEAR, eAnimFlag.LOOP);
				}
				if (m_showSensHelperTimer.Value < 10 &&
					Inventory.Instance.GetItemLevel<SensUpgrade>() <= 1)
				{
					CFrameData.Instance.SpriteBatch.Draw(m_animSensHelper.Texture, Position + new Vector2(.5f, -3.0f) + offset,
						m_animSensHelper.SrcRect, new Color(255, 255, 255, 225), 0, Vector2.Zero,
						CSnowfield.INV_TILE_SCALE * 1.25f, SpriteEffects.None, 0);
				}
			}
			else
			{
				if (m_showSensHelperTimer.IsPlaying)
				{
					CFrameData.Instance.SpriteBatch.Draw(m_storyBubble, Position + new Vector2(.5f, -3.0f) + offset,
						null, new Color(255, 255, 255, 225), 0, Vector2.Zero,
						CSnowfield.INV_TILE_SCALE * 1.25f, SpriteEffects.None, 0);
				}
			}
		}

		public void RenderHud()
		{
			float padding = 16;
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;
			Rectangle safeFrame = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			// Draw kuicash amount
			if (CSnowfield.Instance.IsSquirrelHome)
			{
				Vector2 screenPos = new Vector2(
					(float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Left + 32,
					(float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Top + 32);
				SquirrelHelper.DrawString("Stock for the Winter: " + Globals.TotalNutCollected + ((Globals.TotalNutCollected > 60000) ? ("/" + Globals.NUT_GOAL) : ""),
					screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
				if (Inventory.Instance.KuiCash > 0 || !Inventory.Instance.IsEmpty)
				{
					screenPos.Y += 32;
					SquirrelHelper.DrawString("Kuicash: " + Inventory.Instance.KuiCash.ToString(), screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
				}
			}

			if (!Inventory.Instance.IsEmpty || Inventory.Instance.KuiCash > 0)
			{
				sb.Draw(fd.CommonResources.Tex_Buttons,
					new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32),
                    fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnY : fd.CommonResources.rectBtnTab, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
				SquirrelHelper.DrawString("Inventory",
					new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding),
					Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
			}

			if (!CSnowfield.Instance.IsSquirrelHome)
			{
				sb.Draw(fd.CommonResources.Tex_Buttons,
					new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32),
                    fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnX : fd.CommonResources.rectBtnSpace, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
				SquirrelHelper.DrawString("Dig",
					new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding),
					Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

				if (m_canFitBridge)
				{
					sb.Draw(fd.CommonResources.Tex_Buttons,
						new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32 - 48),
                        fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
					SquirrelHelper.DrawString("Build Bridge",
						new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding - 48),
						Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
				}
			}

			sb.End();
		}

		public override void Load(System.IO.BinaryReader fic_in)
		{
			m_outsideFirstTime = fic_in.ReadBoolean();
		}

		public override void Save(System.IO.BinaryWriter fic_out)
		{
			fic_out.Write(m_outsideFirstTime);
		}

		internal override void Dispose()
		{
			m_storeText.Stop();
			m_storeText = null;
			m_showSensHelperTimer.Stop();
			m_showSensHelperTimer = null;
			m_sensRadius.Stop();
			m_sensRadius = null;
			m_animSprite.Dispose();
			m_animSprite = null;
			m_animSensHelper.Dispose();
			m_animSensHelper = null;
			base.Dispose();
		}
	}
}
