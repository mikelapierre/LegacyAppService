﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace IdentityWebAPI.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;

    public partial class ValueTupleValueTuple2SequenceValueTuple2Sequence
    {
        /// <summary>
        /// Initializes a new instance of the
        /// ValueTupleValueTuple2SequenceValueTuple2Sequence class.
        /// </summary>
        public ValueTupleValueTuple2SequenceValueTuple2Sequence() { }

        /// <summary>
        /// Initializes a new instance of the
        /// ValueTupleValueTuple2SequenceValueTuple2Sequence class.
        /// </summary>
        public ValueTupleValueTuple2SequenceValueTuple2Sequence(IList<ValueTupleStringString> item1 = default(IList<ValueTupleStringString>), IList<ValueTupleStringString> item2 = default(IList<ValueTupleStringString>))
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Item1")]
        public IList<ValueTupleStringString> Item1 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Item2")]
        public IList<ValueTupleStringString> Item2 { get; set; }

    }
}
