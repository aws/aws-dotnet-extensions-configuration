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
    /// This exception is thrown when duplicate Systems Manager parameter keys (case insensitive) are 
    /// detected irrespective of whether the parameter is optional or not.
    /// <para>
    /// For example, keys <c>/some-path/some-key</c> and <c>/some-path/SOME-KEY</c> are considered as duplicates.
    /// </para>
    /// </summary>
    public class DuplicateParameterException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of DuplicateParameterException.
        /// </summary>
        public DuplicateParameterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of DuplicateParameterException.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DuplicateParameterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of DuplicateParameterException.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DuplicateParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
