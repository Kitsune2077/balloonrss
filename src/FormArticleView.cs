/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2026  derived from BalloonRSS by Roman Morawek

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Forms;
using BalloonRss.Properties;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;


namespace BalloonRss
{
    // small window displaying the original web page of an RSS article
    // within an embedded browser (WebView2)
    class FormArticleView : Form
    {
        // the article to display
        private readonly String articleTitle;
        private readonly String articleLink;

        // window elements
        private WebView2 webView;
        private Button externalBrowserButton;
        private Label linkLabel;


        // opens the article in a small web window;
        // falls back to the default browser if the WebView2 runtime is not installed
        public static void Open(String title, String link)
        {
            try
            {
                // this call throws if no WebView2 runtime is available
                _ = CoreWebView2Environment.GetAvailableBrowserVersionString();
                new FormArticleView(title, link).Show();
            }
            catch (Exception)
            {
                // no WebView2 runtime: open the link in the default browser instead
                System.Diagnostics.Process.Start(link);
            }
        }


        public FormArticleView(String title, String link)
        {
            articleTitle = title;
            articleLink = link;

            SuspendLayout();

            // toolbar with the link display and the button to open an external browser
            externalBrowserButton = new Button();
            externalBrowserButton.Text = Resources.str_articleViewOpenExternal;
            externalBrowserButton.AutoSize = true;
            externalBrowserButton.Click += new EventHandler(OnOpenExternal);

            linkLabel = new Label();
            linkLabel.Text = articleLink;
            linkLabel.AutoSize = true;
            linkLabel.Padding = new Padding(4, 6, 0, 0);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Top;
            toolbar.AutoSize = true;
            toolbar.Controls.Add(externalBrowserButton);
            toolbar.Controls.Add(linkLabel);

            // embedded browser
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;

            Controls.Add(webView);
            Controls.Add(toolbar);

            // window settings
            Text = articleTitle;
            Icon = Resources.ico_yellow32;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new System.Drawing.Size(960, 680);

            ResumeLayout();

            Shown += new EventHandler(OnShown);
        }


        // initialize the embedded browser as soon as the window is shown
        private async void OnShown(object sender, EventArgs e)
        {
            try
            {
                // keep the browser data next to the other application data
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, GetUserDataFolder());
                if (IsDisposed)
                    return;
                await webView.EnsureCoreWebView2Async(environment);
                if (IsDisposed)
                    return;

                // open links which request a new window within this window
                webView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;

                webView.CoreWebView2.Navigate(articleLink);
            }
            catch (Exception)
            {
                // the embedded browser could not be initialized: fall back to the default browser
                System.Diagnostics.Process.Start(articleLink);
                Dispose();
            }
        }


        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            webView.CoreWebView2.Navigate(e.Uri);
        }


        private void OnOpenExternal(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(articleLink);
        }


        // the folder for the WebView2 user data (next to the other application data)
        private static String GetUserDataFolder()
        {
#if PORTABLE
            String baseFolder = PortableSettingsProvider.DataDirectory;
#else
            String baseFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + System.IO.Path.DirectorySeparatorChar + "BalloonRSS";
#endif
            return System.IO.Path.Combine(baseFolder, "WebView2");
        }
    }
}
