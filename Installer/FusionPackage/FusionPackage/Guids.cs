// Guids.cs
// MUST match guids.h
using System;

namespace ITMOUniversity.FusionPackage
{
    static class GuidList
    {
        public const string guidFusionPackagePkgString = "32556be7-59f5-4a7b-9afb-7c0cd5914ed1";
        public const string guidFusionPackageCmdSetString = "e8f73d30-0b9d-451a-bd3c-9a50f7764304";

        public static readonly Guid guidFusionPackageCmdSet = new Guid(guidFusionPackageCmdSetString);
    };
}