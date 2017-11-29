using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProcrastinatingSquirrel.Entities;
using Microsoft.Xna.Framework.Audio;

namespace ProcrastinatingSquirrel
{
	class Store
	{
		public static Store Instance = null;

		Texture2D texInvCursor;
		CAnimFloat m_cursorAnim = new CAnimFloat("inv", 1);

		public class StoreItem
		{
			public List<IEntity> items;
			public StoreItem(IEntity[] in_items)
			{
				items = in_items.ToList();
				currentItem = items.First();
			}
			public IEntity currentItem;
		}
		List<StoreItem> m_storeItems = new List<StoreItem>
		{
			new StoreItem(new IEntity[]
			{
				new Shovel("Shovel Level 1", 3, 10, "textures/shovelBlue", 1),
				new Shovel("Shovel Level 2", 9, 100, "textures/shovelGreen", 2),
				new Shovel("Shovel Level 3", 27, 400, "textures/shovelYellow", 3),
				new Shovel("Shovel Level 4", 81, 1200, "textures/shovelOrange", 4),
				new Shovel("Shovel Level 5", 243, 4800, "textures/shovelRed", 5),
				new Shovel("Ice Pick", 729, 14400, "textures/icePick", 6),
			}),
			new StoreItem(new IEntity[]
			{
				new BackBag("Bag Level 1", 20, 16, "textures/bagBlue", 1),
				new BackBag("Bag Level 2", 40, 160, "textures/bagGreen", 2),
				new BackBag("Bag Level 3", 65, 640, "textures/bagYellow", 3),
				new BackBag("Bag Level 4", 100, 2560, "textures/bagOrange", 4),
				new BackBag("Bag Level 5", 140, 10240, "textures/bagRed", 5),
			}),
            new StoreItem(new IEntity[]
            {
                new SensUpgrade("Goggles Level 1", 1, 20, "textures/gogglesBlue", 1),
                new SensUpgrade("Goggles Level 2", 2, 200, "textures/gogglesGreen", 2),
                new SensUpgrade("Goggles Level 3", 3.25f, 800, "textures/gogglesYellow", 3),
                new SensUpgrade("Goggles Level 4", 6.25f, 2400, "textures/gogglesOrange", 4),
                new SensUpgrade("Goggles Level 5", 10, 9600, "textures/gogglesRed", 5),
            }),
            new StoreItem(new IEntity[]
            {
                new SpeedUpgrade("Speed Shoes Level 1", 1.75f, 600, "textures/speedBlue", 1),
                new SpeedUpgrade("Speed Shoes Level 2", 2.5f, 6000, "textures/speedRed", 2),
            }),
            new StoreItem(new IEntity[]
			{
				new Bridge(5000)
			})
		};
		public List<StoreItem> StoreItems
		{
			get { return m_storeItems; }
		}
		StoreItem m_selectedItem = null;
		static SoundEffect s_sndMenuNavigate = CFrameData.Instance.Content.Load<SoundEffect>("sounds/menuNavigate");
		static SoundEffect s_sndBuy = CFrameData.Instance.Content.Load<SoundEffect>("sounds/buy");
		static SoundEffect s_sndError = CFrameData.Instance.Content.Load<SoundEffect>("sounds/error");


		public Store()
		{
			Instance = this;
			m_selectedItem = m_storeItems.First();
			texInvCursor = CFrameData.Instance.Content.Load<Texture2D>("textures/invCursor");
			m_cursorAnim.StartAnim(1, 1.2f, .5f, 0, eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
		}

		private static Vector2 s_textOffset = new Vector2(48, 72);
		private static Vector2 s_cursorOffset = new Vector2(48, 48);
		private static Vector2 s_cursorOrigin = new Vector2(32, 32);

		public void RenderItem(StoreItem item, Vector2 screenPos)
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

		//	SquirrelHelper.DrawString(itemStack.count.ToString(), screenPos + s_textOffset, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.CENTER, SquirrelHelper.eTEXT_ALIGN.TOP);
			item.currentItem.DrawInventoryItem(screenPos);

			if (m_selectedItem == item)
			{
				sb.Draw(texInvCursor, screenPos + s_cursorOffset, null, Globals.IconColor
					, 0, s_cursorOrigin, m_cursorAnim.Value * 1.25f, SpriteEffects.None, 0);
			}
		}

		public void Render()
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;
			Vector2 screenPos = new Vector2(
				(float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Left + 32,
				(float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Top + 32);

			sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			// Fade out the back
			sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, .75f));

