﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesGridSettingsDialog.xaml.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.MainView
{
    using System.Windows;
    using System.Windows.Input;

    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// Interaction logic for IssuesGridSettingsDialog.xaml
    /// </summary>
    public partial class IssuesGridSettingsDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesGridSettingsDialog"/> class.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data model in.
        /// </param>
        public IssuesGridSettingsDialog(ExtensionDataModel dataModelIn)
        {
            this.InitializeComponent();

            this.DataContext = null;
            this.DataContext = dataModelIn;
        }

        /// <summary>
        /// The close command handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The mouse button event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
