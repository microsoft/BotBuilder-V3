/**
 * @module botframework-connector
 */
/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
import { AuthenticationConfiguration, ValidateClaims } from './authenticationConfiguration';
import { SkillValidation } from './skillValidation';
import { JwtTokenValidation } from './jwtTokenValidation';
import { Claim } from './claimsIdentity';

export class DefaultAuthenticationConfiguration extends AuthenticationConfiguration {
    private allowedCallers: string[];

    /**
     * General configuration settings for authentication.
     * @param allowedCallers  
     * @param validateClaims Function that validates a list of Claims and should throw an exception if the validation fails.
     */
    constructor(allowedCallers: string[]) { 
            super()
            if (!allowedCallers || allowedCallers.length == 0) {
                throw new Error(`DefaultAuthenticationConfiguration allowedCallers must contain at least one element of '*' or valid MicrosoftAppId(s).`);
            }
            this.allowedCallers = allowedCallers;
        }

        public validateClaims: ValidateClaims = async (claims: Claim[]) => {
            if (!claims || claims.length < 1) {
                throw new Error(`DefaultAuthenticationConfiguration.validateClaims.claims parameter must contain at least one element.`);
            }
            // If allowedCallers contains '*' we allow all callers
            if (SkillValidation.isSkillClaim(claims)) {
                
                if(this.allowedCallers[0] === '*') {
                    return;
                }
                // Check that the appId claim in the skill request is in the list of skills configured for this bot.
                const appId = JwtTokenValidation.getAppIdFromClaims(claims);
                if (this.allowedCallers.includes(appId)) {
                    return;
                }
                throw new Error(`Received a request from a bot with an app ID of "${ appId }". To enable requests from this caller, add the app ID to your configuration file.`);
            }
            throw new Error(`DefaultAuthenticationConfiguration.validateClaims called without a Skill claim in claims.`);
        };
}