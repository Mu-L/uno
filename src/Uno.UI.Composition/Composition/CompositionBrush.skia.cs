﻿#nullable enable

using SkiaSharp;

namespace Microsoft.UI.Composition
{
	public partial class CompositionBrush
	{
		internal virtual void UpdatePaint(SKPaint paint, SKRect bounds)
		{
		}

		internal virtual bool CanPaint() => false;

		internal virtual bool RequiresRepaintOnEveryFrame => false;
	}
}
