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
var jwtTokenExtractor_1 = require("./jwtTokenExtractor");
var jwtTokenValidation_1 = require("./jwtTokenValidation");
var SkillValidation;
(function (SkillValidation) {
    var _tokenValidationParameters = {
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
    function isSkillToken(authHeader) {
        if (!jwtTokenValidation_1.JwtTokenValidation.isValidTokenFormat(authHeader)) {
            return false;
        }
        var bearerToken = authHeader.trim().split(' ')[1];
        var payload = jsonwebtoken_1.decode(bearerToken);
        var claims = Object.keys(payload).reduce(function (acc, key) {
            acc.push({ type: key, value: payload[key] });
            return acc;
        }, []);
        return isSkillClaim(claims);
    }
    SkillValidation.isSkillToken = isSkillToken;
    function isSkillClaim(claims) {
        if (!claims) {
            throw new TypeError("SkillValidation.isSkillClaim(): missing claims.");
        }
        var versionClaim = claims.find(function (c) { return c.type === authenticationConstants_1.AuthenticationConstants.VersionClaim; });
        var versionValue = versionClaim && versionClaim.value;
        if (!versionValue) {
            return false;
        }
        var audClaim = claims.find(function (c) { return c.type === authenticationConstants_1.AuthenticationConstants.AudienceClaim; });
        var audienceValue = audClaim && audClaim.value;
        if (!audClaim || authenticationConstants_1.AuthenticationConstants.ToBotFromChannelTokenIssuer === audienceValue) {
            return false;
        }
        var appId = jwtTokenValidation_1.JwtTokenValidation.getAppIdFromClaims(claims);
        if (!appId) {
            return false;
        }
        return appId !== audienceValue;
    }
    SkillValidation.isSkillClaim = isSkillClaim;
    function authenticateChannelToken(authHeader, credentials, channelId, authConfig) {
        return __awaiter(this, void 0, void 0, function () {
            var openIdMetadataUrl, tokenExtractor, parts, identity, err_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!authConfig) {
                            throw new Error('SkillValidation.authenticateChannelToken(): invalid authConfig parameter');
                        }
                        openIdMetadataUrl = authenticationConstants_1.AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl;
                        tokenExtractor = new jwtTokenExtractor_1.JwtTokenExtractor(_tokenValidationParameters, openIdMetadataUrl, authenticationConstants_1.AuthenticationConstants.AllowedSigningAlgorithms);
                        parts = authHeader.split(' ');
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 4, , 5]);
                        return [4, tokenExtractor.getIdentity(parts[0], parts[1], channelId, authConfig.requiredEndorsements)];
                    case 2:
                        identity = _a.sent();
                        return [4, validateIdentity(identity, credentials)];
                    case 3:
                        _a.sent();
                        return [2, identity];
                    case 4:
                        err_1 = _a.sent();
                        throw new Error(err_1);
                    case 5: return [2];
                }
            });
        });
    }
    SkillValidation.authenticateChannelToken = authenticateChannelToken;
    function validateIdentity(identity, credentials) {
        return __awaiter(this, void 0, void 0, function () {
            var versionClaim, audienceClaim, isValid, err_2, appId;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!identity) {
                            throw new Error('SkillValidation.validateIdentity(): Invalid identity');
                        }
                        if (!identity.isAuthenticated) {
                            throw new Error('SkillValidation.validateIdentity(): Token not authenticated');
                        }
                        versionClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.VersionClaim);
                        if (!versionClaim) {
                            throw new Error("SkillValidation.validateIdentity(): '" + authenticationConstants_1.AuthenticationConstants.VersionClaim + "' claim is required on skill Tokens.");
                        }
                        audienceClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.AudienceClaim);
                        if (!audienceClaim) {
                            throw new Error("SkillValidation.validateIdentity(): '" + authenticationConstants_1.AuthenticationConstants.AudienceClaim + "' claim is required on skill Tokens.");
                        }
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4, credentials.isValidAppId(audienceClaim)];
                    case 2:
                        isValid = _a.sent();
                        if (!isValid)
                            throw new Error('SkillValidation.validateIdentity(): Invalid audience.');
                        return [3, 4];
                    case 3:
                        err_2 = _a.sent();
                        throw new Error(err_2);
                    case 4:
                        appId = jwtTokenValidation_1.JwtTokenValidation.getAppIdFromClaims(identity.claims);
                        if (!appId) {
                            throw new Error('SkillValidation.validateIdentity(): Invalid appId.');
                        }
                        return [2];
                }
            });
        });
    }
    SkillValidation.validateIdentity = validateIdentity;
})(SkillValidation = exports.SkillValidation || (exports.SkillValidation = {}));
