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


import * as runtime from '../runtime';
import type {
  FullSongRequestDto,
  Song,
  TranslationRequestDto,
} from '../models/index';
import {
    FullSongRequestDtoFromJSON,
    FullSongRequestDtoToJSON,
    SongFromJSON,
    SongToJSON,
    TranslationRequestDtoFromJSON,
    TranslationRequestDtoToJSON,
} from '../models/index';

export interface ApiAlbumAlbumNameGetRequest {
    albumName: string;
    artist?: string;
}

export interface ApiDatabaseSongGetRequest {
    title: string;
    artist: string;
    albumName?: string;
}

export interface ApiDatabaseSongPostRequest {
    song?: Song;
}

export interface ApiLyricsLrcTranslationPostRequest {
    translationRequestDto?: TranslationRequestDto;
}

export interface ApiLyricsLrclibSearchGetRequest {
    searchType: string;
    title?: string;
    artist?: string;
    albumName?: string;
    duration?: number;
    qString?: string;
}

export interface ApiSongsGeniusSearchBatchPostRequest {
    forceRefresh?: boolean;
    body?: any | null;
}

export interface ApiSongsGeniusSearchGetRequest {
    title?: string;
    artist?: string;
    lyrics?: string;
    forceRefresh?: boolean;
}

export interface ApiSongsSearchPostRequest {
    fullSongRequestDto?: FullSongRequestDto;
}

export interface ApiYoutubePlaylistsPlaylistIdGetRequest {
    playlistId: string;
    shuffle?: boolean;
}

/**
 * 
 */
export class SongApi extends runtime.BaseAPI {

    /**
     */
    async apiAlbumAlbumNameGetRaw(requestParameters: ApiAlbumAlbumNameGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['albumName'] == null) {
            throw new runtime.RequiredError(
                'albumName',
                'Required parameter "albumName" was null or undefined when calling apiAlbumAlbumNameGet().'
            );
        }

        const queryParameters: any = {};

        if (requestParameters['artist'] != null) {
            queryParameters['artist'] = requestParameters['artist'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/album/{albumName}`.replace(`{${"albumName"}}`, encodeURIComponent(String(requestParameters['albumName']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiAlbumAlbumNameGet(requestParameters: ApiAlbumAlbumNameGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiAlbumAlbumNameGetRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiDatabaseSongGetRaw(requestParameters: ApiDatabaseSongGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['title'] == null) {
            throw new runtime.RequiredError(
                'title',
                'Required parameter "title" was null or undefined when calling apiDatabaseSongGet().'
            );
        }

        if (requestParameters['artist'] == null) {
            throw new runtime.RequiredError(
                'artist',
                'Required parameter "artist" was null or undefined when calling apiDatabaseSongGet().'
            );
        }

        const queryParameters: any = {};

        if (requestParameters['title'] != null) {
            queryParameters['title'] = requestParameters['title'];
        }

        if (requestParameters['artist'] != null) {
            queryParameters['artist'] = requestParameters['artist'];
        }

        if (requestParameters['albumName'] != null) {
            queryParameters['albumName'] = requestParameters['albumName'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/database/song`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiDatabaseSongGet(requestParameters: ApiDatabaseSongGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiDatabaseSongGetRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiDatabaseSongPostRaw(requestParameters: ApiDatabaseSongPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/database/song`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: SongToJSON(requestParameters['song']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiDatabaseSongPost(requestParameters: ApiDatabaseSongPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiDatabaseSongPostRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiHealthGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/health`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiHealthGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiHealthGetRaw(initOverrides);
    }

    /**
     */
    async apiLyricsLrcTranslationPostRaw(requestParameters: ApiLyricsLrcTranslationPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/lyrics/lrc/translation`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: TranslationRequestDtoToJSON(requestParameters['translationRequestDto']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiLyricsLrcTranslationPost(requestParameters: ApiLyricsLrcTranslationPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiLyricsLrcTranslationPostRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiLyricsLrclibSearchGetRaw(requestParameters: ApiLyricsLrclibSearchGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['searchType'] == null) {
            throw new runtime.RequiredError(
                'searchType',
                'Required parameter "searchType" was null or undefined when calling apiLyricsLrclibSearchGet().'
            );
        }

        const queryParameters: any = {};

        if (requestParameters['searchType'] != null) {
            queryParameters['searchType'] = requestParameters['searchType'];
        }

        if (requestParameters['title'] != null) {
            queryParameters['title'] = requestParameters['title'];
        }

        if (requestParameters['artist'] != null) {
            queryParameters['artist'] = requestParameters['artist'];
        }

        if (requestParameters['albumName'] != null) {
            queryParameters['albumName'] = requestParameters['albumName'];
        }

        if (requestParameters['duration'] != null) {
            queryParameters['duration'] = requestParameters['duration'];
        }

        if (requestParameters['qString'] != null) {
            queryParameters['qString'] = requestParameters['qString'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/lyrics/lrclib/search`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiLyricsLrclibSearchGet(requestParameters: ApiLyricsLrclibSearchGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiLyricsLrclibSearchGetRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiSongsGeniusSearchBatchPostRaw(requestParameters: ApiSongsGeniusSearchBatchPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        if (requestParameters['forceRefresh'] != null) {
            queryParameters['forceRefresh'] = requestParameters['forceRefresh'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/songs/genius/search/batch`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: requestParameters['body'] as any,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiSongsGeniusSearchBatchPost(requestParameters: ApiSongsGeniusSearchBatchPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiSongsGeniusSearchBatchPostRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiSongsGeniusSearchGetRaw(requestParameters: ApiSongsGeniusSearchGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        if (requestParameters['title'] != null) {
            queryParameters['title'] = requestParameters['title'];
        }

        if (requestParameters['artist'] != null) {
            queryParameters['artist'] = requestParameters['artist'];
        }

        if (requestParameters['lyrics'] != null) {
            queryParameters['lyrics'] = requestParameters['lyrics'];
        }

        if (requestParameters['forceRefresh'] != null) {
            queryParameters['forceRefresh'] = requestParameters['forceRefresh'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/songs/genius/search`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiSongsGeniusSearchGet(requestParameters: ApiSongsGeniusSearchGetRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiSongsGeniusSearchGetRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiSongsSearchPostRaw(requestParameters: ApiSongsSearchPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/songs/search`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: FullSongRequestDtoToJSON(requestParameters['fullSongRequestDto']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiSongsSearchPost(requestParameters: ApiSongsSearchPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiSongsSearchPostRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiYoutubePlaylistsPlaylistIdGetRaw(requestParameters: ApiYoutubePlaylistsPlaylistIdGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        if (requestParameters['playlistId'] == null) {
            throw new runtime.RequiredError(
                'playlistId',
                'Required parameter "playlistId" was null or undefined when calling apiYoutubePlaylistsPlaylistIdGet().'
            );
        }

        const queryParameters: any = {};

        if (requestParameters['shuffle'] != null) {
            queryParameters['shuffle'] = requestParameters['shuffle'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/youtube/playlists/{playlistId}`.replace(`{${"playlistId"}}`, encodeURIComponent(String(requestParameters['playlistId']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiYoutubePlaylistsPlaylistIdGet(requestParameters: ApiYoutubePlaylistsPlaylistIdGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiYoutubePlaylistsPlaylistIdGetRaw(requestParameters, initOverrides);
    }

}
