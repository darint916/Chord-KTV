using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ChordKTV.Dtos;

//Follows ISO 639 Language Codes
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LanguageCode
{
    [Description("Unknown")] UNK,
    [Description("Afrikaans")] AF,
    [Description("Arabic")] AR,
    [Description("Bulgarian")] BG,
    [Description("Bengali")] BN,
    [Description("Catalan")] CA,
    [Description("Czech")] CS,
    [Description("Danish")] DA,
    [Description("German")] DE,
    [Description("Greek")] EL,
    [Description("English")] EN,
    [Description("Spanish")] ES,
    [Description("Estonian")] ET,
    [Description("Persian")] FA,
    [Description("Finnish")] FI,
    [Description("French")] FR,
    [Description("Gujarati")] GU,
    [Description("Hebrew")] HE,
    [Description("Hindi")] HI,
    [Description("Croatian")] HR,
    [Description("Hungarian")] HU,
    [Description("Indonesian")] ID,
    [Description("Italian")] IT,
    [Description("Japanese")] JA,
    [Description("Korean")] KO,
    [Description("Lithuanian")] LT,
    [Description("Latvian")] LV,
    [Description("Malay")] MS,
    [Description("Dutch")] NL,
    [Description("Norwegian")] NO,
    [Description("Polish")] PL,
    [Description("Portuguese")] PT,
    [Description("Romanian")] RO,
    [Description("Russian")] RU,
    [Description("Slovak")] SK,
    [Description("Slovenian")] SL,
    [Description("Serbian")] SR,
    [Description("Swedish")] SV,
    [Description("Tamil")] TA,
    [Description("Telugu")] TE,
    [Description("Thai")] TH,
    [Description("Turkish")] TR,
    [Description("Ukrainian")] UK,
    [Description("Vietnamese")] VI,
    [Description("Chinese")] ZH
}
