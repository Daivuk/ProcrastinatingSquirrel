using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DK8;
using Microsoft.Xna.Framework;
using ProcrastinatingSquirrel.Entities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace ProcrastinatingSquirrel
{
	class Inventory
	{
		public static Inventory Instance = null;

        int m_kuicash = 0;
        //int m_kuicash = 100000; // Just enough to buy everything

        public class InventoryStack
		{
			public int count;
			public IEntity entity;
		}

		public bool IsEmpty
		{
			get
			{
				return (m_itemStacks.Count == 0);
			}
		}
		List<InventoryStack> m_itemStacks = new List<InventoryStack>();
		int m_inventoryLimit = 15;
		public int ItemsCount
		{
			get
			{
				int invCount = 0;
				foreach (InventoryStack itemStack in m_itemStacks)
				{
					invCount += itemStack.count;
				}
				return invCount;
			}
		}
		public bool HasItemsToSell
		{
			get
			{
				// Remove all nuts stacks
				for (int i = 0; i < m_itemStacks.Count(); ++i)
				{
					InventoryStack stack = m_itemStacks[i];
					if (stack.entity.GetType() == typeof(CNut))
					{
						return true;
					}
				}
				return false;
			}
		}
		public bool IsInventoryFull
		{
			get
			{
				return (ItemsCount >= InventoryLimit);
			}
		}
		public int InventoryLimit
		{
			get
			{
				int realLimit = m_inventoryLimit;
				foreach (InventoryStack itemStack in m_itemStacks)
				{
					if (itemStack.entity.GetType() == typeof(BackBag))
					{
						BackBag backBag = itemStack.entity as BackBag;
						realLimit += backBag.Capacity;
					}
				}
				return realLimit;
			}
		}
		public float SensUpgradeValue
		{
			get
			{
				float upgrade = 0;
				foreach (InventoryStack itemStack in m_itemStacks)
				{
					if (itemStack.entity.GetType() == typeof(SensUpgrade))
					{
						SensUpgrade sensUpgrade = itemStack.entity as SensUpgrade;
						upgrade += sensUpgrade.SensUpgradeValue;
					}
				}
				return upgrade;
			}
		}
		public int DiggingStrength
		{
			get
			{
				int ds = 0;
				foreach (InventoryStack itemStack in m_itemStacks)
				{
					ds += itemStack.entity.GetDiggingStrength() * itemStack.count;
				}
				return ds;
			}
		}
        public float SpeedMultiplier
        {
			get
			{
				float upgrade = 1.0f;
				foreach (InventoryStack itemStack in m_itemStacks)
				{
					if (itemStack.entity.GetType() == typeof(SpeedUpgrade))
					{
                        SpeedUpgrade speedUpgrade = itemStack.entity as SpeedUpgrade;
						upgrade = Math.Max(speedUpgrade.SpeedUpgradeValue, upgrade);
					}
				}
				return upgrade;
			}
        }
		CAnimFloat m_cursorAnim = new CAnimFloat("inv", 1);
		InventoryStack m_selectedItem = null;
		Texture2D texInvCursor;
		static SoundEffect s_sndMenuNavigate = CFrameData.Instance.Content.Load<SoundEffect>("sounds/menuNavigate");
		public int GetItemLevel<T>()
		{
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				if (itemStack.entity.GetType() == typeof(T))
				{
					return itemStack.entity.Level;
				}
			}
			return 0;
		}
		CAnimFloat m_allowInputDelay = new CAnimFloat();

		internal int GetItemCount<T>()
		{
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				if (itemStack.entity.GetType() == typeof(T))
				{
					return itemStack.count;
				}
			}
			return 0;
		}

		internal void RemoveOneOf<T>()
		{
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				if (itemStack.entity.GetType() == typeof(T))
				{
					itemStack.count--;
					if (itemStack.count == 0)
					{
						m_itemStacks.Remove(itemStack);
					}
					return;
				}
			}
		}

		internal bool HasItem<T>()
		{
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				if (itemStack.entity.GetType() == typeof(T))
				{
					return true;
				}
			}
			return false;
		}
		public int KuiCash
		{
			get { return m_kuicash; }
			set
			{
				m_kuicash = value;
				if (m_kuicash < 0) m_kuicash = 0;
			}
		}

		public Inventory()
		{
			Instance = this;
			texInvCursor = CFrameData.Instance.Content.Load<Texture2D>("textures/invCursor");
			m_cursorAnim.StartAnim(1, 1.2f, .5f, 0, eAnimType.EASE_BOTH, eAnimFlag.LOOP | eAnimFlag.PINGPONG);
		}

		bool HasPlaceForNewItem(IEntity in_item)
		{
			if (ItemsCount + in_item.Count <= InventoryLimit) return true;

			return false;
		}

		public void RemoveItem(IEntity in_item)
		{
			for (int i = 0; i < m_itemStacks.Count(); ++i)
			{
				InventoryStack itemStack = m_itemStacks[i];
				if (itemStack.entity.IsSameInventoryType(in_item))
				{
					itemStack.count--;
					if (itemStack.count == 0)
					{
						m_itemStacks.RemoveAt(i--);
					}
				}
			}
		}

		public bool AddItem(IEntity in_item)
		{
			return AddItem(in_item, null);
		}
		public bool AddItem(IEntity in_item, IEntity in_toReplace)
		{
			int remainingPlace;
			int toAdd;
			List<CSnowfield.MsgIcon> msgIcons;

			if (IsInventoryFull)
			{
				CSnowfield.Instance.WarningMessage = "Inventory Full";
				return false;
			}

			if (in_toReplace != null)
			{
				// Remove it
				RemoveItem(in_toReplace);
			}

			foreach (InventoryStack itemStack in m_itemStacks)
			{
				if (itemStack.entity.IsSameInventoryType(in_item))
				{
					remainingPlace = InventoryLimit - ItemsCount;
					toAdd = Math.Min(in_item.Count, remainingPlace);
					itemStack.count += toAdd;
					if (IsInventoryFull)
					{
						CSnowfield.Instance.WarningMessage = "Inventory Full";
					}
					else if (in_item.Name != "")
					{
					/*	if (toAdd > 1)
						{
							CSnowfield.Instance.InfoMessage = in_item.Name + " x " + toAdd;
						}
						else*/
						{
							CSnowfield.Instance.InfoMessage = in_item.Name;
						}
					}
					msgIcons = new List<CSnowfield.MsgIcon>();
					for (int i = 0; i < toAdd; ++i)
					{
						msgIcons.Add(new CSnowfield.MsgIcon(in_item));
					}
					CSnowfield.Instance.MsgIcons = msgIcons;
					return true;
				}
			}
			InventoryStack invStack = new InventoryStack();
			remainingPlace = InventoryLimit - ItemsCount;
			toAdd = Math.Min(in_item.Count, remainingPlace);
			invStack.count = toAdd;
			invStack.entity = in_item;
			m_itemStacks.Add(invStack);
			m_selectedItem = invStack;
			if (IsInventoryFull)
			{
				CSnowfield.Instance.WarningMessage = "Inventory Full";
			}
			else if (in_item.Name != "")
			{
			/*	if (toAdd > 1)
				{
					CSnowfield.Instance.InfoMessage = in_item.Name + " x " + toAdd;
				}
				else*/
				{
					CSnowfield.Instance.InfoMessage = in_item.Name;
				}
			}
			msgIcons = new List<CSnowfield.MsgIcon>();
			for (int i = 0; i < toAdd; ++i)
			{
				msgIcons.Add(new CSnowfield.MsgIcon(in_item));
			}
			CSnowfield.Instance.MsgIcons = msgIcons;
			return true;
		}

		public void Update()
		{
			CFrameData fd = CFrameData.Instance;

			// Move cursor around to select different items
			if (m_selectedItem == null) return;
			int currentIndex = m_itemStacks.IndexOf(m_selectedItem);
			int newIndex = currentIndex;
			int colCount = (fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 64 - 32 - 32) / 96;
			int col = currentIndex % colCount;
			int row = currentIndex / colCount;

			float inputDelayTime = .15f;

			if (!m_allowInputDelay.IsPlaying)
			{
				if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickLeft) ||
					fd.InputMgr.IsButtonFirstDown(Buttons.DPadLeft) ||
					fd.InputMgr.IsKeyFirstDown(Keys.A) ||
					fd.InputMgr.IsKeyFirstDown(Keys.Left))
				{
					col--;
					if (col < 0) col = colCount - 1;
					m_allowInputDelay.StartAnim(0, 1, inputDelayTime, 0, eAnimType.LINEAR);
				}
				else if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickRight) ||
					fd.InputMgr.IsButtonFirstDown(Buttons.DPadRight) ||
					fd.InputMgr.IsKeyFirstDown(Keys.D) ||
					fd.InputMgr.IsKeyFirstDown(Keys.Right))
				{
					col++;
					if (col >= colCount || currentIndex == m_itemStacks.Count - 1)
					{
						col = 0;
					}
					m_allowInputDelay.StartAnim(0, 1, inputDelayTime, 0, eAnimType.LINEAR);
				}
				else if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickUp) ||
					fd.InputMgr.IsButtonFirstDown(Buttons.DPadUp) ||
					fd.InputMgr.IsKeyFirstDown(Keys.W) ||
					fd.InputMgr.IsKeyFirstDown(Keys.Up))
				{
					int oldRow = row;
					row--;
					if (row < 0) row = m_itemStacks.Count() / colCount;
					if (row < 0) row = 0;
					newIndex = row * colCount + col;
					if (newIndex >= m_itemStacks.Count()) row = oldRow;
					m_allowInputDelay.StartAnim(0, 1, inputDelayTime, 0, eAnimType.LINEAR);
				}
				else if (fd.InputMgr.IsButtonFirstDown(Buttons.LeftThumbstickDown) ||
					fd.InputMgr.IsButtonFirstDown(Buttons.DPadDown) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.S) ||
                    fd.InputMgr.IsKeyFirstDown(Keys.Down))
				{
					int oldRow = row;
					row++;
					if (row > m_itemStacks.Count() / colCount) row = 0;
					newIndex = row * colCount + col;
					if (newIndex >= m_itemStacks.Count()) row = oldRow;
					m_allowInputDelay.StartAnim(0, 1, inputDelayTime, 0, eAnimType.LINEAR);
				}

				newIndex = Math.Min(row * colCount + col, m_itemStacks.Count() - 1);
				if (newIndex < 0) newIndex = 0;
				if (newIndex != currentIndex)
				{
					// Play sound
					s_sndMenuNavigate.Play();
					m_selectedItem = m_itemStacks[newIndex];
				}
			}

			// If we are within home radius, we can sell our shit
		/*	if (CSnowfield.Instance.IsSquirrelHome)
			{
				if (fd.InputMgr.IsButtonFirstDown(Buttons.X))
				{
				}
			}*/
		}

		public void SellAllItems()
		{
			// Sell all nuts stacks
			int itemsSoldCount = 0;
			List<CSnowfield.MsgIcon> icons = new List<CSnowfield.MsgIcon>();
			for (int i = 0; i < m_itemStacks.Count(); ++i)
			{
				InventoryStack stack = m_itemStacks[i];
				if (stack.entity.GetType() == typeof(CNut))
				{
					// Sell it
				//	m_kuicash += stack.entity.Value * stack.count;
					for (int j = 0; j < stack.count; ++j)
					{
						icons.Insert(
							CFrameData.Instance.Random.Next(icons.Count), 
							new CSnowfield.MsgIcon(stack.entity));
					}
					itemsSoldCount++;
					m_itemStacks.RemoveAt(i--);
					if (stack == m_selectedItem)
					{
						m_selectedItem = null;
						if (i >= m_itemStacks.Count())
						{
							if (m_itemStacks.Count() > 0)
							{
								m_selectedItem = m_itemStacks[m_itemStacks.Count() - 1];
							}
						}
						else
						{
							if (m_itemStacks.Count() > 0)
							{
								m_selectedItem = m_itemStacks[Math.Min(i + 1, m_itemStacks.Count() - 1)];
							}
						}
					}
				}
			}
			if (itemsSoldCount > 0)
			{
				CSnowfield.Instance.DepositIcons = icons;
		//		s_sndSell.Play();
			}
		}

		private static Vector2 s_textOffset = new Vector2(48, 72);
		private static Vector2 s_cursorOffset = new Vector2(48, 48);
		private static Vector2 s_cursorOrigin = new Vector2(32, 32);

		public void RenderItemStack(InventoryStack itemStack, Vector2 screenPos)
		{
			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

			SquirrelHelper.DrawString(itemStack.count.ToString(), screenPos + s_textOffset, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.CENTER, SquirrelHelper.eTEXT_ALIGN.TOP);
			itemStack.entity.DrawInventoryItem(screenPos);

			if (m_selectedItem == itemStack)
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
			int colCount = (fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 64 - 32 - 32) / 96;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			// Fade out the back
			sb.Draw(fd.CommonResources.Tex_White, fd.Graphics.GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, .75f));

			// Draw kuicash amount
			SquirrelHelper.DrawString("Kuicash: " + m_kuicash.ToString(), screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
			screenPos.Y += 64;

			// Capacity
			int itemsCount = ItemsCount;
			int invLimit = InventoryLimit;
			SquirrelHelper.DrawString("Capacity: " + itemsCount + "/" + invLimit + 
				((itemsCount == invLimit) ? " - FULL" : ""),
				screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
			screenPos.Y += 64;

			// Write selected item
			if (m_selectedItem != null)
			{
				SquirrelHelper.DrawString("Selected: " + m_selectedItem.entity.Name,
					screenPos, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.TOP);
				if (m_selectedItem.entity.Value > 0)
				{
					SquirrelHelper.DrawString("Value: " + m_selectedItem.entity.Value,
						new Vector2((float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 32, screenPos.Y)
							, Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.TOP);
				}
			}
			screenPos.Y += 64;

			// Draw items
			int curCol = 0;
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				RenderItemStack(itemStack, screenPos);
				screenPos.X += 96;
				curCol++;
				if (curCol >= colCount)
				{
					curCol = 0;
					screenPos.X = (float)fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea.Left + 32;
					screenPos.Y += 128;
				}
			}
			float padding = 16;

			Rectangle safeFrame = fd.Graphics.GraphicsDevice.Viewport.TitleSafeArea;

			sb.Draw(fd.CommonResources.Tex_Buttons,
				new Vector2((float)safeFrame.Right - padding - 32, (float)safeFrame.Bottom - padding - 32),
                fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnB : fd.CommonResources.rectBtnEsc, Color.White, 0, fd.CommonResources.btnOrigin, 1, SpriteEffects.None, 0);
			SquirrelHelper.DrawString("Back",
				new Vector2((float)safeFrame.Right - padding - 64, (float)safeFrame.Bottom - padding),
				Color.White, SquirrelHelper.eTEXT_ALIGN.RIGHT, SquirrelHelper.eTEXT_ALIGN.BOTTOM);
				
			sb.End();
		}

		internal void LoseAllNuts()
		{
			// Remove all nuts stacks
			for (int i = 0; i < m_itemStacks.Count(); ++i)
			{
				InventoryStack stack = m_itemStacks[i];
				if (stack.entity.GetType() == typeof(CNut))
				{
					// Sell it
					m_itemStacks.RemoveAt(i--);
					if (stack == m_selectedItem)
					{
						m_selectedItem = null;
						if (i >= m_itemStacks.Count())
						{
							if (m_itemStacks.Count() > 0)
							{
								m_selectedItem = m_itemStacks[m_itemStacks.Count() - 1];
							}
						}
						else
						{
							if (m_itemStacks.Count() > 0)
							{
								m_selectedItem = m_itemStacks[Math.Min(i + 1, m_itemStacks.Count() - 1)];
							}
						}
					}
				}
			}
		}

		internal void Dispose()
		{
			m_allowInputDelay.Stop(); m_allowInputDelay = null;
			m_cursorAnim.Stop();
			m_cursorAnim = null;
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				itemStack.entity = null;
			}
			m_selectedItem = null;
			Instance = null;
		}

		public void Save(System.IO.BinaryWriter fic_out)
		{
			// Cash
			fic_out.Write(m_kuicash);

			// Inventory items
			fic_out.Write(m_itemStacks.Count());
			List<Store.StoreItem> storeItems = Store.Instance.StoreItems;
			foreach (InventoryStack itemStack in m_itemStacks)
			{
				// -1 if nuts, store item reference otherwise
				int entityType = -2;
				if (itemStack.entity.GetType() == typeof(CNut))
				{
					entityType = -1;
					fic_out.Write(entityType);
					fic_out.Write(itemStack.count);
					itemStack.entity.Save(fic_out);
				}
				else
				{
					int i, j;
					for (i = 0; i < storeItems.Count(); ++i)
					{
						for (j = 0; j < storeItems[i].items.Count(); ++j)
						{
							if (itemStack.entity == storeItems[i].items[j])
							{
								fic_out.Write(i);
								fic_out.Write(j);
								fic_out.Write(itemStack.count);
								break;
							}
						}
						if (j != storeItems[i].items.Count()) break;
					}
				}
			}
		}

		internal void Load(System.IO.BinaryReader fic_in)
		{
			m_itemStacks.Clear();

			// Cash
			m_kuicash = fic_in.ReadInt32();

			// Inventory items
			int stackCount = fic_in.ReadInt32();
			List<Store.StoreItem> storeItems = Store.Instance.StoreItems;
			for (int i = 0; i < stackCount; ++i)
			{
				int entityType = fic_in.ReadInt32();
				if (entityType == -1)
				{
					// Its a nut!
					InventoryStack invStack = new InventoryStack();
					invStack.count = fic_in.ReadInt32();
					invStack.entity = new CNut();
					invStack.entity.Load(fic_in);
					m_itemStacks.Add(invStack);
				}
				else
				{
					int storeItemIndex = fic_in.ReadInt32();
					InventoryStack invStack = new InventoryStack();
					invStack.count = fic_in.ReadInt32();
					invStack.entity = storeItems[entityType].items[storeItemIndex];
					m_itemStacks.Add(invStack);

					// Remove it from the store now
					if (storeItems[entityType].items.Count() > 1)
					{
						if (storeItemIndex == storeItems[entityType].items.Count() - 1)
						{
							storeItems[entityType].currentItem = null;
						}
						else
						{
							storeItems[entityType].currentItem = storeItems[entityType].items[storeItemIndex + 1];
						}
					}
				}
			}

			if (m_itemStacks.Count() > 0) m_selectedItem = m_itemStacks.First();
		}
	}
}
