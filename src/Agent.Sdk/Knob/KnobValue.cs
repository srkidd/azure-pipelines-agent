// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.VisualStudio.Services.Agent.Util;

namespace Agent.Sdk.Knob
{
    public class KnobValue
    {
        public IKnobSource Source { get; private set; }
        private string _value;

        public KnobValue(string value, IKnobSource source)
        {
            _value = value;
            Source = source;
        }

        public string AsString()
        {
            return _value;
        }

        public bool AsBoolean()
        {
            // TODO: Are we really sure we want to cast something like "treu" to false?
            // It might make sense to think about FormatException, which already throws AsInt.
            return StringUtil.ConvertToBoolean(_value);
        }

        public bool AsBooleanStrict()
        {
            return StringUtil.ConvertToBooleanStrict(_value);
        }

        public int AsInt()
        {
            return Int32.Parse(_value);
        }
    }
}
