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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heiflow.Models.Generic;
using System.IO;
using ILNumerics;
using System.ComponentModel;
using System.Diagnostics;
using Heiflow.Core.Data;
using Heiflow.Models.IO;
using System.ComponentModel.Composition;
using DotSpatial.Data;

namespace Heiflow.Models.Subsurface
{
    public class CBCPackage : MFDataPackage
    {
        public static string CBCName = "CBC";
        public CBCPackage()
        {
            Name = CBCName;
            _MaxTimeStep = 10;
            Layer = 0;
            NumTimeStep = 0;
            Variables = new string[] { "FLOW RIGHT FACE", "FLOW FRONT FACE", "FLOW LOWER FACE",
                "STREAM LEAKAGE", "UZF RECHARGE", "SURFACE LEAKAGE", "GW ET", "CONSTANT HEAD" };
            _PackageInfo.Format = FileFormat.Binary;
            _PackageInfo.IOState = IOState.REPLACE;
            _PackageInfo.FileExtension = ".cbc";
            _PackageInfo.ModuleName = "DATA";
            _PackageInfo.FID = 9;
            IsMandatory = true;
            _Layer3DToken = "RegularGrid";
        }
        public override void Initialize()
        {
            this.Grid = Owner.Grid;
            this.TimeService = Owner.TimeService;
            this.TimeService.Updated += this.OnTimeServiceUpdated;
            State = ModelObjectState.Ready;
            _Initialized = true;
        }
        public override bool Load(ICancelProgressHandler progress)
        {
            if (File.Exists(FileName))
            {
                _ProgressHandler = progress;
                var grid = Owner.Grid as MFGrid;
                CBCFile cbc = new CBCFile(FileName, grid);
                //cbc.Source = Values;
                cbc.Layer = this.Layer;
                cbc.Loading += cbc_Loading;
                cbc.DataCubeLoaded += cbc_DataCubeLoaded;
                cbc.LoadDataCube();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Scan()
        {
            var grid = Owner.Grid as MFGrid;
            CBCFile cbc = new CBCFile(FileName, grid);
            cbc.Scan();
            this.NumTimeStep = cbc.NumTimeStep;
            this.Variables = cbc.Variables;

            _StartLoading = TimeService.Start;
            MaxTimeStep = NumTimeStep;
            return true;
        }

        public override bool Load(int var_index, ICancelProgressHandler progress)
        {
            _ProgressHandler = progress;
            var grid = Owner.Grid as MFGrid;
            int nstep = StepsToLoad;
            if (DataCube == null || DataCube.Size[1] != nstep)
            {
                DataCube = new DataCube<float>(Variables.Length, nstep, grid.ActiveCellCount)
                {
                    Name = "CBC",
                    TimeBrowsable = true,
                    AllowTableEdit = false
                };
            }
            DataCube.Topology = (this.Grid as RegularGrid).Topology;
            DataCube.DateTimes = this.TimeService.IOTimeline.Take(StepsToLoad).ToArray();
            CBCFile cbc = new CBCFile(FileName, grid);
            cbc.Layer = this.Layer;
            cbc.Scale = (float)this.ScaleFactor;
            cbc.MaxTimeStep = nstep;
            cbc.NumTimeStep = this.NumTimeStep;
            cbc.DataCube = this.DataCube;
            cbc.Loading += cbc_Loading;
            cbc.DataCubeLoaded += cbc_DataCubeLoaded;
            cbc.LoadFailed += cbc_LoadFailed;
            cbc.LoadDataCube(var_index);
            return true;
        }

        public override void Clear()
        {
            if (_Initialized)
            {
                this.TimeService.Updated -= this.OnTimeServiceUpdated;
            }
            State = ModelObjectState.Standby;
            _Initialized = false;
        }
        public override void Attach(DotSpatial.Controls.IMap map,  string directory)
        {
            this.Feature = Owner.Grid.FeatureSet;
            this.FeatureLayer = Owner.Grid.FeatureLayer;
        }
        private void cbc_Loading(object sender, int e)
        {
            OnLoading(e);
        }
        private void cbc_LoadFailed(object sender, string e)
        {
            OnLoadFailed(e,_ProgressHandler);
        }
        private void cbc_DataCubeLoaded(object sender, DataCube<float> e)
        {
            OnLoaded(_ProgressHandler);
        }
    }
}