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

using DotSpatial.Data;
using Heiflow.Models.Generic;
using Heiflow.Models.Generic.Project;
using Heiflow.Models.GHM;
using Heiflow.Models.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heiflow.Models.Integration
{
    public class GHMLoader : IModelLoader
    {
        public event EventHandler<string> LoadFailed;
        public string FileTypeDescription
        {
            get
            {
                return "General Hydrological Model";
            }
        }

        public string Extension
        {
            get
            {
                return ".ghm";
            }
        }

        public bool CanImport(IProject project)
        {
            return true;
        }

        public void Import(IProject project, IImportProperty property, ICancelProgressHandler progress)
        {
            GHModel model = new GHModel();
            model.Project = project;
            model.Load(progress);
        }

        public LoadingState Load(IProject project, ICancelProgressHandler progress)
        {
            var succ = LoadingState.Normal;
            GHModel model = new GHModel();
            ModelService.WorkDirectory = project.FullModelWorkDirectory;
            model.ControlFileName = project.RelativeControlFileName;
            model.GridFileFactory = project.GridFileFactory;
            model.Version = project.SelectedVersion;
            succ= model.Load( progress);
            succ = model.LoadGrid(progress) ? LoadingState.Normal : LoadingState.FatalError;
            project.Model = model;
            return succ;
        }

        private void OnLoadFailed(string msg)
        {
            if (LoadFailed != null)
                LoadFailed(this, msg);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}