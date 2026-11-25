// @file IAttributeHolderDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

namespace Prism.Metadata.Builders;

public interface IAttributeHolderDefinitionBuilder
{
    ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes { get; }

    CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type);
}
