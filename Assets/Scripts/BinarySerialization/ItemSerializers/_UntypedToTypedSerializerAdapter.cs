// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;

using Bercetech.Games.Fleepas.Extensions;
using Bercetech.Games.Fleepas.BinarySerialization.Contexts;
using Bercetech.Games.Fleepas.Collections;

namespace Bercetech.Games.Fleepas.BinarySerialization.ItemSerializers
{
  internal sealed class _UntypedToTypedSerializerAdapter<T>:
    BaseItemSerializer<T>
  {
    private readonly IItemSerializer _untypedSerializer;
    internal _UntypedToTypedSerializerAdapter(IItemSerializer untypedSerializer)
    {
      _untypedSerializer = untypedSerializer;
    }

    protected override void DoSerialize(BinarySerializer serializer, T item)
    {
      _untypedSerializer.Serialize(serializer, item);
    }
    protected override T DoDeserialize(BinaryDeserializer deserializer)
    {
      object result = _untypedSerializer.Deserialize(deserializer);
      return (T)result;
    }
  }
}
