namespace ChordKTV.Models.ApiModels;

using System.Collections.Generic;

public record PlaylistDetailsDto(string PlaylistTitle, List<VideoInfo> Videos);
