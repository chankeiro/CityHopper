// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Bercetech.Games.Fleepas.Extensions;

namespace Bercetech.Games.Fleepas.BinarySerialization.ItemSerializers
{
  public sealed class ByteSerializer:
    BaseItemSerializer<byte>
  {
    public static readonly ByteSerializer Instance = new ByteSerializer();

    private ByteSerializer()
    {
    }

    protected override void DoSerialize(BinarySerializer serializer, byte item)
    {
      serializer.Stream.WriteByte(item);
    }
    protected override byte DoDeserialize(BinaryDeserializer deserializer)
    {
      return deserializer.Stream.ReadByteOrThrow();
    }
  }
}
