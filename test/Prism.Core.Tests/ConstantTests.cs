// @file ConstantTests.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Constants;

namespace Prism.Core.Tests;

public class ConstantTests
{
    [Test]
    public void SignExtensionWorksCorrectly()
    {
        IntegerConstant value = (sbyte)-15;

        var asShort = value.ConvertTo(IntegerKind.I16);
        if (asShort is not short sh)
        {
            Assert.Fail("Conversion failed");
            return;
        }
        Assert.That(sh, Is.EqualTo((short)-15));
    }

    [Test]
    public void SignTruncationWorksCorrectly()
    {
        IntegerConstant value = -14;

        var asShort = value.ConvertTo(IntegerKind.I16);
        if (asShort is not short sh)
        {
            Assert.Fail("Conversion failed");
            return;
        }
        Assert.That(sh, Is.EqualTo((short)-14));
    }

    [Test]
    public void NegatingUnsignedIncreasesPrecision()
    {
        IntegerConstant value = 14U;

        var negated = value.TryNegate();
        Assert.That(negated, Is.Not.Null);
        if (negated is not long l)
        {
            Assert.Fail("Conversion failed");
            return;
        }

        Assert.That(l, Is.EqualTo(-14L));
    }
}
