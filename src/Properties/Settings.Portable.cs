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

#if PORTABLE
using System.Configuration;

namespace BalloonRss.Properties
{
    // in the portable build, the user settings are stored in the "data" folder
    // next to the executable instead of the per-user profile
    [SettingsProvider(typeof(BalloonRss.PortableSettingsProvider))]
    internal sealed partial class Settings
    {
    }
}
#endif
