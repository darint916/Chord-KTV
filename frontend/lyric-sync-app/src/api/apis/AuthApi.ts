/* tslint:disable */
/* eslint-disable */
/**
 * ChordKTV
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0
 * 
 *
 * NOTE: This class is manually created to match the pattern of auto-generated OpenAPI stubs.
 */

import * as runtime from '../runtime';

/**
 * 
 */
export class AuthApi extends runtime.BaseAPI {

    /**
     * Google Authentication
     */
    async apiAuthGooglePostRaw(
        credential: string,
        initOverrides?: RequestInit | runtime.InitOverrideFunction
    ): Promise<runtime.ApiResponse<void>> {
        const headerParameters: runtime.HTTPHeaders = {};
        
        if (credential !== undefined && credential !== null) {
            headerParameters['Authorization'] = `Bearer ${credential}`;
        }

        const response = await this.request({
            path: `/api/auth/google`,
            method: 'POST',
            headers: headerParameters,
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     * Google Authentication
     */
    async apiAuthGooglePost(
        credential: string,
        initOverrides?: RequestInit | runtime.InitOverrideFunction
    ): Promise<void> {
        await this.apiAuthGooglePostRaw(credential, initOverrides);
    }
} 