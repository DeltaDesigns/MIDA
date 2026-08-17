namespace Tiger.Schema.Audio;

public class Dialogue : Tag<SDialogueTable>
{
    public Dialogue(FileHash hash) : base(hash)
    {

    }

    /// <summary>
    /// Generates a nested list of different sequences of audio, collapsing redundant structures.
    /// </summary>
    /// <returns>A dynamic list of S3FAA8080, in lists of their sequence and structure.</returns>
    public List<dynamic?> Load()
    {
        List<dynamic?> result = new();
        foreach (var entry1 in _tag.Unk18)
        {
            foreach (var u in _tag.Unk18)
            {
                var entry = u.Unk08.GetValue(GetReader());
                switch (entry)
                {
                    case S8080AA35:
                        List<dynamic?> res2d = Collapse35AA(entry);
                        if (res2d.Count > 0)
                        {
                            result.Add(res2d.Count > 1 ? res2d : res2d[0]);
                        }
                        break;
                    case S8080AA38:
                        List<dynamic?> res2a = Collapse38AA(entry);
                        if (res2a.Count > 0)
                        {
                            result.Add(res2a.Count > 1 ? res2a : res2a[0]);
                        }
                        break;
                    case S8080AA3F:
                        result.Add(entry);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        return result;
    }

    private List<dynamic?> Collapse35AA(S8080AA35 entry)
    {
        List<dynamic?> sounds = new();
        foreach (dynamic? e in entry.Unk30.Select(u => u.Unk40.GetValue(GetReader())))
        {
            switch (e)
            {
                case S8080AA38:
                    List<dynamic?> result = Collapse38AA(e);
                    if (result.Count > 0)
                    {
                        sounds.Add(result.Count > 1 ? result : result[0]);
                    }
                    break;
                case S8080AA3F:
                    sounds.Add(e);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }

    private List<dynamic?> Collapse38AA(S8080AA38 entry)
    {
        List<dynamic?> sounds = new();

        // todo GetReader() here is wrong
        // todo do a performance comparison of using the manual GetReader vs loading automatically and ignoring it

        foreach (var e in entry.Unk28.Select(u => u.Unk20.GetValue(GetReader())))
        {
            switch (e)
            {
                case S8080AA38:
                    List<dynamic?> result = Collapse38AA(e);
                    if (result.Count > 0)
                    {
                        sounds.Add(result.Count > 1 ? result : result[0]);
                    }
                    break;
                case S8080AA35:
                    List<dynamic?> result2 = Collapse35AA(e);
                    if (result2.Count > 0)
                    {
                        sounds.Add(result2.Count > 1 ? result2 : result2[0]);
                    }
                    break;
                case S8080AA3F:
                    sounds.Add(e);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }
}
