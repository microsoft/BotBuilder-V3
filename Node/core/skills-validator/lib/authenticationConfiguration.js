"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var AuthenticationConfiguration = (function () {
    function AuthenticationConfiguration(requiredEndorsements, validateClaims) {
        if (requiredEndorsements === void 0) { requiredEndorsements = []; }
        this.requiredEndorsements = requiredEndorsements;
        this.validateClaims = validateClaims;
    }
    return AuthenticationConfiguration;
}());
exports.AuthenticationConfiguration = AuthenticationConfiguration;
