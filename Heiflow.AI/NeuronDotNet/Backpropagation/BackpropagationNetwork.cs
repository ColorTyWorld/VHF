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
using System.Runtime.Serialization;

namespace  Heiflow.AI.NeuronDotNet.Core.Backpropagation
{
    /// <summary>
    /// This class extends a <see cref="Network"/> and represents a Backpropagation neural network.
    /// </summary>
    [Serializable]
    public class BackpropagationNetwork : Network
    {
        private double meanSquaredError;
        private bool isValidMSE;

        /// <summary>
        /// Gets the value of mean squared error
        /// </summary>
        /// <value>
        /// Mean squared value of error in current training epoch
        /// </value>
        public double MeanSquaredError
        {
            get { return isValidMSE ? meanSquaredError : 0d; }
        }

        /// <summary>
        /// Creates a new Back Propagation Network, with the specified input and output layers. (You
        /// are required to connect all layers using appropriate synapses, before using the constructor.
        /// Any changes made to the structure of the network after its creation may lead to complete
        /// malfunctioning)
        /// </summary>
        /// <param name="inputLayer">
        /// The input layer
        /// </param>
        /// <param name="outputLayer">
        /// The output layer
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>inputLayer</c> or <c>outputLayer</c> is <c>null</c>
        /// </exception>
        public BackpropagationNetwork(ActivationLayer inputLayer, ActivationLayer outputLayer)
            : base(inputLayer, outputLayer, TrainingMethod.Supervised)
        {
            this.meanSquaredError = 0d;
            this.isValidMSE = false;
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">
        /// Serialization information to deserialize and obtain the data
        /// </param>
        /// <param name="context">
        /// Serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public BackpropagationNetwork(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// <para>
        /// Trains the network for the given training sample (Online training mode). Note that this
        /// method trains the sample only once irrespective of the values of <c>currentIteration</c>
        /// and <c>trainingEpochs</c>. Those arguments are just used to adjust training parameters
        /// which are dependent on training progress.
        /// </para>
        /// </summary>
        /// <param name="trainingSample">
        /// Training sample to use
        /// </param>
        /// <param name="currentIteration">
        /// Current training epoch
        /// </param>
        /// <param name="trainingEpochs">
        /// Number of training epochs
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>trainingSample</c> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <c>trainingEpochs</c> is not positive, or if <c>currentIteration</c> is negative or if
        /// <c>currentIteration</c> is less than <c>trainingEpochs</c>
        /// </exception>
        public override void Learn(TrainingSample trainingSample, int currentIteration, int trainingEpochs)
        {
            meanSquaredError = 0d;
            isValidMSE = true;
            base.Learn(trainingSample, currentIteration, trainingEpochs);
        }

        /// <summary>
        /// Invokes BeginEpochEvent
        /// </summary>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingSet">
        /// Training set which is about to be trained
        /// </param>
        protected override void OnBeginEpoch(int currentIteration, TrainingSet trainingSet)
        {
            meanSquaredError = 0d;
            isValidMSE = false;
            base.OnBeginEpoch(currentIteration, trainingSet);
        }

        /// <summary>
        /// Invokes EndEpochEvent
        /// </summary>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingSet">
        /// Training set which got trained successfully this epoch
        /// </param>
        protected override void OnEndEpoch(int currentIteration, TrainingSet trainingSet)
        {
            meanSquaredError /= trainingSet.TrainingSampleCount;
            isValidMSE = true;
            base.OnEndEpoch(currentIteration, trainingSet);
        }

        /// <summary>
        /// A protected helper function used to train single learning sample
        /// </summary>
        /// <param name="trainingSample">
        /// Training sample to use
        /// </param>
        /// <param name="currentIteration">
        /// Current training epoch (Assumed to be positive and less than <c>trainingEpochs</c>)
        /// </param>
        /// <param name="trainingEpochs">
        /// Number of training epochs (Assumed to be positive)
        /// </param>
        protected override void LearnSample(TrainingSample trainingSample, int currentIteration, int trainingEpochs)
        {
            // No validation here
            int layerCount = layers.Count;

            // Set input vector
            inputLayer.SetInput(trainingSample.InputVector);

            for (int i = 0; i < layerCount; i++)
            {
                layers[i].Run();
            }

            // Set Errors
            meanSquaredError += (outputLayer as ActivationLayer).SetErrors(trainingSample.OutputVector);

            // Backpropagate errors
            for (int i = layerCount; i > 0; )
            {
                ActivationLayer layer = layers[--i] as ActivationLayer;
                if (layer != null)
                {
                    layer.EvaluateErrors();
                }
            }

            // Optimize synapse weights and neuron bias values
            for (int i = 0; i < layerCount; i++)
            {
                layers[i].Learn(currentIteration, trainingEpochs);
            }
        }
    }
}