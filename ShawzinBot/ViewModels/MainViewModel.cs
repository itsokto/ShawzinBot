﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Timers;
using Caliburn.Micro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.Win32;
using ShawzinBot.Models;
using ShawzinBot.Services;
using ShawzinBot.WinApi;

namespace ShawzinBot.ViewModels;

public class MainViewModel : Screen
{
	#region Private Variables

	private string _songName;
	private double _songSlider;
	private double _maximumTime = 1;
	private TimeSpan _currentTime = TimeSpan.Zero;
	private TimeSpan _totalTime = TimeSpan.Zero;
	private string _playPauseIcon = "Play";
	private ShawzinScale _scale = ShawzinScale.PentatonicMinor;

	private BindableCollection<MidiInputModel> _midiInputs = new();
	private BindableCollection<MidiTrackModel> _midiTracks = new();
	private BindableCollection<MidiSpeedModel> _midiSpeeds = new();
	private MidiInputModel _selectedMidiInput;
	private MidiSpeedModel _selectedMidiSpeed;

	private bool _enableVibrato = true;
	private bool _transposeNotes = true;
	private bool _playThroughSpeakers;
	private bool _ignoreSliderChange;

	private Timer _playTimer;
	private ITimeSpan _playTime = new MidiTimeSpan();

	private readonly Version _programVersion = Assembly.GetExecutingAssembly().GetName().Version;
	private string _versionString = "";

	private MidiFile _midiFile;
	private Playback _playback;
	private InputDevice _inputDevice;
	private readonly OutputDevice _outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");

	private static bool _reloadPlayback;

	#endregion

	#region Constructor

	public MainViewModel()
	{
		VersionString = _programVersion.ToString();

		try
		{
			using var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");
			var p = httpClient
				.GetFromJsonAsync<GitVersion>("https://api.github.com/repos/ianespana/ShawzinBot/releases/latest")
				.ConfigureAwait(false).GetAwaiter().GetResult();
			if (!(p.Draft || p.Prerelease) && p.TagName != _programVersion.ToString())
				VersionString = _programVersion + " - Update available!";
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}

		RefreshDevices();

		MidiSpeeds.Add(new MidiSpeedModel("0.25", 0.25));
		MidiSpeeds.Add(new MidiSpeedModel("0.5", 0.5));
		MidiSpeeds.Add(new MidiSpeedModel("0.75", 0.75));
		MidiSpeeds.Add(new MidiSpeedModel("Normal", 1));
		MidiSpeeds.Add(new MidiSpeedModel("1.25", 1.25));
		MidiSpeeds.Add(new MidiSpeedModel("1.5", 1.5));
		MidiSpeeds.Add(new MidiSpeedModel("1.75", 1.75));
		MidiSpeeds.Add(new MidiSpeedModel("2", 2));

		SelectedMidiSpeed = MidiSpeeds[3];

		EnableVibrato = Properties.Settings.Default.EnableVibrato;
		TransposeNotes = Properties.Settings.Default.TransposeNotes;
		PlayThroughSpeakers = Properties.Settings.Default.PlayThroughSpeakers;
		PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromSeconds(1);
	}

	#endregion

	#region Properties

	public string VersionString
	{
		get => _versionString;
		set
		{
			_versionString = "v" + value;
			NotifyOfPropertyChange(() => VersionString);
		}
	}

	public string SongName
	{
		get => _songName;
		set
		{
			_songName = value;
			NotifyOfPropertyChange(() => SongName);
		}
	}

	public TimeSpan CurrentTime
	{
		get => _currentTime;
		set
		{
			_currentTime = value;
			NotifyOfPropertyChange(() => CurrentTime);
		}
	}

	public TimeSpan TotalTime
	{
		get => _totalTime;
		set
		{
			_totalTime = value;
			NotifyOfPropertyChange(() => TotalTime);
		}
	}

	public double SongSlider
	{
		get => _songSlider;
		set
		{
			_songSlider = value;
			NotifyOfPropertyChange(() => SongSlider);
			if (!_ignoreSliderChange && _playback != null)
			{
				if (_playback.IsRunning)
				{
					_playback.Stop();
					PlayPauseIcon = "Play";
				}

				var time = TimeSpan.FromSeconds(_songSlider);

				CurrentTime = time;
				_playback.MoveToTime((MetricTimeSpan) time);
			}

			_ignoreSliderChange = false;
		}
	}

