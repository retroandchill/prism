// @file NameTest.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Strings;

namespace Prism.Core.Tests;

public class NameTest
{
    [Test]
    public void FindTypeFindDoesNotCreateNewEntries()
    {
        var existing = new Name("Knight");
        Assert.That(existing.IsNone, Is.False);

        var existingId = existing.Id;

        var foundExisting = new Name("Knight", FindName.Find);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(foundExisting.IsNone, Is.False);
            Assert.That(foundExisting.Id, Is.EqualTo(existingId));
        }

        var unknownName = Guid.CreateVersion7().ToString();
        var notCreated = new Name(unknownName, FindName.Find);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(notCreated.IsNone, Is.True);
        }
    }
}
