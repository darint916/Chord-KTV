/* tslint:disable */
/* eslint-disable */
/**
 * ChordKTV
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { mapValues } from '../runtime';
import type { GeniusMetaDataDto } from './GeniusMetaDataDto';
import {
    GeniusMetaDataDtoFromJSON,
    GeniusMetaDataDtoFromJSONTyped,
    GeniusMetaDataDtoToJSON,
    GeniusMetaDataDtoToJSONTyped,
} from './GeniusMetaDataDto';

/**
 * 
 * @export
 * @interface FullSongResponseDto
 */
export interface FullSongResponseDto {
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    id?: string;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    title?: string | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof FullSongResponseDto
     */
    alternateTitles?: Array<string> | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    artist?: string | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof FullSongResponseDto
     */
    featuredArtists?: Array<string> | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof FullSongResponseDto
     */
    albumNames?: Array<string> | null;
    /**
     * 
     * @type {Date}
     * @memberof FullSongResponseDto
     */
    releaseDate?: Date | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    genre?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    plainLyrics?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    lrcLyrics?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    lrcRomanizedLyrics?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    lrcTranslatedLyrics?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FullSongResponseDto
     */
    youTubeUrl?: string | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof FullSongResponseDto
     */
    alternateYoutubeUrls?: Array<string> | null;
    /**
     * 
     * @type {GeniusMetaDataDto}
     * @memberof FullSongResponseDto
     */
    geniusMetaData?: GeniusMetaDataDto;
}

/**
 * Check if a given object implements the FullSongResponseDto interface.
 */
export function instanceOfFullSongResponseDto(value: object): value is FullSongResponseDto {
    return true;
}

export function FullSongResponseDtoFromJSON(json: any): FullSongResponseDto {
    return FullSongResponseDtoFromJSONTyped(json, false);
}

export function FullSongResponseDtoFromJSONTyped(json: any, ignoreDiscriminator: boolean): FullSongResponseDto {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'] == null ? undefined : json['id'],
        'title': json['title'] == null ? undefined : json['title'],
        'alternateTitles': json['alternateTitles'] == null ? undefined : json['alternateTitles'],
        'artist': json['artist'] == null ? undefined : json['artist'],
        'featuredArtists': json['featuredArtists'] == null ? undefined : json['featuredArtists'],
        'albumNames': json['albumNames'] == null ? undefined : json['albumNames'],
        'releaseDate': json['releaseDate'] == null ? undefined : (new Date(json['releaseDate'])),
        'genre': json['genre'] == null ? undefined : json['genre'],
        'plainLyrics': json['plainLyrics'] == null ? undefined : json['plainLyrics'],
        'lrcLyrics': json['lrcLyrics'] == null ? undefined : json['lrcLyrics'],
        'lrcRomanizedLyrics': json['lrcRomanizedLyrics'] == null ? undefined : json['lrcRomanizedLyrics'],
        'lrcTranslatedLyrics': json['lrcTranslatedLyrics'] == null ? undefined : json['lrcTranslatedLyrics'],
        'youTubeUrl': json['youTubeUrl'] == null ? undefined : json['youTubeUrl'],
        'alternateYoutubeUrls': json['alternateYoutubeUrls'] == null ? undefined : json['alternateYoutubeUrls'],
        'geniusMetaData': json['geniusMetaData'] == null ? undefined : GeniusMetaDataDtoFromJSON(json['geniusMetaData']),
    };
}

export function FullSongResponseDtoToJSON(json: any): FullSongResponseDto {
    return FullSongResponseDtoToJSONTyped(json, false);
}

export function FullSongResponseDtoToJSONTyped(value?: FullSongResponseDto | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        
        'id': value['id'],
        'title': value['title'],
        'alternateTitles': value['alternateTitles'],
        'artist': value['artist'],
        'featuredArtists': value['featuredArtists'],
        'albumNames': value['albumNames'],
        'releaseDate': value['releaseDate'] == null ? undefined : ((value['releaseDate'] as any).toISOString().substring(0,10)),
        'genre': value['genre'],
        'plainLyrics': value['plainLyrics'],
        'lrcLyrics': value['lrcLyrics'],
        'lrcRomanizedLyrics': value['lrcRomanizedLyrics'],
        'lrcTranslatedLyrics': value['lrcTranslatedLyrics'],
        'youTubeUrl': value['youTubeUrl'],
        'alternateYoutubeUrls': value['alternateYoutubeUrls'],
        'geniusMetaData': GeniusMetaDataDtoToJSON(value['geniusMetaData']),
    };
}

