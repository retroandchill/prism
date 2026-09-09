// @file ThreeState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Prism.Core.Utils;

internal enum ThreeState : byte
{
    Unknown,
    False,
    True,
}

internal static class ThreeStateExtensions
{
    public static ThreeState ToThreeState(this bool value)
    {
        return value ? ThreeState.True : ThreeState.False;
    }

    extension(ThreeState value)
    {
        public bool HasValue => value != ThreeState.Unknown;

        public bool Value
        {
            get
            {
                Debug.Assert(value != ThreeState.Unknown);
                return value == ThreeState.True;
            }
        }
    }
}
