namespace ChordKTV.Dtos;

using System;

public record VideoInfo
(
    string Title,
    string Artist,
    string Url,
    TimeSpan Duration
);
