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


namespace ProcrastinatingSquirrel
{
	class IEntity
	{
		Vector2 m_position;
		float m_angle = 0;
		CTile m_tile;
		public Vector2 Position
		{
			get { return m_position; }
			set { m_position = value; }
		}
		public CTile Tile
		{
			get { return m_tile; }
			set { m_tile = value; }
		}
		public float Angle
		{
			get { return m_angle; }
			set { m_angle = value; }
		}
		public virtual int Value
		{
			get { return 0; }
		}
		public virtual int Count
		{
			get { return 1; }
		}
		public virtual int Cost
		{
			get { return 0; }
		}
		public virtual string Name
		{
			get { return "ERR_UNDEFINED_ENTITY"; }
		}
		public virtual bool CanDig
		{
			get { return true; }
		}

		public virtual bool IsSameInventoryType(IEntity in_entity) { return (in_entity.GetType() == GetType()); }
		public virtual void Update() { }
		public virtual void RenderUnder() { }
		public virtual void Render() { }
		public virtual void RenderSens() { }
		public virtual void RenderLayer(int layer, ref Vector2 offset) { }
		public virtual int GetDiggingStrength() { return 0; }
		public virtual bool Dig(IEntity digger) { return false; }
		public virtual void GiveItem(IEntity item) { }

		public void DrawInventoryItem(Vector2 screenPos)
		{
			DrawInventoryItem(screenPos, null, null, false);
		}
		public void DrawInventoryItem(Vector2 screenPos, float? preferedSize)
		{
			DrawInventoryItem(screenPos, preferedSize, null, false);
		}
		public void DrawInventoryItem(Vector2 screenPos, float? preferedSize, Color? in_color)
		{
			DrawInventoryItem(screenPos, preferedSize, in_color, false);
		}
		public virtual void DrawInventoryItem(Vector2 screenPos, float? preferedSize, Color? in_color, bool centered) { }

		public virtual void Trigger(IEntity digger, int in_x, int in_y) { }

		public int Level { get; set; }

		public virtual void Save(System.IO.BinaryWriter fic_out) { }
		public virtual void Load(System.IO.BinaryReader fic_in) {}

		internal virtual void Dispose()
		{
			m_tile = null;
		}
	}
}
