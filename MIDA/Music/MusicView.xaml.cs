using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity.MARATHON;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;

namespace MIDA;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public void Load(FileHash fileHash, dynamic extra = null)
    {
        if (extra is Entity entity)
        {
            List<SSequenceAudioEvent> sounds = new();
            foreach (FileHash? resourceHash in entity.Components)
            {
                EntityComponent e = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is S80809F51 a)
                {
                    foreach (var d2ClassF1918080 in a.Array1)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is SSequenceAudioEvent b)
                        {
                            sounds.Add(b);
                        }
                    }
                    foreach (var d2ClassF1918080 in a.Array2)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is SSequenceAudioEvent b)
                        {
                            sounds.Add(b);
                        }
                    }
                }
            }
            WemsControl.Load(sounds);
            return;
        }
        else if (extra is Tag<SA4BC8080> cine)
        {
            List<WwiseSound> sounds = new();
            if (cine.TagData.Unk08.Count != 0)
                sounds.AddRange(cine.TagData.Unk08.Select(x => x.Sound));

            WemsControl.Load(sounds);
            return;
        }

        Tag<SMusicTemplate> music = FileResourcer.Get().GetSchemaTag<SMusicTemplate>(fileHash);

        if (music == null || music.TagData.Unk28.Count == 0)
            return;
        //if (music.TagData.Unk28.Count != 1)
        //{
        //    throw new NotImplementedException();
        //}

        // TODO
        //dynamic? resource = music.TagData.Unk28[0].Unk00.GetValue(music.GetReader());
        //if (resource is SF5458080 f5458080)
        //{
        //    WemsControl.Load(f5458080);
        //    EventsControl.Load(f5458080);
        //    FileHash sbhash = f5458080.MusicLoopSound.TagData.Soundbank.TagData.SoundBank.Hash;
        //    SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
        //}
        //else if (resource is SF7458080 res)
        //{
        //    WemsControl.Load(res);
        //    EventsControl.Load(res);
        //    if (res.AmbientMusicSet != null)
        //    {
        //        FileHash sbhash = res.AmbientMusicSet.TagData.Unk08[0].MusicLoopSound.TagData.Soundbank.TagData.SoundBank.Hash;
        //        SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
        //    }
        //}
        //else if (resource is SUnkMusicE6BF8080 rese6bf)
        //{
        //    EventsControl.Load(rese6bf, music.TagData.MusicTemplateName?.Value);
        //}
        //else
        //{
        //    if (resource is not SF7458080)
        //    {
        //        //throw new NotImplementedException();
        //        Log.Error($"Music Resource F7458080 Not Implemented");
        //    }
        //}
    }


    // This is bit of a hack since music stuff isnt actually a part of TagListView so gotta jump through some hoops to
    // export fully. (At least I don't think there's a way of doing it right in TagListView?)
    public void Export(ExportInfo info)
    {
        WemsControl.Export();
    }
}
