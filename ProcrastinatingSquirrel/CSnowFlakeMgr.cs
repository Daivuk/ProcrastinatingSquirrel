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
	class CSnowFlakes
	{
		public Vector2 Position = Vector2.Zero;
		public float Angle = (float)CFrameData.Instance.Random.NextDouble() * MathHelper.TwoPi;
		public float percent = 0;
		public Texture2D texture;
	}


	enum eWEATHER
	{ 
		WEATHER_CLAIR,
		WEATHER_SNOW,
		WEATHER_SNOW_STORM
	}


	class CSnowFlakeMgr
	{
		Texture2D m_texSnowFlake;
		Texture2D m_texSnowSmoke;
		CSnowFlakes[] m_snowFlakes = new CSnowFlakes[500];
		Vector2 m_origin = new Vector2(256, 256);
		Vector2 m_tempV2 = Vector2.Zero;
		Color m_tempCol = Color.White;
		struct WeatherProperties
		{
			public float windSpeed;
			public float opacity;
			public int smokePercent;
			public float visibility;

			static public void Lerp(ref WeatherProperties out_w, ref WeatherProperties in_from, ref WeatherProperties in_to, float in_percent)
			{
				out_w.windSpeed = MathHelper.Lerp(in_from.windSpeed, in_to.windSpeed, in_percent);
				out_w.opacity = MathHelper.Lerp(in_from.opacity, in_to.opacity, in_percent);
				out_w.smokePercent = (int)MathHelper.Lerp((float)in_from.smokePercent, (float)in_to.smokePercent, in_percent);
				out_w.visibility = MathHelper.Lerp(in_from.visibility, in_to.visibility, in_percent);
			}
		}
		static WeatherProperties m_clair = new WeatherProperties
		{
			windSpeed = 16,
			opacity = .15f,
			smokePercent = 0,
			visibility = 1
		};
		static WeatherProperties m_heavy = new WeatherProperties
		{
			windSpeed = 1024,
			opacity = .75f,
			smokePercent = 60,
			visibility = .5f
		};
		WeatherProperties m_current = m_clair;


		public CSnowFlakeMgr()
		{
			m_texSnowFlake = CFrameData.Instance.Content.Load<Texture2D>("textures\\snowFlakes");
			m_texSnowSmoke = CFrameData.Instance.Content.Load<Texture2D>("textures\\snowSmoke");

			// Start the anims
			for (int i = 0; i < m_snowFlakes.Count(); ++i)
			{
				m_snowFlakes[i] = new CSnowFlakes();
			}
			foreach (CSnowFlakes snowFlakes in m_snowFlakes)
			{
				snowFlakes.texture = (CFrameData.Instance.Random.Next(100) < m_current.smokePercent) ? m_texSnowSmoke : m_texSnowFlake;
				snowFlakes.percent = (float)CFrameData.Instance.Random.NextDouble();
				snowFlakes.Position = new Vector2(
					(float)CFrameData.Instance.Random.NextDouble() * (1920 + 2048) - 1024,
					(float)CFrameData.Instance.Random.NextDouble() * 1080 * 3 - 1080);
			}
		}


		public void Move(Vector2 delta)
		{
			foreach (CSnowFlakes snowFlakes in m_snowFlakes)
			{
				snowFlakes.Position += delta;
			}
		}


		Vector2 m_direction = new Vector2(-1, .25f);
		public void Update()
		{
			m_direction.Normalize();
			float dt = CFrameData.Instance.GetDeltaSecond();

			if (!CSnowfield.Instance.IsSquirrelHome)
			{
				float percent = Vector2.Distance(CSnowfield.Instance.Squirrel.Position, CSnowfield.Instance.HomePos) / 450;
				WeatherProperties.Lerp(ref m_current, ref m_clair, ref m_heavy, percent);
			}

			foreach (CSnowFlakes snowFlakes in m_snowFlakes)
			{
				snowFlakes.Position += m_direction * m_current.windSpeed * dt;
				snowFlakes.percent -= dt * .5f;
				snowFlakes.Angle += dt * .1f;
				if (snowFlakes.percent <= 0)
				{
					snowFlakes.texture = (CFrameData.Instance.Random.Next(100) < m_current.smokePercent) ? m_texSnowSmoke : m_texSnowFlake;
					snowFlakes.percent = 1;
					m_tempV2.X = (float)CFrameData.Instance.Random.NextDouble() * (1920 + 2048) - 1024;
					m_tempV2.Y = (float)CFrameData.Instance.Random.NextDouble() * 1080 * 3 - 1080;
					snowFlakes.Position = m_tempV2;
				}
			}
		}


		public void Render()
		{
			if (CSnowfield.Instance.IsSquirrelHome) return;

			CFrameData fd = CFrameData.Instance;
			SpriteBatch sb = fd.SpriteBatch;

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			m_tempCol.R = (byte)(0.5882353f * 255.0f);
			m_tempCol.G = (byte)(0.7607843f * 255.0f);
			m_tempCol.B = (byte)(0.9137255f * 255.0f);
			m_tempCol.A = (byte)((1 - m_current.visibility) * 255.0f);
			sb.Draw(fd.CommonResources.Tex_White,
				fd.Graphics.GraphicsDevice.Viewport.Bounds,
				m_tempCol);
			sb.End();

			float alpha;
			sb.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied);
			m_tempCol.R = 255;
			m_tempCol.G = 255;
			m_tempCol.B = 255;
			foreach (CSnowFlakes snowFlakes in m_snowFlakes)
			{
				if (snowFlakes.percent < .5f) alpha = snowFlakes.percent * 2;
				else alpha = (1 - snowFlakes.percent) * 2;
				m_tempCol.A = (byte)((alpha * m_current.opacity) * 255.0f);
				sb.Draw(snowFlakes.texture, snowFlakes.Position, null, m_tempCol,
					snowFlakes.Angle, m_origin,
					1, 
					SpriteEffects.None, 0);
			}
			sb.End();
		}

		internal void Dispose()
		{
			m_snowFlakes = null;
		}
	}
}