			// Draw kuicash amount
			SquirrelHelper.DrawString("Kuicash: " + Inventory.Instance.KuiCash.ToString(), screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
			screenPos.Y += 64;

			// Capacity
		/*	int itemsCount = Inventory.Instance.ItemsCount;
			int invLimit = Inventory.Instance.InventoryLimit;
			Vector2 capacityRightPos = screenPos;
			string capacityString = "Capacity: " + itemsCount + "/" + invLimit +
				((itemsCount == invLimit) ? " - FULL" : "");
			capacityRightPos.X += fd.CommonResources.Font_AgentOrange.MeasureString(capacityString).X + 64;
			capacityRightPos.Y += 24;
			SquirrelHelper.DrawString(capacityString, screenPos, Globals.TextColor, 
				SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
			screenPos.Y += 64;*/

			// Selected name and price
			if (m_selectedItem != null)
			{
				SquirrelHelper.DrawString("Selected: " + m_selectedItem.currentItem.Name, screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
				if (Inventory.Instance.KuiCash < m_selectedItem.currentItem.Cost)
				{
					SquirrelHelper.DrawString("Cost: " + m_selectedItem.currentItem.Cost,
						new Vector2((float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 32, screenPos.Y)
							, Color.Red, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.TOP);
				}
				else
				{
					SquirrelHelper.DrawString("Cost: " + m_selectedItem.currentItem.Cost,
						new Vector2((float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 32, screenPos.Y)
							, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.TOP);
				}
				screenPos.Y += 64;
			}

			// Draw items
			foreach (StoreItem storeItem in m_storeItems)
			{
				RenderItem(storeItem, screenPos);
				screenPos.X += 96;
				if (screenPos.X > (float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 64 - 32)
				{
					screenPos.X = (float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Left + 32;
					screenPos.Y += 128;
				}
			}

			Rectangle safeFrame = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;
			float padding = 16;
			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Left + padding + 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Buy",
				new Vector2((float)safeFrame.Left + padding + 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnB : fd.CommonResources.rectBtnEsc, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Back",
				new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);

			// Button to sell stuff if we have some to sell
	/*		if (Inventory.Instance.HasItemsToSell)
			{
				sb.Draw(fd.CommonResources.Tex_Buttons,
					new Vector2(capacityRightPos.X, capacityRightPos.Y),
					fd.CommonResources.rectBtnY, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
				SquirrelHelper.DrawString(m_sellItemsText,
					new Vector2(capacityRightPos.X + 32, capacityRightPos.Y + 32),
					Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
			}*/

			sb.End();
		}

		public void OnActivate()
		{
			Inventory inv = Inventory.Instance;
		/*	if (inv.HasItemsToSell)
			{
				inv.SellAllItems();
//				m_sellItemsText = new CAnimStringBubble("store", "Sell all nuts");
//				m_sellItemsText.StartAnimFromCurrent(m_sellItemsText.Value, .75f, 0, eAnimType.LINEAR);
			}*/

			// If selection doesn't have enough money, select the next more expensive one we can buy
			if (m_selectedItem != null)
			{
				if (m_selectedItem.currentItem.Cost > inv.KuiCash)
				{
					foreach (StoreItem storeItem in m_storeItems)
					{
						if (storeItem.currentItem.Cost <= inv.KuiCash)
						{
							m_selectedItem = storeItem;
							break;
						}
					}
				}
			}
		}

		public void Update()
		{
			CFrameData fd = CFrameData.Instance;

			// Move cursor around to select different items
			if (m_selectedItem == null) return;
			int currentIndex = m_storeItems.IndexOf(m_selectedItem);
			int newIndex = currentIndex;
			int colCount = (fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 64 - 32 - 32) / 96;
			int col = currentIndex % colCount;
			int row = currentIndex / colCount;

			if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickLeft) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Left) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.A))
			{
				col--;
				if (col < 0) col = colCount - 1;
			}
			if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickRight) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Right) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.D))
			{
				col++;
				if (col >= colCount || currentIndex == m_storeItems.Count - 1)
				{
					col = 0;
				}
			}
			if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickUp) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.W) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Up))
			{
				row--;
				if (row < 0) row = m_storeItems.Count() / colCount - 1;
				if (row < 0) row = 0;
			}
			if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickDown) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.S) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Down))
			{
				row++;
				if (row >= m_storeItems.Count() / colCount) row = 0;
			}

			newIndex = Math.Min(row * colCount + col, m_storeItems.Count() - 1);
			if (newIndex < 0) newIndex = 0;
			if (newIndex != currentIndex)
			{
				// Play sound
				s_sndMenuNavigate.Play();
				m_selectedItem = m_storeItems[newIndex];
			}

			// If we are within home radius, we can sell our shit
		/*	if (CSnowfield.Instance.IsSquirrelHome)
			{
				if (fd.InputMgr.IsButtonFirstDown(Buttons.Y))
				{
					Inventory.Instance.SellAllItems();
				}
			}*/

			// Buy if we press A
			if (m_selectedItem != null)
			{
				if (fd.InputMgr.IsButtonFirstDown(Buttons.A) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Enter) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Space))
				{
					if (Inventory.Instance.KuiCash >= m_selectedItem.currentItem.Cost &&
						!Inventory.Instance.IsInventoryFull)
					{
						s_sndBuy.Play();
						IEntity toReplace = null;
						if (m_selectedItem.items.IndexOf(m_selectedItem.currentItem) > 0)
						{
							toReplace = m_selectedItem.items[m_selectedItem.items.IndexOf(m_selectedItem.currentItem) - 1];
						}
						Inventory.Instance.AddItem(m_selectedItem.currentItem, toReplace);
						Inventory.Instance.KuiCash = Inventory.Instance.KuiCash - m_selectedItem.currentItem.Cost;
						if (m_selectedItem.items.Count() > 1)
						{
							if (m_selectedItem.items.Last() == m_selectedItem.currentItem)
							{
								// Remove this item from the store
								m_selectedItem.currentItem = null;
								m_storeItems.Remove(m_selectedItem);
								m_selectedItem = null;
								if (m_storeItems.Count() > 0)
								{
									m_selectedItem = m_storeItems.First();
								}
							}
							else
							{
								m_selectedItem.currentItem =
									m_selectedItem.items[m_selectedItem.items.IndexOf(m_selectedItem.currentItem) + 1];
							}
						}
					}
					else
					{
						s_sndError.Play();
					}
				}
			}
		}

		internal void Dispose()
		{
			m_cursorAnim.Stop();
			m_cursorAnim = null;
			foreach (StoreItem storeItem in m_storeItems)
			{
				storeItem.items = null;
				storeItem.currentItem = null;
			}
			m_storeItems = null;
			m_selectedItem = null;
			Instance = null;
		}
	}
}
