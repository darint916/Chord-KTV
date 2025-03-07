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
import type { QuizQuestionDto } from './QuizQuestionDto';
import {
    QuizQuestionDtoFromJSON,
    QuizQuestionDtoFromJSONTyped,
    QuizQuestionDtoToJSON,
    QuizQuestionDtoToJSONTyped,
} from './QuizQuestionDto';

/**
 * 
 * @export
 * @interface QuizResponseDto
 */
export interface QuizResponseDto {
    /**
     * 
     * @type {string}
     * @memberof QuizResponseDto
     */
    quizId?: string;
    /**
     * 
     * @type {string}
     * @memberof QuizResponseDto
     */
    songId?: string;
    /**
     * 
     * @type {number}
     * @memberof QuizResponseDto
     */
    difficulty?: number;
    /**
     * 
     * @type {Date}
     * @memberof QuizResponseDto
     */
    timestamp?: Date;
    /**
     * 
     * @type {Array<QuizQuestionDto>}
     * @memberof QuizResponseDto
     */
    questions?: Array<QuizQuestionDto> | null;
}

/**
 * Check if a given object implements the QuizResponseDto interface.
 */
export function instanceOfQuizResponseDto(value: object): value is QuizResponseDto {
    return true;
}

export function QuizResponseDtoFromJSON(json: any): QuizResponseDto {
    return QuizResponseDtoFromJSONTyped(json, false);
}

export function QuizResponseDtoFromJSONTyped(json: any, ignoreDiscriminator: boolean): QuizResponseDto {
    if (json == null) {
        return json;
    }
    return {
        
        'quizId': json['quizId'] == null ? undefined : json['quizId'],
        'songId': json['songId'] == null ? undefined : json['songId'],
        'difficulty': json['difficulty'] == null ? undefined : json['difficulty'],
        'timestamp': json['timestamp'] == null ? undefined : (new Date(json['timestamp'])),
        'questions': json['questions'] == null ? undefined : ((json['questions'] as Array<any>).map(QuizQuestionDtoFromJSON)),
    };
}

export function QuizResponseDtoToJSON(json: any): QuizResponseDto {
    return QuizResponseDtoToJSONTyped(json, false);
}

export function QuizResponseDtoToJSONTyped(value?: QuizResponseDto | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        
        'quizId': value['quizId'],
        'songId': value['songId'],
        'difficulty': value['difficulty'],
        'timestamp': value['timestamp'] == null ? undefined : ((value['timestamp']).toISOString()),
        'questions': value['questions'] == null ? undefined : ((value['questions'] as Array<any>).map(QuizQuestionDtoToJSON)),
    };
}

