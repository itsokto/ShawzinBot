using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;
using Melanchall.DryWetMidi.Core;
using Timer = System.Threading.Timer;

namespace ShawzinBot
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

	public class ActionManager
	{
		private static IntPtr _warframeWindow = IntPtr.Zero;

		// Dictionary of note IDs and a series of ints. In order: Scale, Fret, Key, Vibrato
		private static readonly Dictionary<int, ShawzinNote> ShawzinNotes = new()
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

		private static readonly Dictionary<ShawzinFret, VirtualKeyCode> ShawzinFrets = new()
		{
			{ ShawzinFret.None, VirtualKeyCode.None }, // No Fret
			{ ShawzinFret.Sky, VirtualKeyCode.LEFT }, // Sky Fret
			{ ShawzinFret.Earth, VirtualKeyCode.DOWN }, // Earth Fret
			{ ShawzinFret.Water, VirtualKeyCode.RIGHT }, // Water Fret
		};

		private static readonly Dictionary<ShawzinString, VirtualKeyCode> ShawzinStrings = new()
		{
			{ ShawzinString.S1, VirtualKeyCode.VK_1 }, // 1st String
			{ ShawzinString.S2, VirtualKeyCode.VK_2 }, // 2nd String
			{ ShawzinString.S3, VirtualKeyCode.VK_3 }, // 3rd String
		};


		private static readonly Dictionary<ShawzinSpecial, VirtualKeyCode> ShawzinSpecials = new()
		{
			{ ShawzinSpecial.Vibrato, VirtualKeyCode.SPACE }, // Vibrato
			{ ShawzinSpecial.Scale, VirtualKeyCode.TAB }, // Scale change
		};
		
		private static readonly InputSimulator InputSimulator = new();

		private const int ScaleSize = 9;

		public static int _activeScale;

		private static bool _vibratoActive = false;

		private static VirtualKeyCode _fretKey;
		private static VirtualKeyCode _vibratoKey;

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(string className, string windowTitle);

		public static IntPtr FindWindow(string lpWindowName)
		{
			return FindWindow(null, lpWindowName);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		/// <summary>
		/// Play a MIDI note inside Warframe.
		/// </summary>
		/// <param name="note"> The note to be played.</param>
		/// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
		/// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
		public static bool PlayNote(NoteOnEvent note, bool enableVibrato, bool transposeNotes)
		{
			if (!IsWindowFocused("Warframe")) return false;

			var noteId = (int) note.NoteNumber;
			if (!ShawzinNotes.ContainsKey(noteId))
			{
				if (transposeNotes)
				{
					if (noteId < ShawzinNotes.Keys.First())
					{
						noteId = ShawzinNotes.Keys.First() + noteId % 12;
					}
					else if (noteId > ShawzinNotes.Keys.Last())
					{
						noteId = ShawzinNotes.Keys.Last() - 15 + noteId % 12;
					}
				}
				else
				{
					return false;
				}
			}

			PlayNote(noteId, enableVibrato, transposeNotes);
			return true;
		}

		/// <summary>
		/// Play a MIDI note inside Warframe.
		/// </summary>
		/// <param name="noteId"> The MIDI ID of the note to be played.</param>
		/// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
		/// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
		private static void PlayNote(int noteId, bool enableVibrato, bool transposeNotes)
		{
			var shawzinNote = ShawzinNotes[noteId];
			SetScale(shawzinNote.Scale);
			var stringKey = ShawzinStrings[shawzinNote.String];


			InputSimulator.Keyboard.KeyUp(_fretKey);
			_fretKey = ShawzinFrets[shawzinNote.Fret];

			InputSimulator.Keyboard.KeyUp(_vibratoKey);
			_vibratoKey = ShawzinSpecials[ShawzinSpecial.Vibrato];

			if (shawzinNote.Vibrato && enableVibrato)
			{
				KeyHold(_vibratoKey, TimeSpan.FromMilliseconds(100));
				InputSimulator.Keyboard.KeyDown(_vibratoKey);
			}

			InputSimulator.Keyboard.KeyDown(_fretKey);
			KeyTap(stringKey);
		}

		private static void SetScale(int scaleIndex)
		{
			var scaleDifference = 0;

			if (scaleIndex < _activeScale)
			{
				scaleDifference = ScaleSize - (_activeScale - scaleIndex);
			}
			else if (scaleIndex > _activeScale)
			{
				scaleDifference = scaleIndex - _activeScale;
			}

			for (var i = 0; i < scaleDifference; i++)
			{
				KeyTap(ShawzinSpecials[ShawzinSpecial.Scale]);
			}

			_activeScale = scaleIndex;
		}

		/// <summary>
		/// Tap a key.
		/// </summary>
		/// <param name="key"> The key to be tapped.</param>
		public static void KeyTap(VirtualKeyCode key)
		{
			InputSimulator.Keyboard.KeyPress(key);
		}

		/// <summary>
		/// Hold key for certain amount of time and release. (UNTESTED)
		/// </summary>
		/// <param name="key"> The key to be held.</param>
		/// <param name="time"> The amount of time the key should be held for.</param>
		private static void KeyHold(VirtualKeyCode key, TimeSpan time)
		{
			InputSimulator.Keyboard.KeyDown(key);
			var timer = new Timer(_ => InputSimulator.Keyboard.KeyUp(key), null, time, Timeout.InfiniteTimeSpan);
		}

		public static bool OnSongPlay()
		{
			_warframeWindow = FindWindow("Warframe");
			SetForegroundWindow(_warframeWindow);
			var hWnd = GetForegroundWindow();
			return !_warframeWindow.Equals(IntPtr.Zero) && hWnd.Equals(_warframeWindow);
		}

		private static bool IsWindowFocused(IntPtr windowPtr)
		{
			var hWnd = GetForegroundWindow();
			return !windowPtr.Equals(IntPtr.Zero) && hWnd.Equals(windowPtr);
		}

		public static bool IsWindowFocused(string windowName)
		{
			var windowPtr = FindWindow(windowName);
			return IsWindowFocused(windowPtr);
		}
	}
}