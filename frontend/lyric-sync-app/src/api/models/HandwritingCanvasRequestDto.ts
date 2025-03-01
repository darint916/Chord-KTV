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
import type { LanguageCode } from './LanguageCode';
import {
    LanguageCodeFromJSON,
    LanguageCodeFromJSONTyped,
    LanguageCodeToJSON,
    LanguageCodeToJSONTyped,
} from './LanguageCode';

/**
 * 
 * @export
 * @interface HandwritingCanvasRequestDto
 */
export interface HandwritingCanvasRequestDto {
    /**
     * 
     * @type {string}
     * @memberof HandwritingCanvasRequestDto
     */
    image?: string | null;
    /**
     * 
     * @type {LanguageCode}
     * @memberof HandwritingCanvasRequestDto
     */
    language?: LanguageCode;
    /**
     * 
     * @type {string}
     * @memberof HandwritingCanvasRequestDto
     */
    expectedMatch?: string | null;
}



/**
 * Check if a given object implements the HandwritingCanvasRequestDto interface.
 */
export function instanceOfHandwritingCanvasRequestDto(value: object): value is HandwritingCanvasRequestDto {
    return true;
}

export function HandwritingCanvasRequestDtoFromJSON(json: any): HandwritingCanvasRequestDto {
    return HandwritingCanvasRequestDtoFromJSONTyped(json, false);
}

export function HandwritingCanvasRequestDtoFromJSONTyped(json: any, ignoreDiscriminator: boolean): HandwritingCanvasRequestDto {
    if (json == null) {
        return json;
    }
    return {
        
        'image': json['image'] == null ? undefined : json['image'],
        'language': json['language'] == null ? undefined : LanguageCodeFromJSON(json['language']),
        'expectedMatch': json['expectedMatch'] == null ? undefined : json['expectedMatch'],
    };
}

export function HandwritingCanvasRequestDtoToJSON(json: any): HandwritingCanvasRequestDto {
    return HandwritingCanvasRequestDtoToJSONTyped(json, false);
}

export function HandwritingCanvasRequestDtoToJSONTyped(value?: HandwritingCanvasRequestDto | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        
        'image': value['image'],
        'language': LanguageCodeToJSON(value['language']),
        'expectedMatch': value['expectedMatch'],
    };
}

