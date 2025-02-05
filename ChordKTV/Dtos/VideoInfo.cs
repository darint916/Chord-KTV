namespace ChordKTV.Dtos;

using System;

public record VideoInfo
(
    string Title,
    string Channel,
    string Url,
    TimeSpan Duration
);
