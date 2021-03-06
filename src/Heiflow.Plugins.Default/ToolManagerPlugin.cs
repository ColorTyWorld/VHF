﻿//
// The Visual HEIFLOW License
//
// Copyright (c) 2015-2018 Yong Tian, SUSTech, Shenzhen, China. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Note:  The software also contains contributed files, which may have their own 
// copyright notices. If not, the GNU General Public License holds for them, too, 
// but so that the author(s) of the file have the Copyright.
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DotSpatial.Controls;
using DotSpatial.Controls.Docking;
using DotSpatial.Controls.Header;
using DotSpatial.Modeling.Forms;
using Heiflow.Plugins.Default.Properties;

namespace DotSpatial.Plugins.ToolManager
{
    public class ToolManagerPlugin : Extension, IPartImportsSatisfiedNotification
    {
        private Controls.ToolManager toolManager;

        [Import("Shell", typeof(ContainerControl))]
        private ContainerControl Shell { get; set; }

        /// <summary>
        /// Gets the list tools available.
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<ITool> Tools { get; set; }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            if (IsActive)
            {
                // This method may be called on another thread after recomposition.
                if (Shell.InvokeRequired)
                {
                    Shell.Invoke((MethodInvoker)ShowToolsPanel);
                }
                else
                {
                    ShowToolsPanel();
                }
            }
        }

        #endregion

        public override void Activate()
        {
            ShowToolsPanel();
            var showTools = new SimpleActionItem("kView", Resources.Spatial_Tools,
              delegate(object sender, EventArgs e)
              { App.DockManager.ShowPanel("kTools"); })
            {
                Key = "kShowPackageTools",
                ToolTipText =  Resources.Spatial_Tools_tips,
                GroupCaption =Resources.Map_Group,
                LargeImage = toolManager.ImageList.Images["Hammer"]
            };
            App.HeaderControl.Add(showTools);


            base.Activate();
        }

        public override void Deactivate()
        {
            App.HeaderControl.RemoveAll();
            App.DockManager.Remove("kTools");
            toolManager = null;
            base.Deactivate();
        }


        private void ShowToolsPanel()
        {
            if (Tools != null && Tools.Any())
            {
                if (toolManager != null) return;
                toolManager = new Controls.ToolManager
                {
                    App = App,
                    Legend = App.Legend,
                    Location = new Point(208, 12),
                    Name = "toolManager",
                    Size = new Size(192, 308),
                    TabIndex = 1
                };
                App.CompositionContainer.ComposeParts(toolManager);
                Shell.Controls.Add(toolManager);
                App.DockManager.Add(new DockablePanel("kTools", Resources.Spatial_Tools, toolManager, DockStyle.Left) 
                { SmallImage = toolManager.ImageList.Images["Hammer"] });


            }
            else
            {
                toolManager = null;
                App.DockManager.Remove("kTools");
            }
        }
    }
}