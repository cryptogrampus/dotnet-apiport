﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AnalyzeResponse : IComparable
    {
        public AnalyzeResponse()
        {
            MissingDependencies = new List<MemberInfo>();
            UnresolvedUserAssemblies = new List<string>();
            Targets = new List<FrameworkName>();
        }

        public IList<MemberInfo> MissingDependencies { get; set; }

        public IList<string> UnresolvedUserAssemblies { get; set; }

        public IList<FrameworkName> Targets { get; set; }

        public string SubmissionId { get; set; }

        [JsonIgnore]
        public ReportingResult ReportingResult { get; set; }

        public int CompareTo(object obj)
        {
            var analyzeObject = obj as AnalyzeResponse;
            return SubmissionId.CompareTo(analyzeObject.SubmissionId);
        }
    }
}
