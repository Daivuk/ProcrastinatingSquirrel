using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DK8;

namespace ProcrastinatingSquirrel
{
	class Globals
	{
		public static Color TextColor = new Color(233, 198, 150);
		public static Color IconColor = new Color(255, 148, 0);
		public static CAnimFloat[] WindAnims = new CAnimFloat[]{
			new CAnimFloat("game", 0),
			new CAnimFloat("game", 0),
			new CAnimFloat("game", 0),
			new CAnimFloat("game", 0)};
		public static RandAndNoise Random = null;
		public static float HDScale = 1;
		public static int NUT_GOAL = 300000;
		public static int TotalNutCollected = 0;
	}
}
