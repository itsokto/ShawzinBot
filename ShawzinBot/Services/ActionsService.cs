using System;
using System.Linq;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;
using Melanchall.DryWetMidi.Core;
using ShawzinBot.WinApi;
using Timer = System.Threading.Timer;

namespace ShawzinBot.Services
{
    public class ActionsService
    {
        private static IntPtr _warframeWindow = IntPtr.Zero;

        private static readonly InputSimulator InputSimulator = new();

        private const int ScaleSize = 9;
        public static int ActiveScale { get; private set; }

        private static bool _vibratoActive = false;

        private static VirtualKeyCode _fretKey;
        private static VirtualKeyCode _vibratoKey;
        
        /// <summary>
        /// Play a MIDI note inside Warframe.
        /// </summary>
        /// <param name="note"> The note to be played.</param>
        /// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        public static bool PlayNote(NoteOnEvent note, bool enableVibrato, bool transposeNotes)
        {
            if (!User32.IsWindowFocused("Warframe")) return false;

            var noteId = (int) note.NoteNumber;
            if (!ShawzinService.Notes.ContainsKey(noteId))
            {
                if (transposeNotes)
                {
                    if (noteId < ShawzinService.Notes.Keys.First())
                    {
                        noteId = ShawzinService.Notes.Keys.First() + noteId % 12;
                    }
                    else if (noteId > ShawzinService.Notes.Keys.Last())
                    {
                        noteId = ShawzinService.Notes.Keys.Last() - 15 + noteId % 12;
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
            var shawzinNote = ShawzinService.Notes[noteId];
            SetScale(shawzinNote.Scale);
            var stringKey = ShawzinService.Strings[shawzinNote.String];

            InputSimulator.Keyboard.KeyUp(_fretKey);
            _fretKey = ShawzinService.Frets[shawzinNote.Fret];

            InputSimulator.Keyboard.KeyUp(_vibratoKey);
            _vibratoKey = ShawzinService.Specials[ShawzinSpecial.Vibrato];

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

            if (scaleIndex < ActiveScale)
            {
                scaleDifference = ScaleSize - (ActiveScale - scaleIndex);
            }
            else if (scaleIndex > ActiveScale)
            {
                scaleDifference = scaleIndex - ActiveScale;
            }

            for (var i = 0; i < scaleDifference; i++)
            {
                KeyTap(ShawzinService.Specials[ShawzinSpecial.Scale]);
            }

            ActiveScale = scaleIndex;
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
            _warframeWindow = User32.FindWindow("Warframe");
            User32.SetForegroundWindow(_warframeWindow);
            var hWnd = User32.GetForegroundWindow();
            return !_warframeWindow.Equals(IntPtr.Zero) && hWnd.Equals(_warframeWindow);
        }
    }
}