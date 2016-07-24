using System;
using System.Windows.Forms;

namespace DraftAnalyzer
{
	public class DraftPoolListView : ListView
	{
		public class KeyDataEventArgs : EventArgs
		{
			public KeyDataEventArgs(Keys keyData)
			{
				mKeyData = keyData;
			}
			private Keys mKeyData;
			public Keys KeyData
			{
				get { return mKeyData; }
			}
		}

		public void MakeDoubleBuffered()
		{
			DoubleBuffered = true;
		}

		public event EventHandler<KeyDataEventArgs> RaiseKeyDataEvent;

		// Wrap event invocations inside a protected virtual method
		// to allow derived classes to override the event invocation behavior
		protected virtual void OnRaiseKeyDataEvent(KeyDataEventArgs e)
		{
			// Make a temporary copy of the event to avoid possibility of
			// a race condition if the last subscriber unsubscribes
			// immediately after the null check and before the event is raised.
			EventHandler<KeyDataEventArgs> handler = RaiseKeyDataEvent;

			// Event will be null if there are no subscribers
			if (handler != null)
			{
				// Use the () operator to raise the event.
				handler(this, e);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (   (keyData >= Keys.A && keyData <= Keys.Z)
				|| (keyData >= Keys.D0 && keyData <= Keys.D9)
				|| (keyData >= Keys.NumPad0 && keyData <= Keys.NumPad1)
				|| keyData == Keys.Oemplus
				|| keyData == Keys.OemMinus
				|| keyData == Keys.Add
				|| keyData == Keys.Subtract
				)
			{
				OnRaiseKeyDataEvent(new KeyDataEventArgs(keyData));
				return true;   // Don't pass to ListView control
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
