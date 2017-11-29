using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcrastinatingSquirrel
{
	class RandAndNoise
	{
		int m_seed;
		public int Seed { get { return m_seed; } }

		public RandAndNoise(int seed)
		{
			m_seed = seed;
			random = new Random(Seed);
			random.NextBytes(values);

			// Copy the source data twice to the generator array
			for (int i = 0; i < 256; i++)
			{
				values[i + 256] = values[i];
			}
		}

		/// <summary>
		/// Interpolate linearly between A and B.
		/// </summary>
		/// <param name="t">The amount of the interpolation</param>
		/// <param name="a">The starting value</param>
		/// <param name="b">The ending value</param>
		/// <returns>The interpolated value between A and B</returns>
		private static double Lerp(double t, double a, double b)
		{
			return a + t * (b - a);
		}

		/// <summary>
		/// Smooth the entry value
		/// </summary>
		/// <param name="t">The entry value</param>
		/// <returns>The smoothed value</returns>
		private static double Fade(double t)
		{
			return t * t * t * (t * (t * 6 - 15) + 10);
		}

		/// <summary>
		/// Modifies the result by adding a directional bias
		/// </summary>
		/// <param name="hash">The random value telling in which direction the bias will occur</param>
		/// <param name="x">The amount of the bias on the X axis</param>
		/// <returns>The directional bias strength</returns>
		private static double Grad(int hash, double x)
		{
			// Result table
			// ---+------+----
			//  0 | 0000 |  x 
			//  1 | 0001 | -x 

			return (hash & 1) == 0 ? x : -x;
		}

		/// <summary>
		/// Modifies the result by adding a directional bias
		/// </summary>
		/// <param name="hash">The random value telling in which direction the bias will occur</param>
		/// <param name="x">The amount of the bias on the X axis</param>
		/// <param name="y">The amount of the bias on the Y axis</param>
		/// <returns>The directional bias strength</returns>
		private static double Grad(int hash, double x, double y)
		{
			// Fetch the last 3 bits
			int h = hash & 3;

			// Result table for U
			// ---+------+---+------
			//  0 | 0000 | x |  x
			//  1 | 0001 | x |  x
			//  2 | 0010 | x | -x
			//  3 | 0011 | x | -x

			double u = (h & 2) == 0 ? x : -x;

			// Result table for V
			// ---+------+---+------
			//  0 | 0000 | y |  y
			//  1 | 0001 | y | -y
			//  2 | 0010 | y |  y
			//  3 | 0011 | y | -y

			double v = (h & 1) == 0 ? y : -y;

			// Result table for U + V
			// ---+------+----+----+--
			//  0 | 0000 |  x |  y |  x + y
			//  1 | 0001 |  x | -y |  x - y
			//  2 | 0010 | -x |  y | -x + y
			//  3 | 0011 | -x | -y | -x - y

			return u + v;
		}

		/// <summary>
		/// Modifies the result by adding a directional bias
		/// </summary>
		/// <param name="hash">The random value telling in which direction the bias will occur</param>
		/// <param name="x">The amount of the bias on the X axis</param>
		/// <param name="y">The amount of the bias on the Y axis</param>
		/// <param name="z">The amount of the bias on the Z axis</param>
		/// <returns>The directional bias strength</returns>		
		private static double Grad(int hash, double x, double y, double z)
		{
			// Fetch the last 4 bits
			int h = hash & 15;

			// Result table for U
			// ---+------+---+------
			//  0 | 0000 | x |  x
			//  1 | 0001 | x | -x
			//  2 | 0010 | x |  x
			//  3 | 0011 | x | -x
			//  4 | 0100 | x |  x
			//  5 | 0101 | x | -x
			//  6 | 0110 | x |  x
			//  7 | 0111 | x | -x
			//  8 | 1000 | y |  y
			//  9 | 1001 | y | -y
			// 10 | 1010 | y |  y
			// 11 | 1011 | y | -y
			// 12 | 1100 | y |  y
			// 13 | 1101 | y | -y
			// 14 | 1110 | y |  y
			// 15 | 1111 | y | -y

			double u = h < 8 ? x : y;

			// Result table for V
			// ---+------+---+------
			//  0 | 0000 | y |  y
			//  1 | 0001 | y |  y
			//  2 | 0010 | y | -y
			//  3 | 0011 | y | -y
			//  4 | 0100 | z |  z
			//  5 | 0101 | z |  z
			//  6 | 0110 | z | -z
			//  7 | 0111 | z | -z
			//  8 | 1000 | z |  z
			//  9 | 1001 | z |  z
			// 10 | 1010 | z | -z
			// 11 | 1011 | z | -z
			// 12 | 1100 | x |  x
			// 13 | 1101 | z |  z
			// 14 | 1110 | x | -x
			// 15 | 1111 | z | -z

			double v = h < 4 ? y : h == 12 || h == 14 ? x : z;

			// Result table for U+V
			// ---+------+----+----+-------
			//  0 | 0000 |  x |  y |  x + y
			//  1 | 0001 | -x |  y | -x + y
			//  2 | 0010 |  x | -y |  x - y
			//  3 | 0011 | -x | -y | -x - y
			//  4 | 0100 |  x |  z |  x + z
			//  5 | 0101 | -x |  z | -x + z
			//  6 | 0110 |  x | -z |  x - z
			//  7 | 0111 | -x | -z | -x - z
			//  8 | 1000 |  y |  z |  y + z
			//  9 | 1001 | -y |  z | -y + z 
			// 10 | 1010 |  y | -z |  y - z
			// 11 | 1011 | -y | -z | -y - z

			// The four last results already exists in the table before
			// They are doubled because you must get a result for all
			// 4-bit combinaisons values.

			// 12 | 1100 |  y |  x |  y + x
			// 13 | 1101 | -y |  z | -y + z
			// 14 | 1110 |  y | -x |  y - x
			// 15 | 1111 | -y | -z | -y - z

			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
		}

		public double Noise(double x)
		{
			// Compute the cell coordinates
			int X = (int)Math.Floor(x) & 255;

			// Retrieve the decimal part of the cell
			x -= (double)Math.Floor(x);

			double u = Fade(x);

			return Lerp(u, Grad(values[X], x), Grad(values[X + 1], x - 1)) * 2;
		}

		/// <summary>
		/// Generates a bi-dimensional noise
		/// </summary>
		/// <param name="x">The X entry value for the noise</param>
		/// <param name="y">The Y entry value for the noise</param>
		/// <returns>A value between [-1;1] representing the noise amount</returns>
		public double Noise(double x, double y)
		{
			// Compute the cell coordinates
			int X = (int)Math.Floor(x) & 255;
			int Y = (int)Math.Floor(y) & 255;

			// Retrieve the decimal part of the cell
			x -= (double)Math.Floor(x);
			y -= (double)Math.Floor(y);

			// Smooth the curve
			double u = Fade(x);
			double v = Fade(y);

			// Fetch some randoms values from the table
			int A = values[X] + Y;
			int B = values[X + 1] + Y;

			// Interpolate between directions 
			return Lerp(v, Lerp(u, Grad(values[A], x, y),
						 Grad(values[B], x - 1, y)),
					 Lerp(u, Grad(values[A + 1], x, y - 1),
						 Grad(values[B + 1], x - 1, y - 1))) * 2;
		}

		/// <summary>
		/// Generates a tri-dimensional noise
		/// </summary>
		/// <param name="x">The X entry value for the noise</param>
		/// <param name="y">The Y entry value for the noise</param>
		/// <param name="z">The Z entry value for the noise</param>
		/// <returns>A value between [-1;1] representing the noise amount</returns>
		public double Noise(double x, double y, double z)
		{
			// Compute the cell coordinates
			int X = (int)Math.Floor(x) & 255;
			int Y = (int)Math.Floor(y) & 255;
			int Z = (int)Math.Floor(z) & 255;

			// Retrieve the decimal part of the cell
			x -= (double)Math.Floor(x);
			y -= (double)Math.Floor(y);
			z -= (double)Math.Floor(z);

			// Smooth the curve
			double u = Fade(x);
			double v = Fade(y);
			double w = Fade(z);

			// Fetch some randoms values from the table
			int A = values[X] + Y;
			int AA = values[A] + Z;
			int AB = values[A + 1] + Z;
			int B = values[X + 1] + Y;
			int BA = values[B] + Z;
			int BB = values[B + 1] + Z;

			// Interpolate between directions
			return Lerp(w, Lerp(v, Lerp(u, Grad(values[AA], x, y, z),
							 Grad(values[BA], x - 1, y, z)),
						 Lerp(u, Grad(values[AB], x, y - 1, z),
							 Grad(values[BB], x - 1, y - 1, z))),
					 Lerp(v, Lerp(u, Grad(values[AA + 1], x, y, z - 1),
							 Grad(values[BA + 1], x - 1, y, z - 1)),
						 Lerp(u, Grad(values[AB + 1], x, y - 1, z - 1),
							 Grad(values[BB + 1], x - 1, y - 1, z - 1)))) * 2;
		}

		public double UNoise(double x)
		{
			return (Noise(x) + 1) * .5;
		}

		public double UNoise(double x, double y)
		{
			return (Noise(x, y) + 1) * .5;
		}

		public double UNoise(double x, double y, double z)
		{
			return (Noise(x, y, z) + 1) * .5;
		}

		public Random random;
		static byte[] values = new byte[512];
	}
}
