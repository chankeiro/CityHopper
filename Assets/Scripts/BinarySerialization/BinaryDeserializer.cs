// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;

using Bercetech.Games.Fleepas.BinarySerialization.Contexts;
using Bercetech.Games.Fleepas.BinarySerialization.ItemSerializers;

namespace Bercetech.Games.Fleepas.BinarySerialization
{
  /// <summary>
  /// Class used to deserialize objects that were serialized using the BinarySerializer class.
  /// </summary>
  public sealed class BinaryDeserializer:
    BinarySerializerOrDeserializer
  {
    private readonly _TypeDeserializationContext _context = new _TypeDeserializationContext();

    /// <summary>
    /// Creates a new BinaryDeserializer that uses the given Stream to read the data to deserialize.
    /// </summary>
    public BinaryDeserializer(Stream stream):
      base(stream)
    {
    }

    public override void Dispose()
    {
      base.Dispose();
    }

    /// <summary>
    /// Deserializes an object from the Stream this deserializer is bound to.
    /// The deserialization can throw if the Stream is closed, if there's no more data to read
    /// or if the Stream data is just corrupted.
    /// </summary>
    public object Deserialize()
    {
      UInt32 id = CompressedUInt32Serializer.Instance.Deserialize(this);
      // ID 0 is the magic number for null.
      if (id == 0)
        return null;

      var itemDeserializer = _context._DeserializeTypeAndGetItemDeserializer(this, id);
      return itemDeserializer.Deserialize(this);
    }
  }
}
