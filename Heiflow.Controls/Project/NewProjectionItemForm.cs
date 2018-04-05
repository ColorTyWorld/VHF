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

using Heiflow.Applications;
using Heiflow.Controls.Tree;
using Heiflow.Models.Generic;
using Heiflow.Models.Generic.Attributes;
using Heiflow.Presentation.Services;
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Collections.Generic;
using Heiflow.Core.Data;
using System.Linq;
using Heiflow.Controls.WinForm.Properties;


namespace Heiflow.Presentation.Controls.Project
{
    [Export(typeof(INewProjectItemWindow))]
    public partial class NewProjectionItemForm : Form,INewProjectItemWindow
    {
        private TreeModel _TreeModel;
        private IBasicModel _BasicModel;
        private List<IPackage> _PckChanged;

        public NewProjectionItemForm(IBasicModel model)
        {
            InitializeComponent();
            _BasicModel = model;
            _TreeModel = new TreeModel();
            this.treeView1.Model = _TreeModel;
            this.treeView1.NodeMouseClick += treeView1_NodeMouseClick;
            this._nodeTextBox.DataPropertyName = "Text";
            this.nodeStateIcon1.DataPropertyName = "Image";
            _PckChanged = new List<IPackage>();
        }

        private void NewPrjForm_Load(object sender, EventArgs e)
        {
            var project_service = MyAppManager.Instance.CompositionContainer.GetExportedValue<IProjectService>();
            var pck_service = MyAppManager.Instance.CompositionContainer.GetExportedValue<IPackageService>();
            Dictionary<IPackage, PackageCategory> dic = new Dictionary<IPackage, PackageCategory>();
            foreach (var pck in pck_service.SupportedMFPackages)
            {
                var atr = pck.GetType().GetCustomAttributes(typeof(PackageCategory), true);
                if (atr.Length == 1)
                {
                    var cat = atr[0] as PackageCategory;
                    dic.Add(pck, cat);
                }
            }
            var buf = from cc in dic.Values group cc by cc.Root into gp select new { root = gp.Key, cats = gp };
            foreach (var gp in buf)
            {
                Node node_root = new Node(gp.root)
                {
                    Image = Resources.MapPackageTiledTPKFile16,
                    Tag = (from dd in dic where dd.Value.Root == gp.root select dd.Key).ToArray()
                };

                var level1 = from cc in dic.Values where cc.Root == gp.root && cc.Depth == 2 select cc;
                if (level1.Count() > 0)
                {
                    var gp_level1 = from ll in level1 group ll by ll.Tokens[1] into lgp select new { token = lgp.Key };

                    foreach (var ll in gp_level1)
                    {
                        var node_level1 = new Node(ll.token)
                         {
                             Image = Resources.MapPackageTiledTPKFile16,
                             Tag = (from dd in dic where dd.Value.Depth == 2 && dd.Value.Tokens[1] == ll.token select dd.Key).ToArray()
                         };
                        node_root.Nodes.Add(node_level1);
                    }
                }
                _TreeModel.Nodes.Add(node_root);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeAdvMouseEventArgs e)
        {
            var node = e.Node.Tag as Node;
            this.olvSimple.SetObjects(node.Tag as IPackage[]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (var pck in _PckChanged)
            {
                if (pck.IsUsed)
                    _BasicModel.Add(pck);
                else
                {
                    var question = pck.Name + " will be removed. Do you really want to reomove it?";
                    if (MessageBox.Show(question, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        _BasicModel.Remove(pck);
                }
            }
            this.Close();
        }

        private void olvSimple_SelectionChanged(object sender, EventArgs e)
        {
            var pck = olvSimple.SelectedObject as IPackage;
            if (pck != null)
                tbModelDes.Text = pck.Description;
            else
                tbModelDes.Text = "";
        }

        private void olvSimple_SubItemChecking(object sender, BrightIdeasSoftware.SubItemCheckingEventArgs e)
        {
            var pck = e.RowObject as IPackage;
            if (pck.IsMandatory)
            {              
                MessageBox.Show("This package is mandatory.", "Add Package", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Canceled = true;
                return;
            }
            if (!_PckChanged.Contains(pck))
                _PckChanged.Add(pck);
        }
    }
}
