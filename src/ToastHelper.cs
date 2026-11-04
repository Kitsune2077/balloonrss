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
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;


namespace BalloonRss
{
    // sends article notifications as real Windows toast notifications; a click on
    // a toast - also from the notification center - activates the application and
    // opens the article page; on any problem the caller falls back to the classic
    // tray balloon
    static class ToastHelper
    {
        // launch argument structure of the article toasts
        private const String launchAction = "openArticle";
        private const String linkParameter = "&link=";


        // sends a toast for the article; returns false if toasts are not available
        public static bool ShowArticleToast(String header, String body, String link)
        {
            try
            {
                ToastContent content = new ToastContent()
                {
                    // the launch string is delivered back when the toast is activated,
                    // including clicks in the notification center
                    Launch = launchAction + linkParameter + Uri.EscapeDataString(link),
                    ActivationType = ToastActivationType.Foreground,
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText() { Text = header },
                                new AdaptiveText() { Text = body },
                            }
                        }
                    }
                };

                ToastNotificationManagerCompat.CreateToastNotifier()
                    .Show(new ToastNotification(content.GetXml()));
                return true;
            }
            catch (Exception)
            {
                // toasts are not available (e.g. blocked by policy): use the balloon
                return false;
            }
        }


        // parses the launch arguments of an activated toast and returns the article
        // link, or null if the toast was not an article toast
        public static String ParseArticleActivation(String launchArguments)
        {
            if (launchArguments == null)
                return null;

            if (!launchArguments.StartsWith(launchAction, StringComparison.Ordinal))
                return null;

            int parameterPosition = launchArguments.IndexOf(linkParameter, StringComparison.Ordinal);
            if (parameterPosition < 0)
                return null;

            return Uri.UnescapeDataString(launchArguments.Substring(parameterPosition + linkParameter.Length));
        }
    }
}
