﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Microsoft.PowerBI.Common.Api.Gateways.Entities
{
    [DataContract]
    public sealed class UpdateGatewayInstallersRequest
    {
        [Required]
        [DataMember(Name = "ids")]
        public IList<string> Ids { get; set; } = new List<string>();

        [Required]
        [DataMember(Name = "operation")]
        public OperationType Operation { get; set; }

        [Required]
        [DataMember(Name = "gatewayType")]
        public GatewayType GatewayType { get; set; }
    }
}
