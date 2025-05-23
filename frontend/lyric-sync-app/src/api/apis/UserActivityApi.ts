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


import * as runtime from '../runtime';
import type {
  LanguageCode,
  LearnedWordDto,
  ProblemDetails,
  UserHandwritingResultDto,
  UserPlaylistActivityDto,
  UserPlaylistActivityFavoriteRequestDto,
  UserQuizResultDto,
  UserSongActivityDto,
  UserSongActivityFavoriteRequestDto,
} from '../models/index';
import {
    LanguageCodeFromJSON,
    LanguageCodeToJSON,
    LearnedWordDtoFromJSON,
    LearnedWordDtoToJSON,
    ProblemDetailsFromJSON,
    ProblemDetailsToJSON,
    UserHandwritingResultDtoFromJSON,
    UserHandwritingResultDtoToJSON,
    UserPlaylistActivityDtoFromJSON,
    UserPlaylistActivityDtoToJSON,
    UserPlaylistActivityFavoriteRequestDtoFromJSON,
    UserPlaylistActivityFavoriteRequestDtoToJSON,
    UserQuizResultDtoFromJSON,
    UserQuizResultDtoToJSON,
    UserSongActivityDtoFromJSON,
    UserSongActivityDtoToJSON,
    UserSongActivityFavoriteRequestDtoFromJSON,
    UserSongActivityFavoriteRequestDtoToJSON,
} from '../models/index';

export interface ApiUserActivityFavoritePlaylistPatchRequest {
    userPlaylistActivityFavoriteRequestDto?: UserPlaylistActivityFavoriteRequestDto;
}

export interface ApiUserActivityFavoriteSongPatchRequest {
    userSongActivityFavoriteRequestDto?: UserSongActivityFavoriteRequestDto;
}

export interface ApiUserActivityHandwritingGetRequest {
    language?: LanguageCode;
}

export interface ApiUserActivityHandwritingPostRequest {
    userHandwritingResultDto?: UserHandwritingResultDto;
}

export interface ApiUserActivityLearnedWordPostRequest {
    learnedWordDto?: LearnedWordDto;
}

export interface ApiUserActivityLearnedWordsGetRequest {
    language?: LanguageCode;
}

export interface ApiUserActivityPlaylistPostRequest {
    userPlaylistActivityDto?: UserPlaylistActivityDto;
}

export interface ApiUserActivityQuizPostRequest {
    userQuizResultDto?: UserQuizResultDto;
}

export interface ApiUserActivityQuizzesGetRequest {
    language?: LanguageCode;
}

export interface ApiUserActivitySongPostRequest {
    userSongActivityDto?: UserSongActivityDto;
}

/**
 * 
 */
export class UserActivityApi extends runtime.BaseAPI {

