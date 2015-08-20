﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenResourceMenu.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Command;

    using View.Helpers;
    using ViewModel;
    using ViewModel.Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The issue handler menu.
    /// </summary>
    internal class PlanMenu : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The model.
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The rest.
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// The config.
        /// </summary>
        private ISonarConfiguration config;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="registerPool">if set to <c>true</c> [register pool].</param>
        private PlanMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, bool registerPool = true)
        {
            this.manager = manager;
            this.model = model;
            this.rest = rest;

            this.ExecuteCommand = new RelayCommand(this.OnPlanCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            if (registerPool)
            {
                SonarQubeViewModel.RegisterNewModelInPool(this);
            }            
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager)
        {
            var topLel = new PlanMenu(rest, model, manager) { CommandText = "Plan", IsEnabled = false };

            topLel.SubItems.Add(new PlanMenu(rest, model, manager) { CommandText = "Add to Existent Plan", IsEnabled = false });
            topLel.SubItems.Add(new PlanMenu(rest, model, manager) { CommandText = "Unplan", IsEnabled = true });
            topLel.SubItems.Add(new PlanMenu(rest, model, manager) { CommandText = "Associate to new plan", IsEnabled = true });
            return topLel;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.visualStudioHelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        public void AssociateWithNewProject(ISonarConfiguration configIn, Resource project, string workingDir)
        {
            this.sourceDir = workingDir;
            this.associatedProject = project;
            this.config = configIn;

            if (this.CommandText.Equals("Add to Existent Plan"))
            {
                Application.Current.Dispatcher.Invoke(
                    delegate
                    {
                        this.SubItems.Clear();

                        foreach (var item in this.rest.GetAvailableActionPlan(this.config, project.Key))
                        {
                            var menu = new PlanMenu(this.rest, this.model, this.manager, false) { CommandText = item.Name, IsEnabled = true };
                            menu.AssociateWithNewProject(configIn, project, workingDir);
                            this.SubItems.Add(menu);
                        }
                    });
            }
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return null;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
            this.config = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on associate command.
        /// </summary>
        private void OnPlanCommand()
        {
            try
            {
                if (this.CommandText.Equals("Associate to new plan"))
                {
                    var availablePlans = this.rest.GetAvailableActionPlan(this.config, this.associatedProject.Key);
                    var newPlan = PromptUserForNewPlan.Prompt(availablePlans);

                    if (newPlan == null)
                    {
                        return;
                    }

                    foreach (var existentPlan in availablePlans)
                    {
                        if (existentPlan.Name.Equals(newPlan))
                        {
                            var associatedWithExistent = QuestionUser.GetInput("Plan exists already, do you want to associate with current plan?");

                            if (associatedWithExistent)
                            {
                                var replies = this.rest.PlanIssues(this.config, this.model.SelectedItems, existentPlan.Key.ToString());
                                foreach (var itemreply in replies)
                                {
                                    this.manager.ReportMessage(new Message() { Data = "Plan Operation Result: " + itemreply.Key + " : " + itemreply.Value  });
                                }
                            }

                            return;
                        }
                    }

                    try
                    {
                        var plan = this.rest.CreateNewPlan(this.config, this.associatedProject.Key, newPlan);
                        var replies = this.rest.PlanIssues(this.config, this.model.SelectedItems, plan.Key.ToString());
                        foreach (var itemreply in replies)
                        {
                            manager.ReportMessage(new Message() { Data = "Plan Operation Result: " + itemreply.Key + " : " + itemreply.Value  });
                        }
                    }
                    catch (Exception ex)
                    {
                        UserExceptionMessageBox.ShowException("Cannot Create Plan, Make sure you have permissions", ex);
                    }                   
                }
                else
                {
                    if (this.CommandText.Equals("Unplan"))
                    {
                        var replies = this.rest.UnPlanIssues(this.config, this.model.SelectedItems);
                        foreach (var itemreply in replies)
                        {
                            this.manager.ReportMessage(new Message() { Data = "Unplan Operation Result: " + itemreply.Key + " : " + itemreply.Value  });
                        }
                    }
                    else
                    {
                        var plans = this.rest.GetAvailableActionPlan(this.config, this.associatedProject.Key);
                        foreach (var plan in plans)
                        {
                            if (plan.Name.Equals(this.CommandText))
                            {
                                var replies = this.rest.PlanIssues(this.config, this.model.SelectedItems, plan.Key.ToString());
                                foreach (var itemreply in replies)
                                {
                                    this.manager.ReportMessage(new Message() { Data = "Plan Operation Result: " + itemreply.Key + " : " + itemreply.Value  });
                                }

                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Perform Operation in Plan: " + ex.Message + " please check vs output log for detailed information", ex);
            }
        }

        #endregion
    }
}