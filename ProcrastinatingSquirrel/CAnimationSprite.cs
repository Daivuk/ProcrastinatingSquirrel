using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using DK8;

namespace ProcrastinatingSquirrel
{
	class CFrameInfo
	{
		public Rectangle rect;
		public Object userData;
		public SoundEffect[] sounds;
		public float volume;
		public CFrameInfo(Rectangle in_rect)
		{
			rect = in_rect;
			userData = null;
			sounds = null;
			volume = 1;
		}
		public CFrameInfo(Rectangle in_rect, Object in_userData)
		{
			rect = in_rect;
			userData = in_userData;
			sounds = null;
			volume = 1;
		}
		public CFrameInfo(Rectangle in_rect, Object in_userData, SoundEffect[] in_sounds)
		{
			rect = in_rect;
			userData = in_userData;
			sounds = in_sounds;
			volume = 1;
		}
		public CFrameInfo(Rectangle in_rect, Object in_userData, SoundEffect[] in_sounds, float in_volume)
		{
			rect = in_rect;
			userData = in_userData;
			sounds = in_sounds;
			volume = in_volume;
		}

		internal void Dispose()
		{
			sounds = null;
			userData = null;
		}
	}


	class CAnimationInfo
	{
		public Texture2D texture;
		public CFrameInfo[] frames;
		public float delay;
		public CAnimationInfo(string in_textureFilename, CFrameInfo[] in_frames, float in_delay)
		{
			texture = CFrameData.Instance.Content.Load<Texture2D>(in_textureFilename);
			frames = in_frames;
			delay = in_delay;
		}

		internal void Dispose()
		{
			foreach (CFrameInfo frame in frames)
			{
				frame.Dispose();
			}
			frames = null;
		}
	}


	class CAnimationSprite
	{
		CAnimationInfo[] m_animations = null;
		int m_lastFrame = 0;
		CAnimInt m_frameAnim = new CAnimInt("game", 0);
		int m_currentAnim = 0;
		public int CurrentAnimation
		{
			get { return m_currentAnim; }
		}
		public Texture2D Texture
		{
			get
			{
				return m_animations[m_currentAnim].texture;
			}
		}
		public Rectangle SrcRect
		{
			get
			{
				int frame = m_frameAnim.Value;
				if (frame >= m_animations[m_currentAnim].frames.Count()) frame = 0;
				return m_animations[m_currentAnim].frames[frame].rect;
			}
		}
		public Object UserData
		{
			get
			{
				int frame = m_frameAnim.Value;
				if (frame >= m_animations[m_currentAnim].frames.Count()) frame = 0;
				return m_animations[m_currentAnim].frames[frame].userData;
			}
		}

		public CAnimationSprite(CAnimationInfo[] in_animations)
		{
			m_animations = in_animations;
		}

		~CAnimationSprite()
		{
			if (m_frameAnim != null) m_frameAnim.Stop();
		}

		public void PlayAnim(int animId, eAnimType animType, eAnimFlag animFlags)
		{
			m_currentAnim = animId;
			m_lastFrame = 0;
			m_frameAnim.Value = 0;
			m_frameAnim.StartAnim(0, m_animations[m_currentAnim].frames.Count(),
				(float)m_animations[m_currentAnim].frames.Count() / m_animations[m_currentAnim].delay,
				0, animType, animFlags);
		}

		public bool IsPlaying
		{
			get { return m_frameAnim.IsPlaying; }
		}

		internal void Update()
		{
			int currentFrame = m_frameAnim.Value;
			if (currentFrame != m_lastFrame)
			{
				// Play sounds or events
				CFrameInfo frame = m_animations[m_currentAnim].frames[currentFrame];
				if (frame.sounds != null)
				{
					frame.sounds[CFrameData.Instance.Random.Next(frame.sounds.Length)].Play(frame.volume, 0, 0);
				}
			}
			m_lastFrame = currentFrame;
		}

		internal void Dispose()
		{
			m_frameAnim.Stop();
			m_frameAnim = null;
			foreach (CAnimationInfo anim in m_animations)
			{
				anim.Dispose();
			}
			m_animations = null;
		}
	}
}
