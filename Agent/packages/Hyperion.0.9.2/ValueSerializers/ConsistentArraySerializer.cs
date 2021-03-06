#region copyright
// -----------------------------------------------------------------------
//  <copyright file="ConsistentArraySerializer.cs" company="Akka.NET Team">
//      Copyright (C) 2015-2016 AsynkronIT <https://github.com/AsynkronIT>
//      Copyright (C) 2016-2016 Akka.NET Team <https://github.com/akkadotnet>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Hyperion.Extensions;

namespace Hyperion.ValueSerializers
{
    public class ConsistentArraySerializer : ValueSerializer
    {
        public const byte Manifest = 252;
        public static readonly ConsistentArraySerializer Instance = new ConsistentArraySerializer();

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var elementSerializer = session.Serializer.GetDeserializerByManifest(stream, session);
            //read the element type
            var elementType = elementSerializer.GetElementType();
            //get the element type serializer
            var length = stream.ReadInt32(session);
            var array = Array.CreateInstance(elementType, length); //create the array
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(array);
            }

            if (elementType.IsFixedSizeType())
            {
                var size = elementType.GetTypeSize();
                var totalSize = size*length;
                var buffer = session.GetBuffer(totalSize);
                stream.Read(buffer, 0, totalSize);
                Buffer.BlockCopy(buffer, 0, array, 0, totalSize);
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    var value = elementSerializer.ReadValue(stream, session); //read the element value
                    array.SetValue(value, i); //set the element value
                }
            }

            return array;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackSerializedObject(value);
            }
            var elementType = value.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(stream, session); //write array element type
            // ReSharper disable once PossibleNullReferenceException
            WriteValues((dynamic)value, stream,elementSerializer,session);
        }

        private static void WriteValues<T>(T[] array, Stream stream, ValueSerializer elementSerializer, SerializerSession session)
        {
            Int32Serializer.WriteValueImpl(stream,array.Length,session);
            if (typeof(T).IsFixedSizeType())
            {
                var size = typeof(T).GetTypeSize();
                var result = new byte[array.Length * size];
                Buffer.BlockCopy(array, 0, result, 0, result.Length);
                stream.Write(result);
            }
            else
            {
                foreach (var value in array)
                {
                    elementSerializer.WriteValue(stream, value, session);
                }
            }    
        }
    }
}