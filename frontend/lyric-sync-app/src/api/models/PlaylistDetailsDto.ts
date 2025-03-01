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
import type { VideoInfo } from './VideoInfo';
import {
    VideoInfoFromJSON,
    VideoInfoFromJSONTyped,
    VideoInfoToJSON,
    VideoInfoToJSONTyped,
} from './VideoInfo';

/**
 * 
 * @export
 * @interface PlaylistDetailsDto
 */
export interface PlaylistDetailsDto {
    /**
     * 
     * @type {string}
     * @memberof PlaylistDetailsDto
     */
    playlistTitle?: string | null;
    /**
     * 
     * @type {Array<VideoInfo>}
     * @memberof PlaylistDetailsDto
     */
    videos?: Array<VideoInfo> | null;
}

/**
 * Check if a given object implements the PlaylistDetailsDto interface.
 */
export function instanceOfPlaylistDetailsDto(value: object): value is PlaylistDetailsDto {
    return true;
}

export function PlaylistDetailsDtoFromJSON(json: any): PlaylistDetailsDto {
    return PlaylistDetailsDtoFromJSONTyped(json, false);
}

export function PlaylistDetailsDtoFromJSONTyped(json: any, ignoreDiscriminator: boolean): PlaylistDetailsDto {
    if (json == null) {
        return json;
    }
    return {
        
        'playlistTitle': json['playlistTitle'] == null ? undefined : json['playlistTitle'],
        'videos': json['videos'] == null ? undefined : ((json['videos'] as Array<any>).map(VideoInfoFromJSON)),
    };
}

export function PlaylistDetailsDtoToJSON(json: any): PlaylistDetailsDto {
    return PlaylistDetailsDtoToJSONTyped(json, false);
}

export function PlaylistDetailsDtoToJSONTyped(value?: PlaylistDetailsDto | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        
        'playlistTitle': value['playlistTitle'],
        'videos': value['videos'] == null ? undefined : ((value['videos'] as Array<any>).map(VideoInfoToJSON)),
    };
}

