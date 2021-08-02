using System.Collections.Generic;
using WindowsInput.Native;

namespace ShawzinBot.Services
{
	public enum ShawzinFret
	{
		None,
		Sky,
		Earth,
		Water
	}

	public enum ShawzinString
	{
		S1,
		S2,
		S3
	}

	public enum ShawzinSpecial
	{
		Vibrato,
		Scale
	}

	public struct ShawzinNote
	{
		public int Scale { get; set; }

		public ShawzinFret Fret { get; set; }

		public ShawzinString String { get; set; }

		public bool Vibrato { get; set; }
	}
	
    public class ShawzinService
    {
        		// Dictionary of note IDs and a series of ints. In order: Scale, Fret, Key, Vibrato
		public static readonly Dictionary<int, ShawzinNote> Notes = new()
		{
			{
				48, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.None,
					String = ShawzinString.S1
				}
			}, // C3
			{
				49, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.None,
					String = ShawzinString.S2
				}
			}, // C#3
			{
				50, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.None,
					String = ShawzinString.S3
				}
			}, // D3
			{
				51, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Sky,
					String = ShawzinString.S1
				}
			}, // D#3
			{
				52, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Sky,
					String = ShawzinString.S2
				}
			}, // E3
			{
				53, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Sky,
					String = ShawzinString.S3
				}
			}, // F3
			{
				54, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S1
				}
			}, // F#3
			{
				55, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S2
				}
			}, // G3
			{
				56, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S3
				}
			}, // G#3
			{
				57, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S1
				}
			}, // A3
			{
				58, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S2
				}
			}, // A#3
			{
				59, new ShawzinNote
				{
					Scale = 0,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S3
				}
			}, // B3
			{
				60, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Sky,
					String = ShawzinString.S3
				}
			}, // C4
			{
				61, new ShawzinNote
				{
					Scale = 4,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S1
				}
			}, // C#4
			{
				62, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S1
				}
			}, // D4
			{
				63, new ShawzinNote
				{
					Scale = 1,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S2
				}
			}, // D#4
			{
				64, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S2
				}
			}, // E4
			{
				65, new ShawzinNote
				{
					Scale = 1,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S3
				}
			}, // F4
			{
				66, new ShawzinNote
				{
					Scale = 1,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S1
				}
			}, // F#4
			{
				67, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S3
				}
			}, // G4
			{
				68, new ShawzinNote
				{
					Scale = 6,
					Fret = ShawzinFret.Earth,
					String = ShawzinString.S3
				}
			}, // G#4
			{
				69, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S1
				}
			}, // A4
			{
				70, new ShawzinNote
				{
					Scale = 1,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S3
				}
			}, // A#4
			{
				71, new ShawzinNote
				{
					Scale = 4,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S2,
					Vibrato = true
				}
			}, // B4
			{
				72, new ShawzinNote
				{
					Scale = 4,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S2
				}
			}, // C5
			{
				73, new ShawzinNote
				{
					Scale = 4,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S3
				}
			}, // C#5
			{
				74, new ShawzinNote
				{
					Scale = 8,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S3
				}
			}, // D5
			{
				75, new ShawzinNote
				{
					Scale = 7,
					Fret = ShawzinFret.Water,
					String = ShawzinString.S3
				}
			}, // D#5
		};

		public static readonly Dictionary<ShawzinFret, VirtualKeyCode> Frets = new()
		{
			{ ShawzinFret.None, VirtualKeyCode.None }, // No Fret
			{ ShawzinFret.Sky, VirtualKeyCode.LEFT }, // Sky Fret
			{ ShawzinFret.Earth, VirtualKeyCode.DOWN }, // Earth Fret
			{ ShawzinFret.Water, VirtualKeyCode.RIGHT }, // Water Fret
		};

		public static readonly Dictionary<ShawzinString, VirtualKeyCode> Strings = new()
		{
			{ ShawzinString.S1, VirtualKeyCode.VK_1 }, // 1st String
			{ ShawzinString.S2, VirtualKeyCode.VK_2 }, // 2nd String
			{ ShawzinString.S3, VirtualKeyCode.VK_3 }, // 3rd String
		};


		public static readonly Dictionary<ShawzinSpecial, VirtualKeyCode> Specials = new()
		{
			{ ShawzinSpecial.Vibrato, VirtualKeyCode.SPACE }, // Vibrato
			{ ShawzinSpecial.Scale, VirtualKeyCode.TAB }, // Scale change
		};
    }
}