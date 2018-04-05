//
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

namespace  Heiflow.AI.Math.Random
{
    using System;

    /// <summary>
    /// Standard random numbers generator.
    /// </summary>
    /// 
    /// <remarks><para>The random number generator generates gaussian
    /// random numbers with zero mean and standard deviation of one. The generator
    /// implements polar form of the Box-Muller transformation.</para>
    /// 
    /// <para>The generator uses <see cref="UniformOneGenerator"/> generator as a base
    /// to generate random numbers.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create instance of random generator
    /// IRandomNumberGenerator generator = new StandardGenerator( );
    /// // generate random number
    /// double randomNumber = generator.Next( );
    /// </code>
    /// </remarks>
    /// 
    public class StandardGenerator : IRandomNumberGenerator
    {
        private UniformOneGenerator rand = null;

        private double  secondValue;
        private bool    useSecond = false;

        /// <summary>
        /// Mean value of the generator.
        /// </summary>
        /// 
        public double Mean
        {
            get { return 0; }
        }

        /// <summary>
        /// Variance value of the generator.
        /// </summary>
        ///
        public double Variance
        {
            get { return 1; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardGenerator"/> class.
        /// </summary>
        /// 
        public StandardGenerator( )
        {
            rand = new UniformOneGenerator( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardGenerator"/> class.
        /// </summary>
        /// 
        /// <param name="seed">Seed value to initialize random numbers generator.</param>
        /// 
        public StandardGenerator( int seed )
        {
            rand = new UniformOneGenerator( seed );
        }

        /// <summary>
        /// Generate next random number.
        /// </summary>
        /// 
        /// <returns>Returns next random number.</returns>
        /// 
        public double Next( )
        {
            // check if we can use second value
            if ( useSecond )
            {
                // return the second number
                useSecond = false;
                return secondValue;
            }

            double x1, x2, w, firstValue;

            // generate new numbers
            do
            {
                x1  = rand.Next( ) * 2.0 - 1.0;
                x2  = rand.Next( ) * 2.0 - 1.0;
                w   = x1 * x1 + x2 * x2;
            }
            while ( w >= 1.0 );

            w = Math.Sqrt( ( -2.0 * Math.Log( w ) ) / w );

            // get two standard random numbers
            firstValue  = x1 * w;
            secondValue = x2 * w;

            useSecond = true;

            // return the first number
            return firstValue;
        }

        /// <summary>
        /// Set seed of the random numbers generator.
        /// </summary>
        /// 
        /// <param name="seed">Seed value.</param>
        /// 
        /// <remarks>Resets random numbers generator initializing it with
        /// specified seed value.</remarks>
        /// 
        public void SetSeed( int seed )
        {
            rand = new UniformOneGenerator( seed );
            useSecond = false;
        }
    }
}