    /**
     */
    async apiUserActivityFavoritePlaylistPatchRaw(requestParameters: ApiUserActivityFavoritePlaylistPatchRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserPlaylistActivityDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/favorite/playlist`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: UserPlaylistActivityFavoriteRequestDtoToJSON(requestParameters['userPlaylistActivityFavoriteRequestDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserPlaylistActivityDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityFavoritePlaylistPatch(requestParameters: ApiUserActivityFavoritePlaylistPatchRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserPlaylistActivityDto> {
        const response = await this.apiUserActivityFavoritePlaylistPatchRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityFavoritePlaylistsGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserPlaylistActivityDto>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/favorite/playlists`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserPlaylistActivityDtoFromJSON));
    }

    /**
     */
    async apiUserActivityFavoritePlaylistsGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserPlaylistActivityDto>> {
        const response = await this.apiUserActivityFavoritePlaylistsGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityFavoriteSongPatchRaw(requestParameters: ApiUserActivityFavoriteSongPatchRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserSongActivityDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/favorite/song`,
            method: 'PATCH',
            headers: headerParameters,
            query: queryParameters,
            body: UserSongActivityFavoriteRequestDtoToJSON(requestParameters['userSongActivityFavoriteRequestDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserSongActivityDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityFavoriteSongPatch(requestParameters: ApiUserActivityFavoriteSongPatchRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserSongActivityDto> {
        const response = await this.apiUserActivityFavoriteSongPatchRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityFavoriteSongsGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserSongActivityDto>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/favorite/songs`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserSongActivityDtoFromJSON));
    }

    /**
     */
    async apiUserActivityFavoriteSongsGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserSongActivityDto>> {
        const response = await this.apiUserActivityFavoriteSongsGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityFullGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<any>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/full`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        if (this.isJsonMime(response.headers.get('content-type'))) {
            return new runtime.JSONApiResponse<any>(response);
        } else {
            return new runtime.TextApiResponse(response) as any;
        }
    }

    /**
     */
    async apiUserActivityFullGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<any> {
        const response = await this.apiUserActivityFullGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityHandwritingGetRaw(requestParameters: ApiUserActivityHandwritingGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserHandwritingResultDto>>> {
        const queryParameters: any = {};

        if (requestParameters['language'] != null) {
            queryParameters['language'] = requestParameters['language'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/handwriting`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserHandwritingResultDtoFromJSON));
    }

    /**
     */
    async apiUserActivityHandwritingGet(requestParameters: ApiUserActivityHandwritingGetRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserHandwritingResultDto>> {
        const response = await this.apiUserActivityHandwritingGetRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityHandwritingPostRaw(requestParameters: ApiUserActivityHandwritingPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserHandwritingResultDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/handwriting`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: UserHandwritingResultDtoToJSON(requestParameters['userHandwritingResultDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserHandwritingResultDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityHandwritingPost(requestParameters: ApiUserActivityHandwritingPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserHandwritingResultDto> {
        const response = await this.apiUserActivityHandwritingPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityLearnedWordPostRaw(requestParameters: ApiUserActivityLearnedWordPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<LearnedWordDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/learned-word`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: LearnedWordDtoToJSON(requestParameters['learnedWordDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => LearnedWordDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityLearnedWordPost(requestParameters: ApiUserActivityLearnedWordPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<LearnedWordDto> {
        const response = await this.apiUserActivityLearnedWordPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityLearnedWordsGetRaw(requestParameters: ApiUserActivityLearnedWordsGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<LearnedWordDto>>> {
        const queryParameters: any = {};

        if (requestParameters['language'] != null) {
            queryParameters['language'] = requestParameters['language'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/learned-words`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(LearnedWordDtoFromJSON));
    }

    /**
     */
    async apiUserActivityLearnedWordsGet(requestParameters: ApiUserActivityLearnedWordsGetRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<LearnedWordDto>> {
        const response = await this.apiUserActivityLearnedWordsGetRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityPlaylistPostRaw(requestParameters: ApiUserActivityPlaylistPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserPlaylistActivityDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/playlist`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: UserPlaylistActivityDtoToJSON(requestParameters['userPlaylistActivityDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserPlaylistActivityDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityPlaylistPost(requestParameters: ApiUserActivityPlaylistPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserPlaylistActivityDto> {
        const response = await this.apiUserActivityPlaylistPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityPlaylistsGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserPlaylistActivityDto>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/playlists`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserPlaylistActivityDtoFromJSON));
    }

    /**
     */
    async apiUserActivityPlaylistsGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserPlaylistActivityDto>> {
        const response = await this.apiUserActivityPlaylistsGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityQuizPostRaw(requestParameters: ApiUserActivityQuizPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserQuizResultDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/quiz`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: UserQuizResultDtoToJSON(requestParameters['userQuizResultDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserQuizResultDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivityQuizPost(requestParameters: ApiUserActivityQuizPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserQuizResultDto> {
        const response = await this.apiUserActivityQuizPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivityQuizzesGetRaw(requestParameters: ApiUserActivityQuizzesGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserQuizResultDto>>> {
        const queryParameters: any = {};

        if (requestParameters['language'] != null) {
            queryParameters['language'] = requestParameters['language'];
        }

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/quizzes`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserQuizResultDtoFromJSON));
    }

    /**
     */
    async apiUserActivityQuizzesGet(requestParameters: ApiUserActivityQuizzesGetRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserQuizResultDto>> {
        const response = await this.apiUserActivityQuizzesGetRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivitySongPostRaw(requestParameters: ApiUserActivitySongPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<UserSongActivityDto>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/song`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: UserSongActivityDtoToJSON(requestParameters['userSongActivityDto']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => UserSongActivityDtoFromJSON(jsonValue));
    }

    /**
     */
    async apiUserActivitySongPost(requestParameters: ApiUserActivitySongPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<UserSongActivityDto> {
        const response = await this.apiUserActivitySongPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     */
    async apiUserActivitySongsGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<UserSongActivityDto>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        if (this.configuration && this.configuration.apiKey) {
            headerParameters["Authorization"] = await this.configuration.apiKey("Authorization"); // Bearer authentication
        }

        const response = await this.request({
            path: `/api/user/activity/songs`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(UserSongActivityDtoFromJSON));
    }

    /**
     */
    async apiUserActivitySongsGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<UserSongActivityDto>> {
        const response = await this.apiUserActivitySongsGetRaw(initOverrides);
        return await response.value();
    }

}
