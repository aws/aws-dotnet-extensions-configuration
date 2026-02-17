/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;

namespace Amazon.Extensions.Configuration.SystemsManager
{
    /// <summary>
    /// Exception thrown when one or more parameters are not found in AWS Systems Manager Parameter Store.
    /// </summary>
    public class ParameterNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        public ParameterNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParameterNotFoundException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ParameterNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
