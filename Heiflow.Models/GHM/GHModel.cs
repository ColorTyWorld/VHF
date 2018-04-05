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

using Heiflow.Core.Data;
using Heiflow.Models.Generic;
using Heiflow.Models.Generic.Packages;
using Heiflow.Models.Generic.Project;
using Heiflow.Models.IO;
using Heiflow.Models.Properties;
using Heiflow.Models.Subsurface;
using Heiflow.Models.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Heiflow.Models.GHM
{
    [Export(typeof(IBasicModel))]
    [ModelItem]
    public class GHModel : BaseModel
    {
        private GHMSerializer _GHMSerializer;
        private GHMPackage _GHMPackage;

        public GHModel()
        {
            Name = "GHM";
            Icon = Resources.ServiceWMSGroup16;
            LargeIcon = Resources.ServiceWMSGroup32;
            Packages = new Dictionary<string, IPackage>();
        }


        public GHMPackage MasterPackage
        {
            get
            {
                return _GHMPackage;
            }
        }

        public override bool New(IProgress progress)
        {
            _GHMSerializer = new GHMSerializer();
            _GHMSerializer.FileName = @"E:\Heihe\HRB\GeoData\GBHM\Model\section_v1.09_ylx\uhrb.ghm";
            _GHMSerializer.TimeReference = new TimeReference()
            {
                Start = new DateTime(2000, 1, 1),
                End = new DateTime(2010, 12, 31),
                TimeStep = 86400
            };
            _GHMSerializer.SpatialReference = new SpatialReference()
            {
                sr = @".\dem\geo_lambert.prj",
                X = 453035,
                Y = 4338102
            };

            Member mem = new Member()
            {
                Name = "Grid",
            };
            Item ele = new Item()
            {
                Name = "Elevation",
                Path = @".\dem\elevation.asc",
                Description = "Elevation"
            };
            mem.Spatial.Add(ele);

            Member para = new Member()
            {
                Name = "Parameter",
            };
            Item alpha = new Item()
            {
                Name = "Alpha",
                Path = @".\dem\elevation.asc",
                Description = "Elevation"
            };
            para.Spatial.Add(alpha);

            _GHMSerializer.Save();
            return true;
        }

        public override void Initialize()
        {

        }

        public override bool Validate()
        {
            return true;
        }


        public override bool Load(IProgress progress)
        {
            string masterfile = ControlFileName;
            if (File.Exists(masterfile))
            {
                ControlFileName = masterfile;
                _GHMSerializer = GHMSerializer.Open(masterfile);
                _GHMPackage = new GHMPackage()
                {
                    Serializer = _GHMSerializer,
                    GHModel = this
                };
                AddInSilence(_GHMPackage);

                foreach (var layer in _GHMSerializer.Layers)
                {
                    foreach (var mem in layer.Members)
                    {
                        foreach (var item in mem.Spatial)
                        {
                            item.FullPath = Path.Combine(ModelService.WorkDirectory, item.Path);
                            item.Grid = this.Grid;
                        }
                        foreach (var item in mem.Spatiotemporal)
                        {
                            item.FullPath = Path.Combine(ModelService.WorkDirectory, item.Path);
                            item.Grid = this.Grid;
                        }
                        foreach (var item in mem.TimeSeries)
                        {
                            item.FullPath = Path.Combine(ModelService.WorkDirectory, item.Path);
                            item.Grid = this.Grid;
                        }
                    }
                }
                return true;
            }   
            {            
                var msg= "\r\nThe model file dose not exist: " + ControlFileName;
                progress.Progress(msg);
                return false;
            }
        }

        public override bool LoadGrid(IProgress progress)
        {
            var gridnode = (from fl in
                                ((from layer in _GHMSerializer.Layers where layer.Name == "Base" select layer).First().Members)
                            where fl.Name == "Grid"
                            select fl).FirstOrDefault();
            var ele = (from gd in gridnode.Spatial where gd.Name == "Elevation" select gd).FirstOrDefault();
    
            //var provider = base.Project.Manager.GridFileFactory.Select(ele.FullPath);
            //var grid = provider.Provide(ele.FullPath) ;

            //grid.Extent(_GHMSerializer.SpatialReference);
            //grid.BuildTopology();

            //this.Grid = grid;
            return true;
        }
        public override void Clear()
        {
            
        }


        public override void Attach(DotSpatial.Controls.IMap map, string directory)
        {
            throw new NotImplementedException();
        }

        public override void Save(IProgress progress)
        {
            throw new NotImplementedException();
        }
    }
}
