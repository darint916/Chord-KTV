/* tslint:disable */
/* eslint-disable */
/**
 * ChordKTV API
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { mapValues } from '../runtime';
import type { GeniusAlbumDto } from './GeniusAlbumDto';
import {
    GeniusAlbumDtoFromJSON,
    GeniusAlbumDtoFromJSONTyped,
    GeniusAlbumDtoToJSON,
    GeniusAlbumDtoToJSONTyped,
} from './GeniusAlbumDto';

/**
 * 
 * @export
 * @interface GeniusResultDto
 */
export interface GeniusResultDto {
    /**
     * 
     * @type {number}
     * @memberof GeniusResultDto
     */
    id: number;
    /**
     * 
     * @type {string}
     * @memberof GeniusResultDto
     */
    title: string;
    /**
     * 
     * @type {string}
     * @memberof GeniusResultDto
     */
    headerImageUrl?: string;
    /**
     * 
     * @type {string}
     * @memberof GeniusResultDto
     */
    songArtImageUrl?: string;
    /**
     * 
     * @type {string}
     * @memberof GeniusResultDto
     */
    primaryArtistNames: string;
    /**
     * 
     * @type {GeniusAlbumDto}
     * @memberof GeniusResultDto
     */
    album?: GeniusAlbumDto | null;
}

/**
 * Check if a given object implements the GeniusResultDto interface.
 */
export function instanceOfGeniusResultDto(value: object): value is GeniusResultDto {
    if (!('id' in value) || value['id'] === undefined) return false;
    if (!('title' in value) || value['title'] === undefined) return false;
    if (!('primaryArtistNames' in value) || value['primaryArtistNames'] === undefined) return false;
    return true;
}

export function GeniusResultDtoFromJSON(json: any): GeniusResultDto {
    return GeniusResultDtoFromJSONTyped(json, false);
}

export function GeniusResultDtoFromJSONTyped(json: any, ignoreDiscriminator: boolean): GeniusResultDto {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'],
        'title': json['title'],
        'headerImageUrl': json['header_image_url'] == null ? undefined : json['header_image_url'],
        'songArtImageUrl': json['song_art_image_url'] == null ? undefined : json['song_art_image_url'],
        'primaryArtistNames': json['primary_artist_names'],
        'album': json['album'] == null ? undefined : GeniusAlbumDtoFromJSON(json['album']),
    };
}

export function GeniusResultDtoToJSON(json: any): GeniusResultDto {
    return GeniusResultDtoToJSONTyped(json, false);
}

export function GeniusResultDtoToJSONTyped(value?: GeniusResultDto | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        
        'id': value['id'],
        'title': value['title'],
        'header_image_url': value['headerImageUrl'],
        'song_art_image_url': value['songArtImageUrl'],
        'primary_artist_names': value['primaryArtistNames'],
        'album': GeniusAlbumDtoToJSON(value['album']),
    };
}