	public double MaximumTime
	{
		get => _maximumTime;
		set
		{
			_maximumTime = value;
			NotifyOfPropertyChange(() => MaximumTime);
		}
	}

	public string PlayPauseIcon
	{
		get => _playPauseIcon;
		set
		{
			_playPauseIcon = value;
			NotifyOfPropertyChange(() => PlayPauseIcon);
		}
	}

	public BindableCollection<MidiInputModel> MidiInputs
	{
		get => _midiInputs;
		set
		{
			_midiInputs = value;
			NotifyOfPropertyChange(() => MidiInputs);
		}
	}

	public MidiInputModel SelectedMidiInput
	{
		get => _selectedMidiInput;
		set
		{
			_selectedMidiInput = value;
			_inputDevice?.Dispose();

			if (value?.DeviceName != null && value.DeviceName != "None")
			{
				_inputDevice = InputDevice.GetByName(value.DeviceName);
				_inputDevice.EventReceived += OnNoteEvent;
				_inputDevice.StartEventsListening();
				ActionsService.OnSongPlay();
			}

			NotifyOfPropertyChange(() => SelectedMidiInput);
		}
	}

	public BindableCollection<MidiSpeedModel> MidiSpeeds
	{
		get => _midiSpeeds;
		set
		{
			_midiSpeeds = value;
			NotifyOfPropertyChange(() => MidiSpeeds);
		}
	}

	public MidiSpeedModel SelectedMidiSpeed
	{
		get => _selectedMidiSpeed;
		set
		{
			_selectedMidiSpeed = value;
			NotifyOfPropertyChange(() => SelectedMidiSpeed);

			if (value?.Speed != null && _playback != null) _playback.Speed = value.Speed;
		}
	}

	public BindableCollection<MidiTrackModel> MidiTracks
	{
		get => _midiTracks;
		set
		{
			_midiTracks = value;
			NotifyOfPropertyChange(() => MidiTracks);
		}
	}

	public bool EnableVibrato
	{
		get => _enableVibrato;
		set
		{
			_enableVibrato = value;
			Properties.Settings.Default.EnableVibrato = value;
			Properties.Settings.Default.Save();
			NotifyOfPropertyChange(() => EnableVibrato);
		}
	}

	public bool TransposeNotes
	{
		get => _transposeNotes;
		set
		{
			_transposeNotes = value;
			Properties.Settings.Default.TransposeNotes = value;
			Properties.Settings.Default.Save();
			NotifyOfPropertyChange(() => TransposeNotes);
		}
	}

	public bool PlayThroughSpeakers
	{
		get => _playThroughSpeakers;
		set
		{
			_playThroughSpeakers = value;
			Properties.Settings.Default.PlayThroughSpeakers = value;
			Properties.Settings.Default.Save();
			NotifyOfPropertyChange(() => PlayThroughSpeakers);
		}
	}

	public ShawzinScale Scale
	{
		get => _scale;
		set
		{
			_scale = value;
			NotifyOfPropertyChange(() => Scale);
		}
	}

	#endregion

	#region Methods

	public void OpenFile()
	{
		var openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "MIDI file|*.mid;*.midi"; // Filter to only midi files
		if (openFileDialog.ShowDialog() != true) return;

		CloseFile();
		MidiTracks.Clear();

		_midiFile = MidiFile.Read(openFileDialog.FileName);
		SongName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

		TimeSpan midiFileDuration = _midiFile.GetDuration<MetricTimeSpan>();
		TotalTime = midiFileDuration;
		MaximumTime = midiFileDuration.TotalSeconds;
		UpdateSlider(0);
		CurrentTime = TimeSpan.Zero;

		foreach (var track in _midiFile.GetTrackChunks()) MidiTracks.Add(new MidiTrackModel(track, true));
	}

	public void CloseFile()
	{
		if (_playback != null)
		{
			_playback.Stop();
			PlaybackCurrentTimeWatcher.Instance.RemovePlayback(_playback);
			_playback.Dispose();
			_playback = null;
		}

		_midiFile = null;
		MidiTracks.Clear();

		PlayPauseIcon = "Play";
		SongName = "";
		TotalTime = TimeSpan.Zero;
		CurrentTime = TimeSpan.Zero;
		MaximumTime = 1;
	}

