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

namespace Heiflow.Core.Data
{
   public class ListTimeSeries<T>
    {
       public ListTimeSeries(int nvar)
       {
           Dates = new List<DateTime>();
           Values = new List<T>[nvar];
           for (int i = 0; i < nvar; i++)
           {
               Values[i] = new List<T>();
           }
       }

       public List<DateTime> Dates
       {
           get;
           set;
       }
       /// <summary>
       /// [var][step]
       /// </summary>
       public List<T>[]  Values
       {
           get;
           set;
       }

       public void Add(DateTime date, T[] row)
       {
           Dates.Add(date);
           var len = Math.Min(row.Length, Values.Length);
           for (int i = 0; i < len; i++)
           {
               Values[i].Add(row[i]);
           }
       }

       public void Clear()
       {
           Dates.Clear();
           for (int i = 0; i < Values.Length; i++)
           {
               Values[i].Clear();
           }
       }
    }
}
