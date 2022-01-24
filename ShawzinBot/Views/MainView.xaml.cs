﻿using System.Windows;
using System.Windows.Input;
using Melanchall.DryWetMidi.Multimedia;

namespace ShawzinBot.Views;

/// <summary>
/// Lógica de interacción para MainView.xaml
/// </summary>
public partial class MainView : Window
{
	public MainView()
	{
		InitializeComponent();
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);

		// Begin dragging the window
		DragMove();
	}

	public void OnCloseProgram(object sender, RoutedEventArgs e)
	{
		PlaybackCurrentTimeWatcher.Instance.Dispose();
		Close();
	}

	public void OnMinimizeProgram(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}
}