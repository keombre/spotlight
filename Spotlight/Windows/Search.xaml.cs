﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spotlight.Windows
{
	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_INVALID_STATE = 4
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AccentPolicy
	{
		public AccentState AccentState;
		public int AccentFlags;
		public int GradientColor;
		public int AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	internal enum WindowCompositionAttribute
	{
		WCA_ACCENT_POLICY = 19
	}

	public partial class Search : Window
    {

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		private readonly Parser parser = new Parser();
		private bool IsClosing = false;

		public Search()
        {
            InitializeComponent();
			Left = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
			Top = (SystemParameters.PrimaryScreenWidth / 7);
			Topmost = true;

			var image = Properties.Resources.search;
		}

		internal void EnableBlur()
		{
			var windowHelper = new WindowInteropHelper(this);

			var accent = new AccentPolicy();
			var accentStructSize = Marshal.SizeOf(accent);
			accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData
			{
				Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
				SizeOfData = accentStructSize,
				Data = accentPtr
			};

			SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			EnableBlur();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			query.Foreground = Brushes.Black;

			if (e.Key == Key.Enter)
				Validate(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
			else if (e.Key == Key.Escape)
				Close();
		}

		private void Validate(bool asAdmin)
		{
			if (parser.Invoke(new Response { type = Response.Type.Command, name = query.Text }, asAdmin))
				Close();
			else
			{
				query.Select(0, query.Text.Length);
				query.Foreground = Brushes.Red;
			}
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			if (query.Text == "" && !IsClosing)
				Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			IsClosing = true;
		}
	}
}