	public void PlayPause()
	{
		if (_midiFile == null || MaximumTime == 0d) return;
		if (_playback == null || _reloadPlayback)
		{
			if (_playback != null)
			{
				_playback.Stop();
				_playTime = _playback.GetCurrentTime(TimeSpanType.Midi);
				_playback.Dispose();
				_playback = null;
				PlayPauseIcon = "Play";
			}

			_midiFile.Chunks.Clear();

			foreach (var trackModel in MidiTracks)
				if (trackModel.IsChecked)
					_midiFile.Chunks.Add(trackModel.Track);

			_playback = _midiFile.GetPlayback();
			_playback.Speed = SelectedMidiSpeed.Speed;
			if (PlayThroughSpeakers) _playback.OutputDevice = _outputDevice;

			_playback.MoveToTime(_playTime);
			_playback.Finished += (s, e) => { CloseFile(); };

			PlaybackCurrentTimeWatcher.Instance.AddPlayback(_playback, TimeSpanType.Metric);
			PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnTick;
			PlaybackCurrentTimeWatcher.Instance.Start();

			_playback.EventPlayed += OnNoteEvent;
			_reloadPlayback = false;
		}

		if (_playback.IsRunning)
		{
			PlayPauseIcon = "Play";
			_playback.Stop();
		}
		else if (PlayPauseIcon == "Pause")
		{
			PlayPauseIcon = "Play";
			_playTimer.Dispose();
		}
		else
		{
			PlayPauseIcon = "Pause";

			ActionsService.OnSongPlay();
			_playTimer = new Timer();
			_playTimer.Interval = 100;
			_playTimer.Elapsed += PlayTimerElapsed;
			_playTimer.Start();
		}
	}

	private void PlayTimerElapsed(object sender, ElapsedEventArgs e)
	{
		if (User32.IsWindowFocused("Warframe") || PlayThroughSpeakers)
		{
			_playback.Start();
			_playTimer.Dispose();
		}
	}

	public void Previous()
	{
		if (_playback != null)
		{
			_playback.MoveToStart();
			UpdateSlider(0);
			CurrentTime = TimeSpan.Zero;
		}
	}

	public void Next()
	{
		CloseFile();
	}

	public void RefreshDevices()
	{
		MidiInputs.Clear();
		MidiInputs.Add(new MidiInputModel("None"));

		foreach (var device in InputDevice.GetAll()) MidiInputs.Add(new MidiInputModel(device.Name));

		SelectedMidiInput = MidiInputs[0];
	}

	#endregion

	#region EventHandlers

	public void OnTick(object sender, PlaybackCurrentTimeChangedEventArgs e)
	{
		foreach (var playbackTime in e.Times)
		{
			TimeSpan time = (MetricTimeSpan) playbackTime.Time;

			UpdateSlider(time.TotalSeconds);
			CurrentTime = time;
		}
	}

	public void OnNoteEvent(object sender, MidiEventPlayedEventArgs e)
	{
		switch (e.Event.EventType)
		{
			//case MidiEventType.SetTempo:
			//	var tempo = e.Event as SetTempoEvent;
			//	_playback.Speed = tempo.MicrosecondsPerQuarterNote;
			//	return;
			case MidiEventType.NoteOn:
				var note = e.Event as NoteOnEvent;
				if (note != null && note.Velocity <= 0) return;

				//Check if the user has tabbed out of warframe, and stop playback to avoid Scale issues
				if (!(ActionsService.PlayNote(note, EnableVibrato, TransposeNotes) || PlayThroughSpeakers))
					PlayPause();
				Scale = ActionsService.ActiveScale;
				return;
			default:
				return;
		}
	}

	public void OnNoteEvent(object sender, MidiEventReceivedEventArgs e)
	{
		if (e.Event.EventType != MidiEventType.NoteOn) return;

		var note = e.Event as NoteOnEvent;
		if (note != null && note.Velocity <= 0) return;

		ActionsService.PlayNote(note, EnableVibrato, TransposeNotes);
	}

	private void UpdateSlider(double value)
	{
		_ignoreSliderChange = true;
		SongSlider = value;
	}

	#endregion
}