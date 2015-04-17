//-----------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//      EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//      OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
//      The example companies, organizations, products, domain names,
//      e-mail addresses, logos, people, places, and events depicted
//      herein are fictitious.  No association with any real company,
//      organization, product, domain name, email address, logo, person,
//      places, or events is intended or should be inferred.
//-----------------------------------------------------------------------

namespace Microsoft.Fabric.Actor.Samples
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TestResults
    {
        [DataMember]
        public long DriverId = 0;

        [DataMember]
        public OperationStats ReadOperationStats = new OperationStats();

        [DataMember]
        public OperationStats WriteOperationStats = new OperationStats();

        [DataMember]
        public long TotalElaspedWallClockMillis = 0;

        [DataMember]
        public long TotalNumberOfActors = 0;
    }
}