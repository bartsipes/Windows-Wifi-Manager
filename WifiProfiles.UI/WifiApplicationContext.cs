using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ServiceModel;
using System.Windows;
using System.ComponentModel;
using WifiProfiles;
using NetSh;

namespace WifiProfiles.UI
{
    /// <summary>
    /// Framework for running application as a tray app.
    /// </summary>
    /// <remarks>
    /// Tray app code adapted from "Creating Applications with NotifyIcon in Windows Forms", Jessica Fosler,
    /// http://windowsclient.net/articles/notifyiconapplications.aspx
    /// </remarks>
    public class WifiApplicationContext : ApplicationContext
    {
        private static readonly string IconFileName = "WifiProfiles.UI.logo.ico";
        private static readonly string DefaultTooltip = "Wifi Profiles";
        private IContainer components;	// a list of components to dispose when the context is disposed
        private NotifyIcon notifyIcon;	// the icon that sits in the system tray

        /// <summary>
        /// This class should be created and passed into Application.Run( ... )
        /// </summary>
        public WifiApplicationContext()
        {
            components = new Container();
            var assembly = Assembly.GetExecutingAssembly();
            notifyIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = new Icon(assembly.GetManifestResourceStream(IconFileName)),
                Text = DefaultTooltip,
                Visible = true
            };
            notifyIcon.ContextMenuStrip.Opening += ContextMenuStripOpening;
            notifyIcon.MouseClick += NotifyIconMouseUp;
        }

        private void ContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
            notifyIcon.ContextMenuStrip.Items.Clear();

            var profiles = NetShWrapper.GetWifiProfiles();

            foreach (var profile in profiles)
            {
                notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(profile.Name));
            }

            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit", ExitItemClick));
        }

        /// <remarks>
        /// From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
        /// </remarks>
        private void NotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            }
        }

        /// <summary>
        /// When the application context is disposed, dispose things like the notify icon.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                if (notifyIcon != null)
                {
                    if (notifyIcon.ContextMenuStrip != null)
                        notifyIcon.ContextMenuStrip.Opening -= ContextMenuStripOpening;
                    notifyIcon.MouseUp -= NotifyIconMouseUp;
                    notifyIcon.Dispose();
                }
            }
        }

        /// <summary>
        /// When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitItemClick(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }

        public ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) { item.Click += eventHandler; }

            return item;
        }
    }
}
