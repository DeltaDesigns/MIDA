using System.Collections.Concurrent;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Activity
{
    public struct Bubble
    {
        public string Name;
        public Tag<SBubbleDefinition> ChildMapReference;
    }

    public struct ActivityEntities
    {
        public string BubbleName;
        public string ActivityPhaseName2;
        public FileHash Hash;
        public List<FileHash> DataTables;
        public Dictionary<ulong, ActivityEntity> WorldIDs; //World ID, name/subname
    }

    public interface IActivity : ISchema
    {
        public FileHash FileHash { get; }
        public string DestinationName { get; }
        public IEnumerable<Bubble> EnumerateBubbles();
        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null);
    }

    public struct ActivityEntity
    {
        public string Name;
        public string SubName;
    }
}

namespace Tiger.Schema.Activity.MARATHON
{
    public class Activity : Tag<SActivity>, IActivity
    {
        public FileHash FileHash => Hash;

        private string _destinationName;
        public string DestinationName
        {
            get
            {
                if (_destinationName != null)
                    return _destinationName;

                _destinationName = Helpers.SanitizeString(GetDestinationName());
                return _destinationName;
            }
        }

        public Activity(FileHash hash) : base(hash)
        {
        }

        private string GetDestinationName()
        {
            return GlobalStrings.Get().GetString(new StringHash(_tag.LocationName.Hash32));
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            var stringContainer = _tag.Destination.TagData.StringContainer;
            foreach (var mapEntry in _tag.Unk50)
            {
                foreach (var mapReference in mapEntry.MapReferences)
                {

                    if (mapReference.MapReference is null || mapReference.MapReference.TagData.ChildMapReference == null)
                        continue;

                    string name = stringContainer is null ? mapEntry.BubbleName : stringContainer.GetStringFromHash(mapEntry.BubbleName);
                    if ((name.Contains("NotFound") || mapEntry.BubbleName.ToString() == name)) // this is dumb
                        name = GlobalStrings.Get().GetString(mapEntry.BubbleName);

                    yield return new Bubble
                    {
                        Name = name,
                        ChildMapReference = mapReference.MapReference.TagData.ChildMapReference
                    };
                }
            }
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null)
        {
            var stringContainer = _tag.Destination.TagData.StringContainer;
            foreach (var entry in _tag.Unk50)
            {
                foreach (var resource in entry.Unk18)
                {
                    string name = stringContainer is null ? resource.BubbleName : stringContainer.GetStringFromHash(resource.BubbleName);
                    yield return new ActivityEntities
                    {
                        BubbleName = name,
                        Hash = resource.UnkEntityReference.Hash,
                        ActivityPhaseName2 = GlobalStrings.Get().GetString(new StringHash(resource.ActivityPhaseName2.Hash32)),
                        DataTables = CollapseResourceParent(resource.UnkEntityReference.Hash),
                        WorldIDs = GetWorldIDs(resource.UnkEntityReference.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();
            var entry = FileResourcer.Get().GetSchemaTag<S8080B381>(hash);
            var Unk18 = FileResourcer.Get().GetSchemaTag<S8080B3EA>(entry.TagData.Unk18.Hash);

            foreach (var resource in Unk18.TagData.EntityComponents)
            {
                if (resource.EntityComponentParent != null)
                {
                    var resourceValue = resource.EntityComponentParent.TagData.EntityComponent.TagData.Unk18.GetValue(resource.EntityComponentParent.TagData.EntityComponent.GetReader());
                    switch (resourceValue)
                    {
                        case S8080B1A5:
                            var tag = (S8080B1A5)resourceValue;
                            if (tag.Unk84 is not null && tag.Unk84.TagData.DataEntries.Count > 0)
                            {
                                items.Add(tag.Unk84.Hash);
                            }
                            break;

                        case SEF8C8080:
                            var tag2 = (SEF8C8080)resourceValue;
                            if (tag2.Unk58 is not null && tag2.Unk58.TagData.DataEntries.Count > 0)
                            {
                                items.Add(tag2.Unk58.Hash);
                            }
                            break;
                    }
                }
            }

            return items.ToList();
        }

        private Dictionary<ulong, ActivityEntity> GetWorldIDs(FileHash hash)
        {
            Dictionary<ulong, ActivityEntity> items = new();
            Dictionary<uint, string> strings = new();
            var entry = FileResourcer.Get().GetSchemaTag<S8080B381>(hash);
            var Unk18 = FileResourcer.Get().GetSchemaTag<S8080B3EA>(entry.TagData.Unk18.Hash);

            foreach (var resource in Unk18.TagData.EntityComponents)
            {
                if (resource.EntityComponentParent != null)
                {
                    var entResource = resource.EntityComponentParent.TagData.EntityComponent;
                    try // Fuck it, bruce forcing time
                    {
                        var resourceValue = (S8080B199)entResource.TagData.Unk18.GetValue(entResource.GetReader());

                        if (entResource.TagData.UnkHash80 != null)
                        {
                            var unk80 = FileResourcer.Get().GetSchemaTag<S8080B1E2>(entResource.TagData.UnkHash80.Hash);
                            foreach (var a in unk80.TagData.Unk08)
                            {
                                if (a.Unk00.Value?.Name.Value is not null)
                                {
                                    strings.TryAdd(Helpers.Fnv1a32(a.Unk00.Value.Value.Name.Value), a.Unk00.Value.Value.Name.Value);
                                }
                            }
                            foreach (var worldid in resourceValue.Unk58)
                            {
                                if (strings.ContainsKey(worldid.FNVHash.Hash32) && strings.Any(kv => kv.Key == worldid.FNVHash.Hash32))
                                {
                                    ActivityEntity ent = new();
                                    if (strings.ContainsKey(resourceValue.FNVHash.Hash32))
                                    {
                                        ent.Name = strings[worldid.FNVHash.Hash32];
                                        ent.SubName = strings[resourceValue.FNVHash.Hash32];
                                        items.TryAdd(worldid.WorldID, ent);
                                    }
                                    else
                                    {
                                        ent.Name = strings[worldid.FNVHash.Hash32];
                                        ent.SubName = "";
                                        items.TryAdd(worldid.WorldID, ent);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //Log.Info(e.Message);
                    }
                }
            }
            return items;
        }
    }
}
