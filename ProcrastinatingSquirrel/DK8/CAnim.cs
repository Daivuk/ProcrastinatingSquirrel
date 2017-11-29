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
	public abstract class IAnimatable
	{
		public object UserData = null;
		string m_category = "";
		bool m_isRegistered = false;
		protected float m_delay = 0;
		static int m_animCount = 0;
		public bool IsPlaying { get { return (m_isRegistered || (m_needToBeRegistered && !m_needToBeUnregistered)) && m_delay <= 0; } }
		public string Category
		{
			get
			{
				return m_category;
			}
			set
			{
				if (IsPlaying) return;
				m_category = value;
			}
		}

	//	static List<IAnimatable> s_playingAnims = new List<IAnimatable>();
		static Dictionary<string, List<IAnimatable>> s_playingAnims = new Dictionary<string, List<IAnimatable>>();
		static List<string> s_categoriesPaused = new List<string>();

		static public void PauseCategory(string in_category)
		{
			if (s_categoriesPaused.Contains(in_category)) return;
			s_categoriesPaused.Add(in_category);
		}

		static public void UnpauseCategory(string in_category)
		{
			if (!s_categoriesPaused.Contains(in_category)) return;
			s_categoriesPaused.Remove(in_category);
		}

		static public int GetActiveAnimsCount()
		{
			int count = 0;
			foreach (KeyValuePair<string, List<IAnimatable>> anims in s_playingAnims)
			{
				count += anims.Value.Count();
			}
			return count;
		}

		static bool isBusy = false;

		static public void UpdateAnims()
		{
			// Play anims
			int i;
			isBusy = true;
			m_animCount = 0;
			foreach (KeyValuePair<string, List<IAnimatable>> anims in s_playingAnims)
			{
				if (s_categoriesPaused.Contains(anims.Key)) continue;
				for (i = 0; i < anims.Value.Count; ++i)
				{
					if (!anims.Value[i].Update())
					{
						IAnimatable anim = anims.Value[i];
						anim.m_isRegistered = false;
						anims.Value.RemoveAt(i);
						--i;
						anim.Callback();
					}
					else m_animCount++;
				}
			}
			isBusy = false;

			// Unregister those that needed to be unregistered while busy
			foreach (KeyValuePair<string, List<IAnimatable>> anims in s_playingAnims)
			{
				for (i = 0; i < anims.Value.Count; ++i)
				{
					if (anims.Value[i].m_needToBeUnregistered)
					{
						IAnimatable anim = anims.Value[i];
						anim.m_isRegistered = false;
						anims.Value[i].m_needToBeUnregistered = false;
						anims.Value.RemoveAt(i);
						--i;
						anim.Callback();
					}
				}
			}

			// Register those that need to be
			foreach (IAnimatable anim in s_toBeRegistered)
			{
				anim.RegisterAnim();
			}
			s_toBeRegistered.Clear();
		}

		static public void StopAllAnims()
		{
			StopAllAnims(null);
		}
		static public void StopAllAnims(string in_category)
		{
			int i;
			foreach (KeyValuePair<string, List<IAnimatable>> anims in s_playingAnims)
			{
				if (anims.Key != in_category) continue;
				for (i = 0; i < anims.Value.Count; ++i)
				{
					anims.Value[i].m_isRegistered = false;
				}
				anims.Value.Clear();
			}
			if (in_category == null) s_playingAnims.Clear();
		}

		virtual public bool Update() { return true; }
		virtual protected void Callback() { }

		protected bool m_needToBeRegistered = false;
		protected bool m_needToBeUnregistered = false;

		static List<IAnimatable> s_toBeRegistered = new List<IAnimatable>();

		protected void RegisterAnim()
		{
			if (m_isRegistered && !m_needToBeUnregistered) return;
			if (isBusy)
			{
				if (m_needToBeUnregistered)
				{
					// This means we were already registered, ignore!
					m_needToBeUnregistered = false;
					m_needToBeRegistered = false;
				}
				else if (!m_needToBeRegistered)
				{
					s_toBeRegistered.Add(this);
					m_needToBeRegistered = true;
				}
				return;
			}
			m_needToBeRegistered = false;
			if (!s_playingAnims.ContainsKey(m_category)) s_playingAnims[m_category] = new List<IAnimatable>();
			s_playingAnims[m_category].Add(this);
			m_isRegistered = true;
		}

		protected void UnregisterAnim()
		{
			if (!m_isRegistered) return;
			if (isBusy)
			{
				if (m_needToBeRegistered)
				{
					// That means we were already unregistered, ignore!
					m_needToBeUnregistered = false;
					m_needToBeRegistered = false;
					s_toBeRegistered.Remove(this);
				}
				else m_needToBeUnregistered = true;
				return;
			}
			m_needToBeUnregistered = false;
			m_isRegistered = false;
			if (s_playingAnims.ContainsKey(m_category))
			{
				s_playingAnims[m_category].Remove(this);
			}
		}

		public static int AnimCount { get { return m_animCount; } }
	}



	public enum eAnimType
	{
		LINEAR,
		EASE_IN,
		EASE_OUT,
		EASE_BOTH
	}

	public enum eAnimFlag
	{
		LOOP = 0x0001,
		PINGPONG = 0x0002
	}

	public delegate void EndAnimCallback(IAnimatable anim);

	public class KeyFrame
	{
		public float time;
		public eAnimType animType;
	}

	public abstract class IAnim<T> : IAnimatable
	{
		class SAnimQueue
		{
			public T from;
			public T to;
			public float duration;
			public float delay;
			public eAnimType animType;
			public eAnimFlag animFlags;
		}
		List<SAnimQueue> m_animQueue = new List<SAnimQueue>();

		class SKeyFrame
		{
			public T from;
			public T to;
			public float startTime;
			public float duration;
			public float delay;
			public eAnimType animType;
		}
		List<SKeyFrame> m_keyFrames = null;
		public List<KeyFrame> KeyFrames
		{
			get
			{
				List<KeyFrame> keyFrames = new List<KeyFrame>();
				foreach (SKeyFrame keyFrame in m_keyFrames)
				{
					KeyFrame nkeyFrame = new KeyFrame();
					nkeyFrame.time = keyFrame.startTime;
					nkeyFrame.animType = keyFrame.animType;
					keyFrames.Add(nkeyFrame);
				}
				return keyFrames;
			}
		}

		public KeyFrame GetKeyInTime(float time, float epsilon, KeyFrame previousClosest)
		{
			float closestDis = 100000;
			if (previousClosest != null)
			{
				closestDis = Math.Abs(previousClosest.time - time);
			}

			foreach (SKeyFrame keyFrame in m_keyFrames)
			{
				if (time >= keyFrame.startTime - epsilon &&
					time <= keyFrame.startTime + epsilon)
				{
					float dis = Math.Abs(keyFrame.startTime - time);
					if (dis < closestDis)
					{
						closestDis = dis;
						previousClosest = new KeyFrame();
						previousClosest.time = keyFrame.startTime;
						previousClosest.animType = keyFrame.animType;
					}
				}
			}

			return previousClosest;
		}

		void SetKeyFrame(T value, float startTime)
		{
			//--- Insert it at the right place
			int i = 0;
			for (i = 0; i < m_keyFrames.Count(); ++i)
			{
				SKeyFrame keyFrame = m_keyFrames[i];
				if (keyFrame.startTime == startTime)
				{
					// The key frame already exists
					keyFrame.from = value;
					if (i > 0)
					{
						m_keyFrames[i - 1].to = value;
					}
					return;
				}
				if (startTime < keyFrame.startTime)
				{
					break;
				}
			}

			SKeyFrame newKeyFrame = new IAnim<T>.SKeyFrame();
			SKeyFrame previous = null;
			SKeyFrame next = null;

			if (i > 0) previous = m_keyFrames[i - 1];
			if (i < m_keyFrames.Count()) next = m_keyFrames[i];

			// If no previous, we are the first key if we are at zero or create one if not
			if (previous == null && startTime > 0)
			{
				SKeyFrame firstKeyFrame = new IAnim<T>.SKeyFrame();
				firstKeyFrame.from = value;
				firstKeyFrame.to = value;
				firstKeyFrame.duration = startTime;
				firstKeyFrame.delay = 0;
				firstKeyFrame.startTime = 0;
				firstKeyFrame.animType = eAnimType.LINEAR;
				m_keyFrames.Insert(0, firstKeyFrame);
			}

			newKeyFrame.from = value;
			newKeyFrame.delay = 0;
			if (previous == null)
			{
				newKeyFrame.animType = eAnimType.LINEAR;
			}
			else
			{
				previous.to = value;
				newKeyFrame.animType = previous.animType;
				if (previous.startTime + previous.delay + previous.duration > startTime)
				{
					if (previous.startTime + previous.delay < startTime)
					{
						previous.duration = startTime - (previous.startTime + previous.delay);
					}
					else
					{
						previous.duration = 0;
						previous.delay = startTime - previous.startTime;
					}
				}
				else
				{
					previous.duration = startTime - (previous.startTime + previous.delay);
				}
			}

			if (next == null)
			{
				newKeyFrame.to = value;
				newKeyFrame.duration = 0;
			}
			else
			{
				newKeyFrame.to = next.from;
				newKeyFrame.duration = next.startTime - startTime;
			}

			newKeyFrame.startTime = startTime;

			m_keyFrames.Insert(i, newKeyFrame);
		}

		protected T m_value;
		public T Value
		{
			get
			{
				if (IsKeyFramed)
				{
					if (m_keyFramedValueUpToDate) return m_value;
					foreach (SKeyFrame keyFrame in m_keyFrames)
					{
						//--- Don't waste time, go to the next frame
						if (m_keyFramedTime > keyFrame.startTime + keyFrame.delay + keyFrame.duration) continue;
						if (m_keyFramedTime <= keyFrame.startTime + keyFrame.delay)
						{
							m_value = keyFrame.from;
							m_keyFramedValueUpToDate = true;
							return m_value;
						}
						if (m_keyFramedTime >= keyFrame.startTime + keyFrame.delay)
						{
							m_time = (m_keyFramedTime - (keyFrame.startTime + keyFrame.delay)) / keyFrame.duration;
							float percent = 0;

							m_from = keyFrame.from;
							m_to = keyFrame.to;

							switch (m_animType)
							{
								case eAnimType.LINEAR:
									percent = m_time;
									break;
								case eAnimType.EASE_IN:
									percent = m_time * m_time;
									break;
								case eAnimType.EASE_OUT:
									percent = 1 - ((1 - m_time) * (1 - m_time));
									break;
								case eAnimType.EASE_BOTH:
									if (m_time >= .5f)
									{
										percent = 1 - ((1 - m_time) * (1 - m_time)) * 2;
									}
									else
									{
										percent = m_time * m_time * 2;
									}
									break;
							}

							Lerp(percent);
							m_keyFramedValueUpToDate = true;
							return m_value;
						}
					}
					if (m_keyFrames.Count > 0)
					{
						m_value = m_keyFrames.Last().to;
					}
					m_keyFramedValueUpToDate = true;
					return m_value;
				}
				else
				{
					return m_value;
				}
			}
			set
			{
				if (IsKeyFramed)
				{
					SetKeyFrame(value, m_keyFramedTime);
					m_keyFramedValueUpToDate = false;
				}
				else
				{
					if (IsPlaying)
					{
						m_to = value;
						Stop(true);
					}
					else
					{
						m_value = value;
						m_from = value;
					}
				}
			}
		}
		protected T m_from;
		public T From { get { return m_from; } }
		protected T m_to;
		public T To { get { return m_to; } }
		float m_time;
		float m_speed;
		bool m_isDoingPong = false;
		eAnimType m_animType;
		eAnimFlag m_animFlags;
		public eAnimFlag AnimFlags { get { return m_animFlags; } }
		EndAnimCallback m_endAnimCallback = null;
		bool m_callbackOnStop = false;

		bool m_isKeyFramed = false;
		public bool IsKeyFramed
		{
			get { return m_isKeyFramed; }
			set
			{
				m_keyFramedValueUpToDate = false;
				m_isKeyFramed = value;
				if (m_isKeyFramed)
				{
					if (m_keyFrames == null) m_keyFrames = new List<IAnim<T>.SKeyFrame>();
				}
				else
				{
					m_keyFrames = null;
				}
			}
		}
		float m_keyFramedTime = 0;
		public float KeyFramedTime
		{
			get { return m_keyFramedTime; }
			set
			{
				if (m_keyFramedTime == value) return;
				m_keyFramedTime = value;
				m_keyFramedValueUpToDate = false;
			}
		}
		bool m_keyFramedValueUpToDate = false;

		public void SetCallback(EndAnimCallback target, bool callbackOnStop)
		{
			m_endAnimCallback = target;
			m_callbackOnStop = callbackOnStop;
		}

		public IAnim(T value)
		{
			m_value = value;
		}

		public void Stop() { Stop(false); }

		public void Stop(bool setToDestination)
		{
			if (!IsPlaying)
			{
				return;
			}
			if (setToDestination)
			{
				m_value = m_to;
			}
			UnregisterAnim();
			if (m_callbackOnStop)
			{
				Callback();
			}
		}

		override protected void Callback()
		{
			if (m_endAnimCallback != null)
			{
				m_endAnimCallback.Invoke(this);
			}
		}

		public void StartAnimFromCurrent(T to, float duration, float delay, eAnimType animType)
		{
			StartAnim(m_value, to, duration, delay, animType, 0);
		}

		public void StartAnimFromCurrent(T to, float duration, float delay, eAnimType animType, eAnimFlag animFlags)
		{
			StartAnim(m_value, to, duration, delay, animType, animFlags);
		}

		public void StartAnim(T from, T to, float duration, float delay, eAnimType animType)
		{
			StartAnim(from, to, duration, delay, animType, 0);
		}

		public void StartAnim(T from, T to, float duration, float delay, eAnimType animType, eAnimFlag animFlags)
		{
			Stop();
			if (duration == 0)
			{
				m_value = to;
				Callback();
				return;
			}
			m_isDoingPong = false;
			m_from = from;
			m_to = to;
			m_time = 0.0f;
			m_speed = 1.0f / duration;
			m_animType = animType;
			m_animFlags = animFlags;
			m_delay = delay;
			RegisterAnim();
			Lerp(0);
		}

		public void QueueAnim(T from, T to, float duration, float delay, eAnimType animType)
		{
			QueueAnim(from, to, duration, delay, animType, 0);
		}

		public void QueueAnimFromCurrent(T to, float duration, float delay, eAnimType animType)
		{
			T from = m_from;
			if (m_animQueue.Count() > 0)
			{
				from = m_animQueue.First().to;
			}
			QueueAnim(from, to, duration, delay, animType, 0);
		}

		public void QueueAnimFromCurrent(T to, float duration, float delay, eAnimType animType, eAnimFlag animFlags)
		{
			T from = m_from;
			if (m_animQueue.Count() > 0)
			{
				from = m_animQueue.First().to;
			}
			QueueAnim(from, to, duration, delay, animType, 0);
		}

		public void QueueAnim(T from, T to, float duration, float delay, eAnimType animType, eAnimFlag animFlags)
		{
			SAnimQueue animQueue = new IAnim<T>.SAnimQueue();
			animQueue.from = from;
			animQueue.to = to;
			animQueue.duration = duration;
			animQueue.delay = delay;
			animQueue.animType = animType;
			animQueue.animFlags = animFlags;

			if (!IsPlaying && !(m_needToBeRegistered && !m_needToBeUnregistered) && m_animQueue.Count() == 0)
			{
				StartAnim(from, to, duration, delay, animType, animFlags);
			}
			else
			{
				m_animQueue.Add(animQueue);
			}
		}

		bool ignoreDoneAnim = false;
		protected void SetIgnoreDoneAnim()
		{
			ignoreDoneAnim = true;
		}

		public override bool Update()
		{
			float dt = CFrameData.Instance.GetDeltaSecond();
			if (m_delay > 0)
			{
				m_delay -= dt;
				return true;
			}
			m_time += dt * m_speed;
			if (m_time >= 1)
			{
				// Animation is done
				if ((m_animFlags & eAnimFlag.PINGPONG) > 0 && !m_isDoingPong)
				{
					m_time -= 1;
					m_isDoingPong = true;
					m_value = m_to;
					m_to = m_from;
					m_from = m_value;
				}
				else if ((m_animFlags & eAnimFlag.LOOP) > 0)
				{
					if ((m_animFlags & eAnimFlag.PINGPONG) > 0 && m_isDoingPong)
					{
						m_time -= 1;
						m_isDoingPong = false;
						m_value = m_to;
						m_to = m_from;
						m_from = m_value;
					}
					else
					{
						m_time -= 1;
						m_value = m_to;
					}
				}
				else
				{
					Lerp(1);
					m_value = m_to;
					// Check if there is a queue anim
					if (m_animQueue.Count > 0)
					{
						SAnimQueue animQueue = m_animQueue.First();
						m_animQueue.RemoveAt(0);
						StartAnim(animQueue.from, animQueue.to, animQueue.duration, animQueue.delay, animQueue.animType, animQueue.animFlags);
						return true;
					}
					if (ignoreDoneAnim)
					{
						ignoreDoneAnim = false;
						return true;
					}
					else return false;
				}
			}

			float percent = 0;

			switch (m_animType)
			{
				case eAnimType.LINEAR:
					percent = m_time;
					break;
				case eAnimType.EASE_IN:
					percent = m_time * m_time;
					break;
				case eAnimType.EASE_OUT:
					percent = 1 - ((1 - m_time) * (1 - m_time));
					break;
				case eAnimType.EASE_BOTH:
					if (m_time >= .5f)
					{
						percent = 1 - ((1 - m_time) * (1 - m_time)) * 2;
					}
					else
					{
						percent = m_time * m_time * 2;
					}
					break;
			}

			Lerp(percent);

			return true;
		}

		protected virtual void Lerp(float percent) { }
	}

	class CAnimInt : IAnim<int>
	{
		public CAnimInt() : base(0) { }
		public CAnimInt(int value) : base(value) { }
		public CAnimInt(string in_category) : base(0) { Category = in_category; }
		public CAnimInt(string in_category, int value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = (int)((float)m_from + (float)(m_to - m_from) * percent);
		}
	}

	class CAnimFloat : IAnim<float>
	{
		public CAnimFloat() : base(0) { }
		public CAnimFloat(float value) : base(value) { }
		public CAnimFloat(string in_category) : base(0) { Category = in_category; }
		public CAnimFloat(string in_category, float value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * percent;
		}
	}

	class CAnimDouble : IAnim<double>
	{
		public CAnimDouble() : base(0) { }
		public CAnimDouble(double value) : base(value) { }
		public CAnimDouble(string in_category) : base(0) { Category = in_category; }
		public CAnimDouble(string in_category, double value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * (double)percent;
		}
	}

	class CAnimString : IAnim<string>
	{
		public CAnimString() : base("") { }
		public CAnimString(string value) : base(value) { }
		public CAnimString(string in_category, string value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_to.Substring(0, (int)((float)m_to.Length * percent));
		}
	}

	class CAnimVector2 : IAnim<Vector2>
	{
		public CAnimVector2() : base(Vector2.Zero) { }
		public CAnimVector2(Vector2 value) : base(value) { }
		public CAnimVector2(string in_category) : base(Vector2.Zero) { Category = in_category; }
		public CAnimVector2(string in_category, Vector2 value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * percent;
		}
	}

	class CAnimVector3 : IAnim<Vector3>
	{
		public CAnimVector3() : base(Vector3.Zero) { }
		public CAnimVector3(Vector3 value) : base(value) { }
		public CAnimVector3(string in_category) : base(Vector3.Zero) { Category = in_category; }
		public CAnimVector3(string in_category, Vector3 value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * percent;
		}
	}

	class CAnimVector4 : IAnim<Vector4>
	{
		public CAnimVector4() : base(Vector4.Zero) { }
		public CAnimVector4(Vector4 value) : base(value) { }
		public CAnimVector4(string in_category) : base(Vector4.Zero) { Category = in_category; }
		public CAnimVector4(string in_category, Vector4 value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * percent;
		}
	}

	class CAnimMatrix : IAnim<Matrix>
	{
		public CAnimMatrix() : base(Matrix.Identity) { }
		public CAnimMatrix(Matrix value) : base(value) { }
		public CAnimMatrix(string in_category) : base(Matrix.Identity) { Category = in_category; }
		public CAnimMatrix(string in_category, Matrix value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value = m_from + (m_to - m_from) * percent;
		}
	}

	class CAnimColor : IAnim<Color>
	{
		public CAnimColor() : base(Color.White) { }
		public CAnimColor(Color value) : base(value) { }
		public CAnimColor(string in_category) : base(Color.White) { Category = in_category; }
		public CAnimColor(string in_category, Color value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value.R = (byte)((float)m_from.R + ((float)m_to.R - (float)m_from.R) * percent);
			m_value.G = (byte)((float)m_from.G + ((float)m_to.G - (float)m_from.G) * percent);
			m_value.B = (byte)((float)m_from.B + ((float)m_to.B - (float)m_from.B) * percent);
			m_value.A = (byte)((float)m_from.A + ((float)m_to.A - (float)m_from.A) * percent);
		}
	}

	class CAnimRectangle : IAnim<Rectangle>
	{
		public CAnimRectangle() : base(Rectangle.Empty) { }
		public CAnimRectangle(Rectangle value) : base(value) { }
		public CAnimRectangle(string in_category) : base(Rectangle.Empty) { Category = in_category; }
		public CAnimRectangle(string in_category, Rectangle value) : base(value) { Category = in_category; }

		protected override void Lerp(float percent)
		{
			m_value.X = (int)((float)m_from.X + (float)(m_to.X - m_from.X) * percent);
			m_value.Y = (int)((float)m_from.Y + (float)(m_to.Y - m_from.Y) * percent);
			m_value.Width = (int)((float)m_from.Width + (float)(m_to.Width - m_from.Width) * percent);
			m_value.Height = (int)((float)m_from.Height + (float)(m_to.Height - m_from.Height) * percent);
		}
	}
}
