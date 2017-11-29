using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using DK8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProcrastinatingSquirrel.Entities
{
	class CStoreItem : IEntity
	{
		Store.StoreItem m_storeItem;
		static SoundEffect s_sndMenuNavigate = CFrameData.Instance.Content.Load<SoundEffect>("sounds/menuNavigate");
		static SoundEffect s_sndBuy = CFrameData.Instance.Content.Load<SoundEffect>("sounds/buy");
		static SoundEffect s_sndError = CFrameData.Instance.Content.Load<SoundEffect>("sounds/error");
		CAnimFloat m_scale = new CAnimFloat(1);
		bool m_squirrelHover = false;

		public CStoreItem(Store.StoreItem in_storeItem)
		{
			m_storeItem = in_storeItem;
		}

		public override void Render()
		{
			if (m_storeItem.currentItem == null) return;
			if (Inventory.Instance.KuiCash == 0 && Inventory.Instance.IsEmpty) return;

			m_storeItem.currentItem.DrawInventoryItem(Position, m_scale.Value, Color.White, true);

			SpriteBatch sb = CFrameData.Instance.SpriteBatch;
			CFrameData fd = CFrameData.Instance;
			bool canAfford = (Inventory.Instance.KuiCash >= m_storeItem.currentItem.Cost);
			if (m_squirrelHover)
			{
				// Draw text n cost n shit
				SquirrelHelper.DrawString(m_storeItem.currentItem.Name, Position + new Vector2(4 - m_scale.Value * 2, -.5f),
					Globals.TextColor, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.CENTER,
					CSnowfield.INV_TILE_SCALE * 1.25f);
				SquirrelHelper.DrawString("Cost: " + m_storeItem.currentItem.Cost.ToString(), Position + new Vector2(4 - m_scale.Value * 2, 0),
					(canAfford ? Globals.TextColor : Color.Red), SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.CENTER,
					CSnowfield.INV_TILE_SCALE * 1.25f);
				if (canAfford)
				{
					sb.Draw(fd.CommonResources.Tex_Buttons, Position + new Vector2(4.5f - m_scale.Value * 2, .5f),
                        fd.InputMgr.ControllerConnected ? fd.CommonResources.rectBtnA : fd.CommonResources.rectBtnEnter,
						Color.White, 0, fd.CommonResources.btnOrigin, CSnowfield.INV_TILE_SCALE * 1.25f, SpriteEffects.None, 0);
					SquirrelHelper.DrawString("Buy", Position + new Vector2(5 - m_scale.Value * 2, .5f),
						Color.White, SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.CENTER,
						CSnowfield.INV_TILE_SCALE * 1.25f);
				}
			}
			else
			{
		/*		float brightness = .65f;
				// Draw text n cost n shit
				SquirrelHelper.DrawString(m_storeItem.currentItem.Name, Position + new Vector2(4 - m_scale.Value * 2, -.5f),
					new Color(Globals.TextColor.ToVector4() * brightness), SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.CENTER,
					CSnowfield.INV_TILE_SCALE * 1.25f);
				SquirrelHelper.DrawString("Cost: " + m_storeItem.currentItem.Cost.ToString(), Position + new Vector2(4 - m_scale.Value * 2, 0),
					(canAfford ? new Color(Globals.TextColor.ToVector4() * brightness) : new Color(brightness, 0, 0, brightness)), 
					SquirrelHelper.eTEXT_ALIGN.LEFT, SquirrelHelper.eTEXT_ALIGN.CENTER,
					CSnowfield.INV_TILE_SCALE * 1.25f);*/
			}
		}

		public override void Update()
		{
			if (m_storeItem.currentItem == null) return;
			if (Inventory.Instance.KuiCash == 0 && Inventory.Instance.IsEmpty) return;

			CSquirrel squirrel = CSnowfield.Instance.Squirrel;
			if (Vector2.DistanceSquared(squirrel.Position, Position) <= .74f &&
				squirrel.Position.Y >= Position.Y - .45f && squirrel.Position.Y <= Position.Y + .45f)
			{
				if (!m_squirrelHover)
				{
					m_scale.StartAnimFromCurrent(1.25f, .20f, 0, eAnimType.EASE_OUT);
				}
				m_squirrelHover = true;
			}
			else
			{
				if (m_squirrelHover)
				{
					m_scale.StartAnimFromCurrent(1, .20f, 0, eAnimType.EASE_IN);
				}
				m_squirrelHover = false;
			}

			if (m_squirrelHover)
			{
				if (CFrameData.Instance.InputMgr.IsButtonFirstDown(Buttons.A) || CFrameData.Instance.InputMgr.IsKeyFirstDown(Keys.Enter))
				{
					if (Inventory.Instance.KuiCash >= m_storeItem.currentItem.Cost &&
						!Inventory.Instance.IsInventoryFull)
					{
						s_sndBuy.Play();
						IEntity toReplace = null;
						if (m_storeItem.items.IndexOf(m_storeItem.currentItem) > 0)
						{
							toReplace = m_storeItem.items[m_storeItem.items.IndexOf(m_storeItem.currentItem) - 1];
						}
						Inventory.Instance.AddItem(m_storeItem.currentItem, toReplace);
						Inventory.Instance.KuiCash = Inventory.Instance.KuiCash - m_storeItem.currentItem.Cost;
						if (m_storeItem.items.Count() > 1)
						{
							if (m_storeItem.items.Last() == m_storeItem.currentItem)
							{
								// Remove this item from the store
								m_storeItem.currentItem = null;
							}
							else
							{
								m_storeItem.currentItem =
									m_storeItem.items[m_storeItem.items.IndexOf(m_storeItem.currentItem) + 1];
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
	}
}
