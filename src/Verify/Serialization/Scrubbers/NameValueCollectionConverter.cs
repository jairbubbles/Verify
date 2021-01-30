﻿using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using VerifyTests;

class NameValueCollectionConverter :
    WriteOnlyJsonConverter<NameValueCollection>
{
    List<string> ignoredByNameMembers;

    public NameValueCollectionConverter(List<string> ignoredByNameMembers)
    {
        this.ignoredByNameMembers = ignoredByNameMembers;
    }

    public override void WriteJson(JsonWriter writer, NameValueCollection? collection, JsonSerializer serializer, IReadOnlyDictionary<string, object> context)
    {
        if (collection is null)
        {
            return;
        }

        var dictionary = new Dictionary<string,string?>();
        foreach (string? key in collection)
        {
            var value = collection.Get(key);

            string? notNullKey;
            if (key == null)
            {
                notNullKey = "null";
            }
            else
            {
                if (ignoredByNameMembers.Contains(key))
                {
                    continue;
                }

                notNullKey = key;
            }

            value ??= "null";

            dictionary[notNullKey] = value;
        }

        serializer.Serialize(writer,dictionary);
    }
}