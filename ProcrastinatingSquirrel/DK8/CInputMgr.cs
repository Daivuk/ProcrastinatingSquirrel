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
	class CInputMgr
	{
		KeyboardState m_previousKeyboardState = Keyboard.GetState();
		KeyboardState m_currentKeyboardState;
		MouseState m_previousMouseState = Mouse.GetState();
		MouseState m_currentMouseState;
		GamePadState m_previousGamePadState;
		GamePadState m_currentGamePadState;
		public GamePadState GamePadState { get { return m_currentGamePadState; } }
		Vector2 m_mouseMovement = new Vector2();
		public Vector2 MouseMovement
		{
			get { return m_mouseMovement; }
		}
		Vector2 m_mousePos = new Vector2();
		public Vector2 MousePos
		{
			get { return m_mousePos; }
			set
			{
				Mouse.SetPosition((int)value.X, (int)value.Y);
				m_mousePos = value;
			}
		}
		public void CenterMouse(GraphicsDevice device)
		{
			Mouse.SetPosition(
				device.Viewport.Width / 2,
				device.Viewport.Height / 2);
        }
        public bool ControllerConnected = false;

        public CInputMgr()
		{
			m_previousGamePadState = GamePad.GetState(m_playerIndex);
			m_currentKeyboardState = m_previousKeyboardState;
			m_currentMouseState = m_previousMouseState;
			m_currentGamePadState = m_previousGamePadState;
		}

		public void Update(GameTime gameTime)
		{
			m_previousKeyboardState = m_currentKeyboardState;
			m_currentKeyboardState = Keyboard.GetState();
			m_previousMouseState = m_currentMouseState;
			m_currentMouseState = Mouse.GetState();
			m_previousGamePadState = m_currentGamePadState;
			m_currentGamePadState = GamePad.GetState(m_playerIndex);
            ControllerConnected = m_currentGamePadState.IsConnected;

            m_mouseMovement.X = (float)m_currentMouseState.X - (float)m_previousMouseState.X;
			m_mouseMovement.Y = (float)m_currentMouseState.Y - (float)m_previousMouseState.Y;
			m_mousePos.X = (float)m_currentMouseState.X;
			m_mousePos.Y = (float)m_currentMouseState.Y;
		}

		private PlayerIndex m_playerIndex = PlayerIndex.One;
		public PlayerIndex PlayerIndex
		{
			get { return m_playerIndex; }
			set
			{
				m_playerIndex = value;
				m_previousGamePadState = GamePad.GetState(m_playerIndex);
				m_currentGamePadState = GamePad.GetState(m_playerIndex);
			}
		}

		public bool IsButtonFirstDown(Buttons button)
		{
			return m_previousGamePadState.IsButtonUp(button) && m_currentGamePadState.IsButtonDown(button);
		}

		public bool IsButtomDown(Buttons button)
		{
			return m_currentGamePadState.IsButtonDown(button);
		}

		public bool IsButtomUp(Buttons button)
		{
			return m_currentGamePadState.IsButtonUp(button);
		}

		public bool IsKeyFirstDown(Keys key)
		{
			return m_previousKeyboardState.IsKeyUp(key) && m_currentKeyboardState.IsKeyDown(key);
		}

		public bool IsKeyDown(Keys key)
		{
			return m_currentKeyboardState.IsKeyDown(key);
		}

		public bool IsKeyUp(Keys key)
		{
			return m_currentKeyboardState.IsKeyUp(key);
		}

		public bool IsLeftMouseFirstDown()
		{
			return m_previousMouseState.LeftButton == ButtonState.Released &&
				m_currentMouseState.LeftButton == ButtonState.Pressed;
		}

		public bool IsLeftMouseDown()
		{
			return m_currentMouseState.LeftButton == ButtonState.Pressed;
		}

		public bool IsLeftMouseUp()
		{
			return m_currentMouseState.LeftButton == ButtonState.Released;
		}

		public bool IsRightMouseFirstDown()
		{
			return m_previousMouseState.RightButton == ButtonState.Released &&
				m_currentMouseState.RightButton == ButtonState.Pressed;
		}

		public bool IsRightMouseDown()
		{
			return m_currentMouseState.RightButton == ButtonState.Pressed;
		}

		public bool IsRightMouseUp()
		{
			return m_currentMouseState.RightButton == ButtonState.Released;
		}

		public bool IsMiddleMouseFirstDown()
		{
			return m_previousMouseState.MiddleButton == ButtonState.Released &&
				m_currentMouseState.MiddleButton == ButtonState.Pressed;
		}

		public bool IsMiddleMouseDown()
		{
			return m_currentMouseState.MiddleButton == ButtonState.Pressed;
		}

		public bool IsMiddleMouseUp()
		{
			return m_currentMouseState.MiddleButton == ButtonState.Released;
		}

		public int GetMouseWheel()
		{
			return m_currentMouseState.ScrollWheelValue - m_previousMouseState.ScrollWheelValue;
		}
	}
}
