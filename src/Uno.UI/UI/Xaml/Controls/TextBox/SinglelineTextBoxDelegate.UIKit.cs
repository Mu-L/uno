﻿#nullable enable
using Foundation;
using Uno.Extensions;
using System;
using System.Linq;
using UIKit;
using Windows.UI.Core;
using System.Threading.Tasks;
using Uno.Foundation.Logging;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class SinglelineTextBoxDelegate : UITextFieldDelegate
	{
		private readonly WeakReference<TextBox> _textBox;

		public SinglelineTextBoxDelegate(WeakReference<TextBox> textbox)
		{
			_textBox = textbox;
		}

		public bool IsKeyboardHiddenOnEnter
		{
			get;
			set;
		}

		public override bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
		{
			if (textField is SinglelineTextBoxView textBoxView)
			{
				if (_textBox.GetTarget() is not TextBox textBox)
				{
					return false;
				}

				// Both IsReadOnly = true and IsTabStop = false can prevent editing
				if (textBox.IsReadOnly || !textBox.IsTabStop)
				{
					return false;
				}

				if (textBox.OnKey(replacementString.FirstOrDefault()))
				{
					return false;
				}

				if (textBox.MaxLength > 0)
				{
					// When replacing text from pasting (multiple characters at once)
					// we should only allow it (return true) when the new text length
					// is lower or equal to the allowed length (TextBox.MaxLength)
					var newLength = textBoxView.Text.Length + replacementString.Length - range.Length;
					return newLength <= textBox.MaxLength;
				}
			}

			return true;
		}

		public override bool ShouldReturn(UITextField textField)
		{
			try
			{
				if (IsKeyboardHiddenOnEnter)
				{
					_ = CoreDispatcher.Main.RunAsync(CoreDispatcherPriority.Normal,
						async () =>
						{
							// Delay losing focus to avoid concurrent interactions when transferring focus to another control. See 101152
							await Task.Delay(TimeSpan.FromMilliseconds(50));
							textField.ResignFirstResponder();
						});
				}

				if (textField is SinglelineTextBoxView textBox)
				{
					if (_textBox.GetTarget()?.OnKey('\n') ?? false)
					{
						return false;
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				if (this.Log().IsEnabled(LogLevel.Error))
				{
					this.Log().LogError("[ShouldReturn] Exception caught while executing method", ex);
				}

				return false;
			}
		}

		/// <summary>
		/// Corresponds to a gain of focus
		/// </summary>
		public override void EditingStarted(UITextField textField)
		{
			if (_textBox.GetTarget() is TextBox textBox && textBox.FocusState == FocusState.Unfocused)
			{
				textBox.Focus(FocusState.Pointer);
			}
		}

		/// <summary>
		/// Corresponds to a loss of focus
		/// </summary>
		public override void EditingEnded(UITextField textField)
		{
			if (_textBox.GetTarget() is TextBox { FocusState: not FocusState.Unfocused, IsKeepingFocusOnEndEditing: false } textBox)
			{
				textBox.Unfocus();
			}
		}
	}
}
