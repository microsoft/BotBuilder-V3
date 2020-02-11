"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var jsonwebtoken_1 = require("jsonwebtoken");
var authenticationConstants_1 = require("./authenticationConstants");
var authenticationConfiguration_1 = require("./authenticationConfiguration");
var jwtTokenExtractor_1 = require("./jwtTokenExtractor");
var EmulatorValidation;
(function (EmulatorValidation) {
    EmulatorValidation.ToBotFromEmulatorTokenValidationParameters = {
        issuer: [
            'https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/',
            'https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0',
            'https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/',
            'https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0',
            'https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/',
            'https://sts.windows.net/cab8a31a-1906-4287-a0d8-4eef66b95f6e/',
            'https://login.microsoftonline.us/cab8a31a-1906-4287-a0d8-4eef66b95f6e/v2.0',
        ],
        audience: undefined,
        clockTolerance: 5 * 60,
        ignoreExpiration: false
    };
    function isTokenFromEmulator(authHeader) {
        if (!authHeader) {
            return false;
        }
        var parts = authHeader.split(' ');
        if (parts.length !== 2) {
            return false;
        }
        var authScheme = parts[0];
        var bearerToken = parts[1];
        if (authScheme !== 'Bearer') {
            return false;
        }
        var token = jsonwebtoken_1.decode(bearerToken, { complete: true });
        if (!token) {
            return false;
        }
        var issuer = token.payload.iss;
        if (!issuer) {
            return false;
        }
        if (EmulatorValidation.ToBotFromEmulatorTokenValidationParameters.issuer && EmulatorValidation.ToBotFromEmulatorTokenValidationParameters.issuer.indexOf(issuer) === -1) {
            return false;
        }
        return true;
    }
    EmulatorValidation.isTokenFromEmulator = isTokenFromEmulator;
    function authenticateEmulatorToken(authHeader, credentials, channelId, authConfig) {
        if (authConfig === void 0) { authConfig = new authenticationConfiguration_1.AuthenticationConfiguration(); }
        return __awaiter(this, void 0, void 0, function () {
            var openIdMetadataUrl, tokenExtractor, identity, versionClaim, appId, appIdClaim, appZClaim;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        openIdMetadataUrl = authenticationConstants_1.AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl;
                        tokenExtractor = new jwtTokenExtractor_1.JwtTokenExtractor(EmulatorValidation.ToBotFromEmulatorTokenValidationParameters, openIdMetadataUrl, authenticationConstants_1.AuthenticationConstants.AllowedSigningAlgorithms);
                        return [4, tokenExtractor.getIdentityFromAuthHeader(authHeader, channelId, authConfig.requiredEndorsements)];
                    case 1:
                        identity = _a.sent();
                        if (!identity) {
                            throw new Error('Unauthorized. No valid identity.');
                        }
                        if (!identity.isAuthenticated) {
                            throw new Error('Unauthorized. Is not authenticated');
                        }
                        versionClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.VersionClaim);
                        if (versionClaim === null) {
                            throw new Error('Unauthorized. "ver" claim is required on Emulator Tokens.');
                        }
                        appId = '';
                        if (!versionClaim || versionClaim === '1.0') {
                            appIdClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.AppIdClaim);
                            if (!appIdClaim) {
                                throw new Error('Unauthorized. "appid" claim is required on Emulator Token version "1.0".');
                            }
                            appId = appIdClaim;
                        }
                        else if (versionClaim === '2.0') {
                            appZClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.AuthorizedParty);
                            if (!appZClaim) {
                                throw new Error('Unauthorized. "azp" claim is required on Emulator Token version "2.0".');
                            }
                            appId = appZClaim;
                        }
                        else {
                            throw new Error("Unauthorized. Unknown Emulator Token version \"" + versionClaim + "\".");
                        }
                        return [4, credentials.isValidAppId(appId)];
                    case 2:
                        if (!(_a.sent())) {
                            throw new Error("Unauthorized. Invalid AppId passed on token: " + appId);
                        }
                        return [2, identity];
                }
            });
        });
    }
    EmulatorValidation.authenticateEmulatorToken = authenticateEmulatorToken;
})(EmulatorValidation = exports.EmulatorValidation || (exports.EmulatorValidation = {}));
